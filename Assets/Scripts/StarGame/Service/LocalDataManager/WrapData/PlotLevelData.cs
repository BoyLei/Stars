//来源表单服副本_Level.xlsm.xlsx -> sheet:PlotLevel
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class PlotLevelData
	{
		[Key(0)]
		public Dictionary<int, PlotLevelDataCell> StaticPlotLevelDatas = new Dictionary<int, PlotLevelDataCell>();
	}
	[MessagePackObject]
	public class PlotLevelDataCell
	{
		//副本ID
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//副本名称
		[Key(1)]
		public string LevelName
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_LevelName); }
			set { _LevelName = value; }
        }
		[IgnoreMember]
		private string _LevelName;
		//副本子类型
		[Key(2)]
		public string SubType;
		//地图的基础信息
		[Key(3)]
		public int BaseID;
		public int GetBaseID()
		{
			return BaseID;
		}
		//副本阻挡物
		[Key(4)]
		public string LevelCollider;
		//等级类型
		[Key(5)]
		public int LevelType;
		public int GetLevelType()
		{
			return LevelType;
		}
		//等级参数
		[Key(6)]
		public int LevelNumber;
		public int GetLevelNumber()
		{
			return LevelNumber;
		}
		//进出副本是否保留状态
		[Key(7)]
		public bool IsKeepState;
		public bool GetIsKeepState()
		{
			return IsKeepState;
		}
		//是否允许战斗状态进入
		[Key(8)]
		public bool IsBattle;
		public bool GetIsBattle()
		{
			return IsBattle;
		}
		//副本持续时间
		[Key(9)]
		public int LimitTime;
		public int GetLimitTime()
		{
			return LimitTime;
		}
		//关联行为树
		[Key(10)]
		public int BehaviorTree;
		public int GetBehaviorTree()
		{
			return BehaviorTree;
		}
		//通用条件
		[Key(11)]
		public int CommonConditionTid;
		public int GetCommonConditionTid()
		{
			return CommonConditionTid;
		}
		//通用消耗
		[Key(12)]
		public int CommonCostTid;
		public int GetCommonCostTid()
		{
			return CommonCostTid;
		}
		//通用限次
		[Key(13)]
		public int CommonCountTid;
		public int GetCommonCountTid()
		{
			return CommonCountTid;
		}
		//复活模板
		[Key(14)]
		public int ReviveTid;
		public int GetReviveTid()
		{
			return ReviveTid;
		}
		//通关奖励
		[Key(15)]
		public int RewardTid;
		public int GetRewardTid()
		{
			return RewardTid;
		}
		//是否自动战斗
		[Key(16)]
		public bool AutoBattle;
		public bool GetAutoBattle()
		{
			return AutoBattle;
		}
		//进入重置CD
		[Key(17)]
		public bool EnterCleanCD;
		public bool GetEnterCleanCD()
		{
			return EnterCleanCD;
		}
		//退出重置CD
		[Key(18)]
		public bool ExitCleanCD;
		public bool GetExitCleanCD()
		{
			return ExitCleanCD;
		}
		//是否隐藏伙伴
		[Key(19)]
		public bool IsHidePartner;
		public bool GetIsHidePartner()
		{
			return IsHidePartner;
		}
		//是否隐藏其他玩家
		[Key(20)]
		public bool IsHideOtherPlayer;
		public bool GetIsHideOtherPlayer()
		{
			return IsHideOtherPlayer;
		}
		//是否可旋转镜头
		[Key(21)]
		public bool IsCameraRotate;
		public bool GetIsCameraRotate()
		{
			return IsCameraRotate;
		}
	}
}
