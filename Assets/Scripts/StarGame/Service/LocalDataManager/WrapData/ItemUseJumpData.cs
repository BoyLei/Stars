//来源表道具配置表_Item.xlsx -> sheet:ItemUseJump
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
namespace StarProjectDef
{
	[MessagePackObject]
	public class ItemUseJumpData
	{
		[Key(0)]
		public Dictionary<int, ItemUseJumpDataCell> StaticItemUseJumpDatas = new Dictionary<int, ItemUseJumpDataCell>();
	}
	[MessagePackObject]
	public class ItemUseJumpDataCell
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
		public string ButtonText;
		//排序优先级
		[Key(2)]
		public int SortPriority;
		public int GetSortPriority()
		{
			return SortPriority;
		}
	}
}
