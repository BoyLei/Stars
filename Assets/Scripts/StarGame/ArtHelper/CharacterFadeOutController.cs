using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Object = System.Object;

public class CharacterFadeOutController : MonoBehaviour
{
    [Header("模型本身透明度")]
    [Range(0.0f,1.0f)]//[Tooltip("模型本身透明度")]
    public float OriginalAlpha = 0.7f; //模型本身透明度 
   
    [Header("半透明扩散模型的初始的alpha强度")]
    [Range(0.0f,1.0f)] //[Tooltip("半透明扩散模型的初始的alpha强度")]
    public float InitialAlpha = 0.4f; //初始的alpha强度
    
    [Header("外扩模型消失时间")]
    public float FadeOutTime = 1.3f;  //外扩模型消失时间 
    
    [Header("外扩模型外扩速度")]
    public float FadeOutSpeed = 0.5f;   //外扩模型外扩速度
    public Material FadeOutMat;       //外扩模型的材质球
    public bool IfBeginVFX = false;           //是否开启隐身vfx
    public bool IfStopVFX = false;            //是否结束隐身vfx
    
    [Header("第二层外扩模型等待时间")]
    public float WaitTime = 1f;
    [ColorUsage(true, true)]
    public Color FirstColor = Color.white;
    [ColorUsage(true, true)]
    public Color SecondColor = Color.white;
    
    private const string CharacterAlphaName = "_CharacterAlpha";
    private const string CharacterOutRangeName = "_CharacterOutRange";
    private const string RingColorName = "_RingColor";

    private const string StencilCompName = "_StencilComp";
    private const string StencilOpName = "_StencilOp";
    private const string OutLineStencilCompName = "_OutLineStencilComp";
    private const string CharacterFadeOutMatPath = "";

    private  int StencilComp = (int)UnityEngine.Rendering.CompareFunction.Always;
    private  int StencilOp = (int)UnityEngine.Rendering.StencilOp.Keep;
    private int OutLineStencilComp = (int)UnityEngine.Rendering.CompareFunction.Always;
    private float CharOutLineRange = 0.1f;  //存储角色描边的粗细
    private Coroutine fadeCoroutine1;
    private Coroutine fadeCoroutine2;
    //private int FadeModelCount = 2; //外扩模型层数
    private Renderer CharRenderer; //角色渲染的Renderer
    private int OriginalMatCount;  //记录renderer原本的材质球数量
    // Start is called before the first frame update
    void Start()
    {
        CharRenderer = this.GetComponent<Renderer>();
        OriginalMatCount = CharRenderer.materials.Length;
    }

    private void OnEnable()
    {
       // throw new NotImplementedException();
        //将两个新的材质球加入模型renderer
        CharRenderer = this.GetComponent<Renderer>();
        // List<Material> newMaterials = new List<Material>( CharRenderer.materials);
        // Material FadeOutMat1 = FadeOutMat;
        // newMaterials.Add(FadeOutMat);
        // newMaterials.Add(FadeOutMat);
        // CharRenderer.materials = newMaterials.ToArray();
    }

    IEnumerator Fade1(Material Mat/*,float waitTime*/)
    {
        // for (float f = waitTime; f >= 0; f -= Time.deltaTime)
        // {
        //     yield return new WaitForSeconds(Time.deltaTime);  
        //     ;
        // }

        float fadeDeltaTime = InitialAlpha/FadeOutTime;
        float modelAlpha = InitialAlpha;
        float AlphaFadeSpeed = InitialAlpha / FadeOutTime;
        float modelOutRange = 0f;
        float timeCount = FadeOutTime;
        while(true)
        {
            modelAlpha -= Time.deltaTime*(InitialAlpha/FadeOutTime);
            if (modelAlpha < 0)
                modelAlpha = 0;
            modelOutRange += (FadeOutSpeed/100)*(InitialAlpha/FadeOutTime);
            Mat.SetFloat(CharacterAlphaName,modelAlpha);
            Mat.SetFloat(CharacterOutRangeName,modelOutRange);
            Mat.SetColor(RingColorName,FirstColor);
            timeCount -= Time.deltaTime;//计时器刷新
            if (timeCount <= 0/*||modelAlpha<=0*/)
            {
                timeCount = FadeOutTime;
                modelAlpha = InitialAlpha;
                modelOutRange = 0;
                Mat.SetFloat(CharacterAlphaName,InitialAlpha);
                Mat.SetFloat(CharacterOutRangeName,0);
            }
            //yield return null;
            yield return new WaitForSeconds(Time.deltaTime);
        }
    }

    IEnumerator Fade2(Material Mat,float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        // for (float f = waitTime; f >= 0; f -= Time.deltaTime)
        // {
        //     yield return new WaitForSeconds(Time.deltaTime);  
        //     ;
        // }

        float fadeDeltaTime = InitialAlpha/FadeOutTime;
        float modelAlpha = InitialAlpha;
        float AlphaFadeSpeed = InitialAlpha / FadeOutTime;
        float modelOutRange = 0f;
        float timeCount = FadeOutTime;
        while(true)
        {
            modelAlpha -= Time.deltaTime*(InitialAlpha/FadeOutTime);
            if (modelAlpha < 0)
                modelAlpha = 0;
            modelOutRange += (FadeOutSpeed/100) * (InitialAlpha/FadeOutTime);
            Mat.SetFloat(CharacterAlphaName,modelAlpha);
            Mat.SetFloat(CharacterOutRangeName,modelOutRange);
            Mat.SetColor(RingColorName,SecondColor);
            timeCount -= Time.deltaTime;//计时器刷新
            if (timeCount <= 0/*||modelAlpha<=0*/)
            {
                timeCount = FadeOutTime;
                modelAlpha = InitialAlpha;
                modelOutRange = 0;
                Mat.SetFloat(CharacterAlphaName,InitialAlpha);
                Mat.SetFloat(CharacterOutRangeName,0);
            }

            //yield return null;
            yield return new WaitForSeconds(Time.deltaTime);  
        }
    }

    IEnumerator VFXControl() //定时重启渐隐特效
    {
        float fadeDeltaTime = 0.05f;
        for (float f = FadeOutTime; f >= 0; f = f - fadeDeltaTime)
        {
            yield return new WaitForSeconds(fadeDeltaTime);  
        }
        IfBeginVFX = true;
    }
    // Update is called once per frame
    void Update()
    {
        // int materialArrayLength1 = CharRenderer.materials.Length;
        // fadeCoroutine1 =  StartCoroutine(Fade1(CharRenderer.materials[materialArrayLength1 - 1]));
        // fadeCoroutine2 = StartCoroutine(Fade2(CharRenderer.materials[materialArrayLength1 - 2],FadeOutTime/2));
        if (IfBeginVFX==true&&!Object.ReferenceEquals(FadeOutMat,null) && !Object.ReferenceEquals(CharRenderer,null))
        {
            if (CharRenderer.materials.Length == OriginalMatCount)
            {
                //将两个材质球加入队列
                List<Material> newMaterials = new List<Material>( CharRenderer.materials);
                Material FadeOutMat1 = new Material(FadeOutMat);
                // newMaterials.Add(FadeOutMat1);
                //  newMaterials[newMaterials.Count - 1].renderQueue = 3001;
                //  newMaterials[newMaterials.Count - 1].SetFloat(CharacterOutRangeName,0);
                //  newMaterials[newMaterials.Count - 1].SetFloat(CharacterAlphaName,InitialAlpha);
                newMaterials.Add(FadeOutMat1);
                newMaterials[newMaterials.Count - 1].renderQueue = 3002;
                newMaterials[newMaterials.Count - 1].SetFloat(CharacterOutRangeName,0);
                newMaterials[newMaterials.Count - 1].SetFloat(CharacterAlphaName,InitialAlpha);
                newMaterials.Add(FadeOutMat1);
                newMaterials[newMaterials.Count - 1].renderQueue = 3003;
                newMaterials[newMaterials.Count - 1].SetFloat(CharacterOutRangeName,0);
                newMaterials[newMaterials.Count - 1].SetFloat(CharacterAlphaName,InitialAlpha);
                CharRenderer.materials = newMaterials.ToArray();
                
                int materialArrayLength = CharRenderer.materials.Length;
                fadeCoroutine1 =  StartCoroutine(Fade1(CharRenderer.materials[materialArrayLength - 1]));
                fadeCoroutine2 = StartCoroutine(Fade2(CharRenderer.materials[materialArrayLength - 2],WaitTime));
                //设置角色本身的材质球的渲染队列为半透明队列(3000),设置材质球的透明度为OriginalAlpha
                CharRenderer.materials[0].renderQueue = 2450; //设置为模板测试
                // //保持Render的Shadow开启
                // CharRenderer.shadowCastingMode = ShadowCastingMode.On;
                //存储角色原始的模板测试参数
                StencilComp = CharRenderer.materials[0].GetInt(StencilCompName);
                StencilOp = CharRenderer.materials[0].GetInt(StencilOpName);
                OutLineStencilComp = CharRenderer.materials[0].GetInt(OutLineStencilCompName);
                //存储角色描边的粗细
                CharOutLineRange = CharRenderer.materials[0].GetFloat("_OutLineWidth");
                //设置角色描边粗细
                CharRenderer.materials[0].SetFloat("_OutLineWidth",0);
                //设置角色的模板测试参数
                CharRenderer.materials[0].SetInt(StencilCompName,(int)UnityEngine.Rendering.CompareFunction.Equal);
                CharRenderer.materials[0].SetInt(StencilOpName,(int)UnityEngine.Rendering.StencilOp.IncrementWrap);
                CharRenderer.materials[0].SetInt(OutLineStencilCompName,(int)UnityEngine.Rendering.CompareFunction.Always);
                
                CharRenderer.materials[0].SetFloat("_Alpha",OriginalAlpha);
                CharRenderer.materials[0].SetFloat("_OutlineAlpha",0);
            }
            
            //StartCoroutine(VFXControl());
            IfBeginVFX = false;
        }
        
        if (IfStopVFX==true)
        {
            
            int materialArrayLength = CharRenderer.materials.Length;
            //StopAllCoroutines();
            StopCoroutine(fadeCoroutine1);
            StopCoroutine(fadeCoroutine2);
            //StopCoroutine(Fade2(CharRenderer.materials[materialArrayLength - 1],WaitTime));
            //初始化材质球属性
            if (CharRenderer.materials.Length > OriginalMatCount)
            {
                CharRenderer.materials[materialArrayLength - 1].SetFloat(CharacterAlphaName,InitialAlpha);
                CharRenderer.materials[materialArrayLength - 2].SetFloat(CharacterOutRangeName,0);
                //删除Material数组最后两个元素
                List<Material> newMaterials = new List<Material>(CharRenderer.materials);
                //newMaterials.RemoveAt(newMaterials.Count-1);
                newMaterials.RemoveAt(newMaterials.Count-1);
                newMaterials.RemoveAt(newMaterials.Count-1);
                CharRenderer.materials = newMaterials.ToArray();
            }
           
            //设置角色本身的材质球的渲染队列为不透明队列(2000),设置材质球的透明度为initialAlpha
            CharRenderer.materials[0].renderQueue = 2000;
            //CharRenderer.castShadows = true;
            CharRenderer.materials[0].SetFloat("_Alpha",1);
            CharRenderer.materials[0].SetFloat("_OutlineAlpha",1);
            //还原角色的模板测试参数
            CharRenderer.materials[0].SetInt(StencilCompName,StencilComp);
            CharRenderer.materials[0].SetInt(StencilOpName,StencilOp);
            CharRenderer.materials[0].SetInt(OutLineStencilCompName,OutLineStencilComp);
            //还原角色描边粗细
            CharRenderer.materials[0].SetFloat("_OutLineWidth", CharOutLineRange);
            Debug.Log("122");
            IfStopVFX = false;
        }
    }

    private void OnDisable()
    {
        //throw new NotImplementedException();
        if (CharRenderer.materials.Length > OriginalMatCount) //如果关闭脚本时,材质球数组的长度和初始不一致,说明两个用于外扩的材质球没删干净
        {
            //设置角色本身的材质球的渲染队列为不透明队列(2000)
            CharRenderer.materials[0].renderQueue = 2000;
            //CharRenderer.castShadows = true;
            CharRenderer.materials[0].SetFloat("_Alpha",1);
            CharRenderer.materials[0].SetFloat("_OutlineAlpha",1);
            //还原角色的模板测试参数
            CharRenderer.materials[0].SetInt(StencilCompName,StencilComp);
            CharRenderer.materials[0].SetInt(StencilOpName,StencilOp);
            CharRenderer.materials[0].SetInt(OutLineStencilCompName,OutLineStencilComp);
            //还原角色描边粗细
            CharRenderer.materials[0].SetFloat("_OutLineWidth", CharOutLineRange);
            //删除Material数组最后两个元素
            List<Material> newMaterials = new List<Material>(CharRenderer.materials);
            //newMaterials.RemoveAt(newMaterials.Count-1);
            newMaterials.RemoveAt(newMaterials.Count-1);
            newMaterials.RemoveAt(newMaterials.Count-1);
            CharRenderer.materials = newMaterials.ToArray();
        }
    }
}
