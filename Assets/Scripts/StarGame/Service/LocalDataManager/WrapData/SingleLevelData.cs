//来源表SingleLevelData
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
namespace StarProjectDef
{
	public class SingleLevelData
	{
		public Dictionary<int, SingleLevelDataCell> StaticSingleLevelDatas = new Dictionary<int, SingleLevelDataCell>();
	}
	public class SingleLevelDataCell
	{
		//副本ID
		public string ID;
		private int _ID = -1;
		public int GetID()
		{
			if (_ID == -1 && int.TryParse(ID, out _ID))
			{
			}
			return _ID;
		}
		//副本名称
		public string LevelName;
		//资源名称
		public string MapName;
		//小地图路径
		public string MiniMap;
		//副本UI
		public string LevelPage;
		//副本阻挡物
		public string LevelCollider;
		//副本实体
		public string SpaceType;
		//副本类型
		public string LevelType;
		private int _LevelType = -1;
		public int GetLevelType()
		{
			if (_LevelType == -1 && int.TryParse(LevelType, out _LevelType))
			{
			}
			return _LevelType;
		}
		//进出副本是否保留状态
		public string IsKeepState;
		public bool GetIsKeepState()
		{
			bool _IsKeepState;
			if (bool.TryParse(IsKeepState, out _IsKeepState))
			{
			}
			return _IsKeepState;
		}
		//是否允许战斗状态进入
		public string IsBattle;
		public bool GetIsBattle()
		{
			bool _IsBattle;
			if (bool.TryParse(IsBattle, out _IsBattle))
			{
			}
			return _IsBattle;
		}
		//副本持续时间
		public string LimitTime;
		private int _LimitTime = -1;
		public int GetLimitTime()
		{
			if (_LimitTime == -1 && int.TryParse(LimitTime, out _LimitTime))
			{
			}
			return _LimitTime;
		}
		//关联行为树
		public string BehaviorTree;
		private int _BehaviorTree = -1;
		public int GetBehaviorTree()
		{
			if (_BehaviorTree == -1 && int.TryParse(BehaviorTree, out _BehaviorTree))
			{
			}
			return _BehaviorTree;
		}
		//关联逻辑表
		public string LevelFlowTid;
		private int _LevelFlowTid = -1;
		public int GetLevelFlowTid()
		{
			if (_LevelFlowTid == -1 && int.TryParse(LevelFlowTid, out _LevelFlowTid))
			{
			}
			return _LevelFlowTid;
		}
		//通用条件
		public string CommonConditionTid;
		private int _CommonConditionTid = -1;
		public int GetCommonConditionTid()
		{
			if (_CommonConditionTid == -1 && int.TryParse(CommonConditionTid, out _CommonConditionTid))
			{
			}
			return _CommonConditionTid;
		}
		//通用消耗
		public string CommonCostTid;
		private int _CommonCostTid = -1;
		public int GetCommonCostTid()
		{
			if (_CommonCostTid == -1 && int.TryParse(CommonCostTid, out _CommonCostTid))
			{
			}
			return _CommonCostTid;
		}
		//通用限次
		public string CommonCountTid;
		private int _CommonCountTid = -1;
		public int GetCommonCountTid()
		{
			if (_CommonCountTid == -1 && int.TryParse(CommonCountTid, out _CommonCountTid))
			{
			}
			return _CommonCountTid;
		}
		//复活模板
		public string ReviveTid;
		private int _ReviveTid = -1;
		public int GetReviveTid()
		{
			if (_ReviveTid == -1 && int.TryParse(ReviveTid, out _ReviveTid))
			{
			}
			return _ReviveTid;
		}
		//退出弹窗
		public string QuitID;
		private int _QuitID = -1;
		public int GetQuitID()
		{
			if (_QuitID == -1 && int.TryParse(QuitID, out _QuitID))
			{
			}
			return _QuitID;
		}
		//通关奖励
		public string RewardTid;
		private int _RewardTid = -1;
		public int GetRewardTid()
		{
			if (_RewardTid == -1 && int.TryParse(RewardTid, out _RewardTid))
			{
			}
			return _RewardTid;
		}
		//是否使用临时背包
		public string IsUseTempBag;
		public bool GetIsUseTempBag()
		{
			bool _IsUseTempBag;
			if (bool.TryParse(IsUseTempBag, out _IsUseTempBag))
			{
			}
			return _IsUseTempBag;
		}

	}
}
