
#if UNITY_IOS 
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;


namespace GSSDKEditor
{
    public class GSXcodeBuild
    {

        [PostProcessBuild(1)]
        internal static void OnPostProcessBuild(BuildTarget target, string path)
        {
            if (target != BuildTarget.iOS)
            {
                Debug.Log("BuildTarget is not iOS.");
                return;
            }

            Debug.Log("On GSSDK post process Xcode build start.");

            AssetDatabase.Refresh();

            // Debug.Log("Run in Xcode as : " + EditorUserBuildSettings.iOSXcodeBuildConfig);
            Debug.Log("Development Build : " + Debug.isDebugBuild);

            GSPBXProject pbxproject = new GSPBXProject(path);
            GSInfoPlist infoPlist = new GSInfoPlist(path);

            infoPlist.SetArbitraryLoads(true);
            infoPlist.RemoveRootKey("UIApplicationExitsOnSuspend");
            infoPlist.SetRootString("NSUserTrackingUsageDescription", "This will help us provide better service and find more players for you.");
            infoPlist.WriteToInfoPlist();
            Dictionary<string, string> mainProperties = new Dictionary<string, string>()
            {
                { "OTHER_LDFLAGS", "-ObjC" }

            };
            pbxproject.UpdateMainBuildProperties(mainProperties);
#if UNITY_2019_1_OR_NEWER
            pbxproject.UpdateFrameworkBuildProperties(mainProperties);
#endif

#if UNITY_2019_1_OR_NEWER
            Dictionary<string, string> frameworkProperties = new Dictionary<string, string>()
            {
                { "LD_RUNPATH_SEARCH_PATHS", "/usr/lib/swift" },
                { "LIBRARY_SEARCH_PATHS", "$(SDKROOT)/usr/lib/swift $(TOOLCHAIN_DIR)/usr/lib/swift/$(PLATFORM_NAME)"}
            };
#else
            Dictionary<string, string> frameworkProperties = new Dictionary<string, string>()
            {
                { "LD_RUNPATH_SEARCH_PATHS", "/usr/lib/swift $(inherited) @executable_path/Frameworks @loader_path/Frameworks" },
                { "LIBRARY_SEARCH_PATHS", "$(SDKROOT)/usr/lib/swift $(TOOLCHAIN_DIR)/usr/lib/swift/$(PLATFORM_NAME)"}
            };
#endif
            pbxproject.UpdateFrameworkBuildProperties(frameworkProperties);

            // List<string> removeFrameworks = new List<string>
            // {
            //     "CoreLocation.framework"
            // };
            // pbxproject.RemoveFrameworksFromProject(removeFrameworks);
            // Dictionary<string, bool> coreFrameworks = new Dictionary<string, bool>()
            // {
            //     { "Accelerate.framework", false },
            //     { "AdServices.framework", false },
            //     { "AdSupport.framework", false },
            //     { "CoreFoundation.framework", false },
            //     { "Foundation.framework", false },
            //     { "Security.framework", false },
            //     { "SystemConfiguration.framework", false },
            //     { "UIKit.framework", false },
            //     { "AppTrackingTransparency.framework", false }

            // List<string> frameworks = GSConfigManager.GetFilesRecursive();

            // };
            // pbxproject.AddFrameworksToProject(coreFrameworks);

            // List<string> files = GSConfigManager.GetFilesRecursive();

            Dictionary<string, string> copyFiles = new Dictionary<string, string>()
                {
                // { "GSSDK/Channels/Resources/iOS/sdkconfig.json", "sdkconfig.json" },
                // { "GSSDK/Channels/Resources/iOS/GSPResources.bundle", "GSPResources.bundle" }
                { "Plugins/iOS/GSSDK/channel_yoka2/res/YokaResources.bundle", "YokaResources.bundle" },
                { "Plugins/iOS/GSSDK/core_ios/res/GSSDK.bundle", "GSSDK.bundle" }
                };



            pbxproject.CopySourceFilesToProject(copyFiles);
            // pbxproject.CopySourceFilesToProject();
            // Debug.Log("ritchie======🙈" + copyFiles);

            // List<string> addFiles = new List<string>
            // {
            // //    "Frameworks/GSSDK/Channels/Resources/iOS/GSPResources.bundle"
            // "Plugins/iOS/sdkconfig.json"
            // };
            // pbxproject.AddSourceFilesToProject(addFiles);

            // pbxproject.AddEntitlementsToProject("");
            // pbxproject.AddCapability(PBXCapabilityType.PushNotifications);

            // infoPlist.SetBackgroundMode("remote-notification");
            // infoPlist.SetBackgroundMode("fetch");
            // infoPlist.WriteToInfoPlist();
        }

    }
}
#endif