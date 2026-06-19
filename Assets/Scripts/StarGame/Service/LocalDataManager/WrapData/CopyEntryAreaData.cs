//来源表组队日常本表_TeamDailyCopy.xlsm.xlsx -> sheet:CopyEntryArea
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class CopyEntryAreaData
	{
		[Key(0)]
		public Dictionary<int, CopyEntryAreaDataCell> StaticCopyEntryAreaDatas = new Dictionary<int, CopyEntryAreaDataCell>();
	}
	[MessagePackObject]
	public class CopyEntryAreaDataCell
	{
		//ID
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//关卡ID
		[Key(1)]
		public int LevelID;
		public int GetLevelID()
		{
			return LevelID;
		}
		//区域名称-KEY
		[Key(2)]
		public string AreaName
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_AreaName); }
			set { _AreaName = value; }
        }
		[IgnoreMember]
		private string _AreaName;
		//所属场景
		[Key(3)]
		public int EntryMapID;
		public int GetEntryMapID()
		{
			return EntryMapID;
		}
		//区域ID
		[Key(4)]
		public int AreaID;
		public int GetAreaID()
		{
			return AreaID;
		}
		//入口NPC
		[Key(5)]
		public int EntryNPCID;
		public int GetEntryNPCID()
		{
			return EntryNPCID;
		}
	}
}
