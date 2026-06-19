//来源表战力表_Power.xlsm.xlsx -> sheet:PartEvalue
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class PartEvalueData
	{
		[Key(0)]
		public Dictionary<int, PartEvalueDataCell> StaticPartEvalueDatas = new Dictionary<int, PartEvalueDataCell>();
	}
	[MessagePackObject]
	public class PartEvalueDataCell
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
		public int PartID;
		public int GetPartID()
		{
			return PartID;
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
