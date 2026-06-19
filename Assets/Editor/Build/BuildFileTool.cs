
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using Debug = UnityEngine.Debug;


public class BuildFileTool
{
    public static bool CheckFolder(string path)
    {
        if (Directory.Exists(path))
        {
            return true;
        }
        UnityEditor.EditorUtility.DisplayDialog("Error", "Path does not exist \n\t" + path, "х╥хо");
        return false;
    }
    public static void OpenFolder(string path)
    {
        if (CheckFolder(path))
        {
            System.Diagnostics.Process.Start(path);
        }

    }

    public static void CopyFolder(Dictionary<string, string> copyDic)
    {
        foreach (KeyValuePair<string, string> path in copyDic)
        {

            if (CheckFolder(path.Key))
            {

                CopyDir(path.Key, path.Value);
                Debug.Log("Copy Success : \n\tFrom:" + path.Key + " \n\tTo:" + path.Value);
            }
        }
        EditorUtility.ClearProgressBar();
    }

    public static void CopyFolder(string fromPath, string toPath)
    {
        CopyDir(fromPath, toPath);
        Debug.Log("Copy Success : \n\tFrom:" + fromPath + " \n\tTo:" + toPath);
        EditorUtility.ClearProgressBar();
    }

    public static void CreateFolder(string path)
    {
        if (Directory.Exists(path))
        {
            Directory.Delete(path, true);
        }
        Directory.CreateDirectory(path);
    }

    public static void DeleteFolder(string path)
    {
        if (Directory.Exists(path))
        {
            Directory.Delete(path, true);
        }
    }

    private static void CopyDir(string origin, string target)
    {
#if UNITY_IOS
      
        if (!origin.EndsWith("/"))
        {
            origin += "/";
        }
 
        if (!target.EndsWith("/"))
        {
            target += "/";
        }
#else
        if (!origin.EndsWith("\\"))
        {
            origin += "\\";
        }

        if (!target.EndsWith("\\"))
        {
            target += "\\";
        }

#endif
        if (!Directory.Exists(target))
        {
            Directory.CreateDirectory(target);
        }

        DirectoryInfo info = new DirectoryInfo(origin);
        FileInfo[] fileList = info.GetFiles();
        DirectoryInfo[] dirList = info.GetDirectories();
        float index = 0;
        foreach (FileInfo fi in fileList)
        {


            if (fi.Extension == ".zip" || fi.Extension == ".meta" || fi.Extension == ".rar")
            {
                Debug.Log("dont copy :" + fi.FullName);
                continue;
            }
            float progress = (index / (float)fileList.Length);
            EditorUtility.DisplayProgressBar("Copy ", "Copying: " + Path.GetFileName(fi.FullName), progress);
            File.Copy(fi.FullName, target + fi.Name, true);
            index++;
        }

        foreach (DirectoryInfo di in dirList)
        {
            if (di.FullName.Contains(".svn"))
            {
                Debug.Log("Continue SVN " + di.FullName);
                continue;
            }

            CopyDir(di.FullName, target + "\\" + di.Name);
        }
    }



}