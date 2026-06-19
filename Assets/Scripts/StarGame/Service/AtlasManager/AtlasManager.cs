///--------------------------------------------------------------------
/// 文件名   :   AtlasManager.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2023/03/29 19:37:06
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using SGF.Module.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace StarProject.Service.AtlasManager
{
    [XLua.LuaCallCSharp]
    public class AtlasManager : ServiceModule<AtlasManager>
    {
        public const string AtlasPathSkill = "UI/Icons/Atlas/IconSkill";
        public const string AtlasPathSkill1 = "UI/Icons/Atlas/IconSkill1";

        public const string AtlasPathItem = "UI/Icons/Atlas/IconItem";
        public const string AtlasPathCommonItem = "ui/common/atlas/commonitem";
        public const string AtlasPathRune = "ui/amulet/atlas/rune";
        public const string AtlasPathShop = "ui/shop/atlas/shop";
        public const string AtlasPathRole = "UI/Icons/Atlas/IconRole";
        public const string AtlasPathBuff = "UI/Icons/Atlas/IconBuff";
        public const string AtlasPathEmo1 = "UI/Icons/Atlas/IconEmo1";
        public const string AtlasPathCommon = "ui/commonatlas/commonatlas";
        public const string AtlasPathHud = "ui/starworld/atlas/hud";
        public const string AtlasPathAdventure = "ui/adventurelevel/atlas/adventure";
        public const string AtlasPathCommonctrl = "ui/common/atlas/commonctrl";
        public const string AtlasPathCopyTeam = "ui/dailyteam/atlas/copyteam";
        public const string AtlasPathTextMap = "ui/text/textmap";
        public const string AtlasPathTextBattle = "ui/text/TextBattle";
        public const string AtlasPathIconAttribute = "ui/icons/atlas/iconattribute";
        public const string AtlasPathLogin = "UI/Login/Atlas/Login";
        public const string AtlasPathMiniMapMask = "UI/WorldMap/Atlas/MiniMapMask";



        public const string AtlasPathRoleHead = "UI/Common/Textures/Role/";


        /// <summary>
        /// 白名单，关闭界面的时候则跳过
        /// </summary>
        public List<string> WhiteList = new()
        {
            "ui/common/atlas/commonitem",
            "ui/common/atlas/commonctrl",
            "ui/common/atlas/commonres"
        };

        public void Init()
        {
        }


        private bool InWhiteList(string path)
        {
            return WhiteList.Contains(path);
        }


        public void GetSpriteAsync(string atlasPath, string spriteName, Action<Sprite> callback)
        {
            if (string.IsNullOrEmpty(atlasPath) || string.IsNullOrEmpty(spriteName) || string.IsNullOrWhiteSpace(spriteName))
            {
                callback?.Invoke(null);
                return;
            }

            StarProject.Service.Resource.ResourceFormalManager.Instance.LoadSpriteAtlasAsync(atlasPath, spriteName,
                callback, true);
        }

        /// <summary>
        /// Atlas 释放接口 ，其计数减一 ，当计数为0时候真正释放
        /// </summary>
        /// <param name="atlasPath">路径</param>
        /// <param name="force">强制释放</param>
        public void ReleaseAtlas(string atlasPath, bool force = false)
        {
            if (InWhiteList(atlasPath))
            {
                return;
            }
        }
    }
}