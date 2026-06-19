using UnityEngine;
using UnityEngine.UI;
using XLua;

namespace SGF.UI.Framework
{
    [LuaCallCSharp]
    public class UIWidget : UIPanel
    {
        public override E_UI_TYPE UIType => E_UI_TYPE.Widget;
        /// <summary>
        /// 打开UI的参数
        /// </summary>
        protected object m_openArg;
        //=======================================================================
        private bool _onCloseDestroy = false;
        public delegate void CloseEvent(object arg = null);//新增挂件关闭事件
        public override bool onCloseDestroy { get { return _onCloseDestroy; } set { _onCloseDestroy = value; } }
        protected System.Action<bool> SetCloseDestroyAction;
        //===========================================================
        /// <summary>
        /// 挂件移除事件(回调)
        /// </summary>
        public event CloseEvent onClose;

        [SerializeField]
        private Button m_btnClose;
        /// <summary>
        /// JButton关闭按钮，大部分窗口都会有关闭按钮
        /// </summary>
        [SerializeField]
        private JButton m_jBtnClose;


        [Sirenix.OdinInspector.LabelText("玩法介绍按钮")]
        [SerializeField]
        private JButton m_IntroButton;

        public System.Action CloseAction;
        public UnityEngine.Events.UnityAction<GameObject> IntroAction;


        protected override void Awake()
        {
            base.Awake();
            CloseAction = DoBtnClose;
            IntroAction = DoBtnIntro;
            SetCloseDestroyAction = SetCloseDestroy;
        }
        public void SetCloseDestroy(bool close)
        {
            onCloseDestroy = close;
        }
        /// <summary>
        /// 调用它打开UIWindow
        /// </summary>
        /// <param name="arg"></param>
        public sealed override void Open(object arg = null)
        {
            this.Log("Open() arg:{0}", arg);
            m_openArg = arg;
            if (!this.gameObject.activeSelf)
            {
                this.gameObject.SetActive(true);
            }

            if (m_btnClose != null)
            {
                m_btnClose.onClick.RemoveAllListeners();
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

            OnOpen(arg);
        }

        /// <summary>
        /// 当点击关闭按钮时调用
        /// 但是并不是每一个Window都有关闭按钮
        /// 封装一层吧
        /// </summary>
        public void DoBtnClose()
        {
            this.Log("OnBtnClose()");
            UIManager.Instance.CloseWidget(Name);
            //Close(0);
        }

        private void OnJBtnClose(GameObject go)
        {
            DoBtnClose();
        }

        /// <summary>
        /// 玩法介绍按钮回调函数
        /// </summary>
        public void DoBtnIntro(GameObject go)
        {
            this.Log("DoBtnIntro()");
            StarProjectDef.UIAPI.ShowGameplayInfo(Name);
        }

        /// <summary>
        /// 调用它以关闭UIWindow
        /// </summary>
        public sealed override void Close(object arg = null)
        {
            base.Close();
            if (m_btnClose != null)
            {
                m_btnClose.onClick.RemoveAllListeners();
            }

            if (m_jBtnClose != null)
            {
                m_jBtnClose.OnClick -= OnJBtnClose;
            }

            OnClose(arg);

            if (onClose != null)
            {
                onClose(arg);
                onClose = null;
            }

            this.Log("Close() arg:{0}", arg);

            if (this.gameObject.activeSelf)
            {
                if (onCloseDestroy)
                {
                    DestroyImmediate(gameObject);
                }
                else
                {
                    gameObject.SetActive(false);
                    SetSelfCanvasGroup(true);
                }
            }

        }

    }
}
