using SGF.UI.Framework;
using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;

namespace StarProject.UI.Common
{
    public class BattleUITips : MonoBehaviour
    {
       
        public Text M_text;
        Action CallBack;
        public CanvasGroup cG;
        private void Awake()
        {
            cG = GetComponent<CanvasGroup>();
        }

        public void Play(string str, Action callBack = null)
        {
            gameObject.SetActive(false);
            gameObject.SetActive(true);
            M_text.text = str;
            CallBack = callBack;

        }

        public void OnFinishAnim()
        {
            gameObject.SetActive(false);
            if (CallBack!=null)
            {
                CallBack.Invoke();
                CallBack = null;
            }
        }

    }
}