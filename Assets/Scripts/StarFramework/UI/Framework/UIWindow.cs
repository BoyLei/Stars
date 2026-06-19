using StarProjectDef;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XLua;

namespace SGF.UI.Framework
{
    [LuaCallCSharp]
    public class UIWindow : UIPanel
    {
        public override E_UI_TYPE UIType => E_UI_TYPE.Window;

        //=======================================================================

        public delegate void CloseEvent(object arg = null);
        private bool _onCloseDestroy = true;
        //=======================================================================
        public override bool onCloseDestroy { get { return _onCloseDestroy; } set { _onCloseDestroy = value; } }
        //=======================================================================
        /// <summary>
        /// 关闭按钮，大部分窗口都会有关闭按钮
        /// </summary>
        [SerializeField]
        private Button m_btnClose;

        /// <summary>
        /// JButton关闭按钮，大部分窗口都会有关闭按钮
        /// </summary>
        [SerializeField]
        private JButton m_jBtnClose;


        /// <summary>
        /// 窗口关闭事件
        /// </summary>
        public event CloseEvent onClose;

        /// <summary>
        /// 打开UI的参数
        /// </summary>
        protected object m_openArg;

        /// <summary>
        /// 该UI的当前实例是否曾经被打开过
        /// </summary>
        private bool m_isOpenedOnce;

        public System.Action CloseAction;
        public UnityEngine.Events.UnityAction<GameObject> IntroAction;

        /// <summary>
        /// Window 打开过的Widgets
        /// </summary>
        private List<string> WindowWidgets = new List<string>();

        private Dictionary<string, WidgetInfo> CachewWidgets = new Dictionary<string, WidgetInfo>();

        public int Index { get; private set; }

        public string parentWin;

        [Sirenix.OdinInspector.LabelText("玩法介绍按钮")]
        [SerializeField]
        private JButton m_IntroButton;
        protected System.Action<bool> SetCloseDestroyAction;//
        public void SetCloseDestroy(bool close)
        {
            onCloseDestroy = close;
        }

        /// <summary>
        /// /清理记录
        /// </summary>
        public void ClearWidget()
        {
            WindowWidgets.Clear();
            CachewWidgets.Clear();
            Index = 0;
        }

        //string name, bool SetAsFirstSibling = false, object arg = null, Transform parent = null, MainPageCommond _mainPageCommond = MainPageCommond.HideNone, bool isInHudWhiteList = false,bool isTopWidget = false
        /// <summary>
        /// 添加Widget
        /// </summary>
        /// <param name="WidgetName"></param>
        public void AddWidget(string WidgetName, WidgetInfo info)
        {
            int index = WindowWidgets.IndexOf(WidgetName);
            if (index == -1)
            {
                WindowWidgets.Add(WidgetName);
                CachewWidgets.Add(WidgetName, info);
                Index = WindowWidgets.Count - 1;
            }
            else
            {
                int count = WindowWidgets.Count;
                for (int i = count - 1; i > 0; i--)
                {
                    if (i == index)
                    {
                        break;
                    }
                    CachewWidgets.Remove(WindowWidgets[i]);
                    WindowWidgets.RemoveAt(i);
                }
                Index = index;
            }
        }

        /// <summary>
        /// 返回上一层
        /// </summary>
        /// <returns>
        /// 返回 true 可以关闭 窗口，返回 false 不可以关闭窗口
        /// </returns>
        public bool BackWidget()
        {
            if (Index > 0)
            {
                CachewWidgets.Remove(WindowWidgets[Index]);

                WindowWidgets.RemoveAt(Index);
            }
            Index = Index - 1;
            return Index < 0;
        }

        public WidgetInfo GetWidgetInfo()
        {
            if (Index > -1 && Index < WindowWidgets.Count)
            {
                return CachewWidgets[WindowWidgets[Index]];
            }
            return null;
        }

        public string GetWidgetName()
        {
            if (Index > -1 && Index < WindowWidgets.Count)
            {
                return WindowWidgets[Index];
            }
            return string.Empty;
        }

        protected override void Awake()
        {
            base.Awake();
            IntroAction = DoBtnIntro;
            CloseAction = DoBtnClose;
            PlayAnimationAction = PlayAnimation;
            ForcePlayAnimationAction = ForcePlayAnimation;
            SetCloseDestroyAction = SetCloseDestroy;
        }
        /// <summary>
        /// 当UI可用时调用
        /// </summary>
        protected override void OnEnable()
        {
            this.Log("OnEnable()");
            if (m_btnClose != null)
            {
                m_btnClose.onClick.AddListener(DoBtnClose);
            }

            if (m_IntroButton != null && m_IntroButton.OnClick == null)
            {
                m_IntroButton.OnClick = IntroAction;
            }

            if (m_jBtnClose != null)
            {
                m_jBtnClose.OnClick += OnJBtnClose;
            }
        }

        /// <summary>
        /// 当UI不可用时调用
        /// </summary>
        protected override void OnDisable()
        {
            this.Log("OnDisable()");

            if (m_btnClose != null)
            {
                m_btnClose.onClick.RemoveAllListeners();
            }

            if (m_jBtnClose != null)
            {
                m_jBtnClose.OnClick -= OnJBtnClose;
            }
        }

        /// <summary>
        /// 当点击关闭按钮时调用
        /// 但是并不是每一个Window都有关闭按钮
        /// </summary>
        public void DoBtnClose()
        {
            this.Log("OnBtnClose()");
            UIManager.Instance.CloseWindow(Name);
            //Close(0);111
        }

        /// <summary>
        /// 玩法介绍按钮回调函数
        /// </summary>
        public void DoBtnIntro(GameObject go)
        {
            this.Log("DoBtnIntro()");
            UIAPI.ShowGameplayInfo(Name);
        }

        private void OnJBtnClose(GameObject go)
        {
            DoBtnClose();
        }

        /// <summary>
        /// 调用它打开UIWindow
        /// </summary>
        /// <param name="arg"></param>
        public sealed override void Open(object arg = null)
        {
            this.Log("Open() arg:{0}", arg);
            m_openArg = arg;
            m_isOpenedOnce = false;
            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }
            OnOpen(arg);
            m_isOpenedOnce = true;
        }

        /// <summary>
        /// 调用它以关闭UIWindow
        /// </summary>
        public sealed override void Close(object arg = null)
        {
            base.Close();
            this.Log("Close()");

            OnClose(arg);
            if (onClose != null)
            {
                onClose(arg);
                onClose = null;
            }

            UIWindowStack.onClose(this);

            if (gameObject.activeSelf)
            {
                if (onCloseDestroy)
                {
                    DestroyImmediate(gameObject);
                }
                else
                {
                    gameObject.SetActive(false);
                }

            }
        }

    }

    [LuaCallCSharp]
    public class WidgetInfo
    {
        public string name;
        public bool SetAsFirstSibling = false;
        public object arg = null;
        public Transform parent = null;
        public MainPageCommond _mainPageCommond = MainPageCommond.HideNone;
        public bool isInHudWhiteList = false;
        public bool isTopWidget = false;
        public bool isSceneWidget = false;
        public Action<UIWidget> cb = null;

    }
}
