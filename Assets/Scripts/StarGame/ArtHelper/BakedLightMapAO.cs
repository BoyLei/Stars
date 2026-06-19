// using System;
// using System.Collections;
// using System.Collections.Generic;
// using Unity.Mathematics;

using System;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class BakedLightMapAO : MonoBehaviour
{
    [Header("全局LightMap的AO控制,仅在烘焙了LightMap后有用")]
    [Header("非投影区域AO强度(值越高越ao越强)")]
    [Range(0,3)]
    public float GlobalAOL = 1f;
    [Header("亮部ao颜色")]
    public Color colorL = Color.black;
    [Header("投影区域AO强度(值越高越ao越弱)")]
    [Range(0,3)]
    public float GlobalAOD = 1f;
    [Header("暗部ao补色")]
    public Color colorD = Color.white;
    
    void Awake()
    {
        RefreshGlobalAOFactor(GlobalAOL,GlobalAOD,colorL,colorD);
    }

    private void OnEnable()
    {
        RefreshGlobalAOFactor(GlobalAOL,GlobalAOD,colorL,colorD);
    }
    
    private void OnValidate()
    {
        RefreshGlobalAOFactor(GlobalAOL,GlobalAOD,colorL,colorD);
    }

    private void OnDestroy()
    {
        RefreshGlobalAOFactor(GlobalAOL,GlobalAOD,colorL,colorD);
    }

    private void OnDisable()
    {
        RefreshGlobalAOFactor(0.01f,0.01f,Color.black,Color.white);
    }

    //刷新lightmap光照强化参数
    void RefreshGlobalAOFactor(float LightMapAOStrengthL, float LightMapAOStrengthD,Color ColorL, Color ColorD)
    {
        //设置LightMap强化相关函数
        Shader.SetGlobalFloat("_LightMapAOStrengthL", LightMapAOStrengthL);
        Shader.SetGlobalFloat("_LightMapAOStrengthD", LightMapAOStrengthD);
        Shader.SetGlobalColor("_ColorL",ColorL);
        Shader.SetGlobalColor("_ColorD",ColorD);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

