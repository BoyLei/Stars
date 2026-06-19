using SGF.UI.Framework;
using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;

namespace StarProject.UI.Common
{
    public class UITips:MonoBehaviour//: UIWidget
    {
        //也没比Mono多多少能力，就挂件把，不算浪费
        //private string LOG_TAG = "[UITips]";
        /* public RectTransform M_Bg;   
        public Image M_BgImg;   */            
        public Text M_text;
        public float Duration = 1;
        public float From = 1;
        public float To = 0;
        public Sequence TweenerSequence;
        //Tweener tweener = null;
        public CanvasGroup cG;
        private void Awake()
        {
            cG = GetComponent<CanvasGroup>();
        }




        /* [ContextMenu("Play")]
         public void Play()
         {
             Play(null);
         }*/
        private Vector3 _scale = Vector3.zero;
        private Vector2 _anchorP = Vector3.zero;
        public void Play(string str, Action callBack = null)
        {
            Reset(str);
            //float _v = 0;//初始值，就0开始，代码控制不必控制perfab了
            //你的获取器具（取得你）（过程化），你的设置器（初始化时-初始值）
            //DOTween.To(() => _v, x => _v = x, 1, 1.5f).OnUpdate(() =>
            //{
            //    cG.alpha = _v;
            //    _scale.x = (_v * 0.2f) + 0.9f;
            //    _scale.y = (_v * 0.2f) + 0.9f;
            //    _scale.z = (_v * 0.2f) + 0.9f;
            //    M_text.transform.localScale = _scale;

            //}).SetAutoKill(true).SetLoops(2, LoopType.Yoyo).SetEase(Ease.InOutQuint).onComplete += () =>
            //{
            //    callBack?.Invoke();
            //};
            RectTransform rt = M_text.GetComponent<RectTransform>();
            _anchorP = rt.anchoredPosition;

            Sequence sequence = DOTween.Sequence();
            TweenerSequence = sequence.
                //Insert(//并行
                //    //分散
                //    DOTween.To(value =>
                //    {
                //        _flyObj.localPosition = CurveTools.ParabolaWidth(flyParams._startPos, firstTarget, 0, value);
                //    }, 0, 1, flyParams._firstflyTarTime).SetEase(flyParams._firstEase)).
                    
                    //第一轨    
                Insert(//并行
                       //分散
                       0,
                    DOTween.To(value =>
                    {
                        _anchorP.y = value;
                        rt.anchoredPosition = _anchorP;

                    }, 0/*_anchorP.y*/,/* _anchorP.y + */150f, 1.51f)//2锚点就是相对的就是local就是0，1自带还原了无限循环，并行移动和透明和缩放3
                    
                    ).

                    //第二轨
                    Insert(
                        0,
                    //移动                    
                    DOTween.To(value => //Value （）形参 set给我===我的Setter
                    {
                        cG.alpha = value;
                        _scale.x = (value * 0.2f) + 0.9f;
                        _scale.y = (value * 0.2f) + 0.9f;
                        _scale.z = (value * 0.2f) + 0.9f;
                        M_text.transform.localScale = _scale;
                    }, 0, 1, 0.5f).SetEase(Ease.InOutQuint)) 
                    .Append(
                    //插入缩小动画
                    DOTween.To(v =>
                    {
                        cG.alpha = v;
                        _scale.x = (v * 0.2f) + 0.9f;
                        _scale.y = (v * 0.2f) + 0.9f;
                        _scale.z = (v * 0.2f) + 0.9f;
                        M_text.transform.localScale = _scale;
                    }, 
                    1, 0, 1.2f).SetEase(Ease.InOutQuint)
                ).AppendCallback(() =>
                {
                    callBack?.Invoke();
                });


        }



        //protected override void OnOpen(object arg)
        public void Reset(string arg)
        {
            //base.OnOpen(arg);
            transform.SetAsLastSibling();

            M_text.text = arg;

            //M_Bg.SetWidth(M_text.fontSize * text.Length + 40f);

            ResetColor();
        }

        [ContextMenu("ResetColor")]
        public void ResetColor()
        {
         
        }
    }
}