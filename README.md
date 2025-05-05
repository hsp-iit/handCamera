# handCamera
Simple minimal application to stream Meta Quest 3 RGB camera and hand/poses.

![Demo](demo.gif)


Huge thanks to [this](https://www.youtube.com/watch?v=A2ZhJt-SIBU&t=324s&ab_channel=Skarredghost) video.

### Installation
If you want to use the application without modification, follow the "Pre-Built Binary" instructions, if you want to modify the app follow the "From Source" instructions.

##### Pre-built binary
- Download [this](https://drive.google.com/file/d/1MnEN5Sz8CtnwWmAh0oMJ_Cny9pJt1Jli/view?usp=sharing) apk
- Install it on the Meta Quest 3 with SideLink

##### From source
- Clone this repository
- This app was built with Unity 6000.0.38f1, consider using the same version to avoid further problems
- From UnityHub, Add -> Add project from disk
- Go to File -> Build Profiles -> Android -> Platform Settings -> Run Device -> Oculus Quest 3

### Launch
- With SideQuest, you need to put a file named metaCub_IP.txt containing only the IP address of the server under sdcard/Android/data/com.HSPHumanoidSensingandPerception.handCamera/files/ (at the same level of il2cpp)
- Launch server.py script (opencv is the only dependency)
- Launch the handCamera app