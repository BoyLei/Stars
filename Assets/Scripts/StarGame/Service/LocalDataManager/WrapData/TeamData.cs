//来源表组队目标配置表_Team.xlsm.xlsx -> sheet:Team
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
namespace StarProjectDef
{
	[MessagePackObject]
	public class TeamData
	{
		[Key(0)]
		public Dictionary<int, TeamDataCell> StaticTeamDatas = new Dictionary<int, TeamDataCell>();
	}
	[MessagePackObject]
	public class TeamDataCell
	{
		//编号
		[Key(0)]
		public int Id;
		public int GetId()
		{
			return Id;
		}
		//玩法类型
		[Key(1)]
		public int PlayType;
		public int GetPlayType()
		{
			return PlayType;
		}
		//玩法描述
		[Key(2)]
		public string Desc;
		//解锁等级
		[Key(3)]
		public int Unlock_Level;
		public int GetUnlock_Level()
		{
			return Unlock_Level;
		}
		//参与人数
		[Key(4)]
		public List<int> Player_Num = new List<int>();
		//开启时间
		[Key(5)]
		public string Open_Time;
		//推荐战力
		[Key(6)]
		public int Power;
		public int GetPower()
		{
			return Power;
		}
		//是否跨服
		[Key(7)]
		public int IsTransSrv;
		public int GetIsTransSrv()
		{
			return IsTransSrv;
		}
	}
}
