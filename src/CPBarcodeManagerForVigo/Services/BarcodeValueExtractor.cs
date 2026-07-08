namespace CPBarcodeManagerForVigo.Services;

public static class BarcodeValueExtractor
{
    public static string Extract(string fileNameWithoutExtension, int skipCharacters, int takeCharacters)
    {
        if (string.IsNullOrWhiteSpace(fileNameWithoutExtension))
            throw new InvalidOperationException("Empty file name.");

        if (fileNameWithoutExtension.Length < skipCharacters + takeCharacters)
            throw new InvalidOperationException($"File name is too short. Required minimum length: {skipCharacters + takeCharacters} characters.");

        var value = fileNameWithoutExtension.Substring(skipCharacters, takeCharacters);

        if (!value.All(char.IsDigit))
            throw new InvalidOperationException($"Extracted value '{value}' is not numeric.");

        return value;
    }
}
