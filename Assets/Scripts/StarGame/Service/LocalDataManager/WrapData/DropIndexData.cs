//来源表掉落配置表_Drops.xlsx -> sheet:DropIndex
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class DropIndexData
	{
		[Key(0)]
		public Dictionary<int, DropIndexDataCell> StaticDropIndexDatas = new Dictionary<int, DropIndexDataCell>();
	}
	[MessagePackObject]
	public class DropIndexDataCell
	{
		//id
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//索引ID
		[Key(1)]
		public long IndexID;
		public long GetIndexID()
		{
			return IndexID;
		}
		//最小
		[Key(2)]
		public int NumMin;
		public int GetNumMin()
		{
			return NumMin;
		}
		//最大
		[Key(3)]
		public int NumMax;
		public int GetNumMax()
		{
			return NumMax;
		}
		//权重
		[Key(4)]
		public int Weight;
		public int GetWeight()
		{
			return Weight;
		}
		//道具id
		[Key(5)]
		public List<long> ItemIDs = new List<long>();
	}
}
