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
        const string url = "https://hutao-metadata2.snapgenshin.com/Genshin/CHS/Meta.json";
        return await _httpClient.GetFromJsonAsync<SnapMeta>(url, JsonSerializerOptions);
    }


    public async Task<List<SnapAvatarInfo>> GetAvatarInfosAsync()
    {
        const string url = "https://hutao-metadata2.snapgenshin.com/Genshin/CHS/Avatar.json";
        return await _httpClient.GetFromJsonAsync<List<SnapAvatarInfo>>(url, JsonSerializerOptions);
    }


    public async Task<List<SnapWeaponInfo>> GetWeaponInfosAsync()
    {
        const string url = "https://hutao-metadata2.snapgenshin.com/Genshin/CHS/Weapon.json";
        return await _httpClient.GetFromJsonAsync<List<SnapWeaponInfo>>(url, JsonSerializerOptions);
    }




    public async Task<List<SnapGachaEventInfo>> GetGachaEventInfosAsync()
    {
        const string url = "https://hutao-metadata2.snapgenshin.com/Genshin/CHS/GachaEvent.json";
        return await _httpClient.GetFromJsonAsync<List<SnapGachaEventInfo>>(url, JsonSerializerOptions);
    }



    public async Task<List<SnapAchievementItem>> GetAchievementItemsAsync()
    {
        const string url = "https://hutao-metadata2.snapgenshin.com/Genshin/CHS/Achievement.json";
        return await _httpClient.GetFromJsonAsync<List<SnapAchievementItem>>(url, JsonSerializerOptions);
    }


    public async Task<List<SnapAchievementGoal>> GetAchievementGoalsAsync()
    {
        const string url = "https://hutao-metadata2.snapgenshin.com/Genshin/CHS/AchievementGoal.json";
        return await _httpClient.GetFromJsonAsync<List<SnapAchievementGoal>>(url, JsonSerializerOptions);
    }


}



