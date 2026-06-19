//来源表奖励广播表_Broadcast.xlsx -> sheet:Broadcast
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class BroadcastData
	{
		[Key(0)]
		public Dictionary<long, BroadcastDataCell> StaticBroadcastDatas = new Dictionary<long, BroadcastDataCell>();
	}
	[MessagePackObject]
	public class BroadcastDataCell
	{
		//宝箱道具ID
		[Key(0)]
		public long BoxItemID;
		public long GetBoxItemID()
		{
			return BoxItemID;
		}
		//道具品质
		[Key(1)]
		public int Quality;
		public int GetQuality()
		{
			return Quality;
		}
	}
}
