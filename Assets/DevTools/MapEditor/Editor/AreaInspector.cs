///--------------------------------------------------------------------
/// 文件名   :   AreaInspector
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/05/26 16:46:50
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector.Editor;

[CustomEditor(typeof(MapEditor.Area))]
public class AreaInspector : OdinEditor
{
    public MapEditor.Area Area
    {
        get
        {
            return this.target as MapEditor.Area;
        }
    }

    private void OnSceneGUI()
    {
        Handles.Label(Area.transform.position+new Vector3(0,1,0), Area.AreaName);
        //if (Area.shapType == MapEditor.AreaShape.Polygon)
        //{
        //    //if (Area.Polygons.Count > 0)
        //    //{
        //    //    Vector3[] vectors = new Vector3[Area.Polygons.Count + 1];
        //    //    int index = 0;
        //    //    foreach (var item in Area.Polygons)
        //    //    {
        //    //        vectors[index++] = item + Area.transform.position;
        //    //    }
        //    //    vectors[index++] = vectors[0];
        //    //    for (int i = 0; i < vectors.Length - 1; i++)
        //    //    {
        //    //        vectors[i] = Handles.DoPositionHandle(vectors[i], Quaternion.identity);
        //    //    }
        //    //    index = 0;
        //    //    for (int i = 0; i < vectors.Length - 1; i++)
        //    //    {
        //    //        Area.Polygons[i] = vectors[i] - Area.transform.position;
        //    //    }
        //    //}

        //}
        //else
        //{
          
        //}
        Area.transform.position = Handles.DoPositionHandle(Area.transform.position, Quaternion.identity);
    }
}
