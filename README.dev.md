# 开发指南

## 环境

- Windows 10 1809 及以上版本
- 安装 Visual Studio 2022
- 安装 [Windows App SDK](https://docs.microsoft.com/zh-cn/windows/apps/windows-app-sdk/set-up-your-development-environment)

## 拉取

- 使用 Git 拉取本项目
- 更新各子模块至最新的提交
- 在 Release 页面下载最新的资源包并解压至 `Desktop\Package\Assets` 文件夹

## 生成

- 打开 `Xunkong.Dev.sln` 文件，并把 `Xunkong.Desktop.Package` 设为启动项目
- 解决方案平台更改为 x64 或 arm64（不会有人用 Windows ARM 开发吧）
- 修改项目 `Xunkong.Desktop`、`Xunkong.Desktop.Background`、`Xunkong.Desktop.Package` 的 `目标操作系统版本` 为**已安装的 Windows SDK 的版本**
- 打开项目 `Xunkong.Desktop.Package` 内的 `Package.appxmanifest` 文件，转到打包栏，修改包名（不修改则会在调试时卸载已安装的侧载版）
- 生成项目

该项目勾选了 **不启动，但是在启动时调试代码**，点击调试后需要手动打开程序

## 发布

- 打开项目 `Xunkong.Desktop.Package` 内的 `Package.appxmanifest` 文件，转到打包栏
- 在发布者右侧点击 `选择证书`，然后创建签名证书
- 右键点击项目 `Xunkong.Desktop.Package`，依次选择 `发布`、`创建应用程序包`
- 勾选 `旁加载`、取消勾选 `启用自动更新`，签名方法选择 `使用当前证书`
- 填写版本号，选择合适的配置方案，创建应用程序包

## 截图

<details>
<summary>VS工作负载</summary>

![vs-workload](https://file.xunkong.cc/static/repo/xunkong/vs-workload.webp)

</details>

<details>
<summary>目标操作系统版本（修改为已安装的 Windows SDK 版本）</summary>

![target-os-1](https://file.xunkong.cc/static/repo/xunkong/target-os-1.webp)
![target-os-2](https://file.xunkong.cc/static/repo/xunkong/target-os-2.webp)

</details>

<details>
<summary>修改包名、创建证书</summary>

![package-operation](https://file.xunkong.cc/static/repo/xunkong/package-operation.webp)

</details>
