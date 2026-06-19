using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//[ExecuteAlways]
[RequireComponent(typeof(Renderer))]
public class TimelineCharMatHelper : MonoBehaviour
{
    // Public开关，控制溶解关键字的开启
    public bool dissolveEnabled;

    // 上次的开关状态，用于检测状态变化
    private bool lastDissolveEnabled;

    // 游戏开始时初始化状态
    private void Start()
    {
        // 初始化时记录开关的当前状态
        lastDissolveEnabled = dissolveEnabled;
        UpdateMaterialKeywords();
    }

    // 游戏运行时监测开关的变化
    private void Update()
    {
        // 检查是否有状态变化
        if (dissolveEnabled != lastDissolveEnabled)
        {
            // 如果有变化，更新材质关键字
            UpdateMaterialKeywords();
            // 更新上次状态
            lastDissolveEnabled = dissolveEnabled;
        }
    }
    // 更新材质关键字
    private void UpdateMaterialKeywords()
    {
        // 获取物件上的所有材质球
        Renderer renderer = GetComponent<Renderer>();
        Material[] materials = renderer.materials;

        foreach (var material in materials)
        {
            // 根据材质球命名后缀判断是否为描边材质球
            if (material.name.EndsWith("Outline"))
            {
                // 描边材质球
                if (dissolveEnabled)
                {
                    // 关闭深度写入
                    material.SetFloat("_ZwriteOp_Char", 0);
                }
                else
                {
                    // 开启深度写入
                    material.SetFloat("_ZwriteOp_Char", 1);
                }
            }
            else
            {
                // 非描边材质球
                if (dissolveEnabled)
                {
                    // 开启溶解关键字
                    material.EnableKeyword("_DISSOLVE_ON");
                }
                else
                {
                    // 关闭溶解关键字
                    material.DisableKeyword("_DISSOLVE_ON");
                }
            }
        }
    }

    // 当Timeline或其他脚本更改开关时更新
    public void SetDissolveEnabled(bool enabled)
    {
        dissolveEnabled = enabled;
        UpdateMaterialKeywords();
    }
}
