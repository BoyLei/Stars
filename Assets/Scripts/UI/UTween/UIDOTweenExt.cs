using DG.Tweening;
using UnityEngine.UI;
using XLua;
/// <summary>
/// C#封装的dotween.to
/// </summary>
[LuaCallCSharp]
public static class UIDOTweenExt
{
    public static Tweener To(float start, float to, float duration, LuaFunction setter)
    {
        return DOTween.To(() => start, (value) => setter.Call(value), to, duration);
    }

    public static Tweener SliderDOValue(Slider target, float endValue, float duration, bool snapping = false)
    {
        return DoTweenFactory.DOValue(target, endValue, duration, snapping);
    }


}
