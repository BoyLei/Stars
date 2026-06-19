///--------------------------------------------------------------------
/// 文件名   :   GizmosHelper.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/12/08 15:01:47
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
#if UNITY_EDITOR
using MapEditor;
using StarProject.Game.Map;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GizmosHelper : MonoBehaviour
{
    public Color Gizmoscolor=Color.green;
    private void OnDrawGizmos()
    {
        if (GameMap.sceneJsonData == null)
        {
            return;
        }

        if(GameMap.sceneJsonData.Areas==null)
        {
            return;
        }

        if (GameMap.sceneJsonData.Areas.Count < 1)
        {
            return;
        }
        Color color = Handles.color;

        Handles.color = Gizmoscolor;
        foreach (var item in GameMap.sceneJsonData.Areas)
        {
            if(item.Value.shapType ==(int)AreaShape.Circle)
            {
                UnityEditor.Handles.DrawWireArc(item.Value.Position.Convert(), Vector3.up, Vector3.forward, 360, item.Value.Radius * 0.01f);
            }
            else if (item.Value.shapType == (int)AreaShape.Rectangle)
            {
                var leftUnit = Vector3.Cross(transform.forward, Vector3.up);
                float halflen = item.Value.Length * 0.01f / 2;
                float halfwid = item.Value.Width * 0.01f / 2;
                var leftBot = transform.position + (leftUnit * halflen) - (transform.forward * halfwid);
                var leftTop = transform.position + (leftUnit * halflen) + (transform.forward * halfwid);
                var rightBot = transform.position - (leftUnit * halflen) - (transform.forward * halfwid);
                var righTop = transform.position - (leftUnit * halflen) + (transform.forward * halfwid);
                Vector3[] rectVerts = new Vector3[4] { leftBot, leftTop, righTop, rightBot };
                UnityEditor.Handles.DrawAAConvexPolygon(rectVerts);
            }
            else if (item.Value.shapType == (int)AreaShape.Polygon)
            {
                if (item.Value.Polygons.Count > 0)
                {
                    List<Vector3> vectors = new List<Vector3>();
                    foreach (var it in item.Value.Polygons)
                    {
                        if (it == null)
                        {
                            continue;
                        }
                        vectors.Add(it.Convert());// + transform.position;
                    }
                    vectors.Add(vectors[0]);
                    UnityEditor.Handles.DrawAAConvexPolygon(vectors.ToArray());
                }

            }
        }
        Handles.color = color;
    }
}
#endif