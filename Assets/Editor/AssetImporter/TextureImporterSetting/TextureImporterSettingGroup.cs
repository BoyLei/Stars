///--------------------------------------------------------------------
/// 文件名   :   TextureImporterSettingGroup.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/10/14 16:56:21
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(menuName = "Importer/Texture")]
[HideMonoScript]
public class TextureImporterSettingGroup : ScriptableObject
{

    [LabelText("黑名单")]
    [PropertyTooltip("文件名包含在这个列表的跳过设置，如法线题图，灯光贴图")]
    public List<string> BlockList = new List<string>();
    

    [LabelText("图片设置配置表")]
    public List<TextureImporterSetting> Settings = new List<TextureImporterSetting>();

    public TextureImporterSetting GetSetting(string path)
    {
        if (!string.IsNullOrEmpty(path))
        {
            if (Settings != null)
            {
                TextureImporterSetting setting = null;
                foreach (var item in Settings)
                {
                    if(string.IsNullOrEmpty(item.FolderPath))
                    {
                        continue;
                    }
                    if(path.Contains(item.FolderPath))
                    {
                        if (setting == null)
                        {
                            setting = item;
                        }
                        else
                        {
                            if(item.Deep>setting.Deep)
                            {
                                setting = item;
                            }
                        }
                    }
                }
                return setting;
            }
        }
        return null;
    }

    public bool InBlockList(string fileName)
    {
        foreach (var item in BlockList)
        {
            if(fileName.Contains(item))
            {
                return true;
            }
        }
        return false;
    }
}
