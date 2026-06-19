//来源表Npc.xlsm.xlsx -> sheet:NpcDialogGroup
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class NpcDialogGroupData
	{
		[Key(0)]
		public Dictionary<int, NpcDialogGroupDataCell> StaticNpcDialogGroupDatas = new Dictionary<int, NpcDialogGroupDataCell>();
	}
	[MessagePackObject]
	public class NpcDialogGroupDataCell
	{
		//ID
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//文本ID
		[Key(1)]
		public int DialogID;
		public int GetDialogID()
		{
			return DialogID;
		}
		//开场白组ID
		[Key(2)]
		public int GroupID;
		public int GetGroupID()
		{
			return GroupID;
		}
		//权重
		[Key(3)]
		public int Weight;
		public int GetWeight()
		{
			return Weight;
		}
		//解锁条件
		[Key(4)]
		public int OpenCondition;
		public int GetOpenCondition()
		{
			return OpenCondition;
		}
		//删除条件
		[Key(5)]
		public int DelCondition;
		public int GetDelCondition()
		{
			return DelCondition;
		}
	}
}
