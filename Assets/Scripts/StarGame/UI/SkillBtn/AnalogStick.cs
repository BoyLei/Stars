using StarProject.Game;
using StarProject.Game.Data;
using StarProject.Service.Business;
using StarProject.Service.Input;
using StarProjectDef;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace StarProject.UI.SkillBtn
{
    public class AnalogStick : UniversalButton
    {
        public RectTransform directionalPointer;    // 方向性的指示器

        private Color m_UpColor = new(255, 255, 255, 0.6f);
        private Color m_DownColor = new(255, 255, 255, 1f);
        private Image m_aimerImage;
        private Image m_pointerImage;
        private Image m_directionalPointer;

        private Vector3 m_InputMoveDirection = Vector3.zero;    // 键盘WASD输入的方向向量
        private Vector3 m_MoveDirection = Vector3.zero; // 转换成手指的方向向量 缩放过后的

        private float pointerRadius;    // 手指节点的半径（缩放后的）
        private float tmpFloat;
        private Vector3 tmpVec;
        private Vector3 m_AimerPosition = Vector3.zero;

        //private PointerEventData m_PointerEventData = null;

        protected override void Awake()
        {
            isAimable = true;
            SkillBtnPos = -1;

            base.Awake();

            pointerRadius = ActivePointer.rect.width / 2f * scaler.scaleFactor;
            InputManager.Instance.AnalogStickRadius = pointerRadius;
            directionalPointer.gameObject.SetActive(false);

            m_aimerImage = ActiveAimer.GetComponent<Image>();
            m_pointerImage = ActivePointer.GetComponent<Image>();
            m_directionalPointer = directionalPointer.GetComponent<Image>();
        }

        //protected override void Update()
        //{
        //    if (m_PointerEventData != null)
        //    {
        //        if (!m_PointerEventData.IsPointerMoving())
        //        {
        //            SetState(E_SkillBtnState.Active);
        //            UpdateIndicatorShow(false);
        //            BtnDirChange?.Invoke(Vector3.zero);
        //        }
        //    }
        //}

        public override void OnXinSGUIShowCB(object arg0)
        {
            bool isShow = true;
            string mapSubType = GameManager.Instance.GetMapSubType();

            if (mapSubType == GameConfig.INSTANCE_PLOT_SECOND)
            {
                isShow = BusinessManager.Instance.GetXinSGStateDic(SkillBtnPos.ToString());
            }
            if (gameObject.activeSelf != isShow)
            {
                gameObject.SetActive(isShow);
            }
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            SGF.Debuger.LogWarning($"方向遥感 id={eventData?.pointerId},state={state} 按下 OnPointerUp");
            //m_PointerEventData = eventData;
            if (state == E_SkillBtnState.Active)
            {
                if (m_MoveDirection.magnitude > 0)
                {
                    return;
                }
                fingerId = eventData.pointerId;
                initialFingerPosition = eventData.position;

                //tmpFloat = (initialFingerPosition - ActivePointer.transform.position).magnitude;

                ActiveAimer.position = initialFingerPosition;
                ActivePointer.position = initialFingerPosition;
                directionalPointer.position = initialFingerPosition;

                //if (tmpFloat < pointerRadius)
                //{
                //    m_AimerPosition.x = initialFingerPosition.x;
                //    m_AimerPosition.x = initialFingerPosition.y < aimerRadius ? aimerRadius : initialFingerPosition.y;
                //    m_AimerPosition.z = initialFingerPosition.z;
                //    ActiveAimer.position = m_AimerPosition;
                //    //pointer.position = aimer.position;
                //}
                //else
                //{
                //    tmpVec = ActivePointer.transform.position - initialFingerPosition;
                //    tmpVec = Vector3.ClampMagnitude(tmpVec, aimerRadius);
                //    tmpVec = initialFingerPosition + tmpVec;
                //    m_AimerPosition.x = tmpVec.x;
                //    m_AimerPosition.x = tmpVec.y < aimerRadius ? aimerRadius : tmpVec.y;
                //    m_AimerPosition.z = tmpVec.z;
                //    ActiveAimer.position = m_AimerPosition;
                //    //rawDir = fingerPosition - aimer.position;
                //    //rawDir = Vector3.ClampMagnitude(rawDir, aimerRadius);
                //    //pointer.position = aimer.position + rawDir;
                //}

                UpdateAiming(eventData);

                SetState(E_SkillBtnState.Pressed);
                UpdateIndicatorShow(true);
                onPointerDown?.Invoke(this);

                GlobalEvent.OnReciveEvent.Invoke(E_EventDefine.DragStick, "DragStick");
            }
        }

        public override void OnBeginDrag(PointerEventData eventData)
        {
            base.OnBeginDrag(eventData);
        }

        public override void OnDrag(PointerEventData eventData)
        {
            base.OnDrag(eventData);
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            SGF.Debuger.LogWarning($"方向遥感 id={eventData?.pointerId},state={state} 抬起 OnPointerUp");
            m_IsKeyDown = false;
            base.OnPointerUp(eventData);
            //m_PointerEventData = null;
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            SGF.Debuger.LogWarning($"方向遥感 id={eventData?.pointerId},state={state} 抬起 OnEndDrag");
            if (state == E_SkillBtnState.Pressed)
            {
                m_IsKeyDown = false;
                base.OnEndDrag(eventData);
            }
            //m_PointerEventData = null;
        }

        protected override void UpdateIndicatorShow(bool isShow, bool isEndPressing = true)
        {
            // ---- 设置滑动框
            Color color = isShow ? m_DownColor : m_UpColor;
            m_aimerImage.color = color;
            ActiveAimer.gameObject.SetActive(true);

            m_pointerImage.color = color;
            ActivePointer.gameObject.SetActive(true);

            directionalPointer.gameObject.SetActive(isShow);
            if (isShow == false)
            {
                ActiveAimer.anchoredPosition = Vector3.zero;
                ActivePointer.anchoredPosition = Vector3.zero;
                directionalPointer.anchoredPosition = Vector3.zero;
            }
            // -- 标记抬起的状态
            GameInput.SetTouchState(transform.name, isShow);
        }

        protected override void UpdateAiming(PointerEventData eventData)
        {
            fingerPosition.x = eventData.position.x;
            fingerPosition.y = eventData.position.y;
            rawDir = fingerPosition - ActiveAimer.position;
            rawDir = Vector3.ClampMagnitude(rawDir, aimerRadius);

            ActivePointer.position = ActiveAimer.position + Vector3.ClampMagnitude(rawDir, aimerRadius - pointerRadius);

            this.UpdateDirection();

            if (direction.magnitude > 0.01f)
            {
                // 方案一： 跟着中心点指示器【移动】
                //directionalPointer.position = aimer.position + direction.normalized * aimerRadius;
                //directionalPointer.up = direction;
                //directionalPointer.gameObject.SetActive(true);
                // 二：跟着中心点指示器【旋转】
                directionalPointer.position = ActiveAimer.position /*+ direction.normalized * aimerRadius*/;
                directionalPointer.up = direction;
                m_directionalPointer.color = m_DownColor;
                directionalPointer.gameObject.SetActive(true);
            }
            else
            {
                directionalPointer.gameObject.SetActive(false);
            }
        }

        protected override void InputVKey(int vkey, float arg)
        {
            if (state == E_SkillBtnState.Pressed)
            {
                return;
            }

            bool hasHandled = DoVKey_Move(vkey, arg);

            if (hasHandled)
            {
                m_MoveDirection.x = m_InputMoveDirection.x * aimerRadius;
                m_MoveDirection.y = m_InputMoveDirection.z * aimerRadius;
                m_MoveDirection = Vector3.ClampMagnitude(m_MoveDirection, aimerRadius);

                ActivePointer.position = ActiveAimer.position + Vector3.ClampMagnitude(m_MoveDirection, aimerRadius);
                GlobalEvent.OnReciveEvent.Invoke(E_EventDefine.DragStick, "DragStick");
                if (m_MoveDirection.magnitude == 0)
                {
                    // 方向摇杆 抬起 的事件通知
                    GameManager.Instance.TriggerEvent("AnalogStick_Up", null);
                }
            }
        }

        private bool DoVKey_Move(int vkey, float args)
        {
            switch (vkey)
            {
                case GameVKey.MoveX_CMD:
                    {
                        m_InputMoveDirection.x = args;
                    }
                    break;
                case GameVKey.MoveZ_CMD:
                    {
                        m_InputMoveDirection.z = args;
                    }
                    break;
                default:
                    return false;
            }
            return true;

        }
    }
}