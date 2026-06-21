///--------------------------------------------------------------------
/// 文件名   :   EasyTweenPosition.cs
/// 内  容   :   根据AnimationCurve 模拟位置移动，适用于UI
/// 说  明   :  
/// 创建日期 :   2022/12/01 18:47:30
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EasyTweenPosition : MonoBehaviour
{
    [LabelText("动画曲线")]
    public AnimationCurve animationCurve;

    [LabelText("时长")]
    public float Duration;

    [LabelText("PlayOnEnable")]
    public bool PlayOnEnable = false;

    [LabelText("起始位置")]
    public Vector3 From;

    [LabelText("结束位置")]
    public Vector3 To;

    [LabelText("true 使用RectTransform ,false Transform")]
    public bool UseRectTransform = false;


    [LabelText("是否循环")]
    public bool Loop = false;

    private RectTransform rectTransfom;
    private float RuningTime = 0;
    public bool IsPlaying { get; private set; }
    private System.Action onComplete;



    private void Awake()
    {
        rectTransfom = GetComponent<RectTransform>();
    }

    public void OnEnable()
    {
        if (PlayOnEnable)
        {
            Play();
        }
    }

    public bool Play(Vector3 from, Vector3 to, System.Action callBack = null)
    {
        if (IsPlaying)
        {
            return false;
        }
        this.From = from;
        this.To = to;
        return Play(callBack);
    }


    public void ApplayFrom()
    {
        if (rectTransfom == null)
        {
            return;
        }
        if(UseRectTransform)
        {
            rectTransfom.anchoredPosition3D =From;
        }
        else
        {
            transform.localPosition =From;
        }
    }
    
    public void ApplayTo()
    {
        if (rectTransfom == null)
        {
            return;
        }
        if(UseRectTransform)
        {
            rectTransfom.anchoredPosition3D =To;
        }
        else
        {
            transform.localPosition =To;
        }
    }
    
    public bool Play(System.Action callBack = null)
    {
        if (IsPlaying)
        {
            return false;
        }
        this.onComplete = callBack;
        RuningTime = 0;
        IsPlaying = true;
        return true;
    }
    // Update is called once per frame
    void Update()
    {

        if (!IsPlaying)
        {
            return;
        }

        if (Duration <= 0)
        {
            return;
        }
        float t = 0;
        RuningTime += Time.deltaTime;

        if (Loop)
        {
            RuningTime = RuningTime % Duration;
        }

        t = RuningTime / Duration;

        float f = animationCurve.Evaluate(t);
        if(UseRectTransform)
        {
            rectTransfom.anchoredPosition3D = Vector3.Lerp(From, To, f);
        }
        else
        {
            transform.localPosition = Vector3.Lerp(From, To, f);
        }
        if (!Loop && RuningTime >= Duration)
        {
            IsPlaying = false;
            onComplete?.Invoke();
        }
    }
}
