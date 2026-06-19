using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;

namespace StarProject.ArtHelper
{
    [XLua.LuaCallCSharp]
    public class DisplayRawimg : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerUpHandler
    {
        //private RawCamera m_RawCamera;

        [LabelText("X轴转速度")]
        public float MoveXSpeed = 0.5f;
        [LabelText("Y轴转速度")]
        public float MoveYSpeed = 0.5f;
        [LabelText("Y轴是否旋转")]
        private bool IsCanAllowYTilt = false;
        [LabelText("旋转是否过渡")]
        public bool IsCanLerp = false;
        [LabelText("Y轴最小度数")]
        public float yMinLimit = -90f;
        [LabelText("Y轴最大度数")]
        public float yMaxLimit = 90f;
        private float x = 0.0f;
        private float y = 0.0f;
        private float targetX = 0f;
        private float targetY = 0f;
        //private float xVelocity = 1f;
        //private float yVelocity = 1f;
        private Quaternion m_LastRotation;

        //---- 缩放
        //public Vector3 pivotOffset = Vector3.zero;
        //public float distance = 10.0f;
        //public float minDistance = 2f;
        //public float maxDistance = 15f;
        //public float zoomSpeed = 1f;
        //public float targetDistance = 0f;


        private RoleViewDisplay m_ModelViewDisplay = null;
        private bool m_IsUpdate = false;

        private Vector3 LastPos;    // 手指初始点击坐标
        private int fingerId = -99;  // EventData手指ID

        //public void Awake()
        //{
        //    //if (m_RawCamera == null)
        //    //{
        //    //    m_RawCamera = CameraManager.Instance.GetCamera(E_CameraType.SpecialCam) as RawCamera;
        //    //}
        //}

        //public void LateUpdate()
        //{
        //    if (/*m_RawCamera != null &&*/ m_ModelViewDisplay != null && m_IsUpdate)
        //    {
        //        //// 缩放
        //        //var scroll = Input.GetAxis("Mouse ScrollWheel");
        //        //if (scroll > 0.0f)
        //        //{
        //        //    targetDistance -= zoomSpeed;
        //        //}
        //        //else if (scroll < 0.0f)
        //        //{
        //        //    targetDistance += zoomSpeed;
        //        //}
        //        //targetDistance = Mathf.Clamp(targetDistance, minDistance, maxDistance);
        //        // 旋转
        //        if (Input.GetMouseButton(0))
        //        {
        //            targetX += Input.GetAxis("Mouse X") * MoveXSpeed;
        //            // Y轴旋转
        //            if (IsCanAllowYTilt)
        //            {
        //                targetY -= Input.GetAxis("Mouse Y") * MoveYSpeed;
        //                targetY = ClampAngle(targetY, yMinLimit, yMaxLimit);
        //            }
        //        }

        //        if (IsCanLerp)
        //        {
        //            // 平滑缓冲  做减速缓冲到指定值
        //            x = Mathf.SmoothDampAngle(x, targetX, ref xVelocity, 0.3f);
        //            y = IsCanAllowYTilt ? Mathf.SmoothDampAngle(y, targetY, ref yVelocity, 0.3f) : targetY;
        //        }
        //        else
        //        {
        //            x = targetX;
        //            y = IsCanAllowYTilt ? targetY : targetY;
        //        }

        //        Quaternion rotation = Quaternion.Euler(y, -x, 0);
        //        if (m_LastRotation != rotation)
        //        {
        //            m_LastRotation = rotation;
        //            m_ModelViewDisplay.SetRotation(rotation);
        //        }
        //        //distance = Mathf.SmoothDamp(distance, targetDistance, ref zoomVelocity, 0.5f);
        //        //m_tempPos.z = -distance;
        //        //Vector3 position = rotation * m_tempPos + m_ModelViewDisplay.transform.position + pivotOffset;
        //        //m_ModelViewDisplay.SetPos(position);
        //    }
        //}


        public void OnPointerDown(PointerEventData eventData)
        {
            if (m_ModelViewDisplay != null && m_IsUpdate)
            {
                fingerId = eventData.pointerId;
                LastPos = eventData.position;
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            //Debug.Log("拖拽 OnBeginDrag");

        }

        public void OnDrag(PointerEventData eventData)
        {
            if (m_ModelViewDisplay != null && m_IsUpdate && eventData.pointerId == fingerId)
            {
                targetX += (eventData.position.x - LastPos.x) * MoveXSpeed;

                // Y轴旋转
                if (IsCanAllowYTilt)
                {
                    targetY -= (LastPos.y - eventData.position.y) * MoveYSpeed;
                    targetY = ClampAngle(targetY, yMinLimit, yMaxLimit);
                }

                x = targetX;
                y = IsCanAllowYTilt ? targetY : targetY;

                Quaternion rotation = Quaternion.Euler(y, -x, 0);
                if (m_LastRotation != rotation)
                {
                    m_LastRotation = rotation;
                    m_ModelViewDisplay.SetRotation(rotation);
                }
                LastPos = eventData.position;
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            LastPos = Vector3.zero;
            fingerId = -99;
            //targetX = 0;
            //targetY = 0;
            //x = 0;
            //y = 0;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            LastPos = Vector3.zero;
            fingerId = -99;
            //targetX = 0;
            //targetY = 0;
            //x = 0;
            //y = 0;
        }

        private float ClampAngle(float angle, float min, float max)
        {
            if (angle < -360)
            {
                angle += 360;
            }

            if (angle > 360)
            {
                angle -= 360;
            }

            return Mathf.Clamp(angle, min, max);
        }

        public void SetModelView(RoleViewDisplay roleViewDisplay)
        {
            targetX = 0;
            targetY = 0;
            //xVelocity = 1;
            //yVelocity = 1;
            x = 0;
            y = 0;
            m_LastRotation = Quaternion.Euler(y, x, 0);
            m_ModelViewDisplay = roleViewDisplay;
            SetState(true);
        }

        public void SetState(bool isUpdate)
        {
            m_IsUpdate = isUpdate;
        }

    }
}

