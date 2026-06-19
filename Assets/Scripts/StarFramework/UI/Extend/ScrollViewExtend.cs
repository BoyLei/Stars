using StarProjectDef;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


namespace StarProject.StarFramework.UI.Extend
{
    [XLua.LuaCallCSharp]
    public class ScrollViewExtend : ScrollRect
    {
        [SerializeField]
        public Button BtnDown;

        public E_ChatMsgTrackState e_ChatMsgTrackState = E_ChatMsgTrackState.LockToNew;
        public bool isDrag = false;

        private ContentSizeFitter m_ContentContentSizeFitter;
        private ContentSizeFitter M_ContentContentSizeFitter
        {
            get
            {
                if (m_ContentContentSizeFitter == null)
                {
                    m_ContentContentSizeFitter = this.content.GetComponent<ContentSizeFitter>();
                }
                return m_ContentContentSizeFitter;
            }
        }


        protected override void Awake()
        {
            BtnDown = transform.Find("BtnDown").GetComponent<Button>();
            BtnDown.onClick.RemoveAllListeners();
            BtnDown.onClick.AddListener(BackDown);
            BtnDown.gameObject.SetActive(false);

            this.onValueChanged.AddListener(onValueChangedAction);
        }

        private void onValueChangedAction(Vector2 arg0)
        {
            if (isDrag)
            {
                return;
            }
            if (e_ChatMsgTrackState == E_ChatMsgTrackState.Free)
            {
                return;
            }
            M_ContentContentSizeFitter.SetLayoutVertical();
            this.SetLayoutVertical();

            BackDown();
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

        private void SetDragState(bool Drag)
        {
            isDrag = Drag;
            e_ChatMsgTrackState = Drag ? E_ChatMsgTrackState.Free : E_ChatMsgTrackState.LockToNew;
            if (isDrag)
            {
                isDrag = this.content.childCount > 0;
            }
            BtnDown.gameObject.SetActive(isDrag);

        }

        public void BackDown()
        {
            SetDragState(false);
            verticalNormalizedPosition = 0;
        }



    }
}
