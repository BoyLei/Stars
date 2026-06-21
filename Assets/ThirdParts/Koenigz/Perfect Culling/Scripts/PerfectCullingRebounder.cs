using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Koenigz.PerfectCulling.SamplingProviders
{
    //[ExecuteInEditMode]
    [RequireComponent(typeof(MeshRenderer))]
    public class PerfectCullingRebounder : MonoBehaviour
    {
        public void setPer(float v) {
            per = v;
        }
        [SerializeField] float per = 1f;
        Bounds bounds;
        Bounds orgBounds;
        MeshRenderer mr;

        Vector3 _newC;
        Bounds _b;
        Vector3 _size = Vector3.zero;
        Vector3 _topOffset = Vector3.zero;

        bool isInit = false;
        public bool IsDraw = true;

        // void Awake()
        // {
        //     _b = new Bounds();
        //     mr = GetComponent<MeshRenderer>();
        //     bounds = mr.bounds;

        // }

        //#if !UNITY_EDITOR
        void Start()
        {
            ChangeBounds();
        }
        //#endif


#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!IsDraw)
            {
                return;
            }
            ChangeBounds();

            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(_b.center, _b.size);

            //Gizmos.DrawSphere(Vector3.zero, 100);

            // if (results != null && results.Length > 0)
            // {
            //     foreach (var item in results)
            //     {
            //         Gizmos.DrawSphere(item.point, 1f);
            //     }
            //     BoxCollider
            // }
        }
#endif

        public void ChangeBounds()
        {
            if (!isInit)
            {
                _b = new Bounds();
                mr = GetComponent<MeshRenderer>();
                bounds = mr.bounds;
                orgBounds = mr.bounds;
                isInit = true;
            }

            var s = orgBounds.size;

            _topOffset.y = s.y / 2;

            var top = orgBounds.center + _topOffset;


            _newC.x = top.x;
            _newC.y = top.y - per * s.y / 2;
            _newC.z = top.z;

            _b.center = _newC;

            _size.x = s.x;
            _size.y = s.y * per;
            _size.z = s.z;
            _b.size = _size;

            mr.bounds = _b;
        }
    }
}