using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class LineRenderHelper : MonoBehaviour
{
    private float TextureUScale=1;
    private float TextureUValue=0;
    private float TextureVScale=1;
    private float TextureVValue=0;

    //[Range(0, 1)]
    // public float UVTest = 0;
    [Range(1, 20)] 
    public float OriginalLength = 1;
    //public Vector2 TextureUVScale;
    
    private LineRenderer thisLineRenderer;

    private Material thisMat;

    private Vector3[] Positions;

    private float LineRendererLength;

    private Vector4 newMaskST;

    private Vector4 oldMaskST;
    // Start is called before the first frame update
    void Awake()
    {
        thisLineRenderer = this.GetComponent<LineRenderer>();
        thisMat = thisLineRenderer.sharedMaterial;
    }

    // Update is called once per frame
    void Update()
    {
        if (thisLineRenderer!=null&&thisMat!=null)
        {
            LineRendererLength = CalculateLength(thisLineRenderer);
            //Debug.Log("Length:" + LineRendererLength);
            TextureUScale = 1f / (OriginalLength);
            TextureVScale = -1;
            TextureVValue = 1;
            thisMat.SetFloat("_TextureUScale",TextureUScale);
            thisMat.SetFloat("_TextureUValue",TextureUValue);
            thisMat.SetFloat("_TextureVScale",TextureVScale);
            thisMat.SetFloat("_TextureVValue",TextureVValue);
            thisMat.SetFloat("_CustomDataToZero",0);
            
            //设置遮罩贴图
            newMaskST = thisMat.GetVector("_MaskTex_ST");
            oldMaskST = newMaskST;
            newMaskST.x = OriginalLength / LineRendererLength;
            thisMat.SetVector("_MaskTex_ST",newMaskST);
            
            //thisMat.SetFloat("_UVTest",UVTest);
        }
        else
        {
            thisLineRenderer = this.GetComponent<LineRenderer>();
            thisMat = thisLineRenderer.sharedMaterial;
        }
    }

    private void OnDisable()
    {
        if (thisLineRenderer!=null&&thisMat!=null)
        {
            //还原默认值
            thisMat.SetFloat("_TextureUScale",1);
            thisMat.SetFloat("_TextureUValue",0);
            thisMat.SetFloat("_TextureVScale",1);
            thisMat.SetFloat("_TextureVValue",0);
            thisMat.SetFloat("_CustomDataToZero",1);
            thisMat.SetVector("_MaskTex_ST",oldMaskST);
        }
    }
    
    float CalculateLength(LineRenderer line)
    {
        float totalLength = 0;
        Positions = new Vector3[line.positionCount]; 
        line.GetPositions(Positions);

        for (int i = 0; i < Positions.Length - 1; i++)
        {
            totalLength += Vector3.Distance(Positions[i], Positions[i + 1]);
        }

        Positions = null;
        return totalLength;
    }
    
}
