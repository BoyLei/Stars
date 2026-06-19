using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
    [MessagePackObject]
    public class LanguageData
    {
        [Key(0)]
        public Dictionary<int, uint> HashToDataIndexMap = new Dictionary<int, uint>();
        //多语言内容数组
        [Key(1)]
        public List<string> LanguageContentDatas = new List<string>();
        [Key(2)]
        public Dictionary<string, uint> KeyToDataIndexMap = new Dictionary<string, uint>();
    }
}
