using System;
using System.Linq;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using UnityEngine;
using UnityEditor;

public class SGAMETerrainLitShaderGUI : UnityEditor.ShaderGUI
{

    private float _NormalScale0;
    private float _NormalScale1;
    private float _NormalScale2;
    private float _NormalScale3;

    private Vector4 _Splat0_ST;
    private Vector4 _Splat1_ST;
    private Vector4 _Splat2_ST;
    private Vector4 _Splat3_ST;

    
    private MaterialProperty Control;

    private MaterialProperty Layer3;
    private MaterialProperty Layer2;
    private MaterialProperty Layer1;
    private MaterialProperty Layer0;

    private MaterialProperty Normal3;
    private MaterialProperty Normal2;
    private MaterialProperty Normal1;
    private MaterialProperty Normal0;


    public void SetKeyword(Material targetMat, bool toggle, string keyWord)
    {
        if (toggle == true)
            targetMat.EnableKeyword(keyWord);
        else
            targetMat.DisableKeyword(keyWord);
    }
    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        //获取目标材质球
        Material targetMat = materialEditor.target as Material;
        string[] keyWords = targetMat.shaderKeywords;
        bool NormalMap = keyWords.Contains("_NORMALMAP");

        //刷新面板里材质球的信息
        _NormalScale0 = targetMat.GetFloat("_NormalScale0");
        _NormalScale1 = targetMat.GetFloat("_NormalScale1");
        _NormalScale2 = targetMat.GetFloat("_NormalScale2");
        _NormalScale3 = targetMat.GetFloat("_NormalScale3");

        _Splat0_ST = targetMat.GetVector("_Splat0_ST");
        _Splat1_ST = targetMat.GetVector("_Splat1_ST");
        _Splat2_ST = targetMat.GetVector("_Splat2_ST");
        _Splat3_ST = targetMat.GetVector("_Splat3_ST");

        //_DiffuseRemapScale0 = targetMat.GetVector("_DiffuseRemapScale0");
        //_DiffuseRemapScale1 = targetMat.GetVector("_DiffuseRemapScale1");
        //_DiffuseRemapScale2 = targetMat.GetVector("_DiffuseRemapScale2");
        //_DiffuseRemapScale3 = targetMat.GetVector("_DiffuseRemapScale3");

        //绘制勾选按钮 控制material Keyword的开关
        //EditorGUI.BeginChangeCheck();
        //NormalMap = EditorGUILayout.Toggle("是否启用法线贴图", NormalMap);

        //_NormalScale0 = EditorGUILayout.Slider("Normal Strength 0", _NormalScale0, -5, 5);
        //_Splat0_ST = EditorGUILayout.Vector4Field("Splat Tiling & Offset 0", _Splat0_ST);

        //_NormalScale1 = EditorGUILayout.Slider("Normal Strength 1", _NormalScale1, -5, 5);
        //_Splat1_ST = EditorGUILayout.Vector4Field("Splat Tiling & Offset 1", _Splat1_ST);

        //_NormalScale2 = EditorGUILayout.Slider("Normal Strength 2", _NormalScale2, -5, 5);
        //_Splat2_ST = EditorGUILayout.Vector4Field("Splat Tiling & Offset 2", _Splat2_ST);

        //_NormalScale3 = EditorGUILayout.Slider("Normal Strength 3", _NormalScale3, -5, 5);
        //_Splat3_ST = EditorGUILayout.Vector4Field("Splat Tiling & Offset 3", _Splat3_ST);

        //_NormalScale4 = EditorGUILayout.FloatField("_NormalScale4", _NormalScale4);
        //_NormalScale5 = EditorGUILayout.FloatField("_NormalScale5", _NormalScale5);

        //_Splat4_ST = EditorGUILayout.Vector4Field("_Splat4_ST", _Splat4_ST);
        //_Splat5_ST = EditorGUILayout.Vector4Field("_Splat5_ST", _Splat5_ST);

        //_DiffuseRemapScale0 = EditorGUILayout.Vector4Field("_DiffuseRemapScale0", _DiffuseRemapScale0);
        //_DiffuseRemapScale1 = EditorGUILayout.Vector4Field("_DiffuseRemapScale1", _DiffuseRemapScale1);
        //_DiffuseRemapScale2 = EditorGUILayout.Vector4Field("_DiffuseRemapScale2", _DiffuseRemapScale2);
        //_DiffuseRemapScale3 = EditorGUILayout.Vector4Field("_DiffuseRemapScale3", _DiffuseRemapScale3);
        //_DiffuseRemapScale4 = EditorGUILayout.Vector4Field("_DiffuseRemapScale4", _DiffuseRemapScale4);
        //_DiffuseRemapScale5 = EditorGUILayout.Vector4Field("_DiffuseRemapScale5", _DiffuseRemapScale5);


        //设置材质球参数
        // targetMat.SetFloat("_NormalScale0", _NormalScale0);
        // targetMat.SetFloat("_NormalScale1", _NormalScale1);
        // targetMat.SetFloat("_NormalScale2", _NormalScale2);
        // targetMat.SetFloat("_NormalScale3", _NormalScale3);
        //targetMat.SetFloat("_NormalScale4", _NormalScale4);
        //targetMat.SetFloat("_NormalScale5", _NormalScale5);

        // targetMat.SetVector("_Splat0_ST", _Splat0_ST);
        // targetMat.SetVector("_Splat1_ST", _Splat1_ST);
        // targetMat.SetVector("_Splat2_ST", _Splat2_ST);
        // targetMat.SetVector("_Splat3_ST", _Splat3_ST);
        //targetMat.SetVector("_Splat4_ST", _Splat4_ST);
        //targetMat.SetVector("_Splat5_ST", _Splat5_ST);

        //targetMat.SetVector("_DiffuseRemapScale0", _DiffuseRemapScale0);
        //targetMat.SetVector("_DiffuseRemapScale1", _DiffuseRemapScale1);
        //targetMat.SetVector("_DiffuseRemapScale2", _DiffuseRemapScale2);
        //targetMat.SetVector("_DiffuseRemapScale3", _DiffuseRemapScale3);
        //targetMat.SetVector("_DiffuseRemapScale4", _DiffuseRemapScale4);
        //targetMat.SetVector("_DiffuseRemapScale5", _DiffuseRemapScale5);
        
        

        //绘制shader基础属性
        base.OnGUI(materialEditor, properties);

        
        //将面板中的Color转换成Diffuse Remap
        Vector4 Color0 = targetMat.GetVector("_Color0");
        Vector4 Color1 = targetMat.GetVector("_Color1");
        Vector4 Color2 = targetMat.GetVector("_Color2");
        Vector4 Color3 = targetMat.GetVector("_Color3");

        float powerFactor = 1;
        Vector4 DiffuseRemapScale0 = new Vector4((float)Math.Pow(Color0.x, powerFactor),(float)Math.Pow(Color0.y, powerFactor),(float)Math.Pow(Color0.z, powerFactor),1);
        Vector4 DiffuseRemapScale1 = new Vector4((float)Math.Pow(Color1.x, powerFactor),(float)Math.Pow(Color1.y, powerFactor),(float)Math.Pow(Color1.z, powerFactor),1);
        Vector4 DiffuseRemapScale2 = new Vector4((float)Math.Pow(Color2.x, powerFactor),(float)Math.Pow(Color2.y, powerFactor),(float)Math.Pow(Color2.z, powerFactor),1);
        Vector4 DiffuseRemapScale3 = new Vector4((float)Math.Pow(Color3.x, powerFactor),(float)Math.Pow(Color3.y, powerFactor),(float)Math.Pow(Color3.z, powerFactor),1);
        
        targetMat.SetVector("_DiffuseRemapScale0", DiffuseRemapScale0);
        targetMat.SetVector("_DiffuseRemapScale1", DiffuseRemapScale1);
        targetMat.SetVector("_DiffuseRemapScale2", DiffuseRemapScale2);
        targetMat.SetVector("_DiffuseRemapScale3", DiffuseRemapScale3);
        //if (EditorGUI.EndChangeCheck())
        //{
        //    SetKeyword(targetMat, NormalMap, "_NORMALMAP");
        //}
    }

    public void FindProperties(MaterialProperty[] props)
    {
        Control = FindProperty("_Control", props);
    }
}





