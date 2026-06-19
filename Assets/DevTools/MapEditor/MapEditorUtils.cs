///--------------------------------------------------------------------
/// 文件名   :   MapEditorUtils
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/07/12 14:22:37
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------

#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MapEditor
{
    public static class MapEditorUtils
    {
        private static RaycastHit m_HitInfo;
        private static Ray m_Ray;
        public static int NavMeshGroundLayerMask = LayerMask.GetMask("Ground");
        public static MapSceneConfig SceneConfig { get; private set; }

        public static void SetMapSceneConfig(MapSceneConfig _sceneConfig)
        {
            SceneConfig = _sceneConfig;
        }

        public static bool GetScreenPosition(out Vector3 position)
        {
            position = Vector3.zero;
            m_Ray = new Ray(SceneView.lastActiveSceneView.camera.transform.position,
                SceneView.lastActiveSceneView.camera.transform.forward);
            Debug.DrawLine(SceneView.lastActiveSceneView.camera.transform.position,
                SceneView.lastActiveSceneView.camera.transform.forward * 100);
            if (Physics.Raycast(m_Ray, out m_HitInfo, NavMeshGroundLayerMask))
            {
                Vector3 point = GetGroundPoint(m_HitInfo.point);
                position = point;
                return true;
            }
#if UNITY_EDITOR
            else
            {
                //Debug.LogWarningFormat("无法在{0}处获取场景地面的高度信息。", point);
            }
#endif
            return false;
        }

        /// <summary>
        /// 给定一点，返回此点在地面上的投影的位置
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public static Vector3 GetGroundPoint(Vector3 point,
            QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.Ignore)
        {
            Vector3 groundPoint = point;
            groundPoint.y = 0;
            float height = Mathf.Min(point.y, 990f) + 10;
            var ray = new Ray();
            ray.origin = groundPoint + Vector3.up * height;
            ray.direction = Vector3.down;

            if (Physics.Raycast(ray, out var hit, 200f, NavMeshGroundLayerMask, queryTriggerInteraction))
            {
                // Debug.Log(hitInfo.transform.gameObject.name);
                groundPoint.y = hit.point.y;
            }
            else
            {
                groundPoint = point;
                //Debug.LogWarningFormat("无法在{0}处获取场景地面的高度信息。", point);
            }

            return groundPoint;
        }

        public static bool MousePosition(SceneView sceneView, out Vector3 worldPos)
        {
            worldPos = Vector3.zero;
            Vector2 mousePosition = Event.current.mousePosition;
            Ray m_Ray = HandleUtility.GUIPointToWorldRay(mousePosition);
            Debug.DrawLine(m_Ray.origin, m_Ray.direction * 100);
            RaycastHit m_HitInfo;
            if (Physics.Raycast(m_Ray, out m_HitInfo, NavMeshGroundLayerMask))
            {
                worldPos = MapEditorUtils.GetGroundPoint(m_HitInfo.point);
                return true;
            }
#if UNITY_EDITOR
            else
            {
                //Debug.LogWarningFormat("无法在{0}处获取场景地面的高度信息。", point);
            }
#endif

            return false;
        }

        public static Transform CreatePort(string portName, Transform parent)
        {
            GameObject go = new GameObject(portName);
            go.transform.SetParent(parent);
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;
            return go.transform;
        }

        /// <summary>
        /// 设置锁定
        /// </summary>
        public static void LockLayer(int layer)
        {
            Tools.lockedLayers |= 1 << layer;
        }

        /// <summary>
        /// 取消锁定
        /// </summary>
        public static void UnLockLayer(int layer)
        {
            Tools.lockedLayers &= ~(1 << layer);
        }

        /// <summary>
        /// 切换锁定
        /// </summary>
        public static void SwichLockLayer(int layer)
        {
            Tools.lockedLayers ^= 1 << layer;
        }

        /// <summary>
        /// 判断是否锁定
        /// </summary>
        public static bool IsLayerLocked(int layer)
        {
            return (Tools.lockedLayers & 1 << layer) == 1 << layer;
        }

        public static void RunBat(string program, string parm)
        {
            try
            {
                System.Diagnostics.Process process = new System.Diagnostics.Process();
                process.StartInfo.FileName = program;
                process.StartInfo.Arguments = string.Format(parm); //this is argument
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.OutputDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                        Debug.Log("Info:" + e.Data);
                };

                process.ErrorDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                        Debug.LogError("Error:" + e.Data);
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                process.WaitForExit();
                process.Close();
            }
            catch (Exception ex)
            {
                Debug.LogWarningFormat("Exception Occurred :{0},{1}", ex.Message, ex.StackTrace.ToString());
            }
        }
    }
}
#endif