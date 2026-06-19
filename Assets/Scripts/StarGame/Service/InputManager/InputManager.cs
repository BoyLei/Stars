///--------------------------------------------------------------------
/// 文件名   :   InputManager
/// 内  容   :   1、鼠标事件  || 单指操作
///              2、双指操作
///              3、虚拟键盘 和摇滚
///              4、点击选中目标
/// 说  明   :  
/// 创建日期 :   2022/07/07 09:46:54
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using ProtoMsg;
using SGF.Module.Framework;
using SGF.Network;
using SGF.UI.Framework;
using SGF.Unity;
using StarProject.Game;
using StarProject.Game.Data;
using StarProject.Game.Entity.View.VitalSign;
using StarProject.Game.Player;
using StarProject.Module;
using StarProject.Service.Cam;
using StarProject.Service.Timeline;
using StarProjectDef;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

using Vector3 = UnityEngine.Vector3;

namespace StarProject.Service.Input
{
    #region 鼠标事件  || 单指操作

    /// <summary>
    /// 点击屏幕
    /// </summary>
    public delegate void OnClickScreenHandler();

    /// <summary>
    /// 单击事件
    /// </summary>
    /// <param name="position">手指位置，鼠标位置</param>
    public delegate void OnTapHandler(Vector3 position);

    /// <summary>
    /// 双击事件
    /// </summary>
    /// <param name="position"></param>
    public delegate void DoubleTapHandler(Vector3 position);

    /// <summary>
    /// 长按事件
    /// </summary>
    /// <param name="position">手指位置，鼠标位置</param>
    /// <param name="holdtime">持续事件</param>
    public delegate void OnPressHandler(Vector3 position, float holdtime);

    /// <summary>
    /// 松开事件
    /// </summary>
    /// <param name="position">手指位置，鼠标位置</param>
    public delegate void OnUpHandler(Vector3 position);

    /// <summary>
    /// 开始滑动
    /// </summary>
    /// <param name="position">手指位置，鼠标位置</param>
    public delegate void SwipeStartHandler(Vector3 position);

    /// <summary>
    /// 滑动中
    /// </summary>
    /// <param name="position">手指位置，鼠标位置</param>
    /// <param name="holdtime">持续事件</param>
    public delegate void SwipeHandler(Vector3 position, float holdtime);

    /// <summary>
    /// 滑动结束
    /// </summary>
    /// <param name="position">手指位置，鼠标位置</param>
    public delegate void SwipeEndHandler(Vector3 position);

    /// <summary>
    /// 鼠标滚轮事件
    /// </summary>
    /// <param name="position"></param>
    public delegate void ScrollWheel(float axis);

    /// <summary>
    /// 鼠标右键--按下
    /// </summary>
    public delegate void MouseRightDown();
    /// <summary>
    /// 鼠标右键--抬起
    /// </summary>
    public delegate void MouseRightUp();

    public delegate void EmulatorMouseSacleCameraDistance(Vector3 position);

    #endregion

    #region 双指操作
    /// <summary>
    /// 双指开始
    /// </summary>
    /// <param name="Finger1Position"></param>
    /// <param name="Finger2Position"></param>
    public delegate void OnDoubleFingersStartHandler(Vector3 Finger1Position, Vector3 Finger2Position);

    /// <summary>
    /// 双指移动
    /// </summary>
    /// <param name="Finger1Position"></param>
    /// <param name="Finger2Position"></param>
    public delegate void OnDoubleFingersMoveHandler(Vector3 Finger1Position, Vector3 Finger2Position, float time);

    /// <summary>
    /// 双指抬起
    /// </summary>
    /// <param name="Finger1Position"></param>
    /// <param name="Finger2Position"></param>
    public delegate void OnDoubleFingersEndHandler(Vector3 Finger1Position, Vector3 Finger2Position);

    #endregion

    public class InputManager : ServiceModule<InputManager>
    {
        #region 鼠标事件  || 单指操作

        public event OnClickScreenHandler On_ClickScreen;

        /// <summary>
        /// 单击事件
        /// </summary>
        public event OnTapHandler On_Tap;

        /// <summary>
        /// 双击事件
        /// </summary>
        public event DoubleTapHandler On_DoubleTap;

        /// <summary>
        /// 长按事件
        public event OnPressHandler On_Press;

        /// <summary>
        /// 松开事件
        /// </summary>
        public event OnUpHandler On_Up;

        /// <summary>
        /// 开始滑动
        /// </summary>
        public event SwipeStartHandler On_Swipe_Start;

        /// <summary>
        /// 滑动中
        /// </summary>
        public event SwipeHandler On_Swipe;

        /// <summary>
        /// 滑动结束
        /// </summary>
        public event SwipeEndHandler On_Swipe_End;

        /// <summary>
        /// 鼠标滚动事件
        /// </summary>
        public event ScrollWheel On_ScrollWheel;

        /// <summary>
        /// 鼠标右键--按下
        /// </summary>
        public event MouseRightDown On_MouseRightDown;
        /// <summary>
        /// 鼠标右键--抬起
        /// </summary>
        public event MouseRightUp On_MouseRightUp;

        /// <summary>
        /// 模拟器缩放相机
        /// </summary>
        public event EmulatorMouseSacleCameraDistance On_EmulatorMouseSacleCameraDistance;
        #endregion

        #region 双指操作
        /// <summary>
        /// 双指开始
        /// </summary>
        /// <param name="Finger1Position"></param>
        /// <param name="Finger2Position"></param>
        public event OnDoubleFingersStartHandler On_DoubleFingersStart;

        /// <summary>
        /// 双指移动
        /// </summary>
        /// <param name="Finger1Position"></param>
        /// <param name="Finger2Position"></param>
        public event OnDoubleFingersMoveHandler On_DoubleFingersMove;

        /// <summary>
        /// 双指抬起
        /// </summary>
        /// <param name="Finger1Position"></param>
        /// <param name="Finger2Position"></param>
        public event OnDoubleFingersEndHandler On_DoubleFingersEnd;

        #endregion

        #region 虚拟按键
        public Action<int, float> OnVirtualInput;
        private DictionaryEx<KeyCode, bool> m_MapKeyState = new();
        private DictionaryEx<KeyCode, bool> m_MapSkillKeyState = new();
        private DictionaryEx<KeyCode, bool> m_TestKeyState = new();
        // 反对应物理按钮
        private Dictionary<KeyCode, KeyCode> m_InversionKeyDic = new()
        {
            { KeyCode.A, KeyCode.D},
            { KeyCode.D, KeyCode.A},
            { KeyCode.S, KeyCode.W},
            { KeyCode.W, KeyCode.S}
        };
        /// <summary> 按键是否按下 </summary>
        private bool isKeyDown = false;
        public bool IskeyDown
        {
            get
            {
                return isKeyDown;
            }
        }
        /// <summary> 摇杆是否移动 </summary>
        private bool isJoystickMove = false;
        public bool IsJoystickMove
        {
            get
            {
                return isJoystickMove;
            }
        }

        #endregion

        private LayerMask layerMask;
        private int maxDistance = 100;
        private float mHoldingTime;
        private bool isDown = false;
        private bool isDownScrollWheel = false;
        private float mLastTapTime = 0;
        private bool m_IsSingleFinger = false;

        private bool isMouseRightDown = false;
        private float mLastInputTime = 0;
        public float AnalogStickRadius;    // 遥感的半径（缩放后的）

        /// <summary>
        /// 是否输入
        /// </summary>
        public bool GameNoneInput = false;

        private EntityCtrlBase m_MainPlayerCtrlBase;
        public EntityCtrlBase M_MainPlayerCtrlBase
        {
            get
            {
                if (m_MainPlayerCtrlBase == null)
                {
                    m_MainPlayerCtrlBase = GameManager.Instance.M_MainPlayerCtrlBase;
                }
                return m_MainPlayerCtrlBase;
            }
            set
            {
                m_MainPlayerCtrlBase = null;
            }
        }

        public void Init()
        {
            CheckSingleton();
            Reset();
            layerMask = LayerMask.GetMask("Entity");
            MonoHelper.AddUpdateListener(OnUpdate, MonoHelper.E_ModuleType.CommonService);
            On_Tap += OnTapEventHandler;
        }

        private void OnUpdate()
        {
            //这是Windows平台
#if UNITY_EDITOR || UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX
            WindowsPlatforms();
            OnKeyBoard();
#elif UNITY_ANDROID || UNITY_IOS || UNITY_IPHONE
           MobilePlatforms();
           OnKeyBoard();
#endif
            CheckPlayerUnInput();
        }


        private void CheckPlayerUnInput()
        {
            var deltalTime = (UnityEngine.Time.realtimeSinceStartup - mLastInputTime) > SystemConstConfigs.Guidance_NoInputTaskHLInterval;
            if (GameNoneInput != deltalTime)
            {
                GameNoneInput = deltalTime;
                GlobalEvent.OnGameNoneInputChange?.Invoke(GameNoneInput);
            }
        }

        private void Reset()
        {
            MonoHelper.RemoveUpdateListener(OnUpdate, MonoHelper.E_ModuleType.CommonService);

            m_MapKeyState.Clear();
            m_MapSkillKeyState.Clear();
            m_TestKeyState.Clear();
            maxDistance = 100;
            mHoldingTime = 0;
            isDown = false;
            mLastTapTime = 0;
            m_IsSingleFinger = false;
            isMouseRightDown = false;
        }

        public override void Release()
        {
            Reset();
            OnVirtualInput = null;
            On_Tap = null;
            On_DoubleTap = null;
            On_Press = null;
            On_Up = null;
            On_Swipe_Start = null;
            On_Swipe = null;
            On_Swipe_End = null;
            On_ScrollWheel = null;
            On_MouseRightDown = null;
            On_MouseRightUp = null;
            base.Release();
        }

        /// <summary>
        /// 获取是否禁止input
        /// </summary>
        /// <returns></returns>
        public bool IsForbidInpitTouch(bool isFiltrationJoystick = false)
        {
            // 有任意page/window活跃就取消信息发送缩放禁止
            if (UIManager.Instance.GetIsHasAnyWindowOpened())
            {
                return true;
            }
            // 切换地图/副本的时候禁止
            if (!GameManager.Instance.GetMapChanageIsSuccess())
            {
                return true;
            }
            //
            if (TimelineManager.Instance.IsPlayTimeLine)
            {
                return true;
            }

            if (!isFiltrationJoystick)
            {
                // 有遥感触摸移动的时候禁止
                if (onJoyMoveUpEvent)
                {
                    return true;
                }
            }
            return false;
        }

        #region 鼠标 手指操作
        private int Cur_Swipe_Start_Touch_Index = -1;
        private void MobilePlatforms()
        {
            if (GameConfig.isEmulator)
            {
                EmulatorMobilePlatforms();
            }
            else
            {
                if (UnityEngine.Input.touchCount == 1)
                {
                    mLastInputTime = UnityEngine.Time.realtimeSinceStartup;
                    Vector3 mousePosition = UnityEngine.Input.GetTouch(0).position;
                    if (UnityEngine.Input.GetTouch(0).phase == TouchPhase.Ended)
                    {
                        On_Up?.Invoke(mousePosition);
                        On_Swipe_End?.Invoke(mousePosition);
                        Cur_Swipe_Start_Touch_Index = -1;
                        //Debug.Log($"旋转相机 111111111111 抬起 Cur_Swipe_Start_Touch_Index={Cur_Swipe_Start_Touch_Index},pos1={mousePosition}");
                    }

                    if (UnityEngine.Input.GetTouch(0).phase == TouchPhase.Began)
                    {
                        mLastInputTime = UnityEngine.Time.realtimeSinceStartup;
                        On_ClickScreen?.Invoke();
                        GlobalEvent.OnClickScreen.Invoke("ClickScreen");
                        //全局检测关闭ItemTips
                        CheckItemTips();

                        if (CheckGuiRaycastObjects())
                        {
                            return;
                        }
                        Cur_Swipe_Start_Touch_Index = 0;
                        //Debug.Log($"旋转相机 111111111111 开始 Cur_Swipe_Start_Touch_Index={Cur_Swipe_Start_Touch_Index},pos1={mousePosition}");
                        On_Tap?.Invoke(mousePosition);
                        On_Swipe_Start?.Invoke(mousePosition);
                        isDown = true;
                        mHoldingTime = 0;
                        if (UnityEngine.Time.realtimeSinceStartup - mLastTapTime <= 0.5f)
                        {
                            On_DoubleTap?.Invoke(mousePosition);
                        }
                        mLastTapTime = UnityEngine.Time.realtimeSinceStartup;
                        ModuleManager.Instance.SendMessage(ModuleDef.Name.PlayerPreviewModule, "ClosePlayerFuncTips", new object[] { });
                    }

                    if (UnityEngine.Input.GetTouch(0).phase == TouchPhase.Moved && isDown)
                    {
                        mHoldingTime += UnityEngine.Time.deltaTime;
                        On_Press?.Invoke(mousePosition, mHoldingTime);
                        On_Swipe?.Invoke(mousePosition, mHoldingTime);
                        //Debug.Log($"旋转相机 111111111111 滑动中 Cur_Swipe_Start_Touch_Index={Cur_Swipe_Start_Touch_Index},pos1={mousePosition}");
                    }
                    m_IsSingleFinger = true;
                }
                // 多点触摸--缩放镜头
                else if (UnityEngine.Input.touchCount > 1)
                {
                    Vector3 position1 = UnityEngine.Input.GetTouch(0).position;
                    Vector3 position2 = UnityEngine.Input.GetTouch(1).position;
                    if (m_IsSingleFinger)
                    {
                        // 是否禁止input输入
                        if (!IsForbidInpitTouch())
                        {
                            On_DoubleFingersStart?.Invoke(position1, position2);
                        }
                        mHoldingTime = 0;
                    }

                    if (Cur_Swipe_Start_Touch_Index < 0)
                    {
                        On_Swipe_Start?.Invoke(position2);
                        Cur_Swipe_Start_Touch_Index = 1;
                    }
                    //Debug.LogWarning($"旋转相机 222222222 开始 Cur_Swipe_Start_Touch_Index={Cur_Swipe_Start_Touch_Index},pos1={position1},pos2={position2}");

                    if (UnityEngine.Input.GetTouch(0).phase == TouchPhase.Moved || UnityEngine.Input.GetTouch(1).phase == TouchPhase.Moved)
                    {
                        mHoldingTime += UnityEngine.Time.deltaTime;
                        if (!IsForbidInpitTouch())
                        {
                            On_DoubleFingersMove?.Invoke(position1, position2, mHoldingTime);
                        }
                        //Debug.LogWarning($"旋转相机 222222222 滑动中 Cur_Swipe_Start_Touch_Index={Cur_Swipe_Start_Touch_Index},pos1={position1},pos2={position2}");

                        if (Cur_Swipe_Start_Touch_Index == 0)
                        {
                            On_Swipe?.Invoke(position1, mHoldingTime);
                        }
                        else if (Cur_Swipe_Start_Touch_Index == 1)
                        {
                            On_Swipe?.Invoke(position2, mHoldingTime);
                        }
                    }

                    if (UnityEngine.Input.GetTouch(0).phase == TouchPhase.Ended || UnityEngine.Input.GetTouch(1).phase == TouchPhase.Ended)
                    {
                        if (!IsForbidInpitTouch())
                        {
                            On_DoubleFingersEnd?.Invoke(position1, position2);
                        }
                        //Debug.LogWarning($"旋转相机 222222222 结束 Cur_Swipe_Start_Touch_Index={Cur_Swipe_Start_Touch_Index},pos1={position1},pos2={position2}");

                        if (Cur_Swipe_Start_Touch_Index == 0)
                        {
                            On_Swipe_End?.Invoke(position1);
                            Cur_Swipe_Start_Touch_Index = -1;
                        }
                        else if (Cur_Swipe_Start_Touch_Index == 1)
                        {
                            On_Swipe_End?.Invoke(position2);
                            Cur_Swipe_Start_Touch_Index = -1;
                        }
                    }

                    m_IsSingleFinger = false;
                }
            }

            HandleInputKey(KeyCode.None, GameVKey.MoveX_CMD, joyMoveDirection.x);
        }

        private int flgindex = -1;

        private void EmulatorMobilePlatforms()
        {
            Vector3 mousePosition = UnityEngine.Input.mousePosition;

            {
                float axis = UnityEngine.Input.GetAxis("Mouse ScrollWheel");
                if (axis != 0 && !IsForbidInpitTouch())
                {
                    SGF.Debuger.Log($"模拟器输入 中间键 滑动中----------------axis={axis}");
                    On_ScrollWheel?.Invoke(axis);
                    isDownScrollWheel = true;
                    flgindex = 1;
                }
                else
                {
                    if (flgindex == 1)
                    {
                        isDownScrollWheel = false;
                    }
                }
            }

            {
                Vector2 scrollDelta = UnityEngine.Input.mouseScrollDelta;
                if (scrollDelta.y != 0 && !IsForbidInpitTouch())
                {
                    SGF.Debuger.Log($"模拟器输入 中间键 Mouse Scroll Delta: {scrollDelta.y}");
                    On_ScrollWheel?.Invoke(scrollDelta.y);
                    isDownScrollWheel = true;
                    flgindex = 2;
                }
                else
                {
                    if (flgindex == 2)
                    {
                        isDownScrollWheel = false;
                    }
                }
            }

            {
                //var scroll = UnityEngine.InputSystem.Mouse.current.scroll.ReadValue();
                //if (scroll.y != 0  && !IsForbidInpitTouch())
                //{
                //    SGF.Debuger.Log($"模拟器输入 中间键 Scroll Wheel: {scroll.y}");
                //    On_ScrollWheel?.Invoke(scroll.y);
                //    isDownScrollWheel = true;
                //    flgindex = 3;
                //}
                //else
                //{
                //    if (flgindex == 3)
                //    {
                //        isDownScrollWheel = false;
                //    }
                //}
            }

            //{
            //    var key = KeyCode.LeftControl;
            //    if (UnityEngine.Input.GetKey(key) && !IsForbidInpitTouch())
            //    {
            //        SGF.Debuger.Log($"模拟器输入 中键 开始----------------pos={mousePosition}");
            //        isDownScrollWheel = true;
            //        On_ScrollWheel?.Invoke(mousePosition.y);
            //        flgindex = 4;
            //    }
            //    else
            //    {
            //        if (flgindex == 4)
            //        {
            //            isDownScrollWheel = false;
            //            SGF.Debuger.Log($"模拟器输入 中键 结束----------------pos={mousePosition}");
            //        }
            //    }
            //}

            if (UnityEngine.Input.GetMouseButtonDown(0) && !isDownScrollWheel)
            {
                SGF.Debuger.Log($"模拟器输入 左键 开始----------------pos={mousePosition}");

                //全局检测关闭ItemTips
                CheckItemTips();
                {
                    On_ClickScreen?.Invoke();
                    GlobalEvent.OnClickScreen.Invoke("ClickScreen");
                    if (CheckGuiRaycastObjects())
                    {
                        return;
                    }

                    isDown = true;
                    SGF.Debuger.Log("模拟器输入 左键 点击滑动 开始----------------");

                    On_Tap?.Invoke(mousePosition);
                    On_Swipe_Start?.Invoke(mousePosition);
                    mHoldingTime = 0;
                    if (UnityEngine.Time.realtimeSinceStartup - mLastTapTime <= 0.5f)
                    {
                        On_DoubleTap?.Invoke(mousePosition);
                    }
                    mLastTapTime = UnityEngine.Time.realtimeSinceStartup;
                }

                ModuleManager.Instance.SendMessage(ModuleDef.Name.PlayerPreviewModule, "ClosePlayerFuncTips", new object[] { });
            }
            if (UnityEngine.Input.GetMouseButtonUp(0) && !isDownScrollWheel)
            {
                isDown = false;
                SGF.Debuger.LogError("模拟器输入 左键 点击滑动 结束----------------");
                On_Up?.Invoke(mousePosition);
                On_Swipe_End?.Invoke(mousePosition);
                mLastInputTime = UnityEngine.Time.realtimeSinceStartup;
            }
            if (isDown && !isDownScrollWheel)
            {
                SGF.Debuger.LogWarning("模拟器输入 左键 点击滑动 中-------------------");
                mLastInputTime = UnityEngine.Time.realtimeSinceStartup;
                mHoldingTime += UnityEngine.Time.deltaTime;
                On_Press?.Invoke(mousePosition, mHoldingTime);
                //if (CheckGuiRaycastObjects())
                if (IsForbidInpitTouch())
                {
                    On_Swipe_End?.Invoke(mousePosition);
                }
                else
                {
                    On_Swipe?.Invoke(mousePosition, mHoldingTime);
                }
            }

            //if (UnityEngine.Input.GetMouseButtonDown(1))
            //{
            //    SGF.Debuger.Log($"模拟器输入 右键 开始----------------pos={mousePosition}");
            //}
            //if (UnityEngine.Input.GetMouseButtonUp(1))
            //{
            //    SGF.Debuger.LogError($"模拟器输入 右键 结束----------------pos={mousePosition}");
            //}

            //if (UnityEngine.Input.GetMouseButtonDown(2))
            //{
            //    SGF.Debuger.Log($"模拟器输入 中键 开始----------------");

            //    if (!isDownScrollWheel)
            //    {
            //        isDownScrollWheel = true;
            //    }
            //}
            //if (UnityEngine.Input.GetMouseButtonUp(2))
            //{
            //    isDownScrollWheel = false;
            //    SGF.Debuger.LogError($"模拟器输入 中键 结束----------------");
            //}

            //if (isDownScrollWheel)
            //{
            //    float axis = UnityEngine.Input.GetAxis("Mouse ScrollWheel");
            //    SGF.Debuger.LogWarning($"模拟器输入 中间键 滑动中----------------axis={axis}");
            //    On_ScrollWheel?.Invoke(axis);
            //}
        }

        public bool CheckGuiRaycastObjects()
        {
#if (UNITY_ANDROID || UNITY_IPHONE) && !UNITY_EDITOR
if (EventSystem.current.IsPointerOverGameObject(UnityEngine.Input.GetTouch(0).fingerId))
            {
            return true;
            }
#else
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return true;
            }
#endif

            return false;
        }

        //全局检测关闭ItemTips核心函数参数
        private PointerEventData _uiPointerEventData;
        private List<RaycastResult> _uiRaycastResultCache = new();

        /// <summary>
        /// 全局检测关闭ItemTips核心函数
        /// </summary>
        /// <param name="screenPos"></param>
        /// <returns></returns>
        public GameObject GetFirstTouchUI(Vector2 screenPos)
        {
            if (null == EventSystem.current)
            {
                return null;
            }

            if (_uiPointerEventData == null)
            {
                _uiPointerEventData = new PointerEventData(EventSystem.current);
            }

            _uiPointerEventData.position = new Vector2(screenPos.x, screenPos.y);

            EventSystem.current.RaycastAll(_uiPointerEventData, _uiRaycastResultCache);

            for (int i = 0; i < _uiRaycastResultCache.Count; i++)
            {
                return _uiRaycastResultCache[i].gameObject;
            }
            return null;
        }


        private void CheckItemTips()
        {
            //全局检测关闭ItemTips
            if (GameManager.Instance.IsOpenItemTips)
            {
                var tr = GetFirstTouchUI(UnityEngine.Input.mousePosition);
                if (tr != null)
                {
                    // 如果当前UI是聊天文本
                    LinkOpener linkOpener = tr.GetComponent<LinkOpener>();
                    if (linkOpener != null)
                    {
                        // 判断是否有道具的链接
                        bool isHaveItemLinks = linkOpener.GetIsHaveItemLinks();
                        if (isHaveItemLinks)
                        {
                            return;
                        }
                        //CameraBase cameraBase = CameraManager.Instance.GetCamera(E_CameraType.UICam);
                        //int linkIndex = TMP_TextUtilities.FindIntersectingLink(pTextMeshPro, eventData.position, cameraBase.Camera);
                    }
                    JButton jbtn = tr.GetComponent<JButton>();
                    if (jbtn == null || (jbtn != null && !jbtn.DontCloseTips))
                    {
                        if (GameManager.Instance.IsOpenItemTips)
                        {
                            ModuleManager.Instance.SendMessage(ModuleDef.Name.ItemTipsModule, "OnCloseTips", new object[] { });
                        }
                    }
                }
                // 我点击到了tips本身也给我关掉了
                else
                {
                    if (GameManager.Instance.IsOpenItemTips)
                    {
                        ModuleManager.Instance.SendMessage(ModuleDef.Name.ItemTipsModule, "OnCloseTips", new object[] { });
                    }
                }
            }

            //全局检测关闭功能Tips
            if (GameManager.Instance.IsOpenFuncTips)
            {
                var tr = GetFirstTouchUI(UnityEngine.Input.mousePosition);
                if (tr != null)
                {
                    JButton jbtn = tr.GetComponent<JButton>();
                    if (jbtn == null || (jbtn != null && !jbtn.DontCloseTips))
                    {
                        if (GameManager.Instance.IsOpenFuncTips)
                        {
                            ModuleManager.Instance.SendMessage(ModuleDef.Name.FuncTipsModule, "OnCloseTips", new object[] { });
                        }
                    }
                }
                // 我点击到了tips本身也给我关掉了
                else
                {
                    if (GameManager.Instance.IsOpenFuncTips)
                    {
                        ModuleManager.Instance.SendMessage(ModuleDef.Name.FuncTipsModule, "OnCloseTips", new object[] { });
                    }
                }
            }

        }

        /// <summary>
        /// TODO:封装成指令集
        /// </summary>
        private void WindowsPlatforms()
        {
            Vector3 mousePosition = UnityEngine.Input.mousePosition;

            //一次生命周期开始
            if (UnityEngine.Input.GetMouseButtonUp(0))
            {
                isDown = false;
                On_Up?.Invoke(mousePosition);
                On_Swipe_End?.Invoke(mousePosition);
                mLastInputTime = UnityEngine.Time.realtimeSinceStartup;
            }

            if (UnityEngine.Input.GetMouseButtonDown(0))
            {
                //全局检测关闭ItemTips
                CheckItemTips();
                {
                    On_ClickScreen?.Invoke();
                    GlobalEvent.OnClickScreen.Invoke("ClickScreen");
                    if (CheckGuiRaycastObjects())
                    {
                        return;
                    }

                    isDown = true;

                    On_Tap?.Invoke(mousePosition);
                    On_Swipe_Start?.Invoke(mousePosition);
                    mHoldingTime = 0;
                    if (UnityEngine.Time.realtimeSinceStartup - mLastTapTime <= 0.5f)
                    {
                        On_DoubleTap?.Invoke(mousePosition);
                    }
                    mLastTapTime = UnityEngine.Time.realtimeSinceStartup;
                }

                ModuleManager.Instance.SendMessage(ModuleDef.Name.PlayerPreviewModule, "ClosePlayerFuncTips", new object[] { });
            }

            if (isDown)
            {
                mLastInputTime = UnityEngine.Time.realtimeSinceStartup;
                mHoldingTime += UnityEngine.Time.deltaTime;
                On_Press?.Invoke(mousePosition, mHoldingTime);
                //if (CheckGuiRaycastObjects())
                if (IsForbidInpitTouch())
                {
                    On_Swipe_End?.Invoke(mousePosition);
                }
                else
                {
                    On_Swipe?.Invoke(mousePosition, mHoldingTime);
                }
            }

            //一次生命周期结束  一次生命周期内容易出现进和出，自己挡住自己|通过传入位置，和scale都可以避免但是要逻辑正确
            //一次生命周期结束  一次生命周期内容易出现进和出，自己挡住自己|通过传入位置，和scale都可以避免但是要逻辑正确
            float axis = UnityEngine.Input.GetAxis("Mouse ScrollWheel");
            if (axis != 0)
            {
                if (!IsForbidInpitTouch())
                {
                    //SGF.Debuger.LogWarning($"模拟器输入 中间键 滑动中----------------axis={axis}");
                    On_ScrollWheel?.Invoke(axis);
                }
            }

            //{
            //    var key = KeyCode.LeftControl;
            //    if (UnityEngine.Input.GetKey(key) && !IsForbidInpitTouch())
            //    {
            //        SGF.Debuger.Log($"模拟器输入 中键 开始----------------pos={mousePosition}");
            //        On_EmulatorMouseSacleCameraDistance?.Invoke(mousePosition);
            //    }
            //}

            // 监听--鼠标右键
            if (UnityEngine.Input.GetMouseButtonUp(1))
            {
                On_MouseRightUp?.Invoke();
                DispatchVKey((int)KeyCode.Mouse0, 0);
                isMouseRightDown = false;
            }
            if (UnityEngine.Input.GetMouseButtonDown(1))
            {
                if (!isMouseRightDown)
                {
                    On_MouseRightDown?.Invoke();
                    DispatchVKey((int)KeyCode.Mouse0, 1);
                    isMouseRightDown = true;
                }
            }
        }

        private void OnTapEventHandler(Vector3 position)
        {
            OnSelectTarget(position);
            // 通知主角 点击了 屏幕 某个坐标(后面 曲爷 会把 业务层逻辑 从InputManager 中 挪开)
            if (M_MainPlayerCtrlBase == null || M_MainPlayerCtrlBase.Data == null)
            {
                return;
            }
            M_MainPlayerCtrlBase.Data.TriggerChange(GameConfig.TOUCH_EVENT, position);
        }

        #endregion

        #region 虚拟按键

        /// <summary>
        /// 键盘输入
        /// </summary>
        private void OnKeyBoard()
        {
            // 移动按键输入
            //多平台如何处理,如果两个输入都开着，我两部分代码也都开着
            //HandleInputKey(KeyCode.Joystick1Button0, GameVKey.JoystickMoveX, -1);
            HandleInputKey(KeyCode.A, GameVKey.MoveX_CMD, -1);
            HandleInputKey(KeyCode.D, GameVKey.MoveX_CMD, 1);
            HandleInputKey(KeyCode.W, GameVKey.MoveZ_CMD, 1);
            HandleInputKey(KeyCode.S, GameVKey.MoveZ_CMD, -1);
            //===============================================================================
            HandleInputKey(KeyCode.None, GameVKey.MoveX_CMD, joyMoveDirection.x);
            // 技能按钮 键盘输入 
            HandleInputKey(KeyCode.Space, GameVKey.Skill, 1);
            HandleInputKey(KeyCode.Q, GameVKey.Skill, 1);
            HandleInputKey(KeyCode.E, GameVKey.Skill, 1);
            HandleInputKey(KeyCode.R, GameVKey.Skill, 1);
            HandleInputKey(KeyCode.T, GameVKey.Skill, 1);
            HandleInputKey(KeyCode.F, GameVKey.Skill, 1);
            HandleInputKey(KeyCode.Z, GameVKey.Skill, 1);
            HandleInputKey(KeyCode.X, GameVKey.Skill, 1);
            HandleInputKey(KeyCode.C, GameVKey.Skill, 1);
            // 测试键盘输入的
            HandleInputKey(KeyCode.K, GameVKey.Test, 1);
            HandleInputKey(KeyCode.L, GameVKey.Test, 1);
            HandleInputKey(KeyCode.J, GameVKey.Test, 1);
            HandleInputKey(KeyCode.M, GameVKey.Test, 1);
            HandleInputKey(KeyCode.V, GameVKey.Test, 1);
            HandleInputKey(KeyCode.B, GameVKey.Test, 1);
            HandleInputKey(KeyCode.G, GameVKey.Test, 1);
            HandleInputKey(KeyCode.P, GameVKey.Test, 1);
            HandleInputKey(KeyCode.F5, GameVKey.Test, 1);
            HandleInputKey(KeyCode.F2, GameVKey.Test, 1);
            HandleInputKey(KeyCode.F3, GameVKey.Test, 1);
            HandleInputKey(KeyCode.F4, GameVKey.Test, 1);
            HandleInputKey(KeyCode.F6, GameVKey.Test, 1);
            HandleInputKey(KeyCode.O, GameVKey.Test, 1);
        }

        /// <summary>
        /// 对【虚拟按键】的处理，将其通过事件回调，抛给监听者
        /// 鼠标、体感、键盘、按钮、手柄
        /// 此方法可添加统一的判断逻辑
        /// </summary>
        /// <param name="vkey">Vkey.</param>
        /// <param name="arg">Argument.</param>
        public bool DispatchVKey(int vkey, float arg, bool isForce = false)
        {
            mLastInputTime = UnityEngine.Time.realtimeSinceStartup;
            // 添加判断--是否能下发
            if (M_MainPlayerCtrlBase != null)
            {
                if (!isForce)
                {
                    if (!M_MainPlayerCtrlBase.GetInterCanBreak())
                    {
                        // 如果是手机的操作：遥感的值要归0
                        //#if UNITY_ANDROID || UNITY_IOS || UNITY_IPHONE
                        if (vkey == GameVKey.MoveX_CMD || vkey == GameVKey.MoveZ_CMD)
                        {
                            OnVirtualInput?.Invoke(vkey, 0);
                        }
                        //#endif
                        Frame.Util.ShowSystemMessage(Language.LanguageManager.Instance.GetLanguageByKey("BattleBtnErrTips"));
                        return false;
                    }

                    //打断制造
                    M_MainPlayerCtrlBase.BreakCreate();

                    //有任意page/window活跃就取消信息发送WSAD 
                    if (UIManager.Instance.GetIsHasAnyWindowOpened())
                    {
                        // 如果是手机的操作：遥感的值要归0
                        //#if UNITY_ANDROID || UNITY_IOS || UNITY_IPHONE
                        if (vkey == GameVKey.MoveX_CMD || vkey == GameVKey.MoveZ_CMD)
                        {
                            OnVirtualInput?.Invoke(vkey, 0);
                        }
                        //#endif
                        return false;
                    }
                    //当前在播放tiemline时，禁止主角移动
                    if (TimelineManager.Instance.IsPlayTimeLine)
                    {
                        // 如果是手机的操作：遥感的值要归0
                        //#if UNITY_ANDROID || UNITY_IOS || UNITY_IPHONE
                        if (vkey == GameVKey.MoveX_CMD || vkey == GameVKey.MoveZ_CMD)
                        {
                            OnVirtualInput?.Invoke(vkey, 0);
                        }
                        //#endif
                        return false;
                    }
                    ///用户输入时打断寻路
                    if (M_MainPlayerCtrlBase is PlayerCtrlGroup player)
                    {
                        // 这里有问题，如果技能安不出来，释放不了就不应该打断
                        player.BreakFollowDynamicEnity();
                        player.BreakFindPath();

                        if (vkey == GameVKey.MoveX_CMD || vkey == GameVKey.MoveZ_CMD)
                        {
                            // 方向摇杆 按下拖动
                            GameManager.Instance.TriggerEvent("AnalogStick_Scrolling", null);
                        }
                    }
                    // 切换地图/副本的时候禁止
                    if (!GameManager.Instance.GetMapChanageIsSuccess())
                    {
                        // 如果是手机的操作：遥感的值要归0
                        //#if UNITY_ANDROID || UNITY_IOS || UNITY_IPHONE
                        if (vkey == GameVKey.MoveX_CMD || vkey == GameVKey.MoveZ_CMD)
                        {
                            OnVirtualInput?.Invoke(vkey, 0);
                        }
                        //#endif
                        Frame.Util.ShowSystemMessage(Language.LanguageManager.Instance.GetLanguageByKey("BattleBtnErrTips2"));
                        return false;
                    }
                }
            }

            // 所有【虚拟按键】的操作，统一抛给监听者
            // 鼠标、体感、键盘、按钮、手柄
            OnVirtualInput?.Invoke(vkey, arg);
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key">pc上的按键</param>
        /// <param name="press_vkey">映射的程序指令集</param>
        /// <param name="press_arg">【key】映射到指令集上【press_arg】需要传递的参数</param>
        private void HandleInputKey(KeyCode key, int press_vkey, float press_arg)
        {
            switch (press_vkey)
            {
                case GameVKey.MoveX_CMD:
                case GameVKey.MoveZ_CMD:
                    {
                        HandleMoveKey(key, press_vkey, press_arg);
                    }
                    break;
                case GameVKey.Skill:
                    {
                        HandleSkillKey(key, press_vkey, press_arg);
                    }
                    break;
                case GameVKey.Test:
                    {
#if STAR_DEV|| UNITY_EDITOR
                        HandleTestKey(key, press_vkey, press_arg);
#endif
                    }
                    break;
                default:
                    break;
            }

        }

        private void HandleMoveKey(KeyCode key, int press_vkey, float press_arg)
        {
            if (key == KeyCode.None)//这里none是主动传进来的，不产生keyBoard触碰状态的歧义
            {
                if (onJoyMoveUpEvent)
                {
                    //遥感
                    DispatchVKey(GameVKey.MoveX_CMD, joyMoveDirection.x);
                    DispatchVKey(GameVKey.MoveZ_CMD, joyMoveDirection.z);
                    onJoyMoveUpEvent = false;
                    joyMoveDirection = Vector3.zero;
                }
            }
            else if (isJoystickMove == false)
            {
                if (UnityEngine.Input.GetKey(key))
                {
                    // 按下
                    if (!m_MapKeyState.ContainsKey(key) || !m_MapKeyState[key])
                    {
                        m_MapKeyState[key] = true;
                        // 查看反对应的key是否【按下】了
                        KeyCode inversionKey = KeyCode.None;
                        if (m_InversionKeyDic.TryGetValue(key, out inversionKey))
                        {

                        }
                        if (inversionKey != KeyCode.None && m_MapKeyState[inversionKey])
                        {
                            isKeyDown = DispatchVKey(press_vkey, 0);
                        }
                        else
                        {
                            isKeyDown = DispatchVKey(press_vkey, press_arg);
                        }
                        mLastInputTime = UnityEngine.Time.realtimeSinceStartup;
                    }
                }
                else
                {
                    // 单次处理抬起
                    OnBoardKeyUp(key, press_vkey, press_arg);
                }
            }

        }

        /// <summary>
        /// 全部处理抬起
        /// </summary>
        public void ClearMoveCommand()
        {
            On_Swipe_End?.Invoke(Vector3.zero);
            On_DoubleFingersEnd?.Invoke(Vector3.zero, Vector3.zero);

            if (m_MapKeyState == null || m_MapKeyState.Count <= 0)
            {
                return;
            }
            //触发一次移动的全部释放
            var list = m_MapKeyState.KToList();
            for (int i = 0; i < list.Count; i++)
            {
                var item = list[i];
                if (item.Value == true)
                {
                    if (item.Key == KeyCode.A || item.Key == KeyCode.D)
                    {
                        OnBoardKeyUp(item.Key, GameVKey.MoveX_CMD, 0, true);
                    }
                    else if (item.Key == KeyCode.W || item.Key == KeyCode.S)
                    {
                        OnBoardKeyUp(item.Key, GameVKey.MoveZ_CMD, 0, true);
                    }
                }
            }
            m_MapKeyState.Clear();
        }

        /// <summary>
        /// 记录所有key然后清理
        /// </summary>
        /// <param name="key"></param>
        /// <param name="press_vkey"></param>
        /// <param name="press_arg"></param>
        private void OnBoardKeyUp(KeyCode key, int press_vkey, float press_arg, bool isForce = false)
        {
            if (m_MapKeyState[key])
            {
                m_MapKeyState[key] = false;
                // 查看反对应的key是否【抬起】了
                KeyCode inversionKey = KeyCode.None;
                if (m_InversionKeyDic.TryGetValue(key, out inversionKey))
                {

                }
                if (inversionKey != KeyCode.None && m_MapKeyState[inversionKey])
                {
                    DispatchVKey(press_vkey, -press_arg, isForce);
                }
                else
                {
                    DispatchVKey(press_vkey, 0, isForce);
                }
                if (isKeyDown)
                {
                    isKeyDown = !GetAllKeyUp();
                }
            }
        }

        /// <summary>
        /// 1，键盘移动ok
        /// 2，遥感移动ok
        /// 3，键盘移动，遥感会高优先ok
        /// 4，遥感触发时候，键盘不生效ok
        /// </summary>
        /// <param name="key"></param>
        /// <param name="press_vkey"></param>
        /// <param name="press_arg"></param>
        private void HandleSkillKey(KeyCode key, int press_vkey, float press_arg)
        {
            // 技能输入按键
            if (UnityEngine.Input.GetKey(key))
            {
                if (!m_MapSkillKeyState.ContainsKey(key) || !m_MapSkillKeyState[key])
                {
                    m_MapSkillKeyState[key] = true;
                    DispatchVKey((int)key, press_arg);
                }
                //有任意page/window活跃就取消信息发送WSAD 
                if (UIManager.Instance.GetIsHasAnyWindowOpened())
                {
                    DispatchVKey((int)key, -1, true);
                }
                //当前在播放tiemline时，禁止主角移动
                if (TimelineManager.Instance.IsPlayTimeLine)
                {
                    DispatchVKey((int)key, -1, true);
                }
                // 切换地图/副本的时候禁止
                if (!GameManager.Instance.GetMapChanageIsSuccess())
                {
                    //SGF.Debuger.LogError($"状态切换 HandleSkillKey ---------------------------------------------------- 切换地图/副本的时候禁止");
                    DispatchVKey((int)key, -1, true);
                }
            }
            else
            {
                if (m_MapSkillKeyState.ContainsKey(key) && m_MapSkillKeyState[key])
                {
                    m_MapSkillKeyState[key] = false;
                    DispatchVKey((int)key, 0);
                }
            }
        }

        private void HandleTestKey(KeyCode key, int press_vkey, float press_arg)
        {
            if (UnityEngine.Input.GetKey(key))
            {
                if (!m_TestKeyState.ContainsKey(key) || !m_TestKeyState[key])
                {
                    m_TestKeyState[key] = true;
#if QU_GM
                    if (key == KeyCode.F2)
                    {
                        SocketBase battleSocket = NetworkManager.Instance.gameSocket;
                        ProtoMsg.GmCmdReq gmCmdReq = new();
                        string msg = $"KillAllMon 1";
                        gmCmdReq.Cmd = msg;
                        battleSocket.SendRPCMsg(ServerType.ServerTypeLobby, gmCmdReq, isAutoChangeMsgTarget: false);
                    }
                    if (key == KeyCode.F3)
                    {
                        SocketBase battleSocket = NetworkManager.Instance.gameSocket;
                        ProtoMsg.GmCmdReq gmCmdReq = new();
                        string msg = $"GMCD 1";
                        gmCmdReq.Cmd = msg;
                        battleSocket.SendRPCMsg(ServerType.ServerTypeLobby, gmCmdReq, isAutoChangeMsgTarget: false);

                        //DisplayProcessDispenser.Instance.AddAchievementMessage(LanguageManager.Instance.GetLanguageByKey("PosUnreachable"));

                        //var list = new List<ItemMD>();
                        //ItemMD itemMD = new() { BaseID = 2, Num = 2 };
                        //list.Add(itemMD);

                        //if (list.Count > 0)
                        //{
                        //    ModuleManager.Instance.SendMessage(ModuleDef.Name.RewardsPopModule, "OnOpenRewardsPop", list);
                        //}
                    }
                    if (key == KeyCode.F4)
                    {
                        SocketBase battleSocket = NetworkManager.Instance.gameSocket;
                        ProtoMsg.GmCmdReq gmCmdReq = new();
                        string msg = $"SetGod 1";
                        gmCmdReq.Cmd = msg;
                        battleSocket.SendRPCMsg(ServerType.ServerTypeLobby, gmCmdReq, isAutoChangeMsgTarget: false);
                    }
                    if (key == KeyCode.F6)
                    {
                        SocketBase battleSocket = NetworkManager.Instance.gameSocket;
                        ProtoMsg.GmCmdReq gmCmdReq = new();
                        string msg = $"AddProp 3026 1000";
                        gmCmdReq.Cmd = msg;
                        battleSocket.SendRPCMsg(ServerType.ServerTypeLobby, gmCmdReq, isAutoChangeMsgTarget: false);
                        UIQueueManager.Instance.GMLogUIQueue();
                    }
                    if (key == KeyCode.K)
                    {
                        SocketBase battleSocket = NetworkManager.Instance.gameSocket;
                        ProtoMsg.GmCmdReq gmCmdReq = new();
                        string msg = $"EnterScene 4";
                        gmCmdReq.Cmd = msg;
                        battleSocket.SendRPCMsg(ServerType.ServerTypeLobby, gmCmdReq, isAutoChangeMsgTarget: false);
                    }
#else
                    if (key == KeyCode.F2)
                    {
                        //SocketBase battleSocket = NetworkManager.Instance.gameSocket;
                        //ProtoMsg.GmCmdReq gmCmdReq = new();
                        //string msg = $"EnterScene 4";
                        //gmCmdReq.Cmd = msg;
                        //battleSocket.SendRPCMsg(ServerType.ServerTypeLobby, gmCmdReq, isAutoChangeMsgTarget: false);

                        StarWorldModule starWorld = (StarWorldModule)ModuleManager.Instance.GetModule(ModuleDef.Name.StarWorldModule);
                        // 根据ID判断是那个副本类型
                        int mmpID = 4;
                        SpaceType spaceType = SpaceType.SpaceScene;
                        starWorld.SendFBChangeReq(ChangeReason.SameServer, mmpID, spaceType, 0, 0);
                    }
#endif
                    if (key == KeyCode.F5)
                    {
                        UIManager.Instance.OpenWidgetAsync(UIDef.GmWidget, null, true, null, UIRoot.UIROOT.transform, MainPageCommond.HideNone, true, true);
                    }
                }
            }
            else
            {
                if (m_TestKeyState.ContainsKey(key) && m_TestKeyState[key])
                {
                    m_TestKeyState[key] = false;
                }
            }
        }

        private bool GetAllKeyUp()
        {
            foreach (var item in m_MapKeyState)
            {
                if (item.Value)
                {
                    // 有一个按键有值（继续按着）
                    return false;
                }
            }
            return true;
        }


        private Vector3 joyMoveDirection = Vector3.zero;
        private bool onJoyMoveUpEvent = false;
        /// <summary>1111
        /// [他触发会update持续给我值；他抬起会告诉我一次Vector.zero（触碰不可能完全是0）];平时不会调用我
        /// 触摸的触发频率 和 update不同
        /// </summary>
        /// <param name="direction"></param>
        public void OnJoystickMove(Vector3 direction)
        {
            if (isKeyDown && joyMoveDirection != Vector3.zero)
            {
                joyMoveDirection = Vector3.zero;
                return;
            }
            isJoystickMove = direction.magnitude > 0.0001f;
            if (isJoystickMove)
            {
                joyMoveDirection = direction;
            }
            else
            {
                joyMoveDirection = Vector3.zero;
            }
            //小圆范围内，活抬起
            onJoyMoveUpEvent = true;
        }

        #endregion

        #region 摄像机选中
        private void OnSelectTarget(Vector3 position)
        {
            if (CameraManager.Instance == null)
            {
                return;
            }

            if (CameraManager.Instance.CurrentPlayCamera == null)
            {
                return;
            }

            if (GameInput.GetIsTouchDown)
            {
                return;
            }

            Ray ray = CameraManager.Instance.CurrentPlayCamera.Camera.ScreenPointToRay(position);
            RaycastHit raycast;
            if (Physics.Raycast(ray, out raycast, maxDistance, layerMask))
            {
                // Debug.DrawLine(ray.origin, ray.direction * maxDistance, Color.red);
                string tag = raycast.collider.tag;
                if (tag.Contains(E_TagType.Enemy.ToString()) || tag.Contains(E_TagType.Player.ToString()) || tag.Contains(E_TagType.NPC.ToString()) || tag.Contains(E_TagType.MainPlayer.ToString()))
                {
                    ViewVitalNPCNormal npc = raycast.collider.gameObject.GetComponent<Game.Entity.View.VitalSign.ViewVitalNPCNormal>();
                    if (npc != null && npc.EntityID > 0)
                    {
                        npc.M_EntityBase.OnSelected();

                        /*

                        //以下为工会测试代码(要加入各种限制)
                        if(npc.M_EntityBase.EntityType== E_EntityType.Player)
                        {
                            var inviteGuildReq = new ProtoMsg.InviteGuildReq();
                            inviteGuildReq.BeInvEid = npc.EntityID;

                           SGF.Network.NetworkManager.Instance.gameSocket.SendRPCMsg(SGF.Network.ServerType.ServerTypeLobby,
                            inviteGuildReq, false);
                        }

                        */
                    }
                }
            }
        }

        public void RegisterClick(OnTapHandler handler)
        {
            On_Tap += handler;
        }
        #endregion

    }
}
