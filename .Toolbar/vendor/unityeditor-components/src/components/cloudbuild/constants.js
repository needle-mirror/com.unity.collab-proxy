angular.module('ut.cloudbuild.constants', []).constant("buildConstants", (function() {
    var constants = {
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
                "icon": "unityicon unityicon-os-ios",
                "editor": "iOS"
            },
            {
                "name": "Android",
                "value": "android",
                "icon": "unityicon unityicon-os-android",
                "editor": "Android"
            },
            {
                "name": "Web Player",
                "value": "webplayer",
                "icon": "unityicon unityicon-os-webplayer",
                "editor": "WebPlayer"
            },
            {
                "name": "WebGL",
                "value": "webgl",
                "icon": "unityicon unityicon-os-webgl",
                "editor": "WebGL"
            },
            {
                "name": "Mac desktop 32-bit",
                "value": "standaloneosxintel",
                "icon": "unityicon unityicon-os-osx-32",
                "editor": "StandaloneOSXIntel"
            },
            {
                "name": "Mac desktop 64-bit",
                "value": "standaloneosxintel64",
                "icon": "unityicon unityicon-os-osx-64",
                "editor": "StandaloneOSXIntel64"
            },
            {
                "name": "Mac desktop Universal",
                "value": "standaloneosxuniversal",
                "icon": "unityicon unityicon-os-osx-U",
                "editor": "StandaloneOSXUniversal"
            },
            {
                "name": "Windows desktop 32-bit",
                "value": "standalonewindows",
                "icon": "unityicon unityicon-os-win-32",
                "editor": "StandaloneWindows"

            },
            {
                "name": "Windows desktop 64-bit",
                "value": "standalonewindows64",
                "icon": "unityicon unityicon-os-win-64",
                "editor": "StandaloneWindows64"
            },
            {
                "name": "Linux desktop 32-bit",
                "value": "standalonelinux",
                "icon": "unityicon unityicon-os-linux-32",
                "editor": "StandaloneLinux"
            },
            {
                "name": "Linux desktop 64-bit",
                "value": "standalonelinux64",
                "icon": "unityicon unityicon-os-linux-64",
                "editor": "StandaloneLinux64"
            },
            {
                "name": "Linux desktop Universal",
                "value": "standalonelinuxuniversal",
                "icon": "unityicon unityicon-os-linux-U",
                "editor": "StandaloneLinuxUniversal"
            }
        ],
        cachingStrategies: [
            { "name": "Off", "value": "none" },
            { "name": "Cache Library Directory", "value": "library" },
            { "name": "Cache Entire Project", "value": "workspace"  }
        ],
        status: [
            { "name": "Queued", "value": "queued", "class": "queued", "icon": "unityicon-pending"},
            { "name": "Queued", "value": "sendToBuilder", "class": "queued", "icon": "unityicon-pending"},
            { "name": "Building", "value": "started", "class": "building", "icon": "logoicon logoloader"},
            { "name": "Succeeded", "value": "success", "class": "success", "icon": "unityicon-check-circle"},
            { "name": "Failed", "value": "failure", "class": "failed", "icon": "unityicon-delete-circle"},
            { "name": "Canceled", "value": "canceled", "class": "canceled", "icon": "unityicon-ban-circle"},
            { "name": "Restarted", "value": "restarted", "class": "canceled", "icon": "unityicon-warning-circle"},
            { "name": "Unknown", "value": "unknown", "class": "failed", "icon": "unityicon-warning-circle"}
        ],
        platformName: function(platform) {
            var foundPlatform = _.find(constants.platforms, {value: platform});
            if (foundPlatform) {
                return foundPlatform.name;
            }
            else {
                return "Unknown Platform";
            }
        },
        platformFromEditorName: function(editorName) {
            var foundPlatform = _.find(constants.platforms, {editor: editorName});
            if (foundPlatform) {
                return foundPlatform.value;
            }
            else {
                return "";
            }
        }
    };
    return constants;
  })()
);