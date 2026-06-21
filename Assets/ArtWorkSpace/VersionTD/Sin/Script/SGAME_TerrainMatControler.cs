using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SGAME_TerrainMatControler : MonoBehaviour
{
    public Material TerrainBaseMat;

    public Material TerrainAddMat;
    
    private int frameCount = 500;
    // Start is called before the first frame update
    void Start()
    {
        RefreshMatParm();
    }

    // Update is called once per frame
    void Update()
    {
        frameCount--;
        if (frameCount < 0)
        {
            frameCount = 500;
            RefreshMatParm();
        }
    }

    void RefreshMatParm()
    {
        Material targetMat;
        
        //刷新基础层材质的属性
        if (!ReferenceEquals(TerrainBaseMat,null))
        { 
           targetMat = TerrainBaseMat;
           targetMat.SetFloat("_NormalScale0",1);
           targetMat.SetFloat("_NormalScale1",0.5f);
           targetMat.SetFloat("_NormalScale2",1);
           targetMat.SetFloat("_NormalScale3",0.2f);
           targetMat.SetFloat("_NumLayersCount", 10);
                 
           targetMat.SetVector("_Splat0_ST", new Vector4(35f, 35f,0, 0));
           targetMat.SetVector("_Splat1_ST", new Vector4(46.66667f, 46.66667f,0, 0));
           targetMat.SetVector("_Splat2_ST", new Vector4(46.66667f, 46.66667f,0, 0));
           targetMat.SetVector("_Splat3_ST", new Vector4(17.5f, 17.5f,0, 0));
           targetMat.SetVector("_DiffuseRemapScale0", new Vector4(0.353184f,0.5f,0.3042453f,1.0f));
           targetMat.SetVector("_DiffuseRemapScale1", new Vector4(0.5849056f,0.5100835f,0.3669455f,0f));
           targetMat.SetVector("_DiffuseRemapScale2", new Vector4(1,1,1,1));
           targetMat.SetVector("_DiffuseRemapScale3", new Vector4(0.7075472f,0.6970015f,0.4438857f,1.0f));
                   
           targetMat.SetVector("_MaskMapRemapScale0", new Vector4(1f,1f,1f,1.0f));
           targetMat.SetVector("_MaskMapRemapScale1", new Vector4(1f,1f,1f,1.0f));
           targetMat.SetVector("_MaskMapRemapScale2", new Vector4(1f,1f,1f,1.0f));
           targetMat.SetVector("_MaskMapRemapScale3", new Vector4(1f,0.6f,1f,1.0f));  
        }
        
        //刷新叠加层的属性
        if (!ReferenceEquals(TerrainAddMat,null))
        {
            targetMat = TerrainAddMat;
            targetMat.SetFloat("_NormalScale0",1);
            targetMat.SetFloat("_NormalScale1",1f);
            targetMat.SetFloat("_NormalScale2",1);
            targetMat.SetFloat("_NormalScale3",1f);
              
            targetMat.SetVector("_Splat0_ST", new Vector4(23f, 23f,-0.03666667f, -0.08f));
            targetMat.SetVector("_Splat1_ST", new Vector4(35, 35,0, 0));
            targetMat.SetVector("_Splat2_ST", new Vector4(35, 35,0, 0));
            targetMat.SetVector("_Splat3_ST", new Vector4(35, 35,0, 0));
            targetMat.SetVector("_DiffuseRemapScale0", new Vector4(1,0.9860737f,0.8537736f,1.0f));
            targetMat.SetVector("_DiffuseRemapScale1", new Vector4(0.3157263f,0.4056604f,0.3893087f,0f));
            targetMat.SetVector("_DiffuseRemapScale2", new Vector4(1,0.9860737f,0.8537736f,1));
            targetMat.SetVector("_DiffuseRemapScale3", new Vector4(0.3157263f,0.4056604f,0.3893087f,1.0f));
                
            targetMat.SetVector("_MaskMapRemapScale0", new Vector4(0.687881f,0.292f,1f,0.4537918f));
            targetMat.SetVector("_MaskMapRemapScale1", new Vector4(1f,1f,1f,1.0f));
            targetMat.SetVector("_MaskMapRemapScale2", new Vector4(1f,1f,1f,1.0f));
            targetMat.SetVector("_MaskMapRemapScale3", new Vector4(1f,1f,1f,1.0f));
        }
    }
}
