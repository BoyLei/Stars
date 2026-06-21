///--------------------------------------------------------------------
/// 文件名   :   UGUIExtents.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/12/15 16:55:02
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

[XLua.LuaCallCSharp]
static public class UGUIExtents
{
    public static void SetGray(this Graphic graphic, bool isGray)
    {
        if (graphic == null)
        {
            return;
        }

        if (graphic.material == null || graphic.material.shader.name.Contains("UI/Default"))
        {
            StarProject.Service.Resource.ResourceFormalManager.Instance.LoadResourceUniRefAsync<Material>("UI/Common/UIMaterals/UIGrayMaterial",
                (mat) =>
                {
                    graphic.material =new Material(mat);
                    graphic.material.SetInt("_Gray", isGray ? 1 : 0);
                });
            
        }

        if (graphic.material != null && graphic.material.shader.name.Contains("UIGrayMaterial"))
        {
            graphic.material.SetFloat("_Grey", isGray ? 1 : 0);
        }
    }

    public static void SetGrayWithChildren(this Transform root, bool isGray, List<GameObject> ignores)
    {
        //当前渲染管线不支持此操作
        if(true)
        {
            return;
        }    
        if (root == null)
        {
            return;
        }

        var graphics = root.GetComponentsInChildren<Graphic>();

        foreach (var item in graphics)
        {
            if (ignores!=null && ignores.Contains(item.gameObject))
            {
                continue;
            }

            if(!isGray)
            {
                item.material = null;
            }
            if (item.material == null || item.material.shader.name.Contains("UI/Default"))
            {
                if (isGray)
                {
                    StarProject.Service.Resource.ResourceFormalManager.Instance.LoadResourceUniRefAsync<Material>("UI/Common/UIMaterals/UIGrayMaterial",
                        (mat) =>
                        {
                            item.material =new Material(mat);
                            item.material.SetInt("_Gray", isGray ? 1 : 0);
                        });
                }
            }
            else
            {
                if (item.material != null && item.material.shader.name.Contains("UIGrayMaterial"))
                {
                    item.material.SetFloat("_Grey", isGray ? 1 : 0);
                }
            }
        }
    }
}
