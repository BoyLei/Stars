using UnityEngine;
[XLua.LuaCallCSharp]
public enum TweenEaseType
{
    easeInQuad,
    easeOutQuad,
    easeInOutQuad,
    easeInCubic,
    easeOutCubic,
    easeInOutCubic,
    easeInQuart,
    easeOutQuart,
    easeInOutQuart,
    easeInQuint,
    easeOutQuint,
    easeInOutQuint,
    easeInSine,
    easeOutSine,
    easeInOutSine,
    easeInExpo,
    easeOutExpo,
    easeInOutExpo,
    easeInCirc,
    easeOutCirc,
    easeInOutCirc,
    linear,
    spring,
    /* GFX47 MOD START */
    //bounce,
    easeInBounce,
    easeOutBounce,
    easeInOutBounce,
    /* GFX47 MOD END */
    easeInBack,
    easeOutBack,
    easeInOutBack,
    /* GFX47 MOD START */
    //elastic,
    easeInElastic,
    easeOutElastic,
    easeInOutElastic,
    /* GFX47 MOD END */
    punch
}

/// <summary>
/// Copy from iTween.
/// </summary>
[XLua.LuaCallCSharp]
public class SGame_TweenUtils
{
    public static float linear(float start, float end, float value)
    {
        return Mathf.Lerp(start, end, value);
    }

    public static float clerp(float start, float end, float value)
    {
        float min = 0.0f;
        float max = 360.0f;
        float half = Mathf.Abs((max - min) / 2.0f);
        float retval = 0.0f;
        float diff = 0.0f;
        if ((end - start) < -half)
        {
            diff = ((max - start) + end) * value;
            retval = start + diff;
        }
        else if ((end - start) > half)
        {
            diff = -((max - end) + start) * value;
            retval = start + diff;
        }
        else retval = start + (end - start) * value;
        return retval;
    }

    public static float spring(float start, float end, float value)
    {
        value = Mathf.Clamp01(value);
        value = (Mathf.Sin(value * Mathf.PI * (0.2f + 2.5f * value * value * value)) * Mathf.Pow(1f - value, 2.2f) + value) * (1f + (1.2f * (1f - value)));
        return start + (end - start) * value;
    }

    public static float easeInQuad(float start, float end, float value)
    {
        end -= start;
        return end * value * value + start;
    }

    public static float easeOutQuad(float start, float end, float value)
    {
        end -= start;
        return -end * value * (value - 2) + start;
    }

    public static float easeInOutQuad(float start, float end, float value)
    {
        value /= .5f;
        end -= start;
        if (value < 1) return end / 2 * value * value + start;
        value--;
        return -end / 2 * (value * (value - 2) - 1) + start;
    }

    public static float easeInCubic(float start, float end, float value)
    {
        end -= start;
        return end * value * value * value + start;
    }

    public static float easeOutCubic(float start, float end, float value)
    {
        value--;
        end -= start;
        return end * (value * value * value + 1) + start;
    }

    public static float easeInOutCubic(float start, float end, float value)
    {
        value /= .5f;
        end -= start;
        if (value < 1) return end / 2 * value * value * value + start;
        value -= 2;
        return end / 2 * (value * value * value + 2) + start;
    }

    public static float easeInQuart(float start, float end, float value)
    {
        end -= start;
        return end * value * value * value * value + start;
    }

    public static float easeOutQuart(float start, float end, float value)
    {
        value--;
        end -= start;
        return -end * (value * value * value * value - 1) + start;
    }

    public static float easeInOutQuart(float start, float end, float value)
    {
        value /= .5f;
        end -= start;
        if (value < 1) return end / 2 * value * value * value * value + start;
        value -= 2;
        return -end / 2 * (value * value * value * value - 2) + start;
    }

    public static float easeInQuint(float start, float end, float value)
    {
        end -= start;
        return end * value * value * value * value * value + start;
    }

    public static float easeOutQuint(float start, float end, float value)
    {
        value--;
        end -= start;
        return end * (value * value * value * value * value + 1) + start;
    }

    public static float easeInOutQuint(float start, float end, float value)
    {
        value /= .5f;
        end -= start;
        if (value < 1) return end / 2 * value * value * value * value * value + start;
        value -= 2;
        return end / 2 * (value * value * value * value * value + 2) + start;
    }

    public static float easeInSine(float start, float end, float value)
    {
        end -= start;
        return -end * Mathf.Cos(value / 1 * (Mathf.PI / 2)) + end + start;
    }

    public static float easeOutSine(float start, float end, float value)
    {
        end -= start;
        return end * Mathf.Sin(value / 1 * (Mathf.PI / 2)) + start;
    }

    public static float easeInOutSine(float start, float end, float value)
    {
        end -= start;
        return -end / 2 * (Mathf.Cos(Mathf.PI * value / 1) - 1) + start;
    }

    public static float easeInExpo(float start, float end, float value)
    {
        end -= start;
        return end * Mathf.Pow(2, 10 * (value / 1 - 1)) + start;
    }

    public static float easeOutExpo(float start, float end, float value)
    {
        end -= start;
        return end * (-Mathf.Pow(2, -10 * value / 1) + 1) + start;
    }

    public static float easeInOutExpo(float start, float end, float value)
    {
        value /= .5f;
        end -= start;
        if (value < 1) return end / 2 * Mathf.Pow(2, 10 * (value - 1)) + start;
        value--;
        return end / 2 * (-Mathf.Pow(2, -10 * value) + 2) + start;
    }

    public static float easeInCirc(float start, float end, float value)
    {
        end -= start;
        return -end * (Mathf.Sqrt(1 - value * value) - 1) + start;
    }

    public static float easeOutCirc(float start, float end, float value)
    {
        value--;
        end -= start;
        return end * Mathf.Sqrt(1 - value * value) + start;
    }

    public static float easeInOutCirc(float start, float end, float value)
    {
        value /= .5f;
        end -= start;
        if (value < 1) return -end / 2 * (Mathf.Sqrt(1 - value * value) - 1) + start;
        value -= 2;
        return end / 2 * (Mathf.Sqrt(1 - value * value) + 1) + start;
    }

    /* GFX47 MOD START */
    public static float easeInBounce(float start, float end, float value)
    {
        end -= start;
        float d = 1f;
        return end - easeOutBounce(0, end, d - value) + start;
    }
    /* GFX47 MOD END */

    /* GFX47 MOD START */
    //public static float bounce(float start, float end, float value){
    public static float easeOutBounce(float start, float end, float value)
    {
        value /= 1f;
        end -= start;
        if (value < (1 / 2.75f))
        {
            return end * (7.5625f * value * value) + start;
        }
        else if (value < (2 / 2.75f))
        {
            value -= (1.5f / 2.75f);
            return end * (7.5625f * (value) * value + .75f) + start;
        }
        else if (value < (2.5 / 2.75))
        {
            value -= (2.25f / 2.75f);
            return end * (7.5625f * (value) * value + .9375f) + start;
        }
        else
        {
            value -= (2.625f / 2.75f);
            return end * (7.5625f * (value) * value + .984375f) + start;
        }
    }
    /* GFX47 MOD END */

    /* GFX47 MOD START */
    public static float easeInOutBounce(float start, float end, float value)
    {
        end -= start;
        float d = 1f;
        if (value < d / 2) return easeInBounce(0, end, value * 2) * 0.5f + start;
        else return easeOutBounce(0, end, value * 2 - d) * 0.5f + end * 0.5f + start;
    }
    /* GFX47 MOD END */

    public static float easeInBack(float start, float end, float value)
    {
        end -= start;
        value /= 1;
        float s = 1.70158f;
        return end * (value) * value * ((s + 1) * value - s) + start;
    }

    public static float easeOutBack(float start, float end, float value)
    {
        float s = 1.70158f;
        end -= start;
        value = (value / 1) - 1;
        return end * ((value) * value * ((s + 1) * value + s) + 1) + start;
    }

    public static float easeInOutBack(float start, float end, float value)
    {
        float s = 1.70158f;
        end -= start;
        value /= .5f;
        if ((value) < 1)
        {
            s *= (1.525f);
            return end / 2 * (value * value * (((s) + 1) * value - s)) + start;
        }
        value -= 2;
        s *= (1.525f);
        return end / 2 * ((value) * value * (((s) + 1) * value + s) + 2) + start;
    }

    public static float punch(float amplitude, float value)
    {
        float s = 9;
        if (value == 0)
        {
            return 0;
        }
        if (value == 1)
        {
            return 0;
        }
        float period = 1 * 0.3f;
        s = period / (2 * Mathf.PI) * Mathf.Asin(0);
        return (amplitude * Mathf.Pow(2, -10 * value) * Mathf.Sin((value * 1 - s) * (2 * Mathf.PI) / period));
    }

    /* GFX47 MOD START */
    public static float easeInElastic(float start, float end, float value)
    {
        end -= start;

        float d = 1f;
        float p = d * .3f;
        float s = 0;
        float a = 0;

        if (value == 0) return start;

        if ((value /= d) == 1) return start + end;

        if (a == 0f || a < Mathf.Abs(end))
        {
            a = end;
            s = p / 4;
        }
        else
        {
            s = p / (2 * Mathf.PI) * Mathf.Asin(end / a);
        }

        return -(a * Mathf.Pow(2, 10 * (value -= 1)) * Mathf.Sin((value * d - s) * (2 * Mathf.PI) / p)) + start;
    }
    /* GFX47 MOD END */

    /* GFX47 MOD START */
    //public static float elastic(float start, float end, float value){
    public static float easeOutElastic(float start, float end, float value)
    {
        /* GFX47 MOD END */
        //Thank you to rafael.marteleto for fixing this as a port over from Pedro's UnityTween
        end -= start;

        float d = 1f;
        float p = d * .3f;
        float s = 0;
        float a = 0;

        if (value == 0) return start;

        if ((value /= d) == 1) return start + end;

        if (a == 0f || a < Mathf.Abs(end))
        {
            a = end;
            s = p / 4;
        }
        else
        {
            s = p / (2 * Mathf.PI) * Mathf.Asin(end / a);
        }

        return (a * Mathf.Pow(2, -10 * value) * Mathf.Sin((value * d - s) * (2 * Mathf.PI) / p) + end + start);
    }

    /* GFX47 MOD START */
    public static float easeInOutElastic(float start, float end, float value)
    {
        end -= start;

        float d = 1f;
        float p = d * .3f;
        float s = 0;
        float a = 0;

        if (value == 0) return start;

        if ((value /= d / 2) == 2) return start + end;

        if (a == 0f || a < Mathf.Abs(end))
        {
            a = end;
            s = p / 4;
        }
        else
        {
            s = p / (2 * Mathf.PI) * Mathf.Asin(end / a);
        }

        if (value < 1) return -0.5f * (a * Mathf.Pow(2, 10 * (value -= 1)) * Mathf.Sin((value * d - s) * (2 * Mathf.PI) / p)) + start;
        return a * Mathf.Pow(2, -10 * (value -= 1)) * Mathf.Sin((value * d - s) * (2 * Mathf.PI) / p) * 0.5f + end + start;
    }
    /* GFX47 MOD END */


    public static float getEasedVal(TweenEaseType tweenEaseType, float delta)
    {
        switch (tweenEaseType)
        {
            case TweenEaseType.easeInQuad:
                return SGame_TweenUtils.easeInQuad(0, 1, delta);
            case TweenEaseType.easeOutQuad:
                return SGame_TweenUtils.easeOutQuad(0, 1, delta);
            case TweenEaseType.easeInOutQuad:
                return SGame_TweenUtils.easeInOutQuad(0, 1, delta);
            case TweenEaseType.easeInCubic:
                return SGame_TweenUtils.easeInQuad(0, 1, delta);
            case TweenEaseType.easeOutCubic:
                return SGame_TweenUtils.easeOutCubic(0, 1, delta);
            case TweenEaseType.easeInOutCubic:
                return SGame_TweenUtils.easeInOutCubic(0, 1, delta);
            case TweenEaseType.easeInQuart:
                return SGame_TweenUtils.easeInQuart(0, 1, delta);
            case TweenEaseType.easeOutQuart:
                return SGame_TweenUtils.easeOutQuart(0, 1, delta);
            case TweenEaseType.easeInOutQuart:
                return SGame_TweenUtils.easeInOutQuart(0, 1, delta);
            case TweenEaseType.easeInQuint:
                return SGame_TweenUtils.easeInQuint(0, 1, delta);
            case TweenEaseType.easeOutQuint:
                return SGame_TweenUtils.easeOutQuint(0, 1, delta);
            case TweenEaseType.easeInOutQuint:
                return SGame_TweenUtils.easeInOutQuint(0, 1, delta);
            case TweenEaseType.easeInSine:
                return SGame_TweenUtils.easeInQuad(0, 1, delta);
            case TweenEaseType.easeOutSine:
                return SGame_TweenUtils.easeOutSine(0, 1, delta);
            case TweenEaseType.easeInOutSine:
                return SGame_TweenUtils.easeInOutSine(0, 1, delta);
            case TweenEaseType.easeInExpo:
                return SGame_TweenUtils.easeInExpo(0, 1, delta);
            case TweenEaseType.easeOutExpo:
                return SGame_TweenUtils.easeOutExpo(0, 1, delta);
            case TweenEaseType.easeInOutExpo:
                return SGame_TweenUtils.easeInOutExpo(0, 1, delta);
            case TweenEaseType.easeInCirc:
                return SGame_TweenUtils.easeInCirc(0, 1, delta);
            case TweenEaseType.easeOutCirc:
                return SGame_TweenUtils.easeOutCirc(0, 1, delta);
            case TweenEaseType.easeInOutCirc:
                return SGame_TweenUtils.easeInOutCirc(0, 1, delta);
            case TweenEaseType.linear:
                return SGame_TweenUtils.linear(0, 1, delta);
            case TweenEaseType.spring:
                return SGame_TweenUtils.spring(0, 1, delta);
            /* GFX47 MOD START */
            /*case EaseType.bounce:
                ease = new EasingFunction(bounce);
                break;*/
            case TweenEaseType.easeInBounce:
                return SGame_TweenUtils.easeInBounce(0, 1, delta);
            case TweenEaseType.easeOutBounce:
                return SGame_TweenUtils.easeOutBounce(0, 1, delta);
            case TweenEaseType.easeInOutBounce:
                return SGame_TweenUtils.easeInOutBounce(0, 1, delta);
            /* GFX47 MOD END */
            case TweenEaseType.easeInBack:
                return SGame_TweenUtils.easeInBack(0, 1, delta);
            case TweenEaseType.easeOutBack:
                return SGame_TweenUtils.easeOutBack(0, 1, delta);
            case TweenEaseType.easeInOutBack:
                return SGame_TweenUtils.easeInOutBack(0, 1, delta);
            /* GFX47 MOD START */
            /*case EaseType.elastic:
                ease = new EasingFunction(elastic);
                break;*/
            case TweenEaseType.easeInElastic:
                return SGame_TweenUtils.easeInElastic(0, 1, delta);
            case TweenEaseType.easeOutElastic:
                return SGame_TweenUtils.easeOutElastic(0, 1, delta);
            case TweenEaseType.easeInOutElastic:
                return SGame_TweenUtils.easeInOutElastic(0, 1, delta);
            /* GFX47 MOD END */
        }
        return delta;
    }

    public static float getEasedVal(float startVal, float endVal, TweenEaseType tweenEaseType, float delta)
    {
        switch (tweenEaseType)
        {
            case TweenEaseType.easeInQuad:
                return SGame_TweenUtils.easeInQuad(startVal, endVal, delta);
            case TweenEaseType.easeOutQuad:
                return SGame_TweenUtils.easeOutQuad(startVal, endVal, delta);
            case TweenEaseType.easeInOutQuad:
                return SGame_TweenUtils.easeInOutQuad(startVal, endVal, delta);
            case TweenEaseType.easeInCubic:
                return SGame_TweenUtils.easeInQuad(startVal, endVal, delta);
            case TweenEaseType.easeOutCubic:
                return SGame_TweenUtils.easeOutCubic(startVal, endVal, delta);
            case TweenEaseType.easeInOutCubic:
                return SGame_TweenUtils.easeInOutCubic(startVal, endVal, delta);
            case TweenEaseType.easeInQuart:
                return SGame_TweenUtils.easeInQuart(startVal, endVal, delta);
            case TweenEaseType.easeOutQuart:
                return SGame_TweenUtils.easeOutQuart(startVal, endVal, delta);
            case TweenEaseType.easeInOutQuart:
                return SGame_TweenUtils.easeInOutQuart(startVal, endVal, delta);
            case TweenEaseType.easeInQuint:
                return SGame_TweenUtils.easeInQuint(startVal, endVal, delta);
            case TweenEaseType.easeOutQuint:
                return SGame_TweenUtils.easeOutQuint(startVal, endVal, delta);
            case TweenEaseType.easeInOutQuint:
                return SGame_TweenUtils.easeInOutQuint(0, endVal, delta);
            case TweenEaseType.easeInSine:
                return SGame_TweenUtils.easeInQuad(startVal, endVal, delta);
            case TweenEaseType.easeOutSine:
                return SGame_TweenUtils.easeOutSine(startVal, endVal, delta);
            case TweenEaseType.easeInOutSine:
                return SGame_TweenUtils.easeInOutSine(startVal, endVal, delta);
            case TweenEaseType.easeInExpo:
                return SGame_TweenUtils.easeInExpo(startVal, endVal, delta);
            case TweenEaseType.easeOutExpo:
                return SGame_TweenUtils.easeOutExpo(startVal, endVal, delta);
            case TweenEaseType.easeInOutExpo:
                return SGame_TweenUtils.easeInOutExpo(startVal, endVal, delta);
            case TweenEaseType.easeInCirc:
                return SGame_TweenUtils.easeInCirc(startVal, endVal, delta);
            case TweenEaseType.easeOutCirc:
                return SGame_TweenUtils.easeOutCirc(startVal, endVal, delta);
            case TweenEaseType.easeInOutCirc:
                return SGame_TweenUtils.easeInOutCirc(startVal, endVal, delta);
            case TweenEaseType.linear:
                return SGame_TweenUtils.linear(startVal, endVal, delta);
            case TweenEaseType.spring:
                return SGame_TweenUtils.spring(startVal, endVal, delta);
            /* GFX47 MOD START */
            /*case EaseType.bounce:
                ease = new EasingFunction(bounce);
                break;*/
            case TweenEaseType.easeInBounce:
                return SGame_TweenUtils.easeInBounce(startVal, endVal, delta);
            case TweenEaseType.easeOutBounce:
                return SGame_TweenUtils.easeOutBounce(startVal, endVal, delta);
            case TweenEaseType.easeInOutBounce:
                return SGame_TweenUtils.easeInOutBounce(startVal, endVal, delta);
            /* GFX47 MOD END */
            case TweenEaseType.easeInBack:
                return SGame_TweenUtils.easeInBack(startVal, endVal, delta);
            case TweenEaseType.easeOutBack:
                return SGame_TweenUtils.easeOutBack(startVal, endVal, delta);
            case TweenEaseType.easeInOutBack:
                return SGame_TweenUtils.easeInOutBack(startVal, endVal, delta);
            /* GFX47 MOD START */
            /*case EaseType.elastic:
                ease = new EasingFunction(elastic);
                break;*/
            case TweenEaseType.easeInElastic:
                return SGame_TweenUtils.easeInElastic(startVal, endVal, delta);
            case TweenEaseType.easeOutElastic:
                return SGame_TweenUtils.easeOutElastic(startVal, endVal, delta);
            case TweenEaseType.easeInOutElastic:
                return SGame_TweenUtils.easeInOutElastic(startVal, endVal, delta);
            /* GFX47 MOD END */
        }
        return delta;
    }
}
