# BilibiliLiveTools

Bilibili（B站）无人值守直播工具。自动登录，自动获取直播推流地址，自动推流（使用ffmpeg），可以用于电脑、树莓派等设备无人值守直播。  

### 前提条件  
- 在Bilibili中通过实名认证，并开通了直播间。[点击连接](https://link.bilibili.com/p/center/index "点击连接")开通直播间（很简单的，实名认证通过后直接就可以开通了）  
- 推流默认使用FFmpeg，树莓派官方系统默认安装了的，我就不再赘述，其它系统请自行安装（Windows版本已经打包ffmpeg，就不用自己装了）。  

### 项目说明
1. BilibiliAutoLiver.Plugin.Base  
视频帧处理插件（目前还不支持）。  
2. BilibiliAutoLiver  
一键开启直播工具。使用方式请查询说明。  
3. Bilibili.AspNetCore.Apis  
哔哩哔哩API封装，封装了完整的登录，Cookie管理相关的API，有其他项目需要可以直接使用。  
4. BilibiliLiverTests  
单元测试神马的。  

### Demo
扫描二维码登录：
![](https://raw.githubusercontent.com/withsalt/BilibiliLiveTools/master/docs/images/demo_qrcode_login.jpg)

自动化推流直播：
![](https://raw.githubusercontent.com/withsalt/BilibiliLiveTools/master/docs/images/demo.jpg)

### 注意  
 - **请勿将端口直接暴露在公网！请勿将端口直接暴露在公网！请勿将端口直接暴露在公网！**  
 - 高级模式命令中，请勿填写危险命令，比如shutdown等  

### 教程

#### 开始推流
1. 获取程序  
   Linux(树莓派 64位操作系统):  
   ```shell
   wget https://github.com/withsalt/BilibiliLiveTools/releases/latest/download/BilibiliAutoLiver_Linux_ARM64.zip --no-check-certificate
   ```

   Windows：  
   点击链接下载：[https://github.com/withsalt/BilibiliLiveTools/releases/latest/download/BilibiliAutoLiver_Windows_x64.zip](https://github.com/withsalt/BilibiliLiveTools/releases/latest/download/BilibiliAutoLiver_Windows_x64.zip "https://github.com/withsalt/BilibiliLiveTools/releases/latest/download/BilibiliLiver_Windows_x64.zip")

2. 解压并授权  
   ```shell
   unzip BilibiliAutoLiver_Linux_ARM64.zip && chmod -R 755 BilibiliAutoLiver_Linux_ARM64 && chmod +x BilibiliAutoLiver_Linux_ARM64/BilibiliAutoLiver
   ```
3. [**Linux**]安装必需组件  
   Linux环境下，需要安装ffmpeg和alsa。Windows环境下，跳过这一步。  
   ```shell
   sudo apt install ffmpeg alsa-utils
   ```
4. 运行  
   在Linux上面运行：  
   ```shell
   ./BilibiliAutoLiver 
   ```
   这行命令的意思是运行程序，并监听18686端口。  

5. 使用哔哩哔哩APP扫码登录  
   程序开始运行后，找到命令行中输出的地址信息，如下图所示：  
   ![](https://raw.githubusercontent.com/withsalt/BilibiliLiveTools/master/docs/images/use_qrcode_login.jpg)
   在任意浏览器中打开上述任意一个地址，将会出现二维码界面  
   ![](https://raw.githubusercontent.com/withsalt/BilibiliLiveTools/master/docs/images/demo_qrcode_login.jpg)
   使用哔哩哔哩APP进行扫码登录。扫码登录成功后，只要账号不主动退出，就不用再进行扫码登录啦。  

6. 编辑直播设置  
   默认情况下，是没有推流配置的。只有配置推流信息之后，才能进行推流。  
   扫码登录之后，进入**直播设置->推流设置**。  
   **简单模式**  
   不做过多说明，按照提示选好就能开播。
   
   **高级模式**  
   按照说明填写推流命令。 **建议填写之前先测试推流命令能否正确执行。**  
   ![](https://raw.githubusercontent.com/withsalt/BilibiliLiveTools/master/docs/images/push_setting.jpg)
   高级模式推流命令中的“{URL}”，是一个配置符号，将在程序中被替换为获取到的Bilibili推流地址，所以一定要在最终命令中，把测试文件或者地址修改为 “{URL}”（URL大写） ，否则无法保存。  
   这里提供常见的两条推流命令：  
   
   a. 树莓派，Linux操作系统：  
   推流名称为`/dev/video0`的摄像头的画面。  
   ```shell
   ffmpeg -thread_queue_size 1024 -f v4l2 -s 1280*720 -input_format mjpeg -i \"/dev/video0\" -stream_loop -1 -i \"Content/demo_music.m4a\" -vcodec h264_omx -pix_fmt yuv420p -r 30 -s 1280*720 -g 60 -b:v 10M -bufsize 10M -acodec aac -ac 2 -ar 44100 -ab 128k -f flv {URL}
   ```
   b. Winodws操作系统  
   推流名称为`HD Pro Webcam C920`的摄像头的画面。  
   ```shell
   ffmpeg -f dshow -video_size 1280x720 -i video=\"HD Pro Webcam C920\" -vcodec libx264 -pix_fmt yuv420p -r 30 -s 1280*720 -g 60 -b:v 5000k -an -preset:v ultrafast -tune:v zerolatency -f flv {URL}
   ```
   命令参数具体含义我就不解释了，建议直接问ChatGPT，或者直接让ChatGPT帮忙写推流命令。  
   ![](https://raw.githubusercontent.com/withsalt/BilibiliLiveTools/master/docs/images/ffmpeg_chatgpt_desc.jpg)  

7. 停止推流  
   进入**直播设置->推流设置**，点击停止直播。  

8. 配置开机自启  
   编写一个系统服务  
   ```shell
   sudo nano /etc/systemd/system/bilibiliautoliver.service
   ```
   将下方代码中的执行路径替换为BilibiliAutoLiver程序所在的绝对路径。比如“/home/pi/BilibiliLiver_Linux_ARM64/BilibiliAutoLiver”，注意大小写。
   ```shell
   [Unit]
   Description=BilibiliLiver
   After=network.target
   
   [Service]
   WorkingDirectory=应用程序所在绝对路径，比如/home/pi/BilibiliLiver_Linux_ARM64
   ExecStart=应用程序本身绝对路径，比如/home/pi/BilibiliLiver_Linux_ARM64/BilibiliAutoLiver
   Restart=always
   SyslogIdentifier=BilibiliAutoLiver
   User=root
   Environment=ASPNETCORE_ENVIRONMENT=Production
   
   [Install]
   WantedBy=multi-user.target
   ```
   

### 常见问题

1. FFmpeg报错  
![](https://raw.githubusercontent.com/withsalt/BilibiliLiveTools/master/docs/images/5.png)
肯定是你的ffmpeg指令有问题。这个工具严格意义上来说就是一个ffmpeg调用工具，ffmpeg的使用，全靠你自己。  

2. 通过Cookie登录失败  
![](https://raw.githubusercontent.com/withsalt/BilibiliLiveTools/master/docs/images/6.png)
长期未登录（至少半年），修改过密码（修改密码后全平台下线），账号失效了？

3. FFmpeg测试失败，An error occurred trying to start process...
![](https://raw.githubusercontent.com/withsalt/BilibiliLiveTools/master/docs/images/not_install_ffmpeg.jpg)
未安装ffmpeg。

4. ffmpeg cannot set channel count to 2 (Invalid argument)
一般情况该报错是在Linux环境中使用USB声卡。应该是声卡不支持2声道，建议换个声卡？

### 题外话
项目中所用的API都是各处扒拉来的野生API，非官方提供的API。因为B站随时在更新API，所以工具有随时挂掉的风险。当发现工具非配置原因导致不可用时，请提交issue。所以在未来遇到无法解决问题且无人接收情况下，此项目将会被废弃。  
还有种情况是，我本人也不用了。也就不更新了.....  

### Todo
1、添加日志功能，能清晰的看到是为何开启推流失败  
2、不再直接使用ffmpeg推流命令进行推流，将改为内置的推流方式。将有更高的可玩性，比如提供一个图像处理管道，通过机器视觉对图像进行二次处理。  
3、弹幕获取功能（有这个计划，但是我感觉没啥用，不一定会搞）  

 ## Stargazers over time
[![Stargazers over time](https://starchart.cc/withsalt/BilibiliLiveTools.svg)](https://starchart.cc/withsalt/BilibiliLiveTools)
