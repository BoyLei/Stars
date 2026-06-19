//来源表通行证表_BattlePass.xlsx -> sheet:BPSeason
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class BPSeasonData
	{
		[Key(0)]
		public Dictionary<int, BPSeasonDataCell> StaticBPSeasonDatas = new Dictionary<int, BPSeasonDataCell>();
	}
	[MessagePackObject]
	public class BPSeasonDataCell
	{
		//赛季ID
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//奖励组
		[Key(1)]
		public int AwardGroup;
		public int GetAwardGroup()
		{
			return AwardGroup;
		}
		//开启时间
		[Key(2)]
		public int StartTime;
		public int GetStartTime()
		{
			return StartTime;
		}
		//结束时间
		[Key(3)]
		public int EndTime;
		public int GetEndTime()
		{
			return EndTime;
		}
		//大奖展示伙伴
		[Key(4)]
		public int BigPrizePartner;
		public int GetBigPrizePartner()
		{
			return BigPrizePartner;
		}
		//大奖获得BP等级
		[Key(5)]
		public int BigPrizeLevel;
		public int GetBigPrizeLevel()
		{
			return BigPrizeLevel;
		}
	}
}
