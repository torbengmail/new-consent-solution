namespace PrivacyService.Api.Helpers;

public static class PathSanitizer
{
    public static string SanitizeSegment(string segment)
    {
        if (string.IsNullOrWhiteSpace(segment))
            throw new ArgumentException("Path segment cannot be empty");

        var sanitized = segment.Replace("..", "").Replace("/", "").Replace("\\", "");
        sanitized = new string(sanitized.Where(c => !Path.GetInvalidFileNameChars().Contains(c)).ToArray());

        if (string.IsNullOrWhiteSpace(sanitized))
            throw new ArgumentException("Path segment contains only invalid characters");

        return sanitized;
    }

    public static string SanitizeFileName(string fileName)
    {
        var name = SanitizeSegment(Path.GetFileNameWithoutExtension(fileName));
        var ext = Path.GetExtension(fileName);
        var safeExt = string.IsNullOrEmpty(ext)
            ? ""
            : "." + new string(ext.Skip(1).Where(char.IsLetterOrDigit).ToArray());
        return name + safeExt;
    }
}
