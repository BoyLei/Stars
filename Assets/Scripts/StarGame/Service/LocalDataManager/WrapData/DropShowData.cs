//来源表掉落配置表_Drops.xlsx -> sheet:DropShow
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class DropShowData
	{
		[Key(0)]
		public Dictionary<int, DropShowDataCell> StaticDropShowDatas = new Dictionary<int, DropShowDataCell>();
	}
	[MessagePackObject]
	public class DropShowDataCell
	{
		//id
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//掉落包id
		[Key(1)]
		public int IndexID;
		public int GetIndexID()
		{
			return IndexID;
		}
		//道具id
		[Key(2)]
		public int ItemID;
		public int GetItemID()
		{
			return ItemID;
		}
		//数量
		[Key(3)]
		public int Num;
		public int GetNum()
		{
			return Num;
		}
		//是否显示概率标签
		[Key(4)]
		public bool CanSell;
		public bool GetCanSell()
		{
			return CanSell;
		}
	}
}
