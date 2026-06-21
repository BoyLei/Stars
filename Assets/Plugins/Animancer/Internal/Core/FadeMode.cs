// Animancer // https://kybernetik.com.au/animancer // Copyright 2021 Kybernetik //

using System;

namespace Animancer
{
    /// 1，由于设计的变更冗余度，和美术设计的冗余度；所以需要支持动画融合。
    /// 2，状态分成state和mixstate，bug1是融合状态不能立刻切换到新的。
    /// 3，当然也不用切换到新的。
    /// <summary>Determines how <see cref="AnimancerLayer.Play(AnimancerState, float, FadeMode)"/> works.</summary>
    /// <remarks>
    /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/blending/fading">Fading</see>
    /// <para></para>
    /// Example: <see href="https://kybernetik.com.au/animancer/docs/examples/basics/playing-and-fading">Playing and Fading</see>
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer/FadeMode
    /// 
    public enum FadeMode
    {
        /************************************************************************************************************************/

        /// <summary>
        /// Calculate the fade speed to bring the <see cref="AnimancerNode.Weight"/> from 0 to 1 over the specified
        /// fade duration (in seconds), regardless of the actual starting weight.
        /// </summary>
        ///用0.25秒切换到新的(逻辑优先0.25秒之内不能发生变化）
        ///在fadeing的时候发送指令不听，因为基于时间，动画时间小于0.25
        ///比如动画时间1秒，融合时间9秒，fading状态，在融合还让我放那就不变
        /// <example>
        /// A fade duration of 0.5 would make the fade last for 0.5 seconds, regardless of how long the animation is.
        /// <para></para>
        /// This is generally the same as <see cref="FixedDuration"/> but differs when starting the fade from a
        /// non-zero <see cref="AnimancerNode.Weight"/>, for example:
        /// <list type="bullet">
        /// <item>Fade Duration: 0.25</item>
        /// <item>To fade from 0 to 1 with either mode would get a speed of 4 and take 0.25 seconds</item>
        /// <item>To fade from 0.5 to 1 with <see cref="FixedDuration"/> would get a speed of 2 and take 0.25 seconds.
        /// It has half the distance to cover so it goes half as fast to maintain the expected duration.</item>
        /// <item>To fade from 0.5 to 1 with <see cref="FixedSpeed"/> would get a speed of 4 and take 0.125 seconds.
        /// It gets the same speed regardless of the distance to cover, so with less distance it completes faster.</item>
        /// </list>
        /// </example>
        ///
        /// <exception cref="InvalidOperationException">The <see cref="AnimancerState.Clip"/> is null.</exception>
        ///
        /// <exception cref="ArgumentOutOfRangeException">
        /// More states have been created for the <see cref="AnimancerState.Clip"/> than the
        /// <see cref="AnimancerLayer.MaxStateDepth"/> allows.
        /// </exception>
        FixedSpeed,

        /// <summary>
        /// Calculate the fade speed to bring the <see cref="AnimancerNode.Weight"/> to the target value over the
        /// specified fade duration (in seconds).
        /// </summary>
        ///
        /// <example>
        /// A fade duration of 0.5 would make the fade last for 0.5 seconds, regardless of how long the animation is.
        /// <para></para>
        /// This is generally the same as <see cref="FixedSpeed"/>, but differs when starting the fade from a
        /// non-zero <see cref="AnimancerNode.Weight"/>:
        /// <list type="bullet">
        /// <item>Fade Duration: 0.25</item>
        /// <item>To fade from 0 to 1 with either mode would get a speed of 4 and take 0.25 seconds</item>
        /// <item>To fade from 0.5 to 1 with <see cref="FixedDuration"/> would get a speed of 2 and take 0.25 seconds.
        /// It has half the distance to cover so it goes half as fast to maintain the expected duration.</item>
        /// <item>To fade from 0.5 to 1 with <see cref="FixedSpeed"/> would get a speed of 4 and take 0.125 seconds.
        /// It gets the same speed regardless of the distance to cover, so with less distance it completes faster.</item>
        /// </list>
        /// </example>
        ///
        /// <remarks>
        /// This was how fading worked prior to the introduction of <see cref="FadeMode"/>s in Animancer v4.0.
        /// </remarks>
        FixedDuration,

        /// <summary>
        /// If the state is not currently at 0 <see cref="AnimancerNode.Weight"/>, this mode will use
        /// <see cref="AnimancerLayer.GetOrCreateWeightlessState"/> to get a copy of it that is at 0 weight so it can
        /// fade the copy in while the original fades out with all other states.
        /// <para></para>
        /// Using this mode repeatedly on subsequent frames will probably have undesirable effects because it will
        /// create a new state each time. In such a situation you most likely want <see cref="FixedSpeed"/> instead.
        /// <para></para>
        /// 【This mode only works for <see cref="ClipState"/>】
        /// </summary>
        ///
        /// <example>
        /// This can be useful when you want to repeat an action while the previous animation is still fading out.
        /// For example, if you play an 'Attack' animation, it ends and starts 【fading】 back to 'Idle', and while it is
        /// doing so you want to start 【another】 'Attack'. The previous 'Attack' can't simply snap back to the start, so
        /// you can use this method to create a second 'Attack' state to fade in while the old one fades out.
        /// </example>
        FromStart,

        /// <summary>
        /// Like <see cref="FixedSpeed"/>, except that the fade duration is multiplied by the animation length.
        /// </summary>
        NormalizedSpeed,//动画的百分比进行切换，释放对他的控制权，逻辑优先并没有控制住，所以表现为input可以驱动

        /// <summary>
        /// Like <see cref="FixedDuration"/>, except that the fade duration is multiplied by the animation length.
        /// </summary>
        NormalizedDuration,

        /// <summary>
        /// Like <see cref="FromStart"/>, except that the fade duration is multiplied by the animation length.
        /// </summary>
        NormalizedFromStart,

        /************************************************************************************************************************/
    }
}

