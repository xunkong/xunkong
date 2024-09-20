using System.Net.Http.Json;
using System.Text.Json;

namespace Xunkong.SnapMetadata;

public class SnapMetadataClient
{


    private readonly HttpClient _httpClient;


    public SnapMetadataClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }


    private static JsonSerializerOptions JsonSerializerOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };


    public async Task<SnapMeta> GetSnapMetaAsync()
    {
        const string url = "https://api.snapgenshin.com/metadata/Genshin/CHS/Meta.json";
        return await _httpClient.GetFromJsonAsync<SnapMeta>(url, JsonSerializerOptions);
    }


    public async Task<SnapAvatarInfo> GetAvatarInfoAsync(string id)
    {
        string url = $"https://api.snapgenshin.com/metadata/Genshin/CHS/{id}.json";
        return await _httpClient.GetFromJsonAsync<SnapAvatarInfo>(url, JsonSerializerOptions);
    }


    public async Task<List<SnapWeaponInfo>> GetWeaponInfosAsync()
    {
        const string url = "https://api.snapgenshin.com/metadata/Genshin/CHS/Weapon.json";
        return await _httpClient.GetFromJsonAsync<List<SnapWeaponInfo>>(url, JsonSerializerOptions);
    }




    public async Task<List<SnapGachaEventInfo>> GetGachaEventInfosAsync()
    {
        const string url = "https://api.snapgenshin.com/metadata/Genshin/CHS/GachaEvent.json";
        return await _httpClient.GetFromJsonAsync<List<SnapGachaEventInfo>>(url, JsonSerializerOptions);
    }



    public async Task<List<SnapAchievementItem>> GetAchievementItemsAsync()
    {
        const string url = "https://api.snapgenshin.com/metadata/Genshin/CHS/Achievement.json";
        return await _httpClient.GetFromJsonAsync<List<SnapAchievementItem>>(url, JsonSerializerOptions);
    }


    public async Task<List<SnapAchievementGoal>> GetAchievementGoalsAsync()
    {
        const string url = "https://api.snapgenshin.com/metadata/Genshin/CHS/AchievementGoal.json";
        return await _httpClient.GetFromJsonAsync<List<SnapAchievementGoal>>(url, JsonSerializerOptions);
    }


    public async Task<List<SnapDisplayItem>> GetDisplayItemsAsync()
    {
        const string url = "https://api.snapgenshin.com/metadata/Genshin/CHS/DisplayItem.json";
        return await _httpClient.GetFromJsonAsync<List<SnapDisplayItem>>(url, JsonSerializerOptions);
    }



    public async Task<List<SnapPromote>> GetPromotesAsync()
    {
        const string url1 = "https://api.snapgenshin.com/metadata/Genshin/CHS/AvatarPromote.json";
        var list1 = await _httpClient.GetFromJsonAsync<List<SnapPromote>>(url1, JsonSerializerOptions);
        const string url2 = "https://api.snapgenshin.com/metadata/Genshin/CHS/WeaponPromote.json";
        var list2 = await _httpClient.GetFromJsonAsync<List<SnapPromote>>(url2, JsonSerializerOptions);
        return list1.Concat(list2).ToList();
    }


    public async Task<List<SnapMaterial>> GetMaterialsAsync()
    {
        const string url = "https://api.snapgenshin.com/metadata/Genshin/CHS/Material.json";
        return await _httpClient.GetFromJsonAsync<List<SnapMaterial>>(url, JsonSerializerOptions);
    }


}



