> 米游社原神部分 API，以及其他内容

## Xunkong.Hoyolab

[![nuget](https://img.shields.io/nuget/v/Xunkong.Hoyolab.svg?style=flat-square)](https://www.nuget.org/packages/Xunkong.Hoyolab/)

更多细节请看代码注释

### 引用

```
dotnet add package Xunkong.Hoyolab
```

### 米游社

``` CSharp
using Xunkong.Hoyolab;

var cookie = "your cookie";
var client = new HoyolabClient();
// 米游社账号
var user = await client.GetHoyolabUserInfoAsync(cookie);
// 原神账号
var roles = await client.GetGenshinRoleInfosAsync(cookie);
var role = roles[0];
// 签到信息
var signInfo = await client.GetSignInInfoAsync(role);
// 开始签到
var isSigned = await client.SignInAsync(role);
// 战绩
var summary = await client.GetGameRecordSummaryAsync(role);
// 角色
var details = await client.GetAvatarDetailsAsync(role);
// 角色技能
var skills = await client.GetAvatarSkillLevelAsync(role, details.FirstOrDefault()?.Id);
// 活动
var act = await client.GetActivitiesAsync(role);
// 便笺
var dailynote = await client.GetDailyNoteAsync(role);
// 札记
var travelNotesSummary = await client.GetTravelNotesSummaryAsync(role);
// 深渊
var spirall = await client.GetSpiralAbyssInfoAsync(role, 1);
// 新闻列表
var newsList = await client.GetNewsListAsync(Xunkong.Hoyolab.News.NewsType.Announce);
// 新闻内容
var newsDetail = await client.GetNewsDetailAsync(newsList.FirstOrDefault()?.PostId ?? 0);

```

### 抽卡记录

> Windows 平台限定

``` CSharp
using Xunkong.Hoyolab.Wishlog;

var url = await WishlogClient.GetWishlogUrlFromLogFileAsync();
var client = new WishlogClient();
var uid = await client.GetUidAsync(url);
var wishlogs = await client.GetAllWishlogAsync(url, lastId: 0);
```
