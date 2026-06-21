///--------------------------------------------------------------------
/// 文件名   :   MapControlEditor
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/08/05 09:30:50
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using  MapEditor;
using  UnityEditor;

[CustomEditor(typeof(MapControl))]
public class MapControlEditor : Editor
{
    public MapControl map
    {
        get
        {
            return  this.target as  MapControl;
        }
    }
    private void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }
        void OnSceneGUI(SceneView sceneView)
        {

            Event e = Event.current;
            if (e != null)
            {
                if ( e.keyCode == KeyCode.A)
                {
                    OnUpdateCameraRotation(false);
                }
                if (e.keyCode == KeyCode.D)
                {
                    OnUpdateCameraRotation(true);
                }
                
                if (e.type == EventType.Repaint)
                {
                    OnUpdateCamera();
                }

                
            }
        }
        
        private void OnUpdateCameraRotation(bool add)
        {
            var angles = map.transform.eulerAngles;
            angles.y+= add ? 5: -5;
            map.transform.rotation=
                Quaternion.Euler(angles);
        }
        
        private void OnUpdateCamera()
        {
            if (map!=null && MapEditorUtils.GetScreenPosition(out var CameraPos))
            {
                map.transform.position = CameraPos;
            }

        }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }
}
