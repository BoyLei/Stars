//来源表新手引导表_PlayerGuidance.xlsm.xlsx -> sheet:ItemGetJump
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class ItemGetJumpData
	{
		[Key(0)]
		public Dictionary<int, ItemGetJumpDataCell> StaticItemGetJumpDatas = new Dictionary<int, ItemGetJumpDataCell>();
	}
	[MessagePackObject]
	public class ItemGetJumpDataCell
	{
		//跳转id
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//跳转途径名
		[Key(1)]
		public string WayName
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_WayName); }
			set { _WayName = value; }
        }
		[IgnoreMember]
		private string _WayName;
		//跳转枚举
		[Key(2)]
		public string WayString;
		//前缀图标链接
		[Key(3)]
		public string Icon;
		//关联系统开放id
		[Key(4)]
		public int SysOpen;
		public int GetSysOpen()
		{
			return SysOpen;
		}
	}
}
