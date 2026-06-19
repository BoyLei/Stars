using System.Collections;
using System.Collections.Generic;
using StarProjectDef;
using UnityEngine;
using UnityEngine.UI;

namespace SGF.UI.Framework
{
    public class RedPointTestWidget : UIWidget
    {
        public Button closeBtn;

        protected override void Awake()
        {
            base.Awake();

            closeBtn.onClick.AddListener(OnBtnClose);
        }

        public void OnBtnClose()
        {
            UIManager.Instance.CloseWidget(UIDef.TestRedPointWidget, UIRoot.UIROOT.transform, true);
        }
    }
}
