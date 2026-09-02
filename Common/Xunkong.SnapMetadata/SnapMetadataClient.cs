using System.IO.Compression;
using System.Net.Http.Json;
using System.Text.Json;

namespace Xunkong.SnapMetadata;

public class SnapMetadataClient
{
    private const string AppDataPathEnvVar = "XUNKONG_APP_DATA_PATH";
    // 没有release途径，直接下载zip包
    private const string MetadataZipUrl = "https://api.github.com/repos/wangdage12/Snap.Metadata/zipball/main";
    // 使用commit hash来判断是否更新
    private const string MetadataCommitHashUrl = "https://api.github.com/repos/wangdage12/Snap.Metadata/git/ref/heads/main";

    private readonly HttpClient _httpClient;
    private readonly string _metadataFolder;

    public SnapMetadataClient(HttpClient httpClient, string? appDataPath = null)
    {
        _httpClient = httpClient;
        _metadataFolder = Path.Combine(
            GetAppDataPath(appDataPath),
            "Snap.Metadata");
    }

    private static string GetAppDataPath(string? appDataPath)
    {
        if (!string.IsNullOrWhiteSpace(appDataPath))
        {
            return appDataPath;
        }
        var fallbackPath = Environment.GetEnvironmentVariable(AppDataPathEnvVar);
        if (!string.IsNullOrWhiteSpace(fallbackPath))
        {
            return fallbackPath;
        }

        throw new InvalidOperationException($"Both environment variable '{AppDataPathEnvVar}' and provide appDataPath are null or whitespace.");
    }

    private static JsonSerializerOptions JsonSerializerOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

    private async Task EnsureMetadataDownloadedAsync()
    {
        Directory.CreateDirectory(_metadataFolder);
        string tempZipPath = Path.Combine(Path.GetTempPath(), $"Snap.Metadata.{Guid.NewGuid():N}.zip");
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, MetadataZipUrl);
            var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            await using (var responseStream = await response.Content.ReadAsStreamAsync())
            await using (var fileStream = File.Create(tempZipPath))
            {
                await responseStream.CopyToAsync(fileStream);
            }

            ExtractToMetadataFolder(tempZipPath);
        }
        finally
        {
            if (File.Exists(tempZipPath))
            {
                File.Delete(tempZipPath);
            }
        }
    }

    private void ExtractToMetadataFolder(string zipPath)
    {
        string tempExtractFolder = Path.Combine(Path.GetTempPath(), $"Snap.Metadata.Extract.{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempExtractFolder);

        try
        {
            ZipFile.ExtractToDirectory(zipPath, tempExtractFolder, overwriteFiles: true);

            string[] topLevelDirectories = Directory.GetDirectories(tempExtractFolder);
            string[] topLevelFiles = Directory.GetFiles(tempExtractFolder);
            string sourceFolder = topLevelDirectories.Length == 1 && topLevelFiles.Length == 0
                ? topLevelDirectories[0]
                : tempExtractFolder;

            CopyDirectory(sourceFolder, _metadataFolder);
        }
        finally
        {
            if (Directory.Exists(tempExtractFolder))
            {
                Directory.Delete(tempExtractFolder, recursive: true);
            }
        }
    }

    private static void CopyDirectory(string sourceFolder, string destinationFolder)
    {
        foreach (string directory in Directory.GetDirectories(sourceFolder, "*", SearchOption.AllDirectories))
        {
            string relativePath = Path.GetRelativePath(sourceFolder, directory);
            Directory.CreateDirectory(Path.Combine(destinationFolder, relativePath));
        }

        foreach (string file in Directory.GetFiles(sourceFolder, "*", SearchOption.AllDirectories))
        {
            string relativePath = Path.GetRelativePath(sourceFolder, file);
            string destinationPath = Path.Combine(destinationFolder, relativePath);
            string? destinationDirectory = Path.GetDirectoryName(destinationPath);
            if (!string.IsNullOrWhiteSpace(destinationDirectory))
            {
                Directory.CreateDirectory(destinationDirectory);
            }

            File.Copy(file, destinationPath, overwrite: true);
        }
    }

    private string GetPackageFilePath(string relativePath)
    {
        string normalizedPath = relativePath.Replace('/', Path.DirectorySeparatorChar);
        return Path.Combine(_metadataFolder, normalizedPath);
    }

    private async Task<T> ReadLocalJsonAsync<T>(string relativePath)
    {
        string filePath = GetPackageFilePath(relativePath);
        await using FileStream stream = File.OpenRead(filePath);
        T? value = await JsonSerializer.DeserializeAsync<T>(stream, JsonSerializerOptions);
        return value ?? throw new JsonException($"Failed to deserialize metadata file: {filePath}");
    }

    public async Task<string> GetLatestMetadataHashAsync()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, MetadataCommitHashUrl);
        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var refResponse = await response.Content.ReadFromJsonAsync<RefResponse>(JsonSerializerOptions);
        return refResponse?.Object?.Sha;
    }

    public async Task<SnapMeta> GetSnapMetaAsync(bool force = false)
    {
        if (!force) await EnsureMetadataDownloadedAsync();
        const string relativePath = "Genshin/CHS/Meta.json";
        return await ReadLocalJsonAsync<SnapMeta>(relativePath);
    }


    public async Task<SnapAvatarInfo> GetAvatarInfoAsync(string id)
    {
        string relativePath = $"Genshin/CHS/{id}.json";
        return await ReadLocalJsonAsync<SnapAvatarInfo>(relativePath);
    }


    public async Task<List<SnapWeaponInfo>> GetWeaponInfosAsync()
    {
        const string relativePath = "Genshin/CHS/Weapon.json";
        return await ReadLocalJsonAsync<List<SnapWeaponInfo>>(relativePath);
    }




    public async Task<List<SnapGachaEventInfo>> GetGachaEventInfosAsync()
    {
        const string relativePath = "Genshin/CHS/GachaEvent.json";
        return await ReadLocalJsonAsync<List<SnapGachaEventInfo>>(relativePath);
    }



    public async Task<List<SnapAchievementItem>> GetAchievementItemsAsync()
    {
        const string relativePath = "Genshin/CHS/Achievement.json";
        return await ReadLocalJsonAsync<List<SnapAchievementItem>>(relativePath);
    }


    public async Task<List<SnapAchievementGoal>> GetAchievementGoalsAsync()
    {
        const string relativePath = "Genshin/CHS/AchievementGoal.json";
        return await ReadLocalJsonAsync<List<SnapAchievementGoal>>(relativePath);
    }


    public async Task<List<SnapDisplayItem>> GetDisplayItemsAsync()
    {
        const string relativePath = "Genshin/CHS/DisplayItem.json";
        return await ReadLocalJsonAsync<List<SnapDisplayItem>>(relativePath);
    }



    public async Task<List<SnapPromote>> GetPromotesAsync()
    {
        const string relativePath1 = "Genshin/CHS/AvatarPromote.json";
        var list1 = await ReadLocalJsonAsync<List<SnapPromote>>(relativePath1);
        const string relativePath2 = "Genshin/CHS/WeaponPromote.json";
        var list2 = await ReadLocalJsonAsync<List<SnapPromote>>(relativePath2);
        return list1.Concat(list2).ToList();
    }


    public async Task<List<SnapMaterial>> GetMaterialsAsync()
    {
        const string relativePath = "Genshin/CHS/Material.json";
        return await ReadLocalJsonAsync<List<SnapMaterial>>(relativePath);
    }


}

internal class RefResponse
{
    public RefObject Object { get; set; }
}

internal class RefObject
{
    public string Sha { get; set; }
}

