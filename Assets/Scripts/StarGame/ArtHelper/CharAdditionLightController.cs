using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[ExecuteInEditMode]
public class CharAdditionLightController : MonoBehaviour
{
    [Header("该脚本可以全局调控该场景所有使用角色Shader的材质球内的光照强度")]
    [Header("专门为暗场景提亮补光使用")]
    [Range(1, 5)]
    public float MainLightStrength = 1;

    [Range(1, 20)]
    public float GIStrength = 1;

    private bool isEnable = false;

    public bool IsEnable
    {
        get
        {
            return isEnable;
        }
        set
        {
            if (isEnable != value)
            {
                isEnable = value;
                if (isEnable)
                {
                    Shader.EnableKeyword("_GLOBAL_CHAR_DARKENVIROMENT");
                }
                else
                {
                    Shader.DisableKeyword("_GLOBAL_CHAR_DARKENVIROMENT");
                }
            }
        }
    }
    
    //private GlobalKeyword[] kW = new[] { "_GLOBAL_CHAR_ADDITIONAL_LIGHTS" };
    private void OnEnable()
    {
        IsEnable = true;
        Shader.SetGlobalFloat("_SGameCharMainLightMultipler", MainLightStrength);
        Shader.SetGlobalFloat("_SGameCharGIMultipler", GIStrength);
    }

    private void OnDisable()
    {
        IsEnable = false;
    }
    
    private void OnDestroy()
    {
        IsEnable = false;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        IsEnable =  (UniversalAdditionalCameraData.ActiveCameraCount <= 1);
        Shader.EnableKeyword("_GLOBAL_CHAR_DARKENVIROMENT");
        if (!IsEnable)
        {
            Shader.SetGlobalFloat("_SGameCharMainLightMultipler", 1);
            Shader.SetGlobalFloat("_SGameCharGIMultipler", 1);
            return;
        }
        Shader.SetGlobalFloat("_SGameCharMainLightMultipler", MainLightStrength);
        Shader.SetGlobalFloat("_SGameCharGIMultipler", GIStrength);
    }
}

