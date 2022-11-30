![烟绯律师](https://file.xunkong.cc/static/repo/xunkong/YanfeiLawyer.webp)

# 寻空

> 记录旅途中发生的事

**寻空**可以帮你统计和分析游戏中的各项数据，快速掌握账号的相关信息。在这里你可以：

- 欣赏由开发者精心挑选的同人图
- 快速浏览每日素材，活动攻略
- 查看你的祈愿，原石摩拉，深境螺旋等数据
- 启动游戏并解锁帧数上限
- 管理成就
- 查看角色和武器图鉴
- 规划养成计划
- ……

## 安装

寻空仅支持 x64 和 arm64 架构的 Windows 10 1809 及以上版本的系统，建议升级到最新版获得更稳定的体验。

商店版和侧载版的功能完全一致，二者可以共存，但是因为共用一个数据库，版本不同时可能存在冲突。

不建议同时安装商店版和侧载版，在进行某些操作时会频繁弹窗影响体验。

### 商店版（推荐）

[从应用商店安装](https://www.microsoft.com/store/apps/9N2SVG0JMT12)

应用商店的审核一般需要2个工作日，所以无法做到实时更新，但是商店应用能增量更新。

### 侧载版

> 安装侧载版需要信任自签名证书，此证书仅用于寻空相关项目的代码签名。

如果你看不懂以下内容，请安装商店版。

- 侧载版仅提供 x64 架构的安装包，arm64 架构的设备请使用商店版
- 首次安装侧载版时请从 [Releases](https://github.com/xunkong/xunkong/releases) 页面下载最新的 zip 文件并解压
- 脚本安装
  - 在系统设置中打开 [**开发者选项**](ms-settings:developers) 界面，勾选 `开发人员模式` 和 `允许 PowerShell 脚本`
  - 找到 `Install.ps1`，在该文件的右键菜单中选择 `使用 PowerShell 运行`
  - **安装完成后一定要关闭 `允许 PowerShell 脚本`**
- 手动安装
  - 将 cer 证书文件添加到 `本地计算机/受信任人`
  - 在 `Dependencies` 文件夹下安装符合 CPU 架构的依赖包
  - 双击 msixbundle 文件进行安装
- 后续更新时下载 msixbundle 文件即可

**开发者选项截图**

<details>
<summary>Windows 10</summary>

![dev-setting-win10](https://file.xunkong.cc/static/repo/xunkong/dev-setting-win10.webp)

</details>

<details>
<summary>Windows 11</summary>

![dev-setting-win11](https://file.xunkong.cc/static/repo/xunkong/dev-setting-win11.webp)

</details>

## 应用截图

![screenshot-home-kongying](https://file.xunkong.cc/static/repo/xunkong/screenshot-home-kongying.webp)

## 开发指南

请移步 https://github.com/xunkong/dev

## 赞助

注意了，赞助者没有额外的权益，和普通用户一样都能够使用软件的全部功能。

https://afdian.net/a/scighost

## 致谢

寻空离不开以下开源项目的帮助：

- [GenshinData](https://github.com/Dimbreath/GenshinData)
- [GI-cutscenes](https://github.com/ToaHartor/GI-cutscenes)
- [Snap.Genshin](https://github.com/DGP-Studio/Snap.Genshin)
- [YaeAchievement](https://github.com/HolographicHat/YaeAchievement)

感谢 [Twitter@アナ](https://twitter.com/anna_drw01)、[Twitter@心臓弱眞君](https://twitter.com/xinzoruo) 以及所有为原神创作同人图的画师们。

感谢微软提供的 Visual Studio 社区版 和 Visual Studio Code 开发工具，JetBrains 提供的开源项目许可证，Syncfusion 提供的免费许可证，Cloudflare 提供的免费 CDN，华为提供的开源字体 HarmonyOS Sans。

<div>
    <img alt="Visual Studio" src="https://file.xunkong.cc/static/repo/xunkong/Visual_Studio_Icon_2019.svg" height="60" />
    <img alt="Visual Studio Code" src="https://file.xunkong.cc/static/repo/xunkong/Visual_Studio_Code_1.35_icon.svg" height="60" />
    <img alt="JetBrains" src="https://file.xunkong.cc/static/repo/xunkong/JetBrains_Logo_2016.svg" height="60" />
    <img alt="Syncfusion" src="https://file.xunkong.cc/static/repo/xunkong/syncfusion.png" height="60" />
    <img alt="Cloudflare" src="https://file.xunkong.cc/static/repo/xunkong/Cloudflare_logo.svg" height="60" />
</div>

<br>

> 头图出自 [空耳狂魔生草视频](https://www.bilibili.com/video/av340612690)
