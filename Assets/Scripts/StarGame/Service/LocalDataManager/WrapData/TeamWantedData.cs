//来源表通缉任务表_TeamWanted.xlsx -> sheet:TeamWanted
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class TeamWantedData
	{
		[Key(0)]
		public Dictionary<int, TeamWantedDataCell> StaticTeamWantedDatas = new Dictionary<int, TeamWantedDataCell>();
	}
	[MessagePackObject]
	public class TeamWantedDataCell
	{
		//目标类型ID
		[Key(0)]
		public int TypeID;
		public int GetTypeID()
		{
			return TypeID;
		}
		//刷新开始时间
		[Key(1)]
		public int CreateTime;
		public int GetCreateTime()
		{
			return CreateTime;
		}
		//刷新结束时间
		[Key(2)]
		public int EndTime;
		public int GetEndTime()
		{
			return EndTime;
		}
		//单次刷新数量
		[Key(3)]
		public int CreateNum;
		public int GetCreateNum()
		{
			return CreateNum;
		}
		//刷新间隔
		[Key(4)]
		public int CreateGap;
		public int GetCreateGap()
		{
			return CreateGap;
		}
		//生成场景
		[Key(5)]
		public List<long> WantedMap = new List<long>();
		//对应怪物组
		[Key(6)]
		public List<long> MonGroup = new List<long>();
		//对应关卡
		[Key(7)]
		public int LevelID;
		public int GetLevelID()
		{
			return LevelID;
		}
		//关联NPC
		[Key(8)]
		public int NPCID;
		public int GetNPCID()
		{
			return NPCID;
		}
		//关联交互物ID
		[Key(9)]
		public int InteractID;
		public int GetInteractID()
		{
			return InteractID;
		}
		//最小进入人数
		[Key(10)]
		public int Enter_Min;
		public int GetEnter_Min()
		{
			return Enter_Min;
		}
	}
}
