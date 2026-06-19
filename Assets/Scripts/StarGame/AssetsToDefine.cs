using MessagePack;
using System.Collections.Generic;
/// 注意,此脚本为 AssetsToDefine.cs 动态生成

namespace StarProjectDef
{
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName: false)]
    /// <summary>
    /// Addressables中动画，模型，特效，路径导出到AssetsToDefine中
    /// </summary>
    public class AssetsToDefine
    {
        [Key(0)]
        // #动画# 路径
        public HashSet<string> AnimationPathList = new();

        [Key(1)]
        // #模型# 路径
        public HashSet<string> RolePathList = new();

        [Key(2)]
        // #特效# 路径
        public HashSet<string> EffectPathList = new();
    }
}