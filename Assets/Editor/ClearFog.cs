using StarProject.Service.UniversalRenderPipeline;
using UnityEditor;
/// <summary>
/// 所有AOI.json中props属性导入到AOIAttrDefine中
/// 所有AOI的json转换成cs类
/// </summary>
public static class ClearFog
{
    
    [MenuItem("gopal/一键去雾/去雾", false, 1)]

    private static void ClearFogBtn()
    {
        UniRenderPipline.Instance.CloseAllRenderPassFeature();


        UnityEngine.Debug.LogError("去雾完成");
    }
    
}
