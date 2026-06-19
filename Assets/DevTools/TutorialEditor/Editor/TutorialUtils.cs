using System.Collections;
using System.Collections.Generic;
using System.Text;
using SGF.UI.Framework;
using UnityEngine;
using UnityEditor;
using UnityEditor.Rendering.LookDev;
using UnityEngine.SceneManagement;

public class TutorialUtils : Editor
{
    [MenuItem("GameObject/输出UI路径", false, 0)]
    static public void CreateEnum(MenuCommand menuCommand)
    {
        if (Selection.activeObject != null)
        {
            string path = string.Empty;
            SceneManager.GetActiveScene().GetRootGameObjects();
            var Current = Selection.activeObject as GameObject;
            if (Current != null)
            {
                CalPath(Current.transform, ref path);
                path = reverseCharArrays(path);
                Debug.LogError(path);
                UnityEditor.EditorGUIUtility.systemCopyBuffer = path;
            }
        }
    }

    private static void CalPath(Transform go, ref string path)
    {
        if (go != null)
        {
            path += go.name + "/";
            CalPath(go.parent, ref path);
        }
    }

    static private string reverseCharArrays(string s)
    {
        if (!string.IsNullOrEmpty(s))
        {
            StringBuilder builder = new StringBuilder();
            string[] array = s.Split('/');
            if (array != null && array.Length > 0)
            {
                string path = string.Empty;
                for (int i = array.Length - 1; i > -1; i--)
                {
                    if (!string.IsNullOrEmpty(array[i]))
                    {
                        path += ("/" + array[i]);
                    }
                }

                path = path.Remove(0, 1);
                return path;
            }
        }

        return s;
    }
}