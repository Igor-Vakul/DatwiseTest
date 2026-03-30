namespace SafetyPortal.Api.Services;

/// <summary>
/// Validates uploaded files by comparing their magic bytes against known signatures.
/// This prevents content-type spoofing — e.g. a .exe renamed to .jpg.
/// </summary>
public static class FileSignatureValidator
{
    // Allowed content types mapped to one or more valid magic-byte sequences
    private static readonly Dictionary<string, byte[][]> Signatures = new()
    {
        ["image/jpeg"] =
        [
            [0xFF, 0xD8, 0xFF]
        ],
        ["image/png"] =
        [
            [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]
        ],
        ["image/gif"] =
        [
            [0x47, 0x49, 0x46, 0x38, 0x37, 0x61],   // GIF87a
            [0x47, 0x49, 0x46, 0x38, 0x39, 0x61]    // GIF89a
        ],
        ["image/webp"] =
        [
            [0x52, 0x49, 0x46, 0x46]                 // RIFF — extra WEBP check below
        ],
        ["application/pdf"] =
        [
            [0x25, 0x50, 0x44, 0x46]                 // %PDF
        ],
        ["application/msword"] =
        [
            [0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1]  // OLE2
        ],
        ["application/vnd.ms-excel"] =
        [
            [0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1]  // OLE2
        ],
        ["application/vnd.openxmlformats-officedocument.wordprocessingml.document"] =
        [
            [0x50, 0x4B, 0x03, 0x04]                 // ZIP (PK)
        ],
        ["application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"] =
        [
            [0x50, 0x4B, 0x03, 0x04]                 // ZIP (PK)
        ],
    };

    public static readonly IReadOnlySet<string> AllowedContentTypes =
        new HashSet<string>(Signatures.Keys);

    /// <summary>
    /// Returns true if the file header matches a known signature for the declared content type.
    /// </summary>
    public static bool IsValid(string contentType, ReadOnlySpan<byte> header)
    {
        if (!Signatures.TryGetValue(contentType, out var candidates))
            return false;

        foreach (var sig in candidates)
        {
            if (header.Length < sig.Length)
                continue;

            if (!header[..sig.Length].SequenceEqual(sig))
                continue;

            // WebP requires "WEBP" at offset 8 in addition to the RIFF header
            if (contentType == "image/webp")
            {
                return header.Length >= 12
                    && header[8]  == 0x57   // W
                    && header[9]  == 0x45   // E
                    && header[10] == 0x42   // B
                    && header[11] == 0x50;  // P
            }

            return true;
        }

        return false;
    }

    /// <summary>
    /// Returns "image" for image content types, "document" for office/PDF types.
    /// </summary>
    public static string GetFileCategory(string contentType) =>
        contentType.StartsWith("image/") ? "image" : "document";
}
