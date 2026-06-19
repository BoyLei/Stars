using System;
using UnityEngine;

public enum FillingOptionsCircle
{
    //光圈扩展方向
  Middle,//从中央到边缘
  Side,//从边缘到中央
  ClockWise,//顺时针
  AntiClockWise//逆时针
}

public class SGAMECircle : SGAMESkillShapeIndicator
{

    // Constants

    public const float CONE_ANIM_SPEED = 0.15f;

    //隐藏父类中的一些变量
    private new float Progress;


    // Fields

    // public GameObject LBorder, RBorder;

    public GameObject BaseCone;

    public GameObject Center;
    // Properties
    
    [SerializeField]
    public FillingOptionsCircle fillOptions = FillingOptionsCircle.Middle;

    private Renderer BaseConeRenderer;

    private Renderer CenterRenderer;

    private MaterialPropertyBlock BCmpb;

    private MaterialPropertyBlock Cmpb;

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
        //SetAngle(outsideRingRadious, insideRingRadious, fill);
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
        // if (LBorder != null)
        // {
        //   LBorderRenderer = LBorder.transform.GetComponent<Renderer>();
        //   LBmpb = new MaterialPropertyBlock();
        // }
        //
        // if (RBorder != null)
        // {
        //    RBorderRenderer = RBorder.transform.GetComponent<Renderer>();
        //    RBmpb = new MaterialPropertyBlock();
        // }

        if (BaseCone != null)
        {
            BaseConeRenderer = BaseCone.transform.GetComponent<Renderer>();
            Bounds bounds = BaseConeRenderer.bounds;
            bounds.Expand(1000f);
            BaseConeRenderer.bounds = bounds;
            BCmpb = new MaterialPropertyBlock();
        }

        if (Center != null)
        {
            CenterRenderer = Center.transform.GetComponent<Renderer>();
            Bounds bounds = CenterRenderer.bounds;
            bounds.Expand(1000f);
            CenterRenderer.bounds = bounds;
            Cmpb = new MaterialPropertyBlock();
        }
    }
    private void SetAngle(float outsideRingRadious, float insideRingRadious, float fill)
    {
        refeshRenderers();
        //设置光圈运行方式
        switch (fillOptions)
        {
            case FillingOptionsCircle.Middle: //从中间到边缘
                BaseConeRenderer.material.EnableKeyword("_START_MIDDLE");
                BaseConeRenderer.material.DisableKeyword("_START_ROTATE");
                BaseConeRenderer.material.DisableKeyword("_FILL_INVERSE");
                break;
            case FillingOptionsCircle.Side: //从边缘到中间
                BaseConeRenderer.material.EnableKeyword("_START_MIDDLE");
                BaseConeRenderer.material.DisableKeyword("_START_ROTATE");
                BaseConeRenderer.material.EnableKeyword("_FILL_INVERSE");
                break;
            case FillingOptionsCircle.ClockWise://顺时针
                BaseConeRenderer.material.DisableKeyword("_START_MIDDLE");
                BaseConeRenderer.material.EnableKeyword("_START_ROTATE");
                BaseConeRenderer.material.DisableKeyword("_FILL_INVERSE");
                break;
            case FillingOptionsCircle.AntiClockWise://逆时针
                BaseConeRenderer.material.DisableKeyword("_START_MIDDLE");
                BaseConeRenderer.material.EnableKeyword("_START_ROTATE");
                BaseConeRenderer.material.EnableKeyword("_FILL_INVERSE");
                break;
        }
        //设置Material参数
        if (BCmpb != null)
        {
            if (BaseConeRenderer != null)
            {
                BaseConeRenderer.GetPropertyBlock(BCmpb);
                BCmpb.SetFloat("_OutsideRingRadious", outsideRingRadious);
                BCmpb.SetFloat("_InsideRingRadious", insideRingRadious);
                BCmpb.SetFloat("_FillAngle", fill);
                BaseConeRenderer.SetPropertyBlock(BCmpb);
            }
            if (CenterRenderer != null)
            {
                CenterRenderer.GetPropertyBlock(Cmpb);
                Cmpb.SetFloat("_OutsideRingRadious", outsideRingRadious);
                CenterRenderer.SetPropertyBlock(Cmpb);
            }
        }
        else
        {
            refeshRenderers();
        }


        // LBorder.transform.localEulerAngles = new Vector3(0, -(angle + 2) / 2, 0);
        // RBorder.transform.localEulerAngles = new Vector3(0, (angle + 2) / 2, 0);
        //
    }
    
 
    public override void SetSectorRange(float radius)
    {
        outsideRingRadious = radius;

        if (BCmpb == null)
        {
            refeshRenderers();
        }
        if (BaseConeRenderer != null)
        {
            BaseConeRenderer.GetPropertyBlock(BCmpb);
            BCmpb.SetFloat("_OutsideRingRadious", outsideRingRadious);
            BaseConeRenderer.SetPropertyBlock(BCmpb);
        }
        if (CenterRenderer != null)
        {
            CenterRenderer.GetPropertyBlock(Cmpb);
            Cmpb.SetFloat("_OutsideRingRadious", outsideRingRadious);
            CenterRenderer.SetPropertyBlock(Cmpb);
        }
    }

    public override void SetGlowRange(float value)
    {
        fill = value;

        if (BCmpb != null)
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
