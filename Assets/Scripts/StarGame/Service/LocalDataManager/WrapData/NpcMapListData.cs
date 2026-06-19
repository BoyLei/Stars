//来源表SceneMap.xlsm.xlsx -> sheet:NpcMapList
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class NpcMapListData
	{
		[Key(0)]
		public Dictionary<long, NpcMapListDataCell> StaticNpcMapListDatas = new Dictionary<long, NpcMapListDataCell>();
	}
	[MessagePackObject]
	public class NpcMapListDataCell
	{
		//NPCID
		[Key(0)]
		public long NPC;
		public long GetNPC()
		{
			return NPC;
		}
		//NPC描述
		[Key(1)]
		public string NPCDesc
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_NPCDesc); }
			set { _NPCDesc = value; }
        }
		[IgnoreMember]
		private string _NPCDesc;
		//NPC分类
		[Key(2)]
		public int TabType;
		public int GetTabType()
		{
			return TabType;
		}
		//NPC对话功能列表
		[Key(3)]
		public List<int> DialogueList = new List<int>();
		//排序优先级
		[Key(4)]
		public int SortPriority;
		public int GetSortPriority()
		{
			return SortPriority;
		}
	}
}
