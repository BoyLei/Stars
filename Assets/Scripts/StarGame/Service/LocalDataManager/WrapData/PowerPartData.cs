//来源表战力表_Power.xlsm.xlsx -> sheet:PowerPart
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class PowerPartData
	{
		[Key(0)]
		public Dictionary<int, PowerPartDataCell> StaticPowerPartDatas = new Dictionary<int, PowerPartDataCell>();
	}
	[MessagePackObject]
	public class PowerPartDataCell
	{
		//ID
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//模块/养成线名称
		[Key(1)]
		public string PartName
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_PartName); }
			set { _PartName = value; }
        }
		[IgnoreMember]
		private string _PartName;
		//所属评分模块ID
		[Key(2)]
		public int FromPart;
		public int GetFromPart()
		{
			return FromPart;
		}
		//养成线描述
		[Key(3)]
		public string PartDesc
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_PartDesc); }
			set { _PartDesc = value; }
        }
		[IgnoreMember]
		private string _PartDesc;
		//养成线跳转
		[Key(4)]
		public int SystemJump;
		public int GetSystemJump()
		{
			return SystemJump;
		}
		//入口图标
		[Key(5)]
		public string PartIcon;
	}
}
