using System;
using UnityEngine;

public enum FillingOptions
{
    //光圈扩展方向
    Middle, //从后向前
    Left,   //从左到右
    Right,  //从右到左
    Side2Middle,//从边缘到中央
    Middle2Side //从中央到边缘
}
//[ExecuteInEditMode]
public class SGAMEConeCircle : SGAMESkillShapeIndicator
{

    // Constants

    public const float CONE_ANIM_SPEED = 0.15f;

    //隐藏父类中的一些变量
    private new float Progress;


    // Fields

    public GameObject LBorder, RBorder;

    public GameObject BaseCone;
    // Properties

    private Renderer LBorderRenderer, RBorderRenderer, BaseConeRenderer;

    private MaterialPropertyBlock LBmpb, RBmpb, BCmpb;

    [SerializeField]
    public FillingOptions fillOptions = FillingOptions.Middle;
    [SerializeField]
    [Range(0, 360)]
    private float angle = 60;

    [SerializeField]
    [Range(0.5f, 50)]
    public float outsideRingRadious = 0.5f;

    //public bool insideMask;
    [SerializeField]
    [Range(0.2f, 50)]
    public float insideRingRadious = 0.2f;

    [SerializeField]
    [Range(0, 1)]
    public float fill = 0.5f;
    //展开角度
    public float Angle
    {
        get { return angle; }
        set
        {
            this.angle = value;
            SetAngle(value, outsideRingRadious, insideRingRadious, fill);
        }
    }

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

    public void Update()
    {
        SetAngle(angle, outsideRingRadious, insideRingRadious, fill);
    }
#endif

    public void OnValueChanged()
    {
        // base.OnValueChanged();
        //SetAngle(angle,outsideRingRadious,insideRingRadious,fill);
    }

    //public void OnEnable()
    //{
    //    refeshRenderers();
    //}


    private void refeshRenderers()
    {
        if (LBorder != null)
        {
            LBorderRenderer = LBorder.transform.GetComponent<Renderer>();
            Bounds bounds = LBorderRenderer.bounds;
            bounds.Expand(1000f);
            LBorderRenderer.bounds = bounds;
            LBmpb = new MaterialPropertyBlock();
        }

        if (RBorder != null)
        {
            RBorderRenderer = RBorder.transform.GetComponent<Renderer>();
            Bounds bounds = RBorderRenderer.bounds;
            bounds.Expand(1000f);
            RBorderRenderer.bounds = bounds;
            RBmpb = new MaterialPropertyBlock();
        }

        if (BaseCone != null)
        {
            BaseConeRenderer = BaseCone.transform.GetComponent<Renderer>();
            Bounds bounds = BaseConeRenderer.bounds;
            bounds.Expand(1000f);
            BaseConeRenderer.bounds = bounds;
            BCmpb = new MaterialPropertyBlock();
        }
    }
    private void SetAngle(float angle, float outsideRingRadious, float insideRingRadious, float fill)
    {
        refeshRenderers();
        //设置材质球KeyWord
        switch (fillOptions)
        {
            case FillingOptions.Left:
                BaseConeRenderer.material.EnableKeyword("_START_LEFT");
                BaseConeRenderer.material.DisableKeyword("_START_MIDDLE");
                BaseConeRenderer.material.DisableKeyword("_START_RIGHT");
                BaseConeRenderer.material.DisableKeyword("_START_SIDE2MIDDLE");
                BaseConeRenderer.material.DisableKeyword("_START_MIDDLE2SIDE");
                break;
            case FillingOptions.Middle:
                BaseConeRenderer.material.EnableKeyword("_START_MIDDLE");
                BaseConeRenderer.material.DisableKeyword("_START_LEFT");
                BaseConeRenderer.material.DisableKeyword("_START_RIGHT");
                BaseConeRenderer.material.DisableKeyword("_START_SIDE2MIDDLE");
                BaseConeRenderer.material.DisableKeyword("_START_MIDDLE2SIDE");
                break;
            case FillingOptions.Right:
                BaseConeRenderer.material.EnableKeyword("_START_RIGHT");
                BaseConeRenderer.material.DisableKeyword("_START_LEFT");
                BaseConeRenderer.material.DisableKeyword("_START_MIDDLE");
                BaseConeRenderer.material.DisableKeyword("_START_SIDE2MIDDLE");
                BaseConeRenderer.material.DisableKeyword("_START_MIDDLE2SIDE");
                break;
            case FillingOptions.Side2Middle:
                BaseConeRenderer.material.DisableKeyword("_START_RIGHT");
                BaseConeRenderer.material.DisableKeyword("_START_LEFT");
                BaseConeRenderer.material.DisableKeyword("_START_MIDDLE");
                BaseConeRenderer.material.EnableKeyword("_START_SIDE2MIDDLE");
                BaseConeRenderer.material.DisableKeyword("_START_MIDDLE2SIDE");
                break;
            case FillingOptions.Middle2Side:
                BaseConeRenderer.material.DisableKeyword("_START_RIGHT");
                BaseConeRenderer.material.DisableKeyword("_START_LEFT");
                BaseConeRenderer.material.DisableKeyword("_START_MIDDLE");
                BaseConeRenderer.material.DisableKeyword("_START_SIDE2MIDDLE");
                BaseConeRenderer.material.EnableKeyword("_START_MIDDLE2SIDE");
                break;
        }
        //设置Material参数
        if (BCmpb != null && LBmpb != null && RBmpb != null)
        {
            if (BaseConeRenderer != null)
            {
                BaseConeRenderer.GetPropertyBlock(BCmpb);
                BCmpb.SetFloat("_Expand", Normalize.GetValue(angle - 1, 360));
                BCmpb.SetFloat("_OutsideRingRadious", outsideRingRadious);
                BCmpb.SetFloat("_InsideRingRadious", insideRingRadious);
                BCmpb.SetFloat("_FillAngle", fill);
                BaseConeRenderer.SetPropertyBlock(BCmpb);
            }

            if (LBorderRenderer != null)
            {
                LBorderRenderer.GetPropertyBlock(LBmpb);
                //LBmpb.SetFloat("_Expand", Normalize.GetValue(angle - 1, 360));
                LBmpb.SetFloat("_OutsideRingRadious", outsideRingRadious);
                LBmpb.SetFloat("_InsideRingRadious", insideRingRadious);
                LBorderRenderer.SetPropertyBlock(LBmpb);
            }

            if (RBorderRenderer!=null)
            {
                RBorderRenderer.GetPropertyBlock(RBmpb);
                //RBmpb.SetFloat("_Expand", Normalize.GetValue(angle - 1, 360));
                RBmpb.SetFloat("_OutsideRingRadious", outsideRingRadious);
                RBmpb.SetFloat("_InsideRingRadious", insideRingRadious);
                RBorderRenderer.SetPropertyBlock(RBmpb);
            }

        }
        else
        {
            refeshRenderers();
        }


        LBorder.transform.localEulerAngles = new Vector3(0, -(angle + 2) / 2, 0);
        RBorder.transform.localEulerAngles = new Vector3(0, (angle + 2) / 2, 0);

    }


    public override void SetSectorRange(float radius)
    {
        outsideRingRadious = radius;

        if (BCmpb == null || LBmpb == null || RBmpb == null)
        {
            refeshRenderers();
        }

        if (BaseConeRenderer != null)
        {
            BaseConeRenderer.GetPropertyBlock(BCmpb);
            BCmpb.SetFloat("_OutsideRingRadious", outsideRingRadious);
            BaseConeRenderer.SetPropertyBlock(BCmpb);
        }

        if (LBorderRenderer != null)
        {
            LBorderRenderer.GetPropertyBlock(LBmpb);
            LBmpb.SetFloat("_OutsideRingRadious", outsideRingRadious);
            LBorderRenderer.SetPropertyBlock(LBmpb);
        }

        if (RBorderRenderer!=null)
        {
            RBorderRenderer.GetPropertyBlock(RBmpb);
            RBmpb.SetFloat("_OutsideRingRadious", outsideRingRadious);
            RBorderRenderer.SetPropertyBlock(RBmpb);
        }
    }

    public override void SetSectorAngle(float _angle)
    {
        angle = _angle;

        if (BCmpb == null || LBmpb == null || RBmpb == null)
        {
            refeshRenderers();
        }

        if (BaseConeRenderer != null)
        {
            BaseConeRenderer.GetPropertyBlock(BCmpb);
            BCmpb.SetFloat("_Expand", Normalize.GetValue(angle - 1, 360));
            BaseConeRenderer.SetPropertyBlock(BCmpb);
        }

        LBorder.transform.localEulerAngles = new Vector3(0, -(angle + 2) / 2, 0);
        RBorder.transform.localEulerAngles = new Vector3(0, (angle + 2) / 2, 0);
    }

    public override void SetRingFanMiddleRingSize(float value)
    {
        insideRingRadious = value;

        if (BCmpb == null || LBmpb == null || RBmpb == null)
        {
            refeshRenderers();
        }

        if (BaseConeRenderer != null)
        {
            BaseConeRenderer.GetPropertyBlock(BCmpb);
            BCmpb.SetFloat("_InsideRingRadious", insideRingRadious);
            BaseConeRenderer.SetPropertyBlock(BCmpb);
        }

        if (LBorderRenderer != null)
        {
            LBorderRenderer.GetPropertyBlock(LBmpb);
            LBmpb.SetFloat("_InsideRingRadious", insideRingRadious);
            LBorderRenderer.SetPropertyBlock(LBmpb);
        }

        if (RBorderRenderer != null)
        {
            RBorderRenderer.GetPropertyBlock(RBmpb);
            RBmpb.SetFloat("_InsideRingRadious", insideRingRadious);
            RBorderRenderer.SetPropertyBlock(RBmpb);
        }
    }

    public override void SetGlowRange(float value)
    {
        fill = value;

        if (BCmpb == null || LBmpb == null || RBmpb == null)
        {
            refeshRenderers();
        }

        if (BaseConeRenderer != null)
        {
            BaseConeRenderer.GetPropertyBlock(BCmpb);
            BCmpb.SetFloat("_FillAngle", fill);
            BaseConeRenderer.SetPropertyBlock(BCmpb);
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
