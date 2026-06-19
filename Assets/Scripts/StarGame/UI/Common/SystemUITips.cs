using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace StarProject.UI.Common
{
    public class SystemUITips : MonoBehaviour
    {
        public Text M_text;
        private Action CallBack;
        private CanvasGroup cG;
        private bool useAnimKeyFrame = true;
        private float offsetY;
        private float fixedOffsetY = 298;
        private Sequence _Sequence;
        private void Awake()
        {
            cG = GetComponent<CanvasGroup>();
        }

        public void OnTweenComplete() 
        {
            ResetStatus();
        }

        public void OnTweenKill() 
        {
            //ResetStatus();
        }

        public void SetOffsetY(int index, int showCount) 
        {
            int newOffset = (showCount - index) * 48;
            if (newOffset > offsetY)
            {
                offsetY = newOffset;
                if (_Sequence != null)
                {
                    _Sequence.Kill();
                    _Sequence = null;

                    _Sequence = DOTween.Sequence();
                    _Sequence.Append(DOTween.To(value =>
                    {
                        Vector3 pos = transform.localPosition;
                        pos.y = value + offsetY + fixedOffsetY;
                        transform.localPosition = pos;
                    }, 0, 48f, 0.5f).SetEase(Ease.Linear))
                    .AppendInterval(index * 1.5f / showCount)
                    .OnComplete(OnTweenComplete).OnKill(OnTweenKill);
                }
            }
        }

        public void ResetStatus() 
        {
            gameObject.SetActive(false);
            offsetY = 0;
            if (CallBack != null)
            {
                CallBack.Invoke();
                CallBack = null;
            }
        }

        public void Play(string str, Action callBack = null)
        {
            useAnimKeyFrame = true;
            gameObject.SetActive(false);
            gameObject.SetActive(true);
            transform.SetAsFirstSibling();
            M_text.text = str;
            CallBack = callBack;
        }

        public void PlayTween(string str, Action callBack = null)
        {
            useAnimKeyFrame = false;
            gameObject.SetActive(false);
            gameObject.SetActive(true);
            transform.SetAsFirstSibling();
            M_text.text = str;
            CallBack = callBack;

            _Sequence = DOTween.Sequence();
            _Sequence.Append(DOTween.To(value =>
            {
                Vector3 pos = transform.localPosition;
                pos.y = value + offsetY + fixedOffsetY;
                transform.localPosition = pos;
            }, 0, 48f, 0.5f).SetEase(Ease.Linear))
            .AppendInterval(1.5f)
            .OnComplete(OnTweenComplete).OnKill(OnTweenKill);
        }

        public void OnFinishAnim()
        {
            if (useAnimKeyFrame)
            {
                gameObject.SetActive(false);
                if (CallBack != null)
                {
                    CallBack.Invoke();
                    CallBack = null;
                }
            }
        }
    }
}