<div align="center">
<p><img src="https://www.wpe64.com/web_images/wpe.png" height="150"></p>

# Winsock Packet Editor 2.0.0.24（WPE x64） 
# 基于官网Winsock Packet Editor 2.0.0.23 功能进行个人升级，官网无此版本

<img src="https://img.shields.io/github/license/x-nas/WinsockPacketEditor" alt="License"></img>
[![Visitors](https://visitor-badge.laobi.icu/badge?page_id=x-nas.WinsockPacketEditor&title=Visitors)](https://github.com/x-nas/WinsockPacketEditor)
![GitHub Repo stars](https://img.shields.io/github/stars/x-nas/WinsockPacketEditor?style=dark)
![GitHub Repo forks](https://img.shields.io/github/forks/x-nas/WinsockPacketEditor?style=dark)

&bull; <a href="https://www.wpe64.com">官方网站</a>
&bull; <a href="https://www.wpe64.com">Official website</a>

</div>

## [⭐] 星星历史

[![Star History Chart](https://api.star-history.com/svg?repos=x-nas/WinsockPacketEditor&type=Date)](https://www.star-history.com/#x-nas/WinsockPacketEditor&Date)

## [📚] 软件简介

WPE x64 是一款可以拦截并修改 WinSock 封包的 Windows 软件，自适应支持 32 位及 64 位的目标程序，软件支持 SOCKS 代理和进程注入两种模式，并且具有高级滤镜和自动化机器人等功能，开发中使用了 C# 的多线程和消息队列技术，测试拦截了 100 万+的封包不会卡死或退出，软件不定期会修复 Bug 和更新功能，每次启动的时候支持在线自动更新.

WPE x64 支持直接注入 Windows 进程来拦截 Winsock 封包，也可以通过 SOCKS 代理模式来拦截 Winsock 封包.

本软件使用了微软的 VS2022 集成开发环境，.NET Framework 4.8 开发框架，以及 ClickOnce 部署资源。每次版本更新后，都会在启动程序时自动下载最新版本。如果更新服务器不可用，也不会导致程序无法使用。当然，如果您不希望自动更新，也可以在启动时手动关闭自动更新，或者直接下载离线打包版使用。

## [🎖️] 软件特色

- [x] 支持 SOCKS 代理和进程注入两种模式，确保在各种情况下都可以拦截到 Winsock 封包.
- [x] 代理模式下支持多种主流代理协议和 SSL 安全协议，并具有端口映射和断点调试等功能.
- [x] 具备自动化的可编程机器人功能，可在满足触发条件的情况下执行预定义的指令集.
- [x] 消息队列缓存模式，所有的封包依次排队进入 MQ 队列，无需等待缓存结束后再显示封包.
- [x] 您可以自定义需要拦截的封包类型，已包含 WinSock 1.1 和 2.0 的 APIs.
- [x] 注入器和封包编辑器相对独立，可一次注入多个软件后，分别获取不同程序的网络封包.
- [x] 您可以通过选择一个尚未运行的程序注入后，从启动阶段即开始获取程序的所有封包数据.
- [x] 直观的封包对比功能，支持多种数据格式之间快速切换.
- [x] 您可以方便的对封包内容进行搜索，支持多种数据格式的快速搜索定位.
- [x] 支持批量发送封包，您可以自定义发送的顺序和循环次数，并支持导入导出和备注功能.
- [x] 强大的滤镜功能，支持高级滤镜，并且可以自定义修改封包的长度和修改次数.
- [x] 支持注入 Winsock 代理程序后，再获取目标程序的网络封包.
- [x] 您可以直接注入各类模拟器，并直接获取模拟器以及运行的程序的网络封包.
- [x] 您对系统的各种配置都会及时的进行保存，下次启动软件时会自动带出上一次的设置.
- [x] 软件运行期间会实时记录运行日志并支持导出，方便定位问题和提交处理.
- [x] 支持 64 位的 Windows 操作系统和 64 位的目标程序，并且会根据目标进程的类型来自动调用 32 位或 64 位的动态库注入目标程序.
- [x] 软件使用的 .NET 程序集不需要在全局程序集缓存（GAC）中注册，大大简化了使用和二次开发.
- [x] 支持多线程技术，处理封包时不会影响程序的正常操作.
- [x] 拦截封包结束后会自动处理挂钩并释放资源，避免对程序运行产生影响.
- [x] 不会使目标程序产生资源和内存泄露风险.
- [x] 软件安装时会自动检测必须的组件和运行库，确保NET框架已安装.
- [x] 采用微软 ClickOnce 发布技术，支持在线自动安装和更新.
- [x] 支持多语言版本，方便不同国家和地区的用户使用.

## [🖼️] 本次更新记录
- [x] 更新发送封包不能复制hex和复制文本，粘贴文本和粘贴hex
- [x] 更新滤镜功能，不能同时选中多和hex
- [x] 更新发送封包右侧，转成的字符不能复制
- [x] 添加发送封包右侧，LEB128和SLEB128转换
- [x] 添加粘贴hex进制快捷键，ctrl+shift+V
- [x] 更新代理收缩是显示在状态栏图标中，而不是工作区
- [x] 更新关闭抓包主程序时，同时结束代理窗口进程

## [🖼️] 软件界面 Software UI

![Proxy](https://github.com/user-attachments/assets/ba1bfe80-3c1c-4839-aa68-24aa5ddb4738)

![Process](https://github.com/user-attachments/assets/6bfe3e16-cfc0-42c3-987c-26724363adb2)

![111](https://github.com/user-attachments/assets/e33412c1-3a9f-41f8-b23e-aada6a1bb104)
![222](https://github.com/user-attachments/assets/6c9f6fa8-94a9-4aea-8119-2ebe152ff7c2)

## [👏] 特别说明 Special Note

本项目已加入 [DotNetGuide](https://github.com/YSGStudyHards/DotNetGuide)  列表。<br/>
本项目已加入 [dotNET China](https://gitee.com/dotnetchina)  组织。<br/>

![dotnetchina](https://images.gitee.com/uploads/images/2021/0324/120117_2da9922c_416720.png "132645_21007ea0_974299.png")
