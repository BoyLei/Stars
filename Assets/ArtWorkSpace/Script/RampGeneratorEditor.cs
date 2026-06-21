using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

//界面部分
#if UNITY_EDITOR
[CustomEditor(typeof(RampGenerator))]
[CanEditMultipleObjects]
public class RampGeneratorEditor : Editor
{
    
    private SerializedProperty rampTextureSize;

    private RampGenerator rampGenerater;
    
    private void OnEnable()
    {
        rampTextureSize = serializedObject.FindProperty("RampTexSize");
        //throw new NotImplementedException();
        rampGenerater = (RampGenerator) target;
        rampGenerater.RampCount = rampGenerater.RampList.Count;
        rampGenerater.oldRampCount = rampGenerater.RampCount;
    }

    void ShowRampTex()
    {
        EditorGUILayout.LabelField("Ramp图集合 顺序由上至下");
        for (int i = 0; i < rampGenerater.RampList.Count; i++)
        {
            EditorGUILayout.TextField("第" + (i + 1).ToString() + "张Ramp贴图:");
            rampGenerater.RampList[i] = EditorGUILayout.GradientField(rampGenerater.RampList[i]);
        }
            
    }
    
    public override void OnInspectorGUI()
    {
        //serializedObject.Update();
        //serializedObject.ApplyModifiedProperties();
        //绘制贴图名字和存储路径
        //rampGenerater.finalTexName =  EditorGUILayout.TextField("输出贴图名称", rampGenerater.finalTexName);
        //rampGenerater.outPutPath = EditorGUILayout.TextField("输出路径", rampGenerater.outPutPath);
        
        
        rampGenerater.RampCount = EditorGUILayout.IntField("Ramp合集包含的RampTex数量(大于等于0)", rampGenerater.RampCount);
        if (rampGenerater.RampCount < 0)
        {
            rampGenerater.RampCount = 0;
        }
        if (rampGenerater.oldRampCount > rampGenerater.RampCount)
        {
            int RampListCount = rampGenerater.RampList.Count;
            for (int i = 0; i < rampGenerater.oldRampCount - rampGenerater.RampCount; i++)
            {
                rampGenerater.RampList.RemoveAt(RampListCount-1-i);
                rampGenerater.RampControllerList.RemoveAt(RampListCount-1-i);
            }
            //将RampList中的RampTex打印在界面上
            ShowRampTex();
            rampGenerater.oldRampCount = rampGenerater.RampCount;
        }
        else if(rampGenerater.oldRampCount < rampGenerater.RampCount)
        {
            for (int i = 0; i < rampGenerater.RampCount - rampGenerater.oldRampCount; i++)
            {
                Gradient temp = new Gradient();
                //rampGenerater.InsertRampController();
                rampGenerater.RampList.Add(temp);
            }
            //将RampList中的RampTex打印在界面上
            ShowRampTex();
            rampGenerater.oldRampCount = rampGenerater.RampCount;
        }
        else
        {
            ShowRampTex();;
        }
        
        //存储RampTex
        if (GUILayout.Button("存储RampTex"))
        {
            if (rampGenerater.RampList.Count > 0)
            {
                rampGenerater.setFinalTex();
                //rampGenerater.setFinalTexGammas();
                string path = EditorUtility.SaveFilePanel("rampTex", rampGenerater.outPutPath, rampGenerater.finalTexName, "png");
                System.IO.File.WriteAllBytes(path, rampGenerater.finalTex.EncodeToPNG());
            }
            
        }

        rampGenerater.targetMaterial = (Material)EditorGUILayout.ObjectField("目标材质球", rampGenerater.targetMaterial, typeof(Material));
        //如果目标材质球不为空 则将FinalTex设置为当前材质球的RampTex
        if (GUILayout.Button("在材质球下查看RampTex效果")&&rampGenerater.targetMaterial != null)
        {
            rampGenerater.setFinalTex();
            //System.IO.File.WriteAllBytes("Assets", rampGenerater.finalTex.EncodeToPNG());
            rampGenerater.targetMaterial.SetTexture("_RampTexture",(Texture)rampGenerater.finalTex);
        }
    }
}
#endif