//来源表宝箱_Boxes.xlsm.xlsx -> sheet:FixAward
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class FixAwardData
	{
		[Key(0)]
		public Dictionary<long, FixAwardDataCell> StaticFixAwardDatas = new Dictionary<long, FixAwardDataCell>();
	}
	[MessagePackObject]
	public class FixAwardDataCell
	{
		//ID
		[Key(0)]
		public long ID;
		public long GetID()
		{
			return ID;
		}
		//奖励包ID
		[Key(1)]
		public long AwardID;
		public long GetAwardID()
		{
			return AwardID;
		}
		//道具数量
		[Key(2)]
		public int Count;
		public int GetCount()
		{
			return Count;
		}
		//道具id
		[Key(3)]
		public List<long> ItemIDs = new List<long>();
	}
}
