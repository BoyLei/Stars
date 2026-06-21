using Sirenix.OdinInspector;
using UnityEngine;

public class EasyScaleAnim : MonoBehaviour
{
    [LabelText("PlayOnEnable")]
    public bool PlayOnEnable = false;

    [LabelText("最小缩放比例")]
    public float minScale = 0.9f;

    [LabelText("最大缩放比例")]
    public float maxScale = 1.5f;

    [LabelText("每个呼吸周期的持续时间")]
    public float breathDuration = 1f;

    [LabelText("缩放次数")]
    public int scaleTime = 2;

    [LabelText("原来的大小")]
    public Vector3 originalScale = Vector3.one;

    private bool isPlaying = false;   
    private float timer = 0f;   // 计时器
    private int curTime = 0;    // 当前呼吸次数
    private bool isScalingUp = true;

    private System.Action animationCompletedCallback;   // 动画结束回调

    public void OnEnable()
    {
        if (PlayOnEnable)
        {
            StartPlay();
        }
    }

    public void Play(float _minScale, float _maxScale, float _breathDuration, int _time, System.Action completedCallback)
    {
        if (isPlaying)
            return;

        minScale = _minScale;
        maxScale = _maxScale;
        breathDuration = _breathDuration;
        animationCompletedCallback = completedCallback;
        scaleTime = _time;

        StartPlay();
    }

    [ContextMenu("播放")]
    public void StartPlay()
    {
        isPlaying = true;
        timer = 0f;
        curTime = 0;
        transform.localScale = originalScale;
        isScalingUp = true;
    }

    // 停止呼吸动画
    public void Stop()
    {
        isPlaying = false;
        transform.localScale = originalScale;
    }

    // Update is called once per frame
    private void Update()
    {
        if (!isPlaying)
            return;

        // 更新计时器
        timer += Time.deltaTime;
        // 计算缩放比例
        // 缩小时，最小不小于minScale传入的最小倍数
        float scaleRatio = Mathf.Lerp(
            isScalingUp ? minScale : maxScale,
            isScalingUp ? maxScale : Mathf.Max(originalScale.x / transform.localScale.x, minScale),
            timer / breathDuration
        );
        // 设置节点的新缩放比例
        transform.localScale = originalScale * scaleRatio;

        if (timer >= breathDuration)
        {
            timer = 0;
            // 切换缩放方向
            isScalingUp = !isScalingUp;
            if (isScalingUp)
            {
                curTime++;
            }

            if (curTime >= scaleTime)
            {
                // 动画结束时执行回调函数
                animationCompletedCallback?.Invoke();
                Stop();
            }
        }
    }
}
