//来源表角色升级表_CharacterAttr.xlsx -> sheet:LevelLimit
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class LevelLimitData
	{
		[Key(0)]
		public Dictionary<int, LevelLimitDataCell> StaticLevelLimitDatas = new Dictionary<int, LevelLimitDataCell>();
	}
	[MessagePackObject]
	public class LevelLimitDataCell
	{
		//ID
		[Key(0)]
		public int Id;
		public int GetId()
		{
			return Id;
		}
		//开服天数
		[Key(1)]
		public int DayId;
		public int GetDayId()
		{
			return DayId;
		}
		//角色等级
		[Key(2)]
		public int Level;
		public int GetLevel()
		{
			return Level;
		}
		//追赶解锁等级
		[Key(3)]
		public int CatchUnlock;
		public int GetCatchUnlock()
		{
			return CatchUnlock;
		}
		//追赶限制最高等级
		[Key(4)]
		public int CatchMaxLv;
		public int GetCatchMaxLv()
		{
			return CatchMaxLv;
		}
		//加速百分比
		[Key(5)]
		public int CatchPercent;
		public int GetCatchPercent()
		{
			return CatchPercent;
		}
		//每日可获得经验上限
		[Key(6)]
		public long ExpDailyLimit;
		public long GetExpDailyLimit()
		{
			return ExpDailyLimit;
		}
		//经验储存上限
		[Key(7)]
		public int StorageLimit;
		public int GetStorageLimit()
		{
			return StorageLimit;
		}
	}
}
