using DG.Tweening;
using Google.Protobuf.WellKnownTypes;
using Sirenix.OdinInspector;
using StarProjectDef;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[XLua.LuaCallCSharp]
public enum AnimType
{
    None,
    Scale,      // 缩放
    Fade,       // 闪烁
    LocalMove,  // 偏移
}

[System.Serializable]
public abstract class AnimBaseParam
{

}

[System.Serializable]
public class ScaleParam : AnimBaseParam
{
    [LabelText("最小缩放比例")]
    public float minScale = 0.9f;
    [LabelText("最大缩放比例")]
    public float maxScale = 1.5f;
    [LabelText("呼吸持续时间")]
    public float breathDuration = 1f;
    [LabelText("缩放次数")]
    public int scaleTime = 2;
    [LabelText("原来的大小")]
    public Vector3 originalScale = Vector3.one;
    [LabelText("动画曲线")]
    public Ease Curve = Ease.Linear;
}

[System.Serializable]
public class AlphaParam : AnimBaseParam
{
    [LabelText("闪烁次数")]
    public int blinkCount = 3;
    [LabelText("闪烁间隔时间")]
    public float blinkInterval = 0.5f;
    [LabelText("闪烁总时长")]
    public float blinkDuration = 2f;
    [LabelText("闪烁时的最小值")]
    public float minAlpha = 0f;
    [LabelText("闪烁时的最大值")]
    public float maxAlpha = 0f;
    [LabelText("动画曲线")]
    public Ease Curve = Ease.Linear;
}

[System.Serializable]
public class LocalMoveParam : AnimBaseParam
{
    [LabelText("动画时长")]
    public float duration = 1f;
    [LabelText("动画间隔")]
    public float delay = 0f;
    [LabelText("循环次数")]
    public int loops = 1;
    //[LabelText("循环类型")]
    //public LoopType loopType = LoopType.Restart;
    [LabelText("偏移坐标")]
    public Vector3 offsetPos = Vector3.zero;
    [LabelText("动画曲线")]
    public Ease animationCurve = Ease.Linear;
    //[LabelText("是否相对运动")]
    //public bool isRelative = false;
    //[LabelText("来去？")]
    //public bool isFrom = false;
}

[System.Serializable]
public class AnimShowTypeSerialize
{
    [LabelText("动画类型")]
    [ValueDropdown("_globalshowtype")]//检测1
    public AnimType GlobalShowType = new();

    [LabelText("动画key")]
    public string AnimKey = "";

    [LabelText("缩放动画")]
    [SerializeField]
    [ShowIf("ShouldSerializeAnim_Scale")]//触发调用3
    public ScaleParam SpecialScale = new();

    [LabelText("闪烁动画")]
    [SerializeField]
    [ShowIf("ShouldSerializeAnim_Fade")]
    public AlphaParam SpecialFade = new();

    [LabelText("本地位移动画")]
    [SerializeField]
    [ShowIf("ShouldSerializeAnim_LocalMove")]
    public LocalMoveParam SpecialLocalMove = new();

    public bool ShouldSerializeAnim_Scale()
    {
        return this.GlobalShowType == AnimType.Scale;//触发效用2
    }

    public bool ShouldSerializeAnim_Fade()
    {
        return this.GlobalShowType == AnimType.Fade;
    }

    public bool ShouldSerializeAnim_LocalMove()
    {
        return this.GlobalShowType == AnimType.LocalMove;
    }

    public IEnumerable animshowtype = new ValueDropdownList<AnimType>()
        {
            {"缩放",AnimType.Scale},
            {"闪烁",AnimType.Fade},
            {"本地移动",AnimType.LocalMove},
        };

    public IEnumerable _globalshowtype()
    {
        return animshowtype;
    }
}

[XLua.LuaCallCSharp]
[System.Serializable]
public class AnimParamsShow : MonoBehaviour
{
    [LabelText("动画类型")]
    public List<AnimShowTypeSerialize> AnimParams = new();

    private Vector3 originalLocalPos;
    private void Start()
    {
        originalLocalPos = transform.localPosition;
    }

    /// <summary>
    /// 按照Key播放
    /// </summary>
    /// <param name="key">数据标识，面板填写的唯一ID</param>
    /// <param name="onfinish"></param>
    public void PlayAnimByKey(string key, UnityAction onfinish = null)
    {
        if (AnimParams == null || AnimParams.Count == 0)
        {
            return;
        }

        AnimType animType = AnimType.None;
        var data = GetAnimBaseParamByKey(key, out animType);
        if (data == null)
        {
            return;
        }

        switch (animType)
        {
            case AnimType.None:
                break;
            case AnimType.Scale:
                {
                    ScaleParam scaleParam = data as ScaleParam;
                    PlayScaleAnim(scaleParam, onfinish);
                }
                break;
            case AnimType.Fade:
                {
                    AlphaParam alphaParam = data as AlphaParam;
                    PlayFadeAnim(alphaParam, onfinish);
                }
                break;
            case AnimType.LocalMove:
                {
                    LocalMoveParam localMoveParam = data as LocalMoveParam;
                    PlayLocalMoveAnim(localMoveParam, onfinish);
                }
                break;
            default:
                break;
        }
    }

    public void PlayAnimByType(AnimType animType, UnityAction onfinish = null)
    {
        if (AnimParams == null || AnimParams.Count == 0)
        {
            return;
        }

        var data = GetAnimBaseParam(animType);
        if (data == null)
        {
            return;
        }

        switch (animType)
        {
            case AnimType.None:
                break;
            case AnimType.Scale:
                {
                    PlayScaleAnim(data as ScaleParam, onfinish);
                }
                break;
            case AnimType.Fade:
                {
                    PlayFadeAnim(data as AlphaParam, onfinish);
                }
                break;
            case AnimType.LocalMove:
                {
                    PlayLocalMoveAnim(data as LocalMoveParam, onfinish);
                }
                break;
            default:
                break;
        }
    }

   /* public void StopAllAnim()
    {
        transform
    }*/

    #region [缩放动画]

    [ContextMenu("播放缩放动画")]
    public void StartPlayScale()
    {
        PlayAnimByType(AnimType.Scale);
    }

    private void PlayScaleAnim(ScaleParam scaleParam, UnityAction onfinish)
    {
        transform.localScale = scaleParam.originalScale;

        Sequence sequence = DOTween.Sequence();
        float time = scaleParam.breathDuration / scaleParam.scaleTime / 2;
        // 添加缩放动画
        sequence.Append(transform.DOScale(scaleParam.maxScale, time));
        sequence.Append(transform.DOScale(scaleParam.minScale, time));
        // 循环添加呼吸次数
        for (int i = 1; i < scaleParam.scaleTime; i++)
        {
            sequence.Append(transform.DOScale(scaleParam.maxScale, time));
            sequence.Append(transform.DOScale(scaleParam.minScale, time));
        }
        sequence.SetEase(scaleParam.Curve);
        sequence.SetAutoKill(true);
        // 在动画完成后添加回调方法
        sequence.OnComplete(() =>
        {
            onfinish?.Invoke();
        });
        // 启动动画序列
        sequence.Play();
    }

    #endregion

    #region [闪烁动画]

    [ContextMenu("播放闪烁动画")]
    public void StartPlayFade()
    {
        PlayAnimByType(AnimType.Fade);
    }

    private void PlayFadeAnim(AlphaParam alphaParam, UnityAction onfinish = null)
    {
        transform.DOKill(true);
        // 使用DOTween进行动画处理
        Sequence sequence = DOTween.Sequence();
        //Component[] components = transform.GetComponents<Component>();
        CanvasGroup componentCanvasGroup = transform.GetComponent<CanvasGroup>();
        TextMeshProUGUI componentTextMeshProUGUI = transform.GetComponent<TextMeshProUGUI>();
        TextMeshPro componentTextMeshPro = transform.GetComponent<TextMeshPro>();
        Renderer componentRenderer = transform.GetComponent<Renderer>();
        Graphic componentGraphic = transform.GetComponent<Graphic>();

        float time = alphaParam.blinkDuration / alphaParam.blinkCount / 2;
        //Graphic _graphic = null;
        //TMPro.TextMeshPro textMeshPross = null;
        // 循环添加闪烁次数
        for (int i = 0; i < alphaParam.blinkCount; i++)
        {
            if (i > 0)
            {
                // 播放次数大于1的，就增加闪烁间隔时间
                sequence.AppendInterval(alphaParam.blinkInterval);
            }
            if (componentCanvasGroup != null)
            {
                sequence.Append(componentCanvasGroup.DOFade(alphaParam.minAlpha, time));
                sequence.AppendInterval(0.01f);
                sequence.Append(componentCanvasGroup.DOFade(alphaParam.maxAlpha, time));
            }
            else if (componentTextMeshProUGUI != null)
            {
                sequence.Append(componentTextMeshProUGUI.DOFade(alphaParam.minAlpha, time));
                sequence.AppendInterval(0.01f);
                sequence.Append(componentTextMeshProUGUI.DOFade(alphaParam.maxAlpha, time));
            }
            else if (componentTextMeshPro != null)
            {
                sequence.Append(componentTextMeshPro.DOFade(alphaParam.minAlpha, time));
                sequence.AppendInterval(0.01f);
                sequence.Append(componentTextMeshPro.DOFade(alphaParam.maxAlpha, time));
            }
            else if (componentRenderer != null)
            {
                Material material = componentRenderer.material;
                sequence.Append(material.DOFade(alphaParam.minAlpha, time));
                sequence.AppendInterval(0.01f);
                sequence.Append(material.DOFade(alphaParam.maxAlpha, time));
            }
            else if (componentGraphic != null)
            {
                sequence.Append(componentGraphic.DOFade(alphaParam.minAlpha, time));
                sequence.AppendInterval(0.01f);
                sequence.Append(componentGraphic.DOFade(alphaParam.maxAlpha, time));
            }
            //foreach (Component component in components)
            //{
            //    if (component is CanvasGroup canvasGroup)
            //    {
            //        sequence.Append(canvasGroup.DOFade(alphaParam.minAlpha, time));
            //        sequence.AppendInterval(0);
            //        sequence.Append(canvasGroup.DOFade(alphaParam.maxAlpha, time));
            //        break;
            //    }
            //    else if (component is TextMeshProUGUI textMeshProUGUI)
            //    {
            //        sequence.Append(textMeshProUGUI.DOFade(alphaParam.minAlpha, time));
            //        sequence.AppendInterval(0);
            //        sequence.Append(textMeshProUGUI.DOFade(alphaParam.maxAlpha, time));
            //        break;
            //    }
            //    else if (component is TextMeshPro textMeshPro)
            //    {
            //        sequence.Append(textMeshPro.DOFade(alphaParam.minAlpha, time));
            //        sequence.SetDelay(0.01f);
            //        sequence.Append(textMeshPro.DOFade(alphaParam.maxAlpha, time));
            //        //textMeshPross = textMeshPro;
            //        break;
            //    }
            //    else if (component is Renderer rendererComponent)
            //    {
            //        Material material = rendererComponent.material;
            //        sequence.Append(material.DOFade(alphaParam.minAlpha, time));
            //        sequence.AppendInterval(0);
            //        sequence.Append(material.DOFade(alphaParam.maxAlpha, time));
            //        break;
            //    }
            //    else if (component is Graphic graphic)
            //    {
            //        sequence.Append(graphic.DOFade(alphaParam.minAlpha, time));
            //        sequence.AppendInterval(0);
            //        sequence.Append(graphic.DOFade(alphaParam.maxAlpha, time));
            //        //_graphic = graphic;
            //        break;
            //    }
            //}
        }
        sequence.SetEase(alphaParam.Curve);
        sequence.SetAutoKill(true);
        //sequence.OnUpdate(() =>
        //{
        //    SGF.Debuger.LogWarning($"动画每帧的值 a={textMeshPross.color.a}");
        //});
        // 在动画完成后添加回调方法
        sequence.OnComplete(() =>
        {
            onfinish?.Invoke();
            //SGF.Debuger.LogError($"动画每帧的值 a={textMeshPross.color.a}");
        });

        // 启动动画序列
        //SGF.Debuger.Log($"动画每帧的值 a={textMeshPross.color.a}");
        sequence.Play();
    }

    #endregion

    #region [偏移动画]
    [ContextMenu("播放本地位移动画")]
    public void StartPlayLocalMove()
    {
        PlayAnimByType(AnimType.LocalMove);
    }

    private void PlayLocalMoveAnim(LocalMoveParam localMoveParam, UnityAction onfinish = null)
    {
        transform.DOKill(true);
        Sequence sequence = DOTween.Sequence();
        float time = localMoveParam.duration / localMoveParam.loops / 2;
        Vector3 from = originalLocalPos + localMoveParam.offsetPos;
        Vector3 to = originalLocalPos + (localMoveParam.offsetPos * -1);
        for (int i = 0; i < localMoveParam.loops; i++)
        {
            sequence.Append(transform.DOLocalMove(from, time, false));
            sequence.Append(transform.DOLocalMove(to, time, false));
            if (localMoveParam.loops > 1 && i + 1 < localMoveParam.loops)
            {
                sequence.AppendInterval(localMoveParam.delay);
            }
        }
        sequence.SetEase(localMoveParam.animationCurve);
        sequence.SetAutoKill(true);
        // 在动画完成后添加回调方法
        sequence.OnComplete(() =>
        {
            onfinish?.Invoke();
            transform.localPosition = originalLocalPos;
        });
        sequence.OnKill(() =>
        {
            sequence = null;
        });
        // 启动动画序列
        sequence.Play();
    }
    #endregion

    #region [获取动画参数]
    /// <summary>
    /// 根据Key获取动画参数
    /// </summary>
    /// <param name="key"></param>
    /// <param name="animType"></param>
    /// <returns></returns>
    private AnimBaseParam GetAnimBaseParamByKey(string key, out AnimType animType)
    {
        foreach (var item in AnimParams)
        {
            if (item != null)
            {
                if (item.AnimKey == key)
                {
                    if (item.GlobalShowType == AnimType.Scale)
                    {
                        animType = AnimType.Scale;
                        return item.SpecialScale;
                    }
                    else if (item.GlobalShowType == AnimType.Fade)
                    {
                        animType = AnimType.Fade;
                        return item.SpecialFade;
                    }
                    else if (item.GlobalShowType == AnimType.LocalMove)
                    {
                        animType = AnimType.LocalMove;
                        return item.SpecialLocalMove;
                    }
                }
            }
        }
        animType = AnimType.None;
        return null;
    }

    /// <summary>
    /// 只取得第一个
    /// </summary>
    /// <param name="animType"></param>
    /// <returns></returns>
    private AnimBaseParam GetAnimBaseParam(AnimType animType)
    {
        foreach (var item in AnimParams)
        {
            if (item != null)
            {
                if (item.GlobalShowType == animType && item.GlobalShowType == AnimType.Scale)
                {
                    return item.SpecialScale;
                }
                else if (item.GlobalShowType == animType && item.GlobalShowType == AnimType.Fade)
                {
                    return item.SpecialFade;
                }
                else if (item.GlobalShowType == animType && item.GlobalShowType == AnimType.LocalMove)
                {
                    return item.SpecialLocalMove;
                }
            }
        }
        return null;
    }
    #endregion
}
