///--------------------------------------------------------------------
/// 文件名   :   Path
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/05/26 11:35:45
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------

# if UNITY_EDITOR
using Cinemachine;
using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace MapEditor
{
    [HideMonoScript]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CinemachinePath))]
    [SelectionBase]
    public class Path : MonoBehaviour
    {
        [FoldoutGroup("路径相关", 0)] [LabelText("路径ID")]
        public int PathID;

        [FoldoutGroup("贴地相关", 3)]
        [Button("一键贴地")]
        public void OnGround()
        {
            Vector3 position = transform.position;
            transform.position = MapEditorUtils.GetGroundPoint(position);
            var wps = path.m_Waypoints;
            List<Vector3> caches = new List<Vector3>();
            for (int i = 0; i < wps.Length; i++)
            {
                caches.Add(MapEditorUtils.GetGroundPoint(transform.position + wps[i].position));
            }

            for (int i = 0; i < caches.Count; i++)
            {
                path.m_Waypoints[i].position = caches[i] - transform.position;
            }
        }


        private CinemachinePath _path;

        public CinemachinePath path
        {
            get
            {
                if (_path == null)
                {
                    _path = gameObject.GetComponent<CinemachinePath>();
                }

                return _path;
            }
        }

        public bool IsLoop
        {
            get
            {
                return path.Looped;
            }
            set
            {
                path.m_Looped = value;
            }
        }



        public List<Vector3> GetPaths()
        {
            List<Vector3> paths = new List<Vector3>();
            var start = Vector3.zero;
            float step = path.PathLength / (path.m_Resolution * path.m_Waypoints.Length - 1);
            float m_Position = 0;
            float s = 0;
            float c_ps = 0;
            bool needbreak = false;
            while (m_Position < path.PathLength)
            {
                float c = path.StandardizeUnit(s, Cinemachine.CinemachinePathBase.PositionUnits.Distance);
                m_Position = c;
                if (m_Position < c_ps)
                {
                    needbreak = true;
                }
                c_ps = m_Position;
                start = path.EvaluatePositionAtUnit(m_Position, Cinemachine.CinemachinePathBase.PositionUnits.Distance);
                paths.Add(start);
                s = m_Position + step;
                if (needbreak)
                {
                    break;
                }
            }

            return paths;
        }

        public void ReshModel()
        {
        }
    }
}
#endif