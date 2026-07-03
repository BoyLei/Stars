# coding=utf-8


def init(config, fun=None):

    qqAppId = ""
    qqAppLinks = ""
    wechatAppId = ""
    wechatUniversalLink = ""

    qqAppId = config["yoka_qq_appid"]
    qqAppLinks = config["yoka_qq_universal_link"]
    wechatAppId = config["yoka_wechat_appid"]
    wechatUniversalLink = config["yoka_weixin_universal_link"]


    InfoPlistAddListList = {
        "NSCameraUsageDescription":"为了上传自定义照片，你必须允许游戏访问你手机的相册和相机。",
        "NSAppTransportSecurity":{
            "NSAllowsArbitraryLoads":True
        },
        "CFBundleURLTypes":[
            {
                'CFBundleURLName': '',
                'CFBundleURLSchemes': ['tencent'+qqAppId],
                'CFBundleTypeRole': 'Editor',
                'CFBundleURLIconFile': ''
            },
            {
                'CFBundleURLName': '',
                'CFBundleURLSchemes': [wechatAppId],
                'CFBundleTypeRole': 'Editor',
                'CFBundleURLIconFile': ''
            }
        ],
        "LSApplicationQueriesSchemes":[
            "mqq",
            "mqqapi",
            "mqqopensdknopasteboard",
            "mqqopensdknopasteboardios16",
            "mqqOpensdkSSoLogin",
            "mqqopensdkapiV2",
            "mqqopensdkapiV4",
            "tencentapi.qq.reqContent",
            "tencentapi.qzone.reqContent",
            "weixin",
            "weixinULAPI",
            "weixinURLParamsAPI"
        ]
    }

    # 添加系统库
    SystemLibraryAddList = {
        "Optional":[
            "SwiftUI.framework"
        ],
        "Required":
        [
            "Foundation.framework",
            "SystemConfiguration.framework",
            "Security.framework",
            "CoreGraphics.framework",
            "WebKit.framework",
            "CommonCrypto.framework",
            "AdSupport.framework"
        ]
    }

    TargetAttributeAddList = {
        "ENABLE_BITCODE": "NO",
        "OTHER_LDFLAGS": ["-fobjc-arc","-ObjC","-all_load"],
        "LD_RUNPATH_SEARCH_PATHS": ["/usr/lib/swift"],
        "LIBRARY_SEARCH_PATHS": ["/usr/lib/swift", "$(TOOLCHAIN_DIR)/usr/lib/swift-5.0/$(PLATFORM_NAME)", "$(TOOLCHAIN_DIR)/usr/lib/swift/$(PLATFORM_NAME)"]
    }

    EntitlementsAddPlist = {
        "com.apple.developer.applesignin": [
            "Default"
        ],
        # # domain 修改
        "com.apple.developer.associated-domains":[
            'applinks:' + qqAppLinks,
            'applinks:' + wechatUniversalLink
        ],
    }

    return InfoPlistAddListList, SystemLibraryAddList, TargetAttributeAddList, EntitlementsAddPlist
