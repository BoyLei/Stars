//来源表宝箱_Boxes.xlsm.xlsx -> sheet:RandAward
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class RandAwardData
	{
		[Key(0)]
		public Dictionary<long, RandAwardDataCell> StaticRandAwardDatas = new Dictionary<long, RandAwardDataCell>();
	}
	[MessagePackObject]
	public class RandAwardDataCell
	{
		//id
		[Key(0)]
		public long ID;
		public long GetID()
		{
			return ID;
		}
		//奖励包ID
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
