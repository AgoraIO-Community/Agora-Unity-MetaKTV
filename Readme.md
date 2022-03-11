## AgoraMetaKTV

![image-20220215153402229](https://download.agora.io/demo/release/image-20220215153402229.png)

# 项目介绍

Agora MetaKTV 解决方案整合 **3D**场景+ Avatar形象 **+** **空间音频+K歌功能+**正版MV**能力**等技术，打造更具有沉浸感的 K 歌社交环境，在线High翻天



# 项目目录

MetaKTV是基于Unity开发的项目，项目目录包含一下文件夹：

- **Agora-Rtc-Unity-SDK** : Agora Unity SDK 插件
- **Mirror** : 轻量级多人游戏同步组件
- **Prefebs**：预制体目录
  - AgoraJoyStick.prefeb：行动控制滚轮
  - KTVGirl_.prefeb：人物预制体
  - MusicItem.prefeb：歌曲UI
  - MusicScroll.prefeb：点歌台
- **Scenes**：项目场景目录
  - OfflineScene：离线场景
  - PBRStageEquipmentDemo：线上场景
- **Scripts**：项目业务逻辑代码目录
  - Component：可挂载在GameObject上的组件脚本
  - Controller：AgoraUnitySDKController
  - HttpRequest：网络请求模块
  - Interface：回调模块
  - Model
  - Utils：工具类
- **UI**：项目模型、UI资源目录



# 集成步骤

1、git clone本仓库

2、下载[AgoraUnitySDK](https://download.agora.io/sdk/release/Agora-Unity-RTC-SDK_3.6.208_video_20220114.zip)，将下载的Agora-Rtc-Unity-SDK文件夹拷贝进项目的Assets目录下；

3、打开项目

4、下载[模型资源](https://download.agora.io/demo/test/AgoraMetaKTV_v1.5_UIModel.zip)，将下载的unitypackage import进项目；

5、在Scripts目录下，GameApplication文件内填入AppId、CustomerKey、CustomerSecret；

6、将Scenes目录下场景加入Build Settings/Scenes in build目录下；

7、点击OfflineScene场景并运行游戏；