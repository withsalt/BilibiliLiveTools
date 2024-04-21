# BilibiliLiveTools

Bilibili（B站）无人值守直播工具。自动登录，自动获取直播推流地址，自动推流（使用ffmpeg），可以用于电脑、树莓派等设备无人值守直播。  

### 前提条件  
- 在Bilibili中通过实名认证，并开通了直播间。[点击连接](https://link.bilibili.com/p/center/index "点击连接")开通直播间（很简单的，实名认证通过后直接就可以开通了）  
- 推流默认使用FFmpeg，树莓派官方系统默认安装了的，我就不再赘述，其它系统请自行安装（Windows版本已经打包ffmpeg，就不用自己装了）。  

### 项目说明
1. BilibiliLive.Tool.UpdateArea  
用于更新项目中的直播分区说明。手动执行也可输出最新的分区信息。  
使用方法：  
```shell
./BilibiliLive.Tool.UpdateArea
```
2. BilibiliAutoLiver  
一键开启直播工具。使用方式请查询说明。  
3. BilibiliLive.Tool.Monitor  
直播监视工具。在直播开播或停播后可以发送邮件进行通知。  
4. BilibiliLiverTests  
单元测试神马的。  

Tips: 除了开播工具BilibiliAutoLiver以外，其余工具没有编译二进制发行版本。如有需要，可以提交Issue或者自行下载编译。  

### Demo
扫描二维码登录：
![](https://raw.githubusercontent.com/withsalt/BilibiliLiveTools/master/docs/images/demo_qrcode_login.jpg)

自动化推流直播：
![](https://raw.githubusercontent.com/withsalt/BilibiliLiveTools/master/docs/images/demo.jpg)


### 教程

#### 在Linux上面推流（这里以树莓派为例，64位操作系统）
1. 获取程序  
```shell
wget https://github.com/withsalt/BilibiliLiveTools/releases/latest/download/BilibiliAutoLiver_Linux_ARM64.zip --no-check-certificate
```

2. 解压并授权  
```shell
unzip BilibiliAutoLiver_Linux_ARM64.zip && chmod -R 755 BilibiliAutoLiver_Linux_ARM64 && chmod +x BilibiliAutoLiver_Linux_ARM64/BilibiliAutoLiver
```

3. 编辑直播设置  
编辑配置文件appsettings.json  
```shell
cd BilibiliAutoLiver_Linux_ARM64/
nano appsettings.json
```
配置文件如下所示，按照提示修改为自己的分区和直播间名称。  
```json
{
  "LiveSetting": {
    //直播间分类
    "LiveAreaId": 369,
    //直播间名称
    "LiveRoomName": "【24H】小金鱼啦~",
    //FFmpeg推流命令，请自行填写对应操作系统和设备的推流命令
    //填写到此处时，请注意将命令中‘"’用‘\’进行转义，将推流的rtmp连接替换为[[URL]]，[[URL]]不需要双引号。
    //下面推流指令默认适配设备树莓派，使用USB摄像头，设备为/dev/video0
    "FFmpegCmd": "ffmpeg -thread_queue_size 1024 -f v4l2 -s 1280*720 -input_format mjpeg -i \"/dev/video0\" -stream_loop -1 -i \"Data/demo_music.m4a\" -vcodec h264_omx -pix_fmt yuv420p -r 30 -s 1280*720 -g 60 -b:v 10M -bufsize 10M -acodec aac -ac 2 -ar 44100 -ab 128k -f flv [[URL]]",
    //ffmpeg异常退出后，是否自动重新启动
    "AutoRestart": true
  }
}
```
由于推流方式不同以及FFMpeg配置比较复杂，不同的平台、不同的硬件的参数都不相同（主要是懒，懒得去写FFMpeg的适配了，直接调用多巴适）。  
这里采用直接填写推流命令的方式。**建议填写之前先测试推流命令能否正确执行。**  
比如默认的推流命令设配树莓派官方系统，并且使用USB摄像头，设备Id为`/dev/video0`。其它系统可能不适用，需要自己修改。  
推流命令（FFmpegCmd）中的“[[URL]]”，是一个配置符号，将在程序中被替换为获取到的Bilibili推流地址，所以一定要在最终命令中，把测试文件或者地址修改为 “[[URL]]”（URL大写） ，否则程序将抛出错误。推流命令中注意半角双引号需要用符号‘\’来进行转义。  
这里提供常见的两条推流命令：  
a. 树莓派，Linux操作系统：  
推流名称为`/dev/video0`的摄像头的画面。  
```shell
ffmpeg -thread_queue_size 1024 -f v4l2 -s 1280*720 -input_format mjpeg -i \"/dev/video0\" -stream_loop -1 -i \"Data/demo_music.m4a\" -vcodec h264_omx -pix_fmt yuv420p -r 30 -s 1280*720 -g 60 -b:v 10M -bufsize 10M -acodec aac -ac 2 -ar 44100 -ab 128k -f flv [[URL]]
```
b. Winodws操作系统  
推流名称为`HD Pro Webcam C920`的摄像头的画面。  
```shell
ffmpeg -f dshow -video_size 1280x720 -i video=\"HD Pro Webcam C920\" -vcodec libx264 -pix_fmt yuv420p -r 30 -s 1280*720 -g 60 -b:v 5000k -an -preset:v ultrafast -tune:v zerolatency -f flv [[URL]]
```
命令参数具体含义我就不解释了，建议直接问ChatGPT，或者直接让ChatGPT帮忙写推流命令。  
![](https://raw.githubusercontent.com/withsalt/BilibiliLiveTools/master/docs/images/ffmpeg_chatgpt_desc.jpg)

5. 安装FFmpeg（可选）  
  为什么是可选？因为树莓派官方系统已经默认内置了ffmpeg，不用自行安装。但是对于一些其他的linux发行版。可能没有安装ffmpeg，所以需要用户自行安装ffmpeg。  
  Linux（基于debian的发行版）：
 
  ```shell
  # 安装，就这一行命令
  sudo apt install ffmpeg
  # 测试是否安装，有输出表示安装完成
  ffmpeg -version
  ```
  
  Windows：  
  前往：https://www.gyan.dev/ffmpeg/builds/ 下载你喜欢版本的ffmpeg。下载之后，解压到你喜欢的路径。然后配置为ffmpeg程序所在路径配置环境变量。  
  （预计下个版本会内置windows版本的ffmpeg）。  


6. 跑起来  
```shell
sudo ./BilibiliAutoLiver
```
第一次运行需要登录。如何登录查看下一小节。  

7. 停止推流  
暂不支持，预计下个版本支持~

8. 配置开机自启等  
Linux上面配置系统服务，可以查看：https://www.quarkbook.com/?p=733  

#### 在Windows系统上面推流

1. 获取程序  
点击链接：[https://github.com/withsalt/BilibiliLiveTools/releases/latest/download/BilibiliLiver_Windows_x64.zip](https://github.com/withsalt/BilibiliLiveTools/releases/latest/download/BilibiliLiver_Windows_x64.zip "https://github.com/withsalt/BilibiliLiveTools/releases/latest/download/BilibiliLiver_Windows_x64.zip")
下载最新的适用于Windows系统的发布包。  
其余步骤和上一节“在Linux上面推流”中步骤一致。  


#### 如何登录
目前已经支持扫码登录和自动维护Cookie。第一次运行程序时，会要求使用Bilibili移动端扫码登录。  
运行程序：  
```shell
./BilibiliAutoLiver
```
程序开始运行后，找到命令行中输出的地址信息，如下图所示：  
![](https://raw.githubusercontent.com/withsalt/BilibiliLiveTools/master/docs/images/use_qrcode_login.jpg)
在任意浏览器中打开上述任意一个地址，将会出现二维码界面  
![](https://raw.githubusercontent.com/withsalt/BilibiliLiveTools/master/docs/images/demo_qrcode_login.jpg)

使用APP进行扫码登录。扫码登录成功后，只要Cookie不过期（Cookie每隔6小时会检查并自动刷新），就不用再进行扫码登录啦。  

### 常见问题

1. FFmpeg报错  
![](https://raw.githubusercontent.com/withsalt/BilibiliLiveTools/master/docs/images/5.png)
肯定是你的ffmpeg指令有问题。这个工具严格意义上来说就是一个ffmpeg调用工具，ffmpeg的使用，全靠你自己。  

2. 通过Cookie登录失败  
![](https://raw.githubusercontent.com/withsalt/BilibiliLiveTools/master/docs/images/6.png)
长期未登录（至少半年），账号失效了？  

### 题外话
项目中所用的API都是各处扒拉来的野生API，非官方提供的API。因为B站随时在更新API，所以工具有随时挂掉的风险。当发现工具非配置原因导致不可用时，请提交issue。所以在未来遇到无法解决问题且无人接收情况下，此项目将会被废弃。还有种情况是，我本人也不用了。也就不更新了.....  

### Todo
1、Windows内置ffmpeg，不用再单独配置环境变量  
2、不再直接使用ffmpeg推流命令进行推流，将改为内置的推流方式。将由更高的可玩性，比如提供一个图像处理管道，通过机器视觉对图像进行二次处理。  
3、弹幕获取功能（有这个计划，但是我感觉没啥用，不一定会搞）  

### 直播分区  
开播时需要将ID填写到LiveSetting中的LiveAreaId中。**请注意正确填写分区ID，不然会有被封的风险。**

[查看直播分区](https://github.com/withsalt/BilibiliLiveTools/blob/master/%E7%9B%B4%E6%92%AD%E5%88%86%E5%8C%BA.md "查看直播分区")

 ## Stargazers over time
[![Stargazers over time](https://starchart.cc/withsalt/BilibiliLiveTools.svg)](https://starchart.cc/withsalt/BilibiliLiveTools)
