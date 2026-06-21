using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Experimental.Rendering;
using UnityEngine.UI;

public class SceneShaderLeafGUI : UnityEditor.ShaderGUI
{

    private static string DepthRimFlagName = "_DepthRingFlag";
    private int DepthRimFlag = 0; //记录是否使用深度边缘光 
    
    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        //获取目标材质球
        Material targetMat = materialEditor.target as Material;
        targetMat.EnableKeyword("_ALPHATEST_ON"); //由于是树叶 则_ALPHATEST_ON保持打开
        int depthRimFlag = targetMat.GetInt(DepthRimFlagName);
        bool depthBool =false;
        if (depthRimFlag == 1)
        {
            depthBool = true;
        }
        else
        {
            depthBool = false;
        }

        //深度边缘光暂时取消 较耗.
       // depthBool = EditorGUILayout.Toggle("是否开启深度边缘光(需要深度图正确渲染)", depthBool);

        // if (depthBool)
        // {
        //     DepthRimFlag = 1;
        //     targetMat.EnableKeyword("_DEPTH_RIM_LIGHT");
        // }
        // else
        // {
        //     DepthRimFlag = 0;
        //     targetMat.DisableKeyword("_DEPTH_RIM_LIGHT");
        // }
        //targetMat.SetInt(DepthRimFlagName,DepthRimFlag);
        //绘制shader面板不隐藏的信息
        base.OnGUI(materialEditor, properties);
    }
}