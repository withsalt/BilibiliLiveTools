# BilibiliLiveTools

Bilibili直播工具。自动登录并获取推流地址，可以用于电脑、树莓派等设备无人值守直播。

#### 致谢
Bilibili登录API以及部分工具类从项目[BilibiliAfk](https://github.com/wwh1004/BilibiliAfk "BilibiliAfk") Fork而来。本项目也是在项目[BilibiliAfk](https://github.com/wwh1004/BilibiliAfk "BilibiliAfk")基础上开发。

#### 前提条件
在Bilibili通过了实名认证并拥有直播间。开启直播间方式很简单，实名认证完了打开就行。

#### 如何使用
首先在Users.json配置Account（登录名称）、Password（密码）。如下所示：
```json
[
  {
    "Account": "xxxxxxx",
    "Password": "xxxxxxxxxxx"
  }
]
```

配置LiveSetting.json中的LiveCategory（直播分类）、LiveRoomName（直播间名称）。如下所示：
```json
{
  "LiveCategory": "28",
  "LiveRoomName": "直播间名称"
}

```

然后运行程序即可。


项目开发当中，未完待续....
目前已经完成了登录和开播。