using System;
using System.Collections;
using System.Collections.Generic;
using StarProjectDef;
using UnityEngine;

namespace StarProject.Service.SystemOpen
{

    /// <summary>
    /// 系统开放 每个 系统类型 生成的 数据
    /// </summary>
    public class SystemItemData
    {
        public SystemOpenType systemOpenType;

        public int CfgID = 0;
        /// <summary>
        /// 系统开放 角色等级要求
        /// </summary>
        public int RoleLevel;
        /// <summary>
        /// 冒险等级要求
        /// </summary>
        public int AdvanceLevel;
        /// <summary>
        /// 完成任务要求
        /// </summary>
        public uint TaskID;

        /// <summary>
        /// Icon 路径
        /// </summary>
        public string Icon;
        /// <summary>
        /// 图集路径名字
        /// </summary>
        public string AtlasPath;
        /// <summary>
        /// icon 名字
        /// </summary>
        public string IconName;
        public string Name;
        public string Desc;

        // public string Note;

        /// <summary>
        /// 是否有 拍脸图 
        /// </summary>
        public bool isHaveOpenPic = false;

        public bool IsHaveOpenPic => isHaveOpenPic;

        private bool isOpen = false;
        public bool IsOpen => isOpen;

        private bool needHightLight = false;

        public SystemItemData(GuidSysOpenDataCell cfg)
        {
            if (!Enum.TryParse<SystemOpenType>(cfg.EnumName, true, out systemOpenType))
            {
                SGF.Debuger.LogWarning($"[SystemOpenManager] cfg[{cfg.GetId()}]: {cfg.EnumName}, 转换 SystemOpenType 失败");
            }
            CfgID = cfg.Id;

            RoleLevel = cfg.GetNeedLv();

            AdvanceLevel = cfg.GetNeedAdvLv();

            TaskID = (uint)cfg.GetNeedQuestCom();

            isHaveOpenPic = cfg.GetIsHaveOpenPic();

            Icon = cfg.OpenIconRes;
            if (!Icon.IsNullOrEmpty())
            {
                var path = Icon.Split("|");
                AtlasPath = path[0];
                IconName = path[1];
            }
            Name = cfg.OpenTitle;
            Desc = cfg.OpenDesc;
            // Note = cfg.Note;

            // 是否需要高亮
            needHightLight = true;
        }

        /// <summary>
        /// 检查这个是否需要高亮. 
        /// 如果点击过了, 就不需要高亮
        /// </summary>
        public bool GetNeedHightLight()
        {
            return needHightLight;
        }

        public void MarkOpen()
        {
            isOpen = true;
        }

        public void MarkClose()
        {
            isOpen = false;
        }

        public void MarkHighLight()
        {
            needHightLight = false;
            SystemOpenManager.Instance.MarkHighLight(systemOpenType);
        }
    }

}
