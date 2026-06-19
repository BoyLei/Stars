using SGF.Module.Framework;
using StarProjectDef;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


namespace StarProject.StarFramework.UI.Extend
{
    [XLua.LuaCallCSharp]
    public class ScrollViewHUDChatExtend : ScrollRect
    {
        private CanvasGroup ScrollViewCanvasGroup;

        private bool IsShow = true;
        private bool IsClickOpenTips = false;
        public bool isDrag = false;
        private float Countdown = 5;

        protected override void Awake()
        {
            ScrollViewCanvasGroup = GetComponent<CanvasGroup>();

            // 添加按钮点击事件
            {
                {
                    var btn = transform.Find("Viewport/Btn").GetComponent<Button>();
                    btn.onClick.RemoveListener(OnBtnClick);
                    btn.onClick.AddListener(OnBtnClick);
                }
            }

            GlobalEvent.OnChatHudShowTipsEvent.AddListener(OnChatHudShowTipsEventAction);
            GlobalEvent.OnTipsCloseEvent.AddListener(OnTipsCloseEventAction);
        }

        //protected void FixedUpdate()
        //{
        //    // 如果一直没有消息的话，5秒钟就隐藏吧
        //    if (!IsClickOpenTips)
        //    {
        //        if (!isDrag)
        //        {
        //            if (Countdown > 0)
        //            {
        //                Countdown -= Time.fixedDeltaTime;
        //                if (Countdown <= 0)
        //                {
        //                    SetShow(false);
        //                }
        //            }
        //        }
        //    }
        //}

        protected override void OnDestroy()
        {
            GlobalEvent.OnChatHudShowTipsEvent.RemoveListener(OnChatHudShowTipsEventAction);
            GlobalEvent.OnTipsCloseEvent.RemoveListener(OnTipsCloseEventAction);
        }

        public override void OnBeginDrag(PointerEventData eventData)
        {
            base.OnBeginDrag(eventData);
        }

        public override void OnDrag(PointerEventData eventData)
        {
            base.OnDrag(eventData);
            if (normalizedPosition.y >= 0.01 && !isDrag)
            {
                SetDragState(true);
            }
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            base.OnEndDrag(eventData);
            if (normalizedPosition.y <= 0.01 && isDrag)
            {
                SetDragState(false);
            }
        }

        private void OnBtnClick()
        {
            // 打开聊天频道界面
            ModuleManager.Instance.SendMessage(ModuleDef.Name.ChatModule, "OnChatHudShow", new object[] { true });
        }

        private void SetDragState(bool Drag)
        {
            isDrag = Drag;
            if (!isDrag)
            {
                Countdown = 5;
            }
        }

        public void SetShow(bool isShow)
        {
            bool show = isShow && Countdown > 0;
            if (show != IsShow)
            {
                IsShow = show;
                ScrollViewCanvasGroup.alpha = IsShow ? 1 : 0;
                ScrollViewCanvasGroup.blocksRaycasts = IsShow;
            }
        }

        public void SetCountdown(float time)
        {
            Countdown = time;
        }
        private void OnChatHudShowTipsEventAction(object arg0)
        {
            IsClickOpenTips = true;
        }

        private void OnTipsCloseEventAction(object arg0)
        {
            IsClickOpenTips = false;
        }





    }
}
