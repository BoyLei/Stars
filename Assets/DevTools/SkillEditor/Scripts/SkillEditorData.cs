using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace SkillEditor
{
#if UNITY_EDITOR
    public static class SkillEditorData
    {
        //private static string jsonText = System.IO.File.ReadAllText("Assets/Res/Config/Excel/Model.json");
        //private static string jsonText2 = System.IO.File.ReadAllText("Assets/Res/Config/Excel/Avatar.json");
        private static byte[] binBytes = System.IO.File.ReadAllBytes("Assets/Res/Config/ExcelBytes/Model.bytes");
        private static byte[] binBytes2 = System.IO.File.ReadAllBytes("Assets/Res/Config/ExcelBytes/Avatar.bytes");
        private static byte[] binBytes3 = System.IO.File.ReadAllBytes("Assets/Res/Config/ExcelBytes/JobSkillDesc.bytes");
        private static byte[] binBytes4 = System.IO.File.ReadAllBytes("Assets/Res/Config/ExcelBytes/PartnerSkillDesc.bytes");
        private static byte[] binBytes5 = System.IO.File.ReadAllBytes("Assets/Res/Config/ExcelBytes/PassiveDesc.bytes");
        private static byte[] binBytes6 = System.IO.File.ReadAllBytes("Assets/Res/Config/ExcelBytes/BuffDesc.bytes");


        //public static StarProjectDef.ModelData modelData = Newtonsoft.Json.JsonConvert.DeserializeObject<StarProjectDef.ModelData>(jsonText);
        //public static StarProjectDef.AvatarData avatarData = Newtonsoft.Json.JsonConvert.DeserializeObject<StarProjectDef.AvatarData>(jsonText2);
        public static StarProjectDef.ModelData modelData = MessagePack.MessagePackSerializer.Deserialize<StarProjectDef.ModelData>(binBytes);
        public static StarProjectDef.AvatarData avatarData = MessagePack.MessagePackSerializer.Deserialize<StarProjectDef.AvatarData>(binBytes2);
        public static StarProjectDef.JobSkillDescData jobSkillDescData = MessagePack.MessagePackSerializer.Deserialize<StarProjectDef.JobSkillDescData>(binBytes3);
        public static StarProjectDef.PartnerSkillDescData partnerSkillDesc = MessagePack.MessagePackSerializer.Deserialize<StarProjectDef.PartnerSkillDescData>(binBytes4);
        public static StarProjectDef.PassiveDescData passiveDesc = MessagePack.MessagePackSerializer.Deserialize<StarProjectDef.PassiveDescData>(binBytes5);
        public static StarProjectDef.BuffDescData buffDesc = MessagePack.MessagePackSerializer.Deserialize<StarProjectDef.BuffDescData>(binBytes6);



        public static Dictionary<int, ExplorerItem> Skills = new Dictionary<int, ExplorerItem>();
        public static Dictionary<int, ExplorerItem> Buffs = new Dictionary<int, ExplorerItem>();
        public static Dictionary<int, ExplorerItem> Bullets = new Dictionary<int, ExplorerItem>();
        public static Dictionary<int, ExplorerItem> Passives = new Dictionary<int, ExplorerItem>();

        public static Dictionary<EffectType, string> EffectTypeDic = new Dictionary<EffectType, string>();

        public static EditorCameraType EditorCameraType = EditorCameraType.Right;

        public static ExplorerItem curExplorerItem;

        public static void Refresh()
        {
#if UNITY_EDITOR
            UnityEditor.AssetDatabase.Refresh();

            //jsonText = System.IO.File.ReadAllText("Assets/Res/Config/Excel/Model.json");
            //jsonText2 = System.IO.File.ReadAllText("Assets/Res/Config/Excel/Avatar.json");

            //modelData = Newtonsoft.Json.JsonConvert.DeserializeObject<StarProjectDef.ModelData>(jsonText);
            //avatarData = Newtonsoft.Json.JsonConvert.DeserializeObject<StarProjectDef.AvatarData>(jsonText2);

            binBytes = System.IO.File.ReadAllBytes("Assets/Res/Config/ExcelBytes/Model.bytes");
            binBytes2 = System.IO.File.ReadAllBytes("Assets/Res/Config/ExcelBytes/Avatar.bytes");

            modelData = MessagePack.MessagePackSerializer.Deserialize<StarProjectDef.ModelData>(binBytes);
            avatarData = MessagePack.MessagePackSerializer.Deserialize<StarProjectDef.AvatarData>(binBytes2);

            ValueDropdownList<EffectType> dropdownItems = EnumDefineMap._effecttype as ValueDropdownList<EffectType>;
            EffectTypeDic.Clear();
            foreach (var item in dropdownItems)
            {
                EffectTypeDic.Add(item.Value, item.Text);
            }
#endif
        }

        public static Dictionary<EffectType, string> GetEffectTypeDic()
        {
            if (EffectTypeDic.Count != 0)
            {
                return EffectTypeDic;
            }
            ValueDropdownList<EffectType> dropdownItems = EnumDefineMap._effecttype as ValueDropdownList<EffectType>;
            EffectTypeDic.Clear();
            foreach (var item in dropdownItems)
            {
                EffectTypeDic.Add(item.Value, item.Text);
            }
            return EffectTypeDic;
        }

        /// <summary>
        /// 判断指定类别编辑器中是否存在
        /// </summary>
        /// <param name="explorerItem"></param>
        /// <param name="ID"></param>
        /// <returns></returns>
        public static bool IsRepeat(BattleRuntimeTypeEnum explorerItem,int ID)
        {
            switch (explorerItem)
            {
                case BattleRuntimeTypeEnum.Skill:
                    if (Skills.ContainsKey(ID))
                    {
                        return true;
                    }
                    break;
                case BattleRuntimeTypeEnum.Buff:
                    if (Buffs.ContainsKey(ID))
                    {
                        return true;
                    }
                    break;
                case BattleRuntimeTypeEnum.Bullet:
                    if (Bullets.ContainsKey(ID))
                    {
                        return true;
                    }
                    break;
                case BattleRuntimeTypeEnum.Passive:
                    if (Passives.ContainsKey(ID))
                    {
                        return true;
                    }
                    break;
            }
            return false;
        }
    }
#endif

    public enum BattleRuntimeTypeEnum
    {
        Skill = 1,
        Buff = 2,
        Bullet = 3,
        Passive = 4,
        EffectLine = 5
    }
    public enum EditorCameraType
    {
        Right = 1,
        Left = 2,
        Front = 3,
        Behind = 4,
        Overlook = 5
    }
}

