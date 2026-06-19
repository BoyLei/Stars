using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using SGF.UI.Framework;

namespace StarProject.UI.Common
{
    #region Tips
    //最多三个按钮，需要自己添加，挂载，
    //window接受前面三个参数，通过挂载映射，按钮反馈
    //都是关闭，然后给回调，就是一个window
    #endregion

    //通用ui信息弹窗
    public class UIMsgBox : UIWindow
    {
        //不单单面对策划，也面向运维
        public class UIMsgBoxArg
        {
            public string title = "";
            public string content = "";
            public string btnText;//"确定|取消|关闭"
        }

        private UIMsgBoxArg m_arg;
        public Text txtContent;
        public UIBehaviour ctlTitle;
        public Button[] buttons;


        protected override void OnOpen(object arg = null)
        {
            base.OnOpen(arg);
            m_arg = arg as UIMsgBoxArg;
            txtContent.text = m_arg.content;
            string[] btnTexts = m_arg.btnText.Split('|');

            UIUtilsFrameWork.SetChildText(ctlTitle, m_arg.title);
            UIUtilsFrameWork.SetActive(ctlTitle, !string.IsNullOrEmpty(m_arg.title));

            float btnWidth = 200;
            float btnStartX = (1 - btnTexts.Length) * btnWidth / 2;

            for (int i = 0; i < buttons.Length; i++)
            {
                if (i < btnTexts.Length)
                {
                    UIUtilsFrameWork.SetButtonText(buttons[i], btnTexts[i]);
                    UIUtilsFrameWork.SetActive(buttons[i], true);
                    Vector3 pos = buttons[i].transform.localPosition;
                    pos.x = btnStartX + i * btnWidth;
                    buttons[i].transform.localPosition = pos;
                }
                else
                {
                    UIUtilsFrameWork.SetActive(buttons[i], false);
                }
            }


        }

        public void OnBtnClick(int btnIndex)
        {
            Button btn = buttons[btnIndex];
            this.Close(btnIndex);
        }
    }
}
