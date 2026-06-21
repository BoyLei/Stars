using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class EasyTweenScale : MonoBehaviour
{
    [LabelText("动画曲线")]
    public AnimationCurve animationCurve;

    [LabelText("时长")]
    public float Duration;

    [LabelText("PlayOnEnable")]
    public bool PlayOnEnable = false;

    [LabelText("初始缩放")]
    public Vector3 From;

    [LabelText("结束缩放")]
    public Vector3 To;

    [LabelText("true 使用RectTransform ,false Transform")]
    public bool UseRectTransform = false;


    [LabelText("是否循环")]
    public bool Loop = false;

    private RectTransform rectTransfom;
    private float RuningTime = 0;
    public bool IsPlaying { get; private set; }
    private System.Action onComplete;

    public float Offset=0.0f;

    private  Vector3  _from;
    private  Vector3  _to;

    private void Awake()
    {
        rectTransfom = GetComponent<RectTransform>();
    }

    public void OnEnable()
    {
        if (PlayOnEnable)
        {
            _from = From;
            _to = To;
            Play();
        }
    }

    
    
    public bool Play(float from, float to, System.Action callBack = null)
    {
        if (IsPlaying)
        {
            return false;
        }
        this.From = Vector3.one*from;
        this.To =Vector3.one*to;
        _from = From;
        _to = To;
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
            rectTransfom.localScale=From;
        }
        else
        {
            transform.localScale=From;
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
            rectTransfom.localScale =To;
        }
        else
        {
            transform.localScale =To;
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
            if (RuningTime < 0.1f)
            {
                Offset = UnityEngine.Random.Range(-0.2f, 0.2f);
                To=_to*(1+Offset);
            }
        }

        t = RuningTime / Duration;

        float f = animationCurve.Evaluate(t);
        if(UseRectTransform)
        {
            
            rectTransfom.localScale = Vector3.Lerp(From, To, f);
        }
        else
        {
            transform.localScale = Vector3.Lerp(From, To, f);
        }

        if (RuningTime >= Duration)
        {
            if (!Loop )
            {
                IsPlaying = false;
                onComplete?.Invoke();
            }
        }

    }
}
