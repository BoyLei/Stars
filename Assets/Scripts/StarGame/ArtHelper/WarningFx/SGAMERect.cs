using System;
using UnityEngine;

public enum FillingOptionsRect
{
    Middle,
    Back,
    Center,
    Side2Middle
}

public class SGAMERect : SGAMESkillShapeIndicator
{

    // Constants

    public const float CONE_ANIM_SPEED = 0.15f;

    //隐藏父类中的一些变量
    private new float Progress;


    // Fields


    public GameObject BaseRect;
    // Properties

    private Renderer BaseRectRenderer;

    private MaterialPropertyBlock BRmpb;

    [SerializeField]
    public FillingOptionsRect fillOptions = FillingOptionsRect.Back;
    [SerializeField]
    [Range(1, 50)]
    public float Length = 1;

    [SerializeField]
    [Range(0.5f, 50)]
    public float Width = 1f;


    [SerializeField]
    [Range(0, 1)]
    public float fill = 0.5f;

    private float ScaleX, ScaleZ = 1;


    // Methods

    public class Normalize
    {
        public float Portion;
        public float Max;
        public float Factor;
        public float Value;

        public Normalize(float portion, float max)
        {
            this.Portion = portion;
            this.Max = max;
            this.Factor = Portion / Max;
            this.Value = Mathf.Clamp(Factor, 0, 1f);
        }

        public static float GetValue(float portion, float max)
        {
            return Mathf.Clamp(portion / max, 0, 1f);
        }
    }

#if UNITY_EDITOR

    //public void Update()
    //{
    //    SetValue(Length, Width, fill);

    //    SGF.Debuger.LogWarning($"矩形缩放 ={BaseRect.transform.localScale.x}");
    //}

#endif

    public void OnValueChanged()
    {
        // base.OnValueChanged();
        //SetAngle(angle,outsideRingRadious,insideRingRadious,fill);
    }

    //public void OnEnable()
    //{
    //    refeshRenderers();
    //    refeshScale();
    //}

    private void refeshScale()
    {
        float scale = Mathf.Min(Length, Width);
        BaseRect.transform.SetLocalScale(new Vector3(scale, 1, scale));

        ScaleX =  BaseRect.transform.localScale.x;
        ScaleZ =  BaseRect.transform.localScale.z;
        Length = Mathf.Max(Length, scale);
        Width = Mathf.Max(Width, scale);

        //SGF.Debuger.LogWarning($"矩形缩放1111111111 ={BaseRect.transform.localScale.x}");

    }

    private void CheckLengthAndWidth()
    {
        Length = Mathf.Max(ScaleZ, Length);
        Width = Mathf.Max(ScaleX, Width);
    }
    private void refeshRenderers()
    {

        if (BaseRect != null)
        {
            BaseRectRenderer = BaseRect.transform.GetComponent<Renderer>();
            //BaseRectRenderer.forceRenderingOff = false;
            Bounds bounds = BaseRectRenderer.bounds;
            bounds.Expand(1000f);
            BaseRectRenderer.bounds = bounds; // 手动更新Renderer的Bounds
            BRmpb = new MaterialPropertyBlock();
        }

        switch (fillOptions)
        {
            case FillingOptionsRect.Back:
                BaseRectRenderer.material.EnableKeyword("_START_BACK");
                BaseRectRenderer.material.DisableKeyword("_START_MIDDLE");
                BaseRectRenderer.material.DisableKeyword("_START_CENTER");
                BaseRectRenderer.material.DisableKeyword("_START_SIDE2MIDDLE");
                break;
            case FillingOptionsRect.Middle:
                BaseRectRenderer.material.EnableKeyword("_START_MIDDLE");
                BaseRectRenderer.material.DisableKeyword("_START_BACK");
                BaseRectRenderer.material.DisableKeyword("_START_CENTER");
                BaseRectRenderer.material.DisableKeyword("_START_SIDE2MIDDLE");
                break;
            case FillingOptionsRect.Center:
                BaseRectRenderer.material.EnableKeyword("_START_CENTER");
                BaseRectRenderer.material.DisableKeyword("_START_MIDDLE");
                BaseRectRenderer.material.DisableKeyword("_START_BACK");
                BaseRectRenderer.material.DisableKeyword("_START_SIDE2MIDDLE");
                break;
            case FillingOptionsRect.Side2Middle:
                BaseRectRenderer.material.DisableKeyword("_START_CENTER");
                BaseRectRenderer.material.DisableKeyword("_START_MIDDLE");
                BaseRectRenderer.material.DisableKeyword("_START_BACK");
                BaseRectRenderer.material.EnableKeyword("_START_SIDE2MIDDLE");
                break;
        }
    }
    private void SetValue(float Length, float Width, float fill)
    {
        refeshRenderers();
        //设置材质球KeyWord
        switch (fillOptions)
        {
            case FillingOptionsRect.Back:
                BaseRectRenderer.material.EnableKeyword("_START_BACK");
                BaseRectRenderer.material.DisableKeyword("_START_MIDDLE");
                BaseRectRenderer.material.DisableKeyword("_START_CENTER");
                BaseRectRenderer.material.DisableKeyword("_START_SIDE2MIDDLE");
                break;
            case FillingOptionsRect.Middle:
                BaseRectRenderer.material.EnableKeyword("_START_MIDDLE");
                BaseRectRenderer.material.DisableKeyword("_START_BACK");
                BaseRectRenderer.material.DisableKeyword("_START_CENTER");
                BaseRectRenderer.material.DisableKeyword("_START_SIDE2MIDDLE");
                break;
            case FillingOptionsRect.Center:
                BaseRectRenderer.material.EnableKeyword("_START_CENTER");
                BaseRectRenderer.material.DisableKeyword("_START_MIDDLE");
                BaseRectRenderer.material.DisableKeyword("_START_BACK");
                BaseRectRenderer.material.DisableKeyword("_START_SIDE2MIDDLE");
                break;
            case FillingOptionsRect.Side2Middle:
                BaseRectRenderer.material.DisableKeyword("_START_CENTER");
                BaseRectRenderer.material.DisableKeyword("_START_MIDDLE");
                BaseRectRenderer.material.DisableKeyword("_START_BACK");
                BaseRectRenderer.material.EnableKeyword("_START_SIDE2MIDDLE");
                break;
        }
        //设置Material参数
        if (BRmpb != null)
        {
            if (BaseRectRenderer != null)
            {
                CheckLengthAndWidth();
                BaseRectRenderer.GetPropertyBlock(BRmpb);
                BRmpb.SetFloat("_ScaleTop", (Length-ScaleZ)/ScaleZ);
                BRmpb.SetFloat("_ScaleLeftAndRight", (Width-ScaleX)/ScaleX);
                BRmpb.SetFloat("_FillAngle", fill);
                BaseRectRenderer.SetPropertyBlock(BRmpb);
            }


        }
        else
        {
            refeshRenderers();
        }

    }

    public override void SetRectWidthAndLength(float width, float length)
    {
        Width = width;
        Length = length;

        refeshScale();

        if (BRmpb == null || BaseRectRenderer == null)
        {
            refeshRenderers();
        }
        //SGF.Debuger.LogWarning($"矩形缩放 22222222222222 ={BaseRect.transform.localScale.x}");

        if (BaseRectRenderer != null && BRmpb != null)
        {
            BaseRectRenderer.GetPropertyBlock(BRmpb);
            BRmpb.SetFloat("_ScaleTop", (Length-ScaleZ)/ScaleZ);
            BRmpb.SetFloat("_ScaleLeftAndRight", (Width-ScaleX)/ScaleX);
            BRmpb.SetFloat("_FillAngle", fill);
            BaseRectRenderer.SetPropertyBlock(BRmpb);
        }
    }

    public override void SetGlowRange(float value)
    {
        fill = value;

        if (BRmpb == null || BaseRectRenderer == null)
        {
            refeshRenderers();
        }

        if (BaseRectRenderer != null)
        {
            BaseRectRenderer.GetPropertyBlock(BRmpb);
            BRmpb.SetFloat("_FillAngle", fill);
            BaseRectRenderer.SetPropertyBlock(BRmpb);
        }
    }



    /// <summary>
    /// Optional animation when Cone is made visible.
    /// </summary>
    // private IEnumerator FadeIn() {
    //   float final = angle;
    //   float current = 0;
    //
    //   foreach (Projector p in Projectors)
    //     p.enabled = true;
    //
    //   while (current < final) {
    //     SetAngle(current);
    //     current += final * CONE_ANIM_SPEED;
    //     yield return null;
    //   }
    //   SetAngle(final);
    //   yield return null;
    // }
}
