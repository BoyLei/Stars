using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
#if UNITY_EDITOR_WIN
using UnityEngine.Windows;
# endif
[ExecuteInEditMode]
public class RampGenerator : MonoBehaviour
{
    //记录ramp颜色的Tex
    //public Gradient GrandientTex;

    public int RampCount = 0;
    public int oldRampCount = 0;
    //记录漫反射过渡情况的Tex
    public int rampTexSize = 512;

    public Texture2D finalTex;  //最终输出的Tex
    public Texture2D finalTexGammma; 

    public List<Gradient> RampList; //存储RampTex的容器

    public List<RampListController> RampControllerList; //存储控制RampTex参数的容器 

    public string finalTexName = "RampTex";

    public string outPutPath = "";
    
    public Material targetMaterial;

    public class RampListController
    {
         public RampListController()
         {
             Transferrange = 0.5f;
             lightColor = Color.gray; 
             darkColor = Color.gray;
         }
        public float Transferrange;
        public Color darkColor;
        public Color lightColor;
    }
    
    
    // Start is called before the first frame update
    public void OnValidate()
    {
        finalTex = new Texture2D(rampTexSize, rampTexSize, TextureFormat.ARGB32, false);
        finalTexGammma = new Texture2D(rampTexSize, rampTexSize, TextureFormat.ARGB32, false);
        RampList = new List<Gradient>();
        RampControllerList = new List<RampListController>();
    }
    
    public void clearFinalTex()
    {
        if (finalTex != null)
        {
            for (int i = 0; i < rampTexSize; i++)
            {
                for (int j = 0; j < rampTexSize; j++)
                {
                    finalTex.SetPixel(i,j,Color.black);
                }
            }
        }
    }
    
    //
    public void  InsertRampController()
    {
        RampListController temp = new RampListController();
        //RampControllerList.Add(temp);
    }
    
    //将存储Grandient 的List中的内容存储到一张Teture2d上去.
    public void setFinalTex()
    {
        if (RampList != null)
        {
            //清理finalTex
           // clearFinalTex();
            int listCount = RampList.Count;
            //将存储了Ramp贴图数组的List中的内容迁移到finalTex上
            if (listCount >= 0)
            {
                for (int i = 0; i < listCount; i++)
                {
                    for (int k = 0; k < rampTexSize / listCount; k++)
                    {
                        for (int j = 0; j < rampTexSize; j++)
                        {
                            float sampleTime = ((float)1 / (float)rampTexSize) * j;
                            Color rampColor = Color.white;
                            rampColor.r = RampList[listCount - 1 - i].Evaluate(sampleTime).r;
                            rampColor.g = RampList[listCount - 1 - i].Evaluate(sampleTime).g;
                            rampColor.b = RampList[listCount - 1 - i].Evaluate(sampleTime).b;
                            rampColor.a = RampList[listCount - 1 - i].Evaluate(sampleTime).a;
                            finalTex.SetPixel(j,k + i * rampTexSize / listCount,rampColor);
                        }
                    }
                }
            }
        }
        finalTex.Apply();
        finalTex.wrapMode = TextureWrapMode.Clamp;
    }
    //读取Texture2d某一纵坐标下所有像素的值,设置对应颜色的Grandient
    // public void setGradient(float v, out Gradient gradient )
    // {
    //     
    // }
    //经过伽马矫正过后生成图像
    public void setFinalTexGammas()
    {
        if (RampList != null)
        {
            //清理finalTex
            // clearFinalTex();
            int listCount = RampList.Count;
            //将存储了Ramp贴图数组的List中的内容迁移到finalTex上
            if (listCount >= 0)
            {
                for (int i = 0; i < listCount; i++)
                {
                    for (int k = 0; k < rampTexSize / listCount; k++)
                    {
                        for (int j = 0; j < rampTexSize; j++)
                        {
                            float sampleTime = ((float)1 / (float)rampTexSize) * j;
                            Color rampColor = Color.white;
                            rampColor.r = RampList[listCount - 1 - i].Evaluate(sampleTime).r;
                            rampColor.g = RampList[listCount - 1 - i].Evaluate(sampleTime).g;
                            rampColor.b = RampList[listCount - 1 - i].Evaluate(sampleTime).b;
                            rampColor.a = RampList[listCount - 1 - i].Evaluate(sampleTime).a;
                            rampColor.r =(float)Math.Pow(rampColor.r, 2.2);
                            rampColor.g =(float)Math.Pow(rampColor.g, 2.2);
                            rampColor.b =(float)Math.Pow(rampColor.b, 2.2);
                            rampColor.a =(float)Math.Pow(rampColor.a, 2.2);
                            finalTexGammma.SetPixel(j,k + i * rampTexSize / listCount,rampColor);
                        }
                    }
                }
            }
        }
        finalTexGammma.Apply();
        finalTexGammma.wrapMode = TextureWrapMode.Clamp;
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}

