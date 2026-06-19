using System;
using UnityEngine;
using UnityEngine.UI;
using XLua;
namespace SGF.UI.Framework
{
    [LuaCallCSharp]
    public class UIPage : UIPanel
    {
        private bool _onCloseDestroy = false;
        public override bool onCloseDestroy { get { return _onCloseDestroy; } set { _onCloseDestroy = value; } }
        public override E_UI_TYPE UIType => E_UI_TYPE.Page;

        /// <summary>
        /// 返回按钮，大部分Page都会有返回按钮
        /// </summary>
        [SerializeField]
        private Button m_btnGoBack;

        /// <summary>
        /// 打开UI的参数
        /// </summary>
        protected object m_openArg;

        /// <summary>
        /// 该UI的当前实例是否曾经被打开过
        /// </summary>
        private bool m_isOpenedOnce;
        protected System.Action<bool> SetCloseDestroyAction;

        protected override void Awake()
        {
            base.Awake();
            PlayAnimationAction = PlayAnimation;
            ForcePlayAnimationAction = ForcePlayAnimation;
            SetCloseDestroyAction = SetCloseDestroy;
        }


        /// <summary>
        /// 当UIPage被激活时调用
        /// </summary>
        protected override void OnEnable()
        {


            this.Log("OnEnable()");
            if (m_btnGoBack != null)
            {
                m_btnGoBack.onClick.AddListener(OnBtnGoBack);
            }

#if UNITY_EDITOR
            if (m_isOpenedOnce)
            {
                //如果UI曾经被打开过，
                //则可以通过UnityEditor来快速触发Open/Close操作
                //方便调试
                OnOpen(m_openArg);
            }
#endif
        }



        /// <summary>
        /// 当UI不可用时调用
        /// </summary>
        protected override void OnDisable()
        {
            this.Log("OnDisable()");
#if UNITY_EDITOR
            if (m_isOpenedOnce)
            {
                //如果UI曾经被打开过，
                //则可以通过UnityEditor来快速触发Open/Close操作
                //方便调试
                OnClose();
            }
#endif
            if (m_btnGoBack != null)
            {
                m_btnGoBack.onClick.RemoveAllListeners();
            }
        }


        /// <summary>
        /// 当点击“返回”时调用
        /// 但是并不是每一个Page都有返回按钮
        /// </summary>
        private void OnBtnGoBack()
        {
            this.Log("OnBtnGoBack()");
            UIManager.Instance.GoBackPage();
            //Worker/Meek
            //*Close,Open/-
            //*page-AllClose-Name
            //*11有控制才控制才缓存，开启，关闭，所以无需关心切换时候的指令缓存
        }

        /// <summary>
        /// 调用它打开UIPage
        /// </summary>
        /// <param name="arg"></param>
        public sealed override void Open(object arg = null)
        {
            this.Log("Open()");
            m_openArg = arg;
            m_isOpenedOnce = false;

            if (!this.gameObject.activeSelf)
            {
                this.gameObject.SetActive(true);
            }

            OnOpen(arg);
            m_isOpenedOnce = true;
        }
        public void SetCloseDestroy(bool close)
        {
            onCloseDestroy = close;
        }
        public sealed override void Close(object arg = null)
        {
            base.Close();//子类不能改写继承，不涉及base.close调用位置先后问题，都是直接调用；所有子类实现的都会走清理逻辑
            this.Log("Close()");
            /*【这里结构也是正确逻辑】【uimgr处理关闭确实，uimgr不会调用base.close如果不调用onclose自身释放大家白写了 】

            uimgr里面的清理
                     uimgrlist遍历  删除
                                page 引发子界面批量删除【自身可以清理是正确逻辑，重这里开始也是正确逻辑】
                                                                                                        控制到UImgrlist又改变了
            uimgr   原来调用dlose没问题是close没有处理uimgr 中list===现在必须延迟处理*/

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

            OnClose(arg);
        }

    }
}
