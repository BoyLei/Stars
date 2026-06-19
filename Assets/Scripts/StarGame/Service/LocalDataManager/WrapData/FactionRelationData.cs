//来源表阵营配置表_FactionRelation.xlsm.xlsx -> sheet:FactionRelation
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class FactionRelationData
	{
		[Key(0)]
		public Dictionary<int, FactionRelationDataCell> StaticFactionRelationDatas = new Dictionary<int, FactionRelationDataCell>();
	}
	[MessagePackObject]
	public class FactionRelationDataCell
	{
		//序号
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//同盟阵营
		[Key(1)]
		public List<int> FriendlyFaction = new List<int>();
		//中立阵营
		[Key(2)]
		public List<int> NeutralFaction = new List<int>();
		//敌对阵营
		[Key(3)]
		public List<int> OpposingFaction = new List<int>();
		//阵营名-key
		[Key(4)]
		public string FactionName
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_FactionName); }
			set { _FactionName = value; }
        }
		[IgnoreMember]
		private string _FactionName;
	}
}
