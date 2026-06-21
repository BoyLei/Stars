
#if UNITY_IOS 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor.iOS.Xcode;


namespace GSSDKEditor
{

    internal class GSEntitlements
    {
        internal readonly string EntitlementsName = "GSSDK.entitlements";
        internal string EntitlementsPath;
        internal PlistDocument Entitlements;

        internal GSEntitlements(string path)
        {
            EntitlementsPath = Path.Combine(path, EntitlementsName);

            DirectoryInfo entitlementsDirectory = Directory.GetParent(EntitlementsPath);
            if (!Directory.Exists(entitlementsDirectory.ToString()))
            {
                Directory.CreateDirectory(entitlementsDirectory.ToString());
            }

            Entitlements = new PlistDocument();

            if (File.Exists(EntitlementsPath))
            {
                Entitlements.ReadFromFile(EntitlementsPath);
            }
            else
            {
                Entitlements.Create();
                WriteToEntitlements();
            }
        }


        internal void WriteToEntitlements()
        {
            Entitlements.WriteToFile(EntitlementsPath);
        }

        internal void SetRootString(string key, string value)
        {
            if (!Entitlements.root.values.ContainsKey(key))
            {
                Entitlements.root.SetString(key, value);
            }
        }


        internal void UpdateRootString(string key, string value)
        {
            Entitlements.root.SetString(key, value);
        }

        internal PlistElementArray GetAppleSignIn()
        {
            if (!Entitlements.root.values.ContainsKey("com.apple.developer.applesignin"))
            {
                return Entitlements.root.CreateArray("com.apple.developer.applesignin");
            }
            return Entitlements.root["com.apple.developer.applesignin"].AsArray();
        }

        internal void SetAppleSignIn(string value)
        {
            PlistElementArray elements = GetAppleSignIn();

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

    }
}
#endif