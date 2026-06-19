//来源表公会表_Guild.xlsm.xlsx -> sheet:GuildJump
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class GuildJumpData
	{
		[Key(0)]
		public Dictionary<int, GuildJumpDataCell> StaticGuildJumpDatas = new Dictionary<int, GuildJumpDataCell>();
	}
	[MessagePackObject]
	public class GuildJumpDataCell
	{
		//ID
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//功能/玩法名称多语言
		[Key(1)]
		public string SystemName
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_SystemName); }
			set { _SystemName = value; }
        }
		[IgnoreMember]
		private string _SystemName;
		//功能/玩法描述多语言
		[Key(2)]
		public string Systemdesc
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_Systemdesc); }
			set { _Systemdesc = value; }
        }
		[IgnoreMember]
		private string _Systemdesc;
		//功能/玩法开启描述多语言
		[Key(3)]
		public string SystemOpenDesc
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_SystemOpenDesc); }
			set { _SystemOpenDesc = value; }
        }
		[IgnoreMember]
		private string _SystemOpenDesc;
		//功能/玩法ICON
		[Key(4)]
		public string SystemICON;
		//功能/玩法跳转
		[Key(5)]
		public int SystemJump;
		public int GetSystemJump()
		{
			return SystemJump;
		}
	}
}
