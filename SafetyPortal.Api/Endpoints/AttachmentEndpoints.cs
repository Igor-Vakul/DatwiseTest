using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using SafetyPortal.Api.Data;
using SafetyPortal.Api.Entities;
using SafetyPortal.Api.Services;

namespace SafetyPortal.Api.Endpoints;

public static class AttachmentEndpoints
{
    public static IEndpointRouteBuilder MapAttachmentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/incidents/{incidentId:int}/attachments")
                       .WithTags("Attachments")
                       .RequireAuthorization();

        // POST /api/incidents/{incidentId}/attachments — upload a single file
        group.MapPost("/", async (
            int incidentId,
            IFormFile file,
            ClaimsPrincipal principal,
            SafetyPortalDbContext db,
            IWebHostEnvironment env) =>
        {
            if (!int.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out int userId))
                return Results.Unauthorized();

            var incident = await db.IncidentReports.FindAsync(incidentId);
            if (incident is null)
                return Results.NotFound(new { error = "Incident not found." });

            // Validate content type
            var contentType = file.ContentType?.ToLowerInvariant() ?? string.Empty;
            if (!FileSignatureValidator.AllowedContentTypes.Contains(contentType))
                return Results.BadRequest(new
                {
                    error = $"File type '{contentType}' is not allowed. " +
                            "Allowed: JPEG, PNG, GIF, WebP, PDF, DOC, DOCX, XLS, XLSX."
                });

            // Validate size
            var category = FileSignatureValidator.GetFileCategory(contentType);
            var maxBytes = category == "image"
                             ? AppConstants.Attachments.MaxImageBytes
                             : AppConstants.Attachments.MaxDocumentBytes;

            if (file.Length > maxBytes)
                return Results.BadRequest(new
                {
                    error = $"File exceeds the {maxBytes / 1024 / 1024} MB limit for {category}s."
                });

            // Validate magic bytes
            var header = new byte[AppConstants.Attachments.SignatureBytesLen];
            int read;
            await using (var stream = file.OpenReadStream())
                read = await stream.ReadAsync(header);

            if (!FileSignatureValidator.IsValid(contentType, header.AsSpan(0, read)))
                return Results.BadRequest(new
                {
                    error = "File content does not match the declared type. Upload rejected."
                });

            // Persist to disk
            var storageDir = Path.Combine(
                env.ContentRootPath,
                AppConstants.Attachments.StorageFolder,
                incidentId.ToString());

            Directory.CreateDirectory(storageDir);

            var ext = Path.GetExtension(file.FileName);
            var storedName = $"{Guid.NewGuid()}{ext}";
            var storedPath = Path.Combine(storageDir, storedName);

            await using (var dest = File.Create(storedPath))
            await using (var src = file.OpenReadStream())
                await src.CopyToAsync(dest);

            // Save record
            var attachment = new IncidentAttachment
            {
                IncidentReportId = incidentId,
                OriginalFileName = Path.GetFileName(file.FileName),
                StoredFileName = storedName,
                ContentType = contentType,
                FileSizeBytes = file.Length,
                FileCategory = category,
                UploadedAt = DateTime.UtcNow,
                UploadedByUserId = userId
            };

            db.IncidentAttachments.Add(attachment);
            await db.SaveChangesAsync();

            return Results.Created(
                $"/api/incidents/{incidentId}/attachments/{attachment.Id}",
                MapToDto(attachment));
        })
        .DisableAntiforgery();

        // GET /api/incidents/{incidentId}/attachments — list
        group.MapGet("/", async (int incidentId, SafetyPortalDbContext db) =>
        {
            var exists = await db.IncidentReports.AnyAsync(x => x.Id == incidentId);
            if (!exists) return Results.NotFound();

            var attachments = await db.IncidentAttachments
                .Where(a => a.IncidentReportId == incidentId)
                .OrderBy(a => a.UploadedAt)
                .ToListAsync();

            return Results.Ok(attachments.Select(MapToDto));
        });

        // GET /api/incidents/{incidentId}/attachments/{id}/download
        group.MapGet("/{id:int}/download", async (
            int incidentId,
            int id,
            SafetyPortalDbContext db,
            IWebHostEnvironment env) =>
        {
            var attachment = await db.IncidentAttachments
                .FirstOrDefaultAsync(a => a.Id == id && a.IncidentReportId == incidentId);

            if (attachment is null) return Results.NotFound();

            var path = Path.Combine(
                env.ContentRootPath,
                AppConstants.Attachments.StorageFolder,
                incidentId.ToString(),
                attachment.StoredFileName);

            if (!File.Exists(path)) return Results.NotFound(new { error = "File not found on disk." });

            var stream = File.OpenRead(path);
            return Results.File(stream, attachment.ContentType, attachment.OriginalFileName);
        });

        // DELETE /api/incidents/{incidentId}/attachments/{id}
        group.MapDelete("/{id:int}", async (
            int incidentId,
            int id,
            SafetyPortalDbContext db,
            IWebHostEnvironment env) =>
        {
            var attachment = await db.IncidentAttachments
                .FirstOrDefaultAsync(a => a.Id == id && a.IncidentReportId == incidentId);

            if (attachment is null) return Results.NotFound();

            // Remove from disk
            var path = Path.Combine(
                env.ContentRootPath,
                AppConstants.Attachments.StorageFolder,
                incidentId.ToString(),
                attachment.StoredFileName);

            if (File.Exists(path)) File.Delete(path);

            db.IncidentAttachments.Remove(attachment);
            await db.SaveChangesAsync();

            return Results.NoContent();
        })
        .RequireAuthorization("SafetyManagerOrAdmin");

        return app;
    }

    private static object MapToDto(IncidentAttachment a) => new
    {
        a.Id,
        a.OriginalFileName,
        a.ContentType,
        a.FileSizeBytes,
        a.FileCategory,
        a.UploadedAt
    };
}
