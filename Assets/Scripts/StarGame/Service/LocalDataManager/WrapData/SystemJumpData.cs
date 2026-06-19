//来源表新手引导表_PlayerGuidance.xlsm.xlsx -> sheet:SystemJump
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class SystemJumpData
	{
		[Key(0)]
		public Dictionary<int, SystemJumpDataCell> StaticSystemJumpDatas = new Dictionary<int, SystemJumpDataCell>();
	}
	[MessagePackObject]
	public class SystemJumpDataCell
	{
		//跳转id
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//按钮文本
		[Key(1)]
		public string ButtonText
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_ButtonText); }
			set { _ButtonText = value; }
        }
		[IgnoreMember]
		private string _ButtonText;
		//跳转枚举
		[Key(2)]
		public string WayString;
		//关联系统开放id
		[Key(3)]
		public int SysOpen;
		public int GetSysOpen()
		{
			return SysOpen;
		}
		//道具TIPS排序优先级
		[Key(4)]
		public int SortPriority;
		public int GetSortPriority()
		{
			return SortPriority;
		}
		//备注列（编辑器使用，游戏内不用，勿读）
		[Key(5)]
		public string Comment;
	}
}
