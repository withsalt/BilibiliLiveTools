# BilibiliLiveTools

Bilibili直播工具。自动登录并获取推流地址，可以用于电脑、树莓派等设备无人值守直播。

#### 注意
因为B站随时在更新API，所以工具有随时挂掉的风险。当发现工具非配置原因导致不可用时，请提交issue。API也是本人参考github其他项目来的，未深入了解过B站APP，所以在未来遇到无法解决问题且无人接收情况下，此项目将会被废弃。

#### 致谢
本工具登录API采用[BiliAccount](https://github.com/LeoChen98/BiliAccount "BiliAccount") 。

#### 前提条件
（1）.首先要有一个连接了摄像头的Linux系统。并能够访问网络。  
（2）.在Bilibili中通过实名认证，并开通了直播间。[点击连接](https://link.bilibili.com/p/center/index "点击连接")开通直播间（很简单的，实名认证通过后直接就可以开通了）  
（3）.FFmpeg。推流默认使用FFmpeg，树莓派官方系统默认安装了的，我就不再赘述，其它系统请自行安装。  

#### 项目说明
（1）BiliAccount  
Bilibili账号操作Api。  
（2）BilibiliLiveCategoryList  
直播分区获取获取工具，可以通过此工具获取直播分区。  
（3）BilibiliLiver  
一键开启直播工具。  
[![Demo](https://github.com/withsalt/BilibiliLiveTools/blob/master/doc/demo.jpg "Demo")](https://github.com/withsalt/BilibiliLiveTools/blob/master/doc/demo.jpg "Demo")

#### 如何使用（树莓派）
1. 获取程序  
```shell
wget https://github.com/withsalt/BilibiliLiveTools/releases/download/2.0.2/BilibiliLiver_Linux_ARM.zip
```

2. 解压并授权
```shell
unzip BilibiliLiver_Linux_ARM.zip && chmod -R 755 BilibiliLiver_Linux_ARM && chmod +x BilibiliLiver_Linux_ARM/BilibiliLiver
```

3. 编辑配置文件  
编辑用户配置文件appsettings.json  
```shell
cd BilibiliLiver_Linux_ARM/
nano appsettings.json
```

编辑直播配置文件  

```json
{
  "AppSetting": {
    //加密密钥，一般不需要修改，要修改的话，至少需要16位
    "Key": "ac131de1-ed20-499f-8fdf-dede054dbaad"
  },
  "LiveSetting": {
    //直播间分类
    "LiveCategory": "369",
    //直播间名称
    "LiveRoomName": "【24H】小金鱼啦~",
    //FFmpeg推流命令，请自行填写对应操作系统和设备的推流命令，默认为树莓派，且摄像头设备为‘/dev/video0’
    //填写到此处时，请注意将命令中‘"’用‘\’进行转义，将推流的rtmp连接替换为[[URL]]，[[URL]]不需要双引号。
    "FFmpegCmd": "ffmpeg -f dshow -video_size 1280x720 -i video=\"5M USB CAM\" -vcodec libx264 -pix_fmt yuv420p -r 30 -s 1280*720 -g 60 -b:v 5000k -an -preset:v ultrafast -tune:v zerolatency -f flv [[URL]]",
    //ffmpeg异常退出后，是否自动重新启动
    "AutoRestart": true
  },
  "UserSetting": {
    //B站账号
    "Account": "*********",
    //B站密码，放心填写，没有后门
    "Password": "*********"
  }
}
```

由于推流方式不同以及FFmpeg配置的多边性，这里采用直接填写推流命令的方式。建议填写之前先测试推流命令能否正确执行。默认的推流命令设配树莓派官方系统，且摄像头设备为‘/dev/video0’，其它系统可能不适用，需要自己修改。  

推流命令（FFmpegCmd）中的“[[URL]]”，是一个配置符号，将在程序中被替换为获取到的Bilibili推流地址，所以一定要在最终命令中，把测试文件或者地址修改为 “[[URL]]”（URL大写） ，否则程序将抛出错误。推流命令中注意半角双引号需要用符号‘\’来进行转义。  

4. 跑起来  
```shell
sudo ./BilibiliLiver
```

配置系统服务等，可以查看：https://www.quarkbook.com/?p=733

#### 直播分区
开播时需要将ID填写到LiveSetting中的LiveCategory中。请注意正确填写分区ID，不然会有被封的风险。

|  ID | 分类名称  | 分区名称  |
| :------------ | :------------ | :------------ |
 | 80 | 绝地求生 | 网游 | 
 | 86 | 英雄联盟 | 网游 | 
 | 88 | 穿越火线 | 网游 | 
 | 89 | CS:GO | 网游 | 
 | 87 | 守望先锋 | 网游 | 
 | 252 | 逃离塔科夫 | 网游 | 
 | 102 | 最终幻想14 | 网游 | 
 | 329 | VALORANT | 网游 | 
 | 84 | 300英雄 | 网游 | 
 | 91 | 炉石传说 | 网游 | 
 | 92 | DOTA2 | 网游 | 
 | 181 | 魔兽争霸3 | 网游 | 
 | 78 | DNF | 网游 | 
 | 388 | FIFA | 网游 | 
 | 82 | 剑网3 | 网游 | 
 | 83 | 魔兽世界 | 网游 | 
 | 240 | APEX英雄 | 网游 | 
 | 318 | 使命召唤:战区 | 网游 | 
 | 249 | 星际战甲 | 网游 | 
 | 115 | 坦克世界 | 网游 | 
 | 248 | 战舰世界 | 网游 | 
 | 316 | 战争雷霆 | 网游 | 
 | 383 | 战意 | 网游 | 
 | 196 | 无限法则 | 网游 | 
 | 114 | 风暴英雄 | 网游 | 
 | 93 | 星际争霸2 | 网游 | 
 | 239 | 刀塔自走棋 | 网游 | 
 | 164 | 堡垒之夜 | 网游 | 
 | 251 | 枪神纪 | 网游 | 
 | 81 | 三国杀 | 网游 | 
 | 112 | 龙之谷 | 网游 | 
 | 173 | 古剑奇谭OL | 网游 | 
 | 176 | 幻想全明星 | 网游 | 
 | 300 | 封印者 | 网游 | 
 | 288 | 怀旧网游 | 网游 | 
 | 298 | 新游前瞻 | 网游 | 
 | 331 | 星战前夜：晨曦 | 网游 | 
 | 350 | 梦幻西游端游 | 网游 | 
 | 107 | 其他游戏 | 网游 | 
 | 35 | 王者荣耀 | 手游 | 
 | 256 | 和平精英 | 手游 | 
 | 321 | 原神 | 手游 | 
 | 163 | 第五人格 | 手游 | 
 | 395 | LOL手游 | 手游 | 
 | 330 | 公主连结Re:Dive | 手游 | 
 | 292 | 火影忍者 | 手游 | 
 | 255 | 明日方舟 | 手游 | 
 | 418 | 四叶草剧场 | 手游 | 
 | 37 | Fate/GO | 手游 | 
 | 449 | 机动战姬：聚变 | 手游 | 
 | 36 | 阴阳师 | 手游 | 
 | 442 | 坎公骑冠剑 | 手游 | 
 | 448 | 天地劫：幽城再临 | 手游 | 
 | 140 | 决战！平安京 | 手游 | 
 | 293 | 战双帕弥什 | 手游 | 
 | 407 | 游戏王：决斗链接 | 手游 | 
 | 408 | 天谕 | 手游 | 
 | 389 | 天涯明月刀 | 手游 | 
 | 40 | 崩坏3 | 手游 | 
 | 386 | 使命召唤手游 | 手游 | 
 | 41 | 狼人杀 | 手游 | 
 | 411 | 幻书启世录 | 手游 | 
 | 286 | 百闻牌 | 手游 | 
 | 280 | 王者模拟战 | 手游 | 
 | 333 | CF手游 | 手游 | 
 | 154 | QQ飞车 | 手游 | 
 | 113 | 碧蓝航线 | 手游 | 
 | 352 | 三国杀移动版 | 手游 | 
 | 269 | 猫和老鼠 | 手游 | 
 | 354 | 综合棋牌 | 手游 | 
 | 250 | 自走棋手游 | 手游 | 
 | 156 | 影之诗 | 手游 | 
 | 206 | 剑网3指尖江湖 | 手游 | 
 | 343 | DNF手游 | 手游 | 
 | 290 | 双生视界 | 手游 | 
 | 342 | 梦幻西游手游 | 手游 | 
 | 305 | 我的勇者 | 手游 | 
 | 262 | 重装战姬 | 手游 | 
 | 189 | 明日之后 | 手游 | 
 | 50 | 部落冲突:皇室战争 | 手游 | 
 | 39 | 少女前线 | 手游 | 
 | 42 | 解密游戏 | 手游 | 
 | 203 | 忍者必须死3 | 手游 | 
 | 178 | 梦幻模拟战 | 手游 | 
 | 258 | BanG Dream | 手游 | 
 | 212 | 非人学园 | 手游 | 
 | 263 | 辐射：避难所Online | 手游 | 
 | 214 | 雀姬 | 手游 | 
 | 265 | 跑跑卡丁车 | 手游 | 
 | 340 | 黑潮之上 | 手游 | 
 | 274 | 新游评测 | 手游 | 
 | 98 | 其他手游 | 手游 | 
 | 236 | 主机游戏 | 单机游戏 | 
 | 216 | 我的世界 | 单机游戏 | 
 | 283 | 独立游戏 | 单机游戏 | 
 | 412 | 怪物猎人:崛起 | 单机游戏 | 
 | 455 | 尼尔：人工生命 | 单机游戏 | 
 | 276 | 恐怖游戏 | 单机游戏 | 
 | 237 | 怀旧游戏 | 单机游戏 | 
 | 424 | 鬼谷八荒 | 单机游戏 | 
 | 357 | 糖豆人 | 单机游戏 | 
 | 218 | 饥荒 | 单机游戏 | 
 | 217 | 怪物猎人：世界 | 单机游戏 | 
 | 438 | Loop Hero | 单机游戏 | 
 | 313 | 仁王2 | 单机游戏 | 
 | 277 | 命运2 | 单机游戏 | 
 | 221 | 战地5 | 单机游戏 | 
 | 245 | 只狼 | 单机游戏 | 
 | 282 | 使命召唤 | 单机游戏 | 
 | 447 | 霓虹深渊 | 单机游戏 | 
 | 426 | 重生细胞 | 单机游戏 | 
 | 443 | 永劫无间 | 单机游戏 | 
 | 431 | 小小梦魇 | 单机游戏 | 
 | 456 | 炼金工房 | 单机游戏 | 
 | 453 | 斩妖Raksasi | 单机游戏 | 
 | 452 | 异界之上 | 单机游戏 | 
 | 451 | 火焰审判 | 单机游戏 | 
 | 441 | 雨中冒险2 | 单机游戏 | 
 | 432 | 英灵神殿 | 单机游戏 | 
 | 422 | 戴森球计划 | 单机游戏 | 
 | 226 | 荒野大镖客2 | 单机游戏 | 
 | 435 | 节奏医生 | 单机游戏 | 
 | 228 | 精灵宝可梦 | 单机游戏 | 
 | 309 | 植物大战僵尸 | 单机游戏 | 
 | 227 | 刺客信条 | 单机游戏 | 
 | 387 | 恐鬼症 | 单机游戏 | 
 | 270 | 人类一败涂地 | 单机游戏 | 
 | 295 | 方舟 | 单机游戏 | 
 | 396 | Among Us | 单机游戏 | 
 | 433 | 格斗游戏 | 单机游戏 | 
 | 362 | NBA2K | 单机游戏 | 
 | 244 | 鬼泣5 | 单机游戏 | 
 | 308 | 塞尔达 | 单机游戏 | 
 | 243 | 全境封锁2 | 单机游戏 | 
 | 247 | 探灵笔记 | 单机游戏 | 
 | 402 | 赛博朋克2077 | 单机游戏 | 
 | 219 | 以撒 | 单机游戏 | 
 | 427 | 烟火 | 单机游戏 | 
 | 257 | 全面战争 | 单机游戏 | 
 | 326 | 骑马与砍杀 | 单机游戏 | 
 | 364 | 枪火重生 | 单机游戏 | 
 | 302 | FORZA 极限竞速 | 单机游戏 | 
 | 311 | 女神异闻录5 | 单机游戏 | 
 | 230 | 任天堂明星大乱斗 | 单机游戏 | 
 | 341 | 盗贼之海 | 单机游戏 | 
 | 273 | 无主之地3 | 单机游戏 | 
 | 261 | 马里奥制造2 | 单机游戏 | 
 | 319 | 东方大战争 | 单机游戏 | 
 | 220 | 辐射76 | 单机游戏 | 
 | 410 | 封灵档案 | 单机游戏 | 
 | 437 | Everhood | 单机游戏 | 
 | 421 | 归家异途 | 单机游戏 | 
 | 382 | 橙光 | 单机游戏 | 
 | 439 | 恐惧之间 | 单机游戏 | 
 | 436 | 泡沫冬景 | 单机游戏 | 
 | 446 | 双人成行 | 单机游戏 | 
 | 440 | 生化入侵 | 单机游戏 | 
 | 450 | 先驱者 | 单机游戏 | 
 | 454 | 甜蜜之家 | 单机游戏 | 
 | 235 | 其他单机 | 单机游戏 | 
 | 379 | 全面战争:竞技场 | 单机游戏 | 
 | 21 | 视频唱见 | 娱乐 | 
 | 145 | 视频聊天 | 娱乐 | 
 | 207 | 舞见 | 娱乐 | 
 | 123 | 户外 | 娱乐 | 
 | 399 | 日常 | 娱乐 | 
 | 339 | 放松电台 | 电台 | 
 | 190 | 唱见电台 | 电台 | 
 | 192 | 聊天电台 | 电台 | 
 | 193 | 配音 | 电台 | 
 | 371 | 虚拟主播 | 虚拟主播 | 
 | 404 | 赛博朋克2077虚拟区 | 虚拟主播 | 
 | 367 | 美食 | 生活 | 
 | 369 | 萌宠 | 生活 | 
 | 378 | 时尚 | 生活 | 
 | 33 | 影音馆 | 生活 | 
 | 376 | 人文社科 | 学习 | 
 | 375 | 科技科普 | 学习 | 
 | 377 | 职业技能 | 学习 | 
 | 372 | 陪伴学习 | 学习 | 
 | 373 | 绘画 | 学习 | 
 
 ## Stargazers over time
[![Stargazers over time](https://starchart.cc/withsalt/BilibiliLiveTools.svg)](https://starchart.cc/withsalt/BilibiliLiveTools)
