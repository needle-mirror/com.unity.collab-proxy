angular.module('ut.cloudbuild.setup').constant("setupConstants", {
    platforms : [
        {
            "name": "Please choose a platform target",
            "value": "",
            "icon": "",
            "hidden": true
        },
        {
            "name": "iOS",
            "value": "ios",
            "icon": "unityicon unityicon-os-ios"
        },
        {
            "name": "Android",
            "value": "android",
            "icon": "unityicon unityicon-os-android"
        },
        {
            "name": "Web Player",
            "value": "webplayer",
            "icon": "unityicon unityicon-os-webplayer",
            "version": {
                "max": "5.3.99"
            }
        },
        {
            "name": "WebGL",
            "value": "webgl",
            "icon": "unityicon unityicon-os-webgl"
        },
        {
            "name": "Mac desktop 32-bit",
            "value": "standaloneosxintel",
            "icon": "unityicon unityicon-os-osx-32"
        },
        {
            "name": "Mac desktop 64-bit",
            "value": "standaloneosxintel64",
            "icon": "unityicon unityicon-os-osx-64"
        },
        {
            "name": "Mac desktop Universal",
            "value": "standaloneosxuniversal",
            "icon": "unityicon unityicon-os-osx-U"
        },
        {
            "name": "Windows desktop 32-bit",
            "value": "standalonewindows",
            "icon": "unityicon unityicon-os-win-32"
        },
        {
            "name": "Windows desktop 64-bit",
            "value": "standalonewindows64",
            "icon": "unityicon unityicon-os-win-64"
        },
        {
            "name": "Linux desktop 32-bit",
            "value": "standalonelinux",
            "icon": "unityicon unityicon-os-linux-32"
        },
        {
            "name": "Linux desktop 64-bit",
            "value": "standalonelinux64",
            "icon": "unityicon unityicon-os-linux-64"
        },
        {
            "name": "Linux desktop Universal",
            "value": "standalonelinuxuniversal",
            "icon": "unityicon unityicon-os-linux-U"
        }
    ],
    cachingStrategies: [
        { "name": "Off", "value": "none" },
        { "name": "Cache Library Directory", "value": "library" },
        { "name": "Cache Entire Project", "value": "workspace"  }
    ]
});