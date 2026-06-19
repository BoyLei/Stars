///--------------------------------------------------------------------
/// 文件名   :   AnimationPreview.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2024/04/18 20:59:59
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
#if UNITY_EDITOR

using System;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

[System.Serializable]
public class AnimationPreview
{
    [LabelText("动画路径")] [ReadOnly] public string AnimationPath;

    [LabelText("动画名称")] [ReadOnly] public string AnimationName;

    [LabelText("动画资源")] [ReadOnly] public AnimationClip AnimationClip;

   
    [ShowInInspector, EnableGUI, ReadOnly, LabelText("动画时长")]
    public float Duration => AnimationClip ? AnimationClip.length : 0;

    [ShowInInspector, EnableGUI, ReadOnly, LabelText("是否循环")]
    public bool IsLoop => AnimationClip ? AnimationClip.isLooping : false;

    [LabelText("播放速度")] public float PlaySpeed = 1.0f;

    [CustomValueDrawer("AnimationPlay")] [OnValueChanged("OnTimeChange")]
    public float time;

    [HideInInspector]
    public GameObject mModel;

    
    public float AnimationPlay(float value, GUIContent label)
    {
#if UNITY_EDITOR
        return UnityEditor.EditorGUILayout.Slider(label, value, 0, Duration);
#else
        return value;
#endif
    }

    public void OnTimeChange()
    {
        if (mModel != null && AnimationClip != null)
        {
            AnimationClip.SampleAnimation(mModel, time);
        }
       
    }

    [Button("预览")]
    public void Privew()
    {
        PlayAnimation();
    }

    public async void PlayAnimation()
    {
        if (mModel != null && AnimationClip != null)
        {
            float len = AnimationClip.length;
            float runtime = 0;
            while (runtime < len)
            {
                AnimationClip.SampleAnimation(mModel, runtime);
                await UniTask.WaitForEndOfFrame();
                runtime += 0.0166f * PlaySpeed;
            }
        }
    }
}
#endif