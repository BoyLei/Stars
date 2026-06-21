#if UNITY_IOS
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;


namespace GSSDKEditor
{
    internal class GSAppleXcodeBuild
    {

        [PostProcessBuild(2)]
        internal static void OnPostProcessBuild(BuildTarget target, string path)
        {
            if (target != BuildTarget.iOS)
            {
                return;
            }

            // bool.TryParse(GSConfigManager.GetValue("apple", "enable").ToString(), out bool enable);

            // if (!enable)
            // {
            //     Debug.Log("Skip Apple post process Xcode build, because it is disable.");
            //     return;
            // }

            Debug.Log("On Apple post process Xcode build.");

            GSPBXProject pbxproject = new GSPBXProject(path);

            // pbxproject.AddEntitlementsToProject("");
            // pbxproject.AddCapability(PBXCapabilityType.SignInWithApple);

            Dictionary<string, bool> appleFrameworks = new Dictionary<string, bool>
            {
                { "AuthenticationServices.framework", true }
            };
            pbxproject.AddFrameworksToProject(appleFrameworks);
        }
    }
}
#endif