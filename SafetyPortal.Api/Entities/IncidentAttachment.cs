namespace SafetyPortal.Api.Entities;

public class IncidentAttachment
{
    public int    Id               { get; set; }

    public int    IncidentReportId { get; set; }
    public IncidentReport IncidentReport { get; set; } = null!;

    public string OriginalFileName { get; set; } = string.Empty;  // original upload name
    public string StoredFileName   { get; set; } = string.Empty;  // GUID-based, safe on disk
    public string ContentType      { get; set; } = string.Empty;
    public long   FileSizeBytes    { get; set; }
    public string FileCategory     { get; set; } = string.Empty;  // "image" | "document"

    public DateTime UploadedAt       { get; set; }
    public int      UploadedByUserId { get; set; }
    public User     UploadedByUser   { get; set; } = null!;
}
