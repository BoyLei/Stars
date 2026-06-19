//来源表跑马灯_Marquee.xlsx -> sheet:MarqueeItem
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class MarqueeItemData
	{
		[Key(0)]
		public Dictionary<long, MarqueeItemDataCell> StaticMarqueeItemDatas = new Dictionary<long, MarqueeItemDataCell>();
	}
	[MessagePackObject]
	public class MarqueeItemDataCell
	{
		//道具ID
		[Key(0)]
		public long ItemID;
		public long GetItemID()
		{
			return ItemID;
		}
		//服务器等级上限
		[Key(1)]
		public int SeverLevel;
		public int GetSeverLevel()
		{
			return SeverLevel;
		}
	}
}
