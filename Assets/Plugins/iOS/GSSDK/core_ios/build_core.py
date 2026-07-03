# coding=utf-8


def init(config, fun=None):
    InfoPlistAddListList = {
        "NSAppTransportSecurity":{
            "NSAllowsArbitraryLoads":True
        }
    }

    # 添加系统库
    SystemLibraryAddList = {
        "Optional":[
            "AdSupport.framework",
            "Network.framework"
        ],
        "Required":
        [
            "CoreTelephony.framework",
            "Foundation.framework",
            "SystemConfiguration.framework",
            "UIKit.framework",
            "CoreGraphics.framework",
            "AppTrackingTransparency.framework",
            "WebKit.framework",
            "Security.framework",
            "libc++.tbd"
        ]
    }

    TargetAttributeAddList = {
        "ENABLE_BITCODE": "NO",
        "OTHER_LDFLAGS": ["-ObjC"]
    }

    EntitlementsAddPlist = {
    }

    return InfoPlistAddListList, SystemLibraryAddList, TargetAttributeAddList, EntitlementsAddPlist
