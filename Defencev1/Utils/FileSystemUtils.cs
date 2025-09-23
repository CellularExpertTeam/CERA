namespace Defencev1.Utils;

public static class FileSystemUtils
{
    public static string GetOrCreateGeodatasetDirectory(long geodatasetId)
    {
        string geodatasetsFolder = Path.Combine(FileSystem.AppDataDirectory, "Geodatasets", $"geodataset_{geodatasetId}");
        if (!Path.Exists(geodatasetsFolder))
        {
            Directory.CreateDirectory(geodatasetsFolder);
        }
        return geodatasetsFolder;
    }
}
