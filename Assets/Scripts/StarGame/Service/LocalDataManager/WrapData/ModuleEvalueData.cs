//来源表战力表_Power.xlsm.xlsx -> sheet:ModuleEvalue
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class ModuleEvalueData
	{
		[Key(0)]
		public Dictionary<int, ModuleEvalueDataCell> StaticModuleEvalueDatas = new Dictionary<int, ModuleEvalueDataCell>();
	}
	[MessagePackObject]
	public class ModuleEvalueDataCell
	{
		//ID
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//模块或养成线ID
		[Key(1)]
		public int ModuleID;
		public int GetModuleID()
		{
			return ModuleID;
		}
		//成长百分比
		[Key(2)]
		public int Percent;
		public int GetPercent()
		{
			return Percent;
		}
		//评价
		[Key(3)]
		public string Evalue
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_Evalue); }
			set { _Evalue = value; }
        }
		[IgnoreMember]
		private string _Evalue;
		//显示品质
		[Key(4)]
		public int ShowQuality;
		public int GetShowQuality()
		{
			return ShowQuality;
		}
	}
}
