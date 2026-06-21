#if UNITY_IOS 
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.iOS.Xcode;

namespace GSSDKEditor
{
    internal class GSInfoPlist
    {
        internal string InfoPlistPath;
        internal PlistDocument InfoPlist;

        internal GSInfoPlist(string buildPath)
        {
            InfoPlistPath = Path.Combine(buildPath, "Info.plist");

            if (!File.Exists(InfoPlistPath))
            {
                throw new FileNotFoundException("Info.plist does not exist at " + InfoPlistPath);
            }

            InfoPlist = new PlistDocument();
            InfoPlist.ReadFromFile(InfoPlistPath);
        }

        internal void WriteToInfoPlist()
        {
            InfoPlist.WriteToFile(InfoPlistPath);
        }

        internal void SetRootString(string key, string value)
        {
            if (!InfoPlist.root.values.ContainsKey(key))
            {
                InfoPlist.root.SetString(key, value);
            }
        }

        internal void UpdateRootString(string key, string value)
        {
            InfoPlist.root.SetString(key, value);
        }


        internal void SetRootBoolean(string key, bool value)
        {
            if (!InfoPlist.root.values.ContainsKey(key))
            {
                InfoPlist.root.SetBoolean(key, value);
            }
        }

        internal void UpdateRootBoolean(string key, bool value)
        {
            InfoPlist.root.SetBoolean(key, value);
        }

        internal void RemoveRootKey(string key)
        {
            if (InfoPlist.root.values.ContainsKey(key))
            {
                InfoPlist.root.values.Remove(key);
            }
        }

        internal PlistElementArray GetURLTypes()
        {
            if (!InfoPlist.root.values.ContainsKey("CFBundleURLTypes"))
            {
                return InfoPlist.root.CreateArray("CFBundleURLTypes");
            }
            return InfoPlist.root["CFBundleURLTypes"].AsArray();
        }

        internal void SetURLScheme(string scheme)
        {
            PlistElementArray schemes = GetURLTypes().AddDict().CreateArray("CFBundleURLSchemes");
            schemes.AddString(scheme);
        }

        internal PlistElementArray GetQueriesSchemes()
        {
            if (!InfoPlist.root.values.ContainsKey("LSApplicationQueriesSchemes"))
            {
                return InfoPlist.root.CreateArray("LSApplicationQueriesSchemes");
            }
            return InfoPlist.root["LSApplicationQueriesSchemes"].AsArray();
        }

        internal void SetQueriesSchemes(List<string> schemes)
        {
            PlistElementArray elements = GetQueriesSchemes();

            foreach (string scheme in schemes)
            {
                bool hasAdd = false;

                foreach (PlistElementString element in elements.values)
                {
                    if (element.value.Equals(scheme))
                    {
                        hasAdd = true;
                    }
                }

                if (!hasAdd)
                {
                    elements.AddString(scheme);
                }
            }
        }

        internal PlistElementArray GetBackgroundModes()
        {
            if (!InfoPlist.root.values.ContainsKey("UIBackgroundModes"))
            {
                return InfoPlist.root.CreateArray("UIBackgroundModes");
            }
            return InfoPlist.root["UIBackgroundModes"].AsArray();
        }

        internal void SetBackgroundMode(string value)
        {
            PlistElementArray elements = GetBackgroundModes();

            bool hasAdd = false;

            foreach (PlistElementString element in elements.values)
            {
                if (element.value.Equals(value))
                {
                    hasAdd = true;
                }
            }

            if (!hasAdd)
            {
                elements.AddString(value);
            }
        }

        internal PlistElementArray GetDeviceCapabilities()
        {
            if (!InfoPlist.root.values.ContainsKey("UIRequiredDeviceCapabilities"))
            {
                return InfoPlist.root.CreateArray("UIRequiredDeviceCapabilities");
            }
            return InfoPlist.root["UIRequiredDeviceCapabilities"].AsArray();
        }

        internal void SetDeviceCapability(string value)
        {
            PlistElementArray elements = GetDeviceCapabilities();

            bool hasAdd = false;

            foreach (PlistElementString element in elements.values)
            {
                if (element.value.Equals(value))
                {
                    hasAdd = true;
                }
            }

            if (!hasAdd)
            {
                elements.AddString(value);
            }
        }

        internal PlistElementDict GetTransportSecurity()
        {
            if (!InfoPlist.root.values.ContainsKey("NSAppTransportSecurity"))
            {
                return InfoPlist.root.CreateDict("NSAppTransportSecurity");
            }
            return InfoPlist.root["NSAppTransportSecurity"].AsDict();
        }

        internal void SetArbitraryLoads(bool value)
        {
            PlistElementDict element = GetTransportSecurity();

            element.SetBoolean("NSAllowsArbitraryLoads", value);
        }


    }
}
#endif
