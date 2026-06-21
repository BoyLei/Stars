using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AssetChecker.Interface
{
    public interface IAssetChecker
    {
        void DoAssetCheck(string template, string webroot, string[] tempAssetPaths);
    }
}