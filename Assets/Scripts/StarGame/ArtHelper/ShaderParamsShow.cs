using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;
using Sirenix.OdinInspector;
using System.Runtime.Serialization;

/// <summary>
/// 3d
/// shader参数动画的参数
/// </summary>
[System.Serializable]
public class ShaderEffectParam
{
    [LabelText("效果类型")]
    public StarProjectDef.E_ShaderShowType ShowType;
    [LabelText("参数初始值")]
    public float InitValue = 0;//初始值
    [LabelText("参数目标值")]
    public float ToValue = 1;//目标值
    [LabelText("持续时间")]
    public float Time = 1.0f;//持续时间
    [LabelText("运动曲线")]
    //public AnimationCurve Curve; //动画曲线
    public Ease Curve = Ease.Linear;//简易动画曲线
}


/// <summary>
/// 用于制作shader参数动画
/// </summary>
[System.Serializable]
public class ShaderParamsShow : MonoBehaviour
{

    [LabelText("Shader效果")]
    //所有的效果
    public List<ShaderEffectParam> ShaderEffectParams = null;


    //需要应用效果的材质
    List<Material> materials=null;

    //是否已经初始化
    bool isInit = false;

    //记录doTween
    List<Tween> tws = new List<Tween>();
    
    // Start is called before the first frame update
    void Init()
    {
        materials = new List<Material>();
        //先找到所有带目标shader的材质保存起来
       var allRender=transform.GetComponentsInChildren<SkinnedMeshRenderer>();//meshrender补一下
        for(int i=0;i< allRender.Length;i++)
        {
            for(int j=0;j< allRender[i].materials.Length;j++)
            {

                if (allRender[i].materials[j].shader.name == "SGAME/SGAME_Scene_01"/*物件没有溶解功能，角色没有透明变化*/|| allRender[i].materials[j].shader.name == "GJ/CartoonChar_4")
                {
                    materials.Add(allRender[i].materials[j]);
                }
            }


        }

        isInit = true;

    }

    ShaderEffectParam GetShaderEffectParamsByShowType(StarProjectDef.E_ShaderShowType showType)
    {
        for(int i=0;i< ShaderEffectParams.Count;i++)
        {
            if(ShaderEffectParams[i].ShowType == showType)
            {
                return ShaderEffectParams[i];
            }
        }

        return null;
    }
    
    public void Play(StarProjectDef.E_ShaderShowType showType,UnityAction onfinish)
    {
        //没有设置动画
        if (ShaderEffectParams == null || ShaderEffectParams.Count == 0)
        {
            return;
        }

        //没有初始化的话就去初始化
        if (!isInit)
        {
            Init();
        }
        
        //身上没有材质
        if(materials==null || materials.Count==0)
        {
            return;
        }

        ShaderEffectParam shaderEffectParam = GetShaderEffectParamsByShowType(showType);

        //不包含这种类型
        if (shaderEffectParam==null)
        {
            return;
        }

        string keyName =showType.ToString();//shader的参数名

        for (int i = 0; i < tws.Count; i++)
        {
            tws[i].Kill();
        }
        tws.Clear();


        for (int i=0;i< materials.Count;i++)
        {


            float initValue = shaderEffectParam.InitValue;
            float toValue = shaderEffectParam.ToValue;
            float time= shaderEffectParam.Time;

            //初始化
            Material mat = materials[i];
            mat.SetFloat(keyName, initValue);

            // 将myFloat的值从0动画到1，持续时间为1秒
            float myFloat = initValue;
            Tween tw=DOTween.To(() => myFloat, x => myFloat = x, toValue, time).OnUpdate(()=> {
                //每帧更新
                mat.SetFloat(keyName, myFloat);
            }).SetEase(shaderEffectParam.Curve).OnComplete(()=>{ 
                //完成回调
                if(onfinish!=null)
                {
                    onfinish();
                }
            });

            tws.Add(tw);

        }


    }

#if UNITY_EDITOR

    private void Update()
    {
        if(Input.GetMouseButtonDown(1))
        {
            Play(StarProjectDef.E_ShaderShowType._Clip, ()=> {
                Debug.LogError("动画完成");
            });
        }
    }
#endif

}
