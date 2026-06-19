using SGF.UI.Framework;
using SGF.Unity;
using StarProject.Game;
using StarProject.Service.Resource;
using StarProject.Service.SDK;
using StarProject.Service.Timeline;
using StarProjectDef;
using UnityEngine;
using UnityEngine.UI;

namespace StarProject.UI.StarWorld
{

    [System.Serializable]
    public class QTEWidget : UIWidget
    {
        //private string LOG_TAG = "[QTEWidget]";
        private Image m_image;
        private Image m_imageBg;
        private Image m_ImgCD;
        private Button m_Btn;
        private System.Action<float> callback = null;
        private bool isCD = false;
        private float m_timer = 0f;
        private float m_timerTotal = 0f;

        protected override void Awake()
        {
            base.Awake();
            SetCloseDestroy(true);
            m_image = transform.Find("BG/bg/IconRoot").GetComponent<Image>();
            m_imageBg = transform.Find("BG/bg").GetComponent<Image>();
            m_ImgCD = transform.Find("BG/bg/ImgCD").GetComponent<Image>();
            m_Btn = transform.Find("BG/bg/IconRoot").GetComponent<Button>();
            m_Btn.onClick.AddListener(OnClick);
        }
        private void Update()
        {
            if (isCD) {
                m_timer = m_timer - Time.deltaTime;
                m_ImgCD.fillAmount = m_timer / m_timerTotal;
                if (m_timer <= 0) 
                {
                    m_ImgCD.fillAmount = 0;
                    isCD = false;
                }
            }
            
        }
        protected override void OnOpen(object arg = null)
        {
            base.OnOpen(arg);
            QTEData qteData = arg as QTEData;
            if (qteData != null && m_image != null)
            {
                if (qteData.cb != null)
                {
                    callback = qteData.cb;
                    callback(qteData.timeScale);
                }
                m_timerTotal = qteData.time / 1000f;
                m_timer = m_timerTotal;

                isCD = true;
                DelayInvoker.DelayInvoke(this, m_timerTotal, (object[] args) =>
                {
                    OnTimerEnd(false);
                }, null);


                ResourceFormalManager.Instance.LoadSpriteAsync(qteData.iconPath, (sprite) =>
                {
                    if (sprite)
                    {
                        m_image.sprite = sprite;
                    }
                });

                Vector2 pos = new Vector2(Screen.width * (qteData.width - 0.5f), Screen.height * (qteData.height - 0.5f));
                m_imageBg.transform.localPosition = pos;
            }

        }

        public void OnClick()
        {
            OnTimerEnd(true);
        }

        public void OnTimerEnd(bool isClick)
        {
            isCD = false;
            if (callback != null)
            {
                callback(1);
            }

            if (isClick)
            {
                GameManager.Instance.EventPreNewPlayerEvent($"9_{GameManager.Instance.GetCurMapId()}_1");
            }
            else
            {
                GameManager.Instance.EventPreNewPlayerEvent($"9_{GameManager.Instance.GetCurMapId()}_2");
            }

            UIManager.Instance.CloseWidget(UIDef.QTEWidget, null, true);
            DelayInvoker.CancelInvoke(this);
        }
        protected override void OnClose(object arg = null)
        {
            base.OnClose();
            DelayInvoker.CancelInvoke(this);
        }

    }
}
