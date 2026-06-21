#if UNITY_IOS
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.iOS.Xcode;

namespace GSSDKEditor
{
    public class GSPBXProject
    {

        internal string BuildPath;

        internal string BuildPBXProjectPath;
        internal PBXProject BuildPBXProject;

        internal string MainGuid;
        internal string FrameworkGuid;

        internal GSEntitlements Entitlements;

        internal GSPBXProject(string buildPath)
        {
            BuildPath = buildPath;

            BuildPBXProjectPath = PBXProject.GetPBXProjectPath(buildPath);
            BuildPBXProject = new PBXProject();
            BuildPBXProject.ReadFromFile(BuildPBXProjectPath);

            if (BuildPBXProject == null)
            {
                throw new FileNotFoundException("*.pbxproject does not exist at " + BuildPBXProjectPath);
            }
#if UNITY_2019_1_OR_NEWER
            MainGuid = BuildPBXProject.GetUnityMainTargetGuid();
            FrameworkGuid = BuildPBXProject.GetUnityFrameworkTargetGuid();
#else
            MainGuid = BuildPBXProject.TargetGuidByName("Unity-iPhone");
            FrameworkGuid = BuildPBXProject.TargetGuidByName("Unity-iPhone");
#endif
            Debug.Log("Read Unity-iPhone guid : " + MainGuid + " UnityFramework guid : " + FrameworkGuid + " in : " + BuildPBXProjectPath);
        }


        internal void WriteToPBXProject()
        {
            BuildPBXProject.WriteToFile(BuildPBXProjectPath);
        }

        internal void UpdateMainBuildProperties(Dictionary<string, string> properties)
        {
            foreach (KeyValuePair<string, string> property in properties)
            {
                BuildPBXProject.UpdateBuildProperty(MainGuid, property.Key, property.Value.Split(' '), null);
            }
            WriteToPBXProject();
        }

        internal void UpdateFrameworkBuildProperties(Dictionary<string, string> properties)
        {
            foreach (KeyValuePair<string, string> property in properties)
            {

                BuildPBXProject.UpdateBuildProperty(FrameworkGuid, property.Key, property.Value.Split(' '), null);
            }
            WriteToPBXProject();
        }

        internal void AddFrameworksToProject(Dictionary<string, bool> frameworks)
        {
            foreach (KeyValuePair<string, bool> framework in frameworks)
            {
                BuildPBXProject.AddFrameworkToProject(FrameworkGuid, framework.Key, framework.Value);
            }
            WriteToPBXProject();
        }

        internal void RemoveFrameworksFromProject(List<string> frameworks)
        {
            foreach (string framework in frameworks)
            {
                BuildPBXProject.RemoveFrameworkFromProject(FrameworkGuid, framework);
            }
            WriteToPBXProject();
        }

        internal void AddTBDsToProject(List<string> tbds)
        {
            foreach (string tbd in tbds)
            {
                string path = Path.Combine("usr/lib", tbd);
                string projectPath = Path.Combine("Frameworks", tbd);
                string tbdGuid = BuildPBXProject.AddFile(path, projectPath, PBXSourceTree.Sdk);
                BuildPBXProject.AddFileToBuild(FrameworkGuid, tbdGuid);
            }
            WriteToPBXProject();
        }

        internal void CopySourceFilesToProject(Dictionary<string, string> files)
        {
            foreach (KeyValuePair<string, string> file in files)
            {
                string fileUnityProjectPath = Path.Combine(Application.dataPath, file.Key);
                string fileXcodeProjectPath = Path.Combine(BuildPath, file.Value);
                if (!File.Exists(fileUnityProjectPath) && !Directory.Exists(fileUnityProjectPath))
                {
                    Debug.Log(fileUnityProjectPath + " not found.");
                    continue;
                }

                DirectoryInfo fileXcodeProjectDirectory = Directory.GetParent(fileXcodeProjectPath);
                if (!Directory.Exists(fileXcodeProjectDirectory.ToString()))
                {
                    Directory.CreateDirectory(fileXcodeProjectDirectory.ToString());
                }
                FileUtil.ReplaceFile(fileUnityProjectPath, fileXcodeProjectPath);

                string fileGuid = BuildPBXProject.AddFile(fileXcodeProjectPath, file.Value, PBXSourceTree.Source);
                BuildPBXProject.AddFileToBuild(MainGuid, fileGuid);

            }

            WriteToPBXProject();
            File.WriteAllText(BuildPBXProjectPath, BuildPBXProject.WriteToString());
        }

        internal void AddSourceFilesToProject(List<string> files)
        {

            foreach (string file in files)
            {
                string fileXcodeProjectPath = Path.Combine(BuildPath, file);
                if (!File.Exists(fileXcodeProjectPath) && !Directory.Exists(fileXcodeProjectPath))
                {
                    Debug.Log(fileXcodeProjectPath + " not found.");
                    break;
                }

                string fileGuid = BuildPBXProject.AddFile(fileXcodeProjectPath, file, PBXSourceTree.Source);
                BuildPBXProject.AddFileToBuild(MainGuid, fileGuid);
            }

            WriteToPBXProject();
            File.WriteAllText(BuildPBXProjectPath, BuildPBXProject.WriteToString());
        }

        internal void AddEntitlementsToProject(string path)
        {
            string filePath = Path.Combine(BuildPath, path);
            Entitlements = new GSEntitlements(filePath);

            string file = Path.Combine(path, Entitlements.EntitlementsName);
            string fileGuid = BuildPBXProject.AddFile(Entitlements.EntitlementsPath, file, PBXSourceTree.Source);
            BuildPBXProject.AddFileToBuild(MainGuid, fileGuid);

            WriteToPBXProject();
            File.WriteAllText(BuildPBXProjectPath, BuildPBXProject.WriteToString());
        }

        // internal void AddCapability(PBXCapabilityType type)
        // {
        //     BuildPBXProject.AddCapability(MainGuid, type, Entitlements.EntitlementsPath, true);
        //     WriteToPBXProject();
        //     File.WriteAllText(BuildPBXProjectPath, BuildPBXProject.WriteToString());

        //     if (type.Equals(PBXCapabilityType.SignInWithApple))
        //     {
        //         Entitlements.SetAppleSignIn("Default");
        //         Entitlements.WriteToEntitlements();
        //     }
        //     else if (type.Equals(PBXCapabilityType.PushNotifications))
        //     {
        //         if (EditorUserBuildSettings.iOSXcodeBuildConfig == XcodeBuildConfig.Debug)
        //         {
        //             Entitlements.UpdateRootString("aps-environment", "development");
        //         }
        //         else
        //         {
        //             Entitlements.UpdateRootString("aps-environment", "production");
        //         }
        //         Entitlements.WriteToEntitlements();
        //     }
        // }

    }
}
#endif