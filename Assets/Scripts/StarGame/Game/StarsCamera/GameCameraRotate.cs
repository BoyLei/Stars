using Sirenix.OdinInspector;
using StarProject.Service.Cam;
using UnityEngine;
using UnityEngine.EventSystems;

namespace StarProject.ArtHelper
{
    public class GameCameraRotate : MonoBehaviour, IPointerDownHandler, IDragHandler, IEndDragHandler, IPointerUpHandler
    {
        [LabelText("Y轴转速度")]
        public float MoveXSpeed = 0.1f;
        [LabelText("Y轴转速度")]
        private float MoveYSpeed = 0.1f;
        [LabelText("是否长按后开启滑动")]
        public bool isHoltOpen = false;
        [LabelText("按住时长秒")]
        public float holtTime = 0.5f;
        [LabelText("X轴是否旋转")]
        private bool IsCanAllowYTilt = false;

        private float x = 0.0f;
        private float y = 0.0f;
        private float targetX = 0f;
        private float targetY = 0f;

        private Quaternion m_LastRotation;

        private Vector3 LastPos;    // 手指初始点击坐标
        private int fingerId = -99;  // EventData手指ID
        private bool isStartDrag = false;
        private bool isFiltration = false;
        private float countdown = 0;

        private void Update()
        {
            if (isHoltOpen)
            {
                if (!isStartDrag && fingerId != -99 && countdown > 0 && !isFiltration)
                {
                    countdown -= Time.deltaTime;
                    if (countdown <= 0)
                    {
                        SGF.Debuger.LogWarning("开始旋转Y轴");
                        isStartDrag = true;
                    }
                }
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            isFiltration = false;
            fingerId = eventData.pointerId;
            LastPos = eventData.position;
            isStartDrag = !isHoltOpen;
            if (isHoltOpen)
            {
                countdown = holtTime;
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (eventData.pointerId == fingerId)
            {
                if (isStartDrag && isHoltOpen == false)
                {
                    targetX += (eventData.position.x - LastPos.x) * MoveXSpeed;
                    // Y轴旋转
                    if (IsCanAllowYTilt)
                    {
                        targetY -= (LastPos.y - eventData.position.y) * MoveYSpeed;
                    }

                    x = targetX;
                    y = targetY;

                    Quaternion rotation = Quaternion.Euler(y, -x, 0);
                    if (m_LastRotation != rotation)
                    {
                        m_LastRotation = rotation;
                        CameraManager.Instance.SetPlayerCameraAngleY(x);
                    }
                    LastPos = eventData.position;
                }
                else
                {
                    isFiltration = true;
                }
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            LastPos = Vector3.zero;
            fingerId = -99;
            isStartDrag = false;
            isFiltration = false;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            LastPos = Vector3.zero;
            fingerId = -99;
            isStartDrag = false;
            isFiltration = false;
        }

    }
}


