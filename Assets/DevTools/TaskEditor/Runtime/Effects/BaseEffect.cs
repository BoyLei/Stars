///--------------------------------------------------------------------
/// 文件名   :   BaseEffect.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2023/03/20 16:56:56
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using Sirenix.OdinInspector;
using StarProject.Service.Language;
using UnityEngine;
namespace Task
{
    [System.Serializable]
    [InlineProperty]
    public abstract class BaseEffect
    {
        [LabelText("效果是否等待结束")]
        [OnValueChanged("OnWaitEndChanged")]
        [ShowIf("OnShowWaitEnd")]
       
        public bool WaitEnd = false;

        [OnValueChanged("OnIndex")]
        [HideInInspector]
        public int Index = 0;

        [ShowInInspector]
        [LabelText("服务显示文本")]
        [ShowIf("OnShowIfShowText")]
        public string ShowText
        {
            get
            {
#if UNITY_EDITOR
                //任务编辑器打开直接返回中文
                if (LanguageManager.Instance.TaskEditorOpened)
                {
                    return showText;
                }
#endif
                //非编辑器模式返回key对应的语言文本
                return LanguageManager.Instance.GetLanguageByKey(ShowText_Key);
            }
            set { showText = value; }
        }
        [TextArea]
        private string showText;
        [HideInInspector]
        public string ShowText_Key;


        [LabelText("超时时间")]
        [SuffixLabel("毫秒")]
        [ShowIf("ShowOutTime")]
        //   [Newtonsoft.Json.JsonDictionary]
        public int OutTime;

        [HideInInspector]
        [OnValueChanged("OnWaitEndChanged")]
        public FunctionType curType = FunctionType.None;

        public float ToFloat(string arg)
        {
            float v = 0;
            System.Single.TryParse(arg, out v);
            return v;
        }

        public int ToInt(string arg)
        {
            int v = 0;
            System.Int32.TryParse(arg, out v);
            return v;
        }

        public bool ToBoolean(string arg)
        {
            return arg == "1";
        }
        private void OnIndex()
        {
            if (Index == 0)
            {
                ShowText = "";
            }
        }
        private bool OnShowIfShowText() 
        {
            return Index == 0;
        }
        public virtual void OnSerialized(EffectJsonData effectJson)
        {
            effectJson.WaitEnd = WaitEnd;
            effectJson.ShowText = ShowText;
            effectJson.ShowText_Key = ShowText_Key;
            effectJson.OutTime = OutTime;
            effectJson.EffectType = curType;
        }

        public virtual void OnDeSerialized(EffectJsonData effectJson)
        {
            WaitEnd = effectJson.WaitEnd;
            ShowText = effectJson.ShowText;
            ShowText_Key = effectJson.ShowText_Key;
            OutTime = effectJson.OutTime;
            curType = effectJson.EffectType;
        }

        private bool ShowOutTime()
        {
            return OnShowWaitEnd() && WaitEnd;
        }
        private void OnWaitEndChanged()
        {
            OutTime = WaitEnd ? -1 : 0;
        }
        private bool OnShowWaitEnd()
        {
            return TaskEnumUtils.ClientTimeOutList.Contains(curType);
        }

    }
}

