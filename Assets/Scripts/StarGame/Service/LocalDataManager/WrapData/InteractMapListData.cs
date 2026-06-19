//来源表SceneMap.xlsx -> sheet:InteractMapList
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
namespace StarProjectDef
{
	[MessagePackObject]
	public class InteractMapListData
	{
		[Key(0)]
		public Dictionary<long, InteractMapListDataCell> StaticInteractMapListDatas = new Dictionary<long, InteractMapListDataCell>();
	}
	[MessagePackObject]
	public class InteractMapListDataCell
	{
		//交互物ID
		[Key(0)]
		public long Interact;
		public long GetInteract()
		{
			return Interact;
		}
		//交互物描述
		[Key(1)]
		public string NPCDesc;
		//交互物分类
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
