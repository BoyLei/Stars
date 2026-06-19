//来源表副本复活配置表_Revive.xlsx -> sheet:LevelRevive
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class LevelReviveData
	{
		[Key(0)]
		public Dictionary<int, LevelReviveDataCell> StaticLevelReviveDatas = new Dictionary<int, LevelReviveDataCell>();
	}
	[MessagePackObject]
	public class LevelReviveDataCell
	{
		//复活id
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//是否能安全复活
		[Key(1)]
		public bool Is_reborn;
		public bool GetIs_reborn()
		{
			return Is_reborn;
		}
		//副本是否能原地复活
		[Key(2)]
		public bool Is_perfect_enable;
		public bool GetIs_perfect_enable()
		{
			return Is_perfect_enable;
		}
		//副本阵营复活点复活
		[Key(3)]
		public bool Is_reborn_point_enable;
		public bool GetIs_reborn_point_enable()
		{
			return Is_reborn_point_enable;
		}
		//副本是否团灭复活
		[Key(4)]
		public bool Is_aced_revive_enable;
		public bool GetIs_aced_revive_enable()
		{
			return Is_aced_revive_enable;
		}
		//副本复活减少副本时间
		[Key(5)]
		public int Inst_time_dec_per_death;
		public int GetInst_time_dec_per_death()
		{
			return Inst_time_dec_per_death;
		}
		//副本安全复活倒计时
		[Key(6)]
		public int Reborn_point_time_limit;
		public int GetReborn_point_time_limit()
		{
			return Reborn_point_time_limit;
		}
		//副本个人限制复活次数
		[Key(7)]
		public int Personal_revive_times;
		public int GetPersonal_revive_times()
		{
			return Personal_revive_times;
		}
		//副本队伍限制复活次数
		[Key(8)]
		public int Team_revive_times;
		public int GetTeam_revive_times()
		{
			return Team_revive_times;
		}
		//副本团灭复活倒计时
		[Key(9)]
		public int Aced_revive_time;
		public int GetAced_revive_time()
		{
			return Aced_revive_time;
		}
		//安全复活血量百分比
		[Key(10)]
		public int Reborn_hp_percent;
		public int GetReborn_hp_percent()
		{
			return Reborn_hp_percent;
		}
	}
}
