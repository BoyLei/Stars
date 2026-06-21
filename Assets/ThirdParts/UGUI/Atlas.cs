///--------------------------------------------------------------------
/// 文件名   :   Atlas.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/12/06 13:55:21
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[HideMonoScript]
[System.Serializable]
public class Atlas : ScriptableObject
{
    [ReadOnly]
    public List<Sprite> sprites = new List<Sprite>();

    [ReadOnly]
    public List<string> names = new List<string>();

    public void Clear()
    {
#if UNITY_EDITOR
        if (sprites == null) { sprites = new List<Sprite>(); }
        if (names == null) { names = new List<string>(); }
        sprites.Clear();
        names.Clear();
#endif
    }

    public void AddSprite(Sprite sprite)
    {
#if UNITY_EDITOR
        sprites.Add(sprite);
        names.Add(sprite.name.Replace(".png",""));
#endif
    }


    public Sprite GetSprite(string name)
    {
        int index = names.IndexOf(name);
        if (index > -1 && index < sprites.Count)
        {
            return sprites[index];
        }
        return null;
    }
}
