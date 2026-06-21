using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Experimental.Rendering;


    public class SGAMETerrainLitShaderAddGUI : UnityEditor.ShaderGUI
    { 
        bool ifAddPass = true;
        public void SetKeyword(Material targetMat, bool toggle, string keyWord)
        {
            if(toggle == true)
                targetMat.EnableKeyword(keyWord);
            else
                targetMat.DisableKeyword(keyWord);
        }
        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
      {
          
          
          //获取目标材质球
          Material targetMat = materialEditor.target as Material;
          string[] keyWords = targetMat.shaderKeywords;
          bool TerrainBasePass = keyWords.Contains("TERRAIN_SPLAT_BASEPASS");
          bool TerrainBlendHeight = keyWords.Contains("_TERRAIN_BLEND_HEIGHT");
          bool NormalMap = keyWords.Contains("_NORMALMAP");
          bool MaskMap = keyWords.Contains("_MASKMAP");
          bool TerrainInstancePerpixelNormal = keyWords.Contains("_TERRAIN_INSTANCED_PERPIXEL_NORMAL");
          
          //绘制勾选按钮 控制material Keyword的开关
          EditorGUI.BeginChangeCheck();
          
          
          ifAddPass = EditorGUILayout.Toggle("是否是叠加层?", ifAddPass);
          TerrainBlendHeight = EditorGUILayout.Toggle("TerrainHeight", TerrainBlendHeight);
          NormalMap = EditorGUILayout.Toggle("NormalMap", NormalMap);
          MaskMap = EditorGUILayout.Toggle("MaskMap", MaskMap);
          TerrainInstancePerpixelNormal = EditorGUILayout.Toggle("Terrain Instanced Perpixel Normal", TerrainInstancePerpixelNormal);
          
          if (EditorGUI.EndChangeCheck())
          {
              SetKeyword(targetMat, TerrainBasePass, "TERRAIN_SPLAT_BASEPASS");
              SetKeyword(targetMat, TerrainBlendHeight, "_TERRAIN_BLEND_HEIGHT");
              SetKeyword(targetMat, NormalMap, "_NORMALMAP");
              SetKeyword(targetMat, MaskMap, "_MASKMAP");
              SetKeyword(targetMat, TerrainInstancePerpixelNormal, "_TERRAIN_INSTANCED_PERPIXEL_NORMAL");

             
          }
          if (ifAddPass == false)
          {
              //设置基础层的一些参数
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
          else
          {
              //设置叠加层的一些参数
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
          
          //绘制shader基础属性
          base.OnGUI(materialEditor, properties);
      }
    }     





