using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//维护UniversalRenderPiplineAsset中的RendererList 
//保持枚举内容和Universal Pipline Asset中的rendererList内容同步
public enum SGRenderingPaths
     {
         ForwardRenderer,
         NewYokaRenderer,                   //Default
         SGameOverdrawRenderData,            //Overdraw测试粒子性能用
         PlanarReflectionRenderData        //平面反射
         
         // 添加其他可能的渲染路径
     }
// public static class SGameRendererList 
// {
//     
//     public static SGRenderingPaths SGRendererList { get; set; }
// }
