///--------------------------------------------------------------------
/// 文件名   :   PathInspector
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/07/11 09:53:59
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Sirenix.OdinInspector.Editor;
using UnityEditor;

[CustomEditor(typeof(MapEditor.Path))]
public class PathInspector : OdinEditor
{
    public MapEditor.Path Path
    {
        get
        {
            return this.target as MapEditor.Path;
        }
    }

    private void OnSceneGUI()
    {
        Path.transform.position = Handles.DoPositionHandle(Path.transform.position, Quaternion.identity);
    }
}
