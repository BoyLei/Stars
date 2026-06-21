///--------------------------------------------------------------------
/// 文件名   :   AutoAlpha
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/07/07 16:27:14
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
[XLua.LuaCallCSharp]
[DisallowMultipleComponent]
[RequireComponent(typeof(MaskableGraphic))]
public class AutoAlpha : MonoBehaviour
{
    public float Duration;

    public float From;

    public float To;

    public AnimationCurve AnimationCurve= AnimationCurve.Linear(0,0,1,1);
    private float RuningTime = 0;

    private bool End=true;
    public bool Loop = false;
    private MaskableGraphic graphic;
    private System.Action CompleteCallBack = null;
    public bool IsPlaying
    {
        get { return RuningTime > 0 && !End; }
    }



    private void Awake()
    {
        graphic = GetComponent<MaskableGraphic>();
    }



    [ContextMenu("Play")]

    public void Play()
    {
        Play(null);
    }
    public void Play(System.Action callBack = null)
    {
        RuningTime = 0;
        End = false;
        CompleteCallBack = callBack;
    }

    private void Update()
    {
        if (End)
        {
            return;
        }
        if (RuningTime >= Duration)
        {
            if (Loop)
            {
                RuningTime = 0;
            }
            else
            {
                End = true;
                CompleteCallBack?.Invoke();
            }
        }

        float alpha = AnimationCurve.Evaluate(RuningTime / Duration);
        float t = From * (1 - alpha) +  alpha * To;
        graphic.SetAlpha(t);
        RuningTime += Time.deltaTime;

    }

    public void Reset()
    {

    }

}
