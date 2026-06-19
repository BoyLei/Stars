using System;
using UnityEngine;

public enum FillingOptionsArrow
{
    Middle,
    Back,
    Center
}

public class SGAMEArrow : SGAMESkillShapeIndicator
{

    // Constants

    public const float CONE_ANIM_SPEED = 0.15f;

    // Fields


    public GameObject BaseRect;
    // Properties

    private Renderer BaseRectRenderer;

    private MaterialPropertyBlock BRmpb;

    //[SerializeField] 
    //public FillingOptionsRect fillOptions = FillingOptionsRect.Middle;
    [SerializeField]
    [Range(3, 50)]
    public float Length = 3;

    [SerializeField]
    [Range(1f, 50)]
    public float Width = 1f;


    // [SerializeField]
    // [Range(0, 1)]
    // public float fill = 0.5f;

#if UNITY_EDITOR

    public void Update()
    {
        SetValue(Length, Width);
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

        if (BaseRect != null)
        {
            BaseRectRenderer = BaseRect.transform.GetComponent<Renderer>();
            Bounds bounds = BaseRectRenderer.bounds;
            bounds.Expand(1000f);
            BaseRectRenderer.bounds = bounds;
            BRmpb = new MaterialPropertyBlock();
        }
    }
    private void SetValue(float Length, float Width)
    {
        //设置材质球KeyWord
        // switch (fillOptions)
        // {
        //   case FillingOptionsRect.Back:
        //     BaseRectRenderer.material.EnableKeyword("_START_BACK");
        //     BaseRectRenderer.material.DisableKeyword("_START_MIDDLE");
        //     BaseRectRenderer.material.DisableKeyword("_START_CENTER");
        //     break;
        //   case FillingOptionsRect.Middle:
        //     BaseRectRenderer.material.EnableKeyword("_START_MIDDLE");
        //     BaseRectRenderer.material.DisableKeyword("_START_BACK");
        //     BaseRectRenderer.material.DisableKeyword("_START_CENTER");
        //     break;
        //   case FillingOptionsRect.Center:
        //     BaseRectRenderer.material.EnableKeyword("_START_CENTER");
        //     BaseRectRenderer.material.DisableKeyword("_START_MIDDLE");
        //     BaseRectRenderer.material.DisableKeyword("_START_BACK");
        //     break;
        // }
        refeshRenderers();
        //设置Material参数
        if (BRmpb != null)
        {
            if (BaseRectRenderer != null)
            {
                if (Length / Width >= 3) //如果长宽比大于3 则使用缩放模式1
                {
                    BaseRectRenderer.material.EnableKeyword("_SCALEOFFSET1");
                    float width = Width - 1;
                    float length = Length - 3 - (3 * width);
                    //float length = Length - 3;
                    //BaseRectRenderer.GetPropertyBlock(BRmpb);
                    BRmpb.SetFloat("_ScaleTop", length);
                    BRmpb.SetFloat("_ScaleLeftAndRight", width);
                    BaseRectRenderer.SetPropertyBlock(BRmpb);
                }
                else //如果长宽比小于3 则不使用缩放模式1
                {
                    BaseRectRenderer.material.DisableKeyword("_SCALEOFFSET1");
                    float width = Width - 1;
                    //float length = Length - 3 - (3 * width);
                    float length = Length - 3;
                    //BaseRectRenderer.GetPropertyBlock(BRmpb);
                    BRmpb.SetFloat("_ScaleTop", length);
                    BRmpb.SetFloat("_ScaleLeftAndRight", width);
                    BaseRectRenderer.SetPropertyBlock(BRmpb);
                }


            }


        }
        else
        {
            refeshRenderers();
        }

    }


    public override void SetRectLength(float value)
    {
        Length = value;

        if (BRmpb == null || BaseRectRenderer == null)
        {
            refeshRenderers();
        }

        if (Length / Width >= 3) //如果长宽比大于3 则使用缩放模式1
        {
            BaseRectRenderer.material.EnableKeyword("_SCALEOFFSET1");
            float width = Width - 1;
            float length = Length - 3 - (3 * width);
            //float length = Length - 3;
            //BaseRectRenderer.GetPropertyBlock(BRmpb);
            BRmpb.SetFloat("_ScaleTop", length);
            BRmpb.SetFloat("_ScaleLeftAndRight", width);
            BaseRectRenderer.SetPropertyBlock(BRmpb);
        }
        else //如果长宽比小于3 则不使用缩放模式1
        {
            BaseRectRenderer.material.DisableKeyword("_SCALEOFFSET1");
            float width = Width - 1;
            //float length = Length - 3 - (3 * width);
            float length = Length - 3;
            //BaseRectRenderer.GetPropertyBlock(BRmpb);
            BRmpb.SetFloat("_ScaleTop", length);
            BRmpb.SetFloat("_ScaleLeftAndRight", width);
            BaseRectRenderer.SetPropertyBlock(BRmpb);
        }
    }

    public override void SetRectRange(float value)
    {
        Width = value;

        if (BRmpb == null || BaseRectRenderer == null)
        {
            refeshRenderers();
        }

        if (Length / Width >= 3) //如果长宽比大于3 则使用缩放模式1
        {
            BaseRectRenderer.material.EnableKeyword("_SCALEOFFSET1");
            float width = Width - 1;
            float length = Length - 3 - (3 * width);
            //float length = Length - 3;
            //BaseRectRenderer.GetPropertyBlock(BRmpb);
            BRmpb.SetFloat("_ScaleTop", length);
            BRmpb.SetFloat("_ScaleLeftAndRight", width);
            BaseRectRenderer.SetPropertyBlock(BRmpb);
        }
        else //如果长宽比小于3 则不使用缩放模式1
        {
            BaseRectRenderer.material.DisableKeyword("_SCALEOFFSET1");
            float width = Width - 1;
            //float length = Length - 3 - (3 * width);
            float length = Length - 3;
            //BaseRectRenderer.GetPropertyBlock(BRmpb);
            BRmpb.SetFloat("_ScaleTop", length);
            BRmpb.SetFloat("_ScaleLeftAndRight", width);
            BaseRectRenderer.SetPropertyBlock(BRmpb);
        }
    }

}
