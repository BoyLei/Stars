using System;
using System.IO;
using System.Text;
using UnityEditor;

public static class ToUTF8
{
    /// <summary>
    /// 把.cs转成UTF-8格式
    /// </summary>
    [MenuItem("自动化工具/ToUTF8/Convert2UTF8", false, 3)]

    public static void Convert2UTF8()
    {
        var dir = "Assets/Scripts/";
        foreach (var f in new DirectoryInfo(dir).GetFiles("*.cs", SearchOption.AllDirectories))
        {
            var s = File.ReadAllText(f.FullName, Encoding.Default);
            try
            {
                File.WriteAllText(f.FullName, s, Encoding.UTF8);
            }
            catch (Exception)
            {
                continue;
            }
        }
    }
}