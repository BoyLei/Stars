//来源表单服副本_Level.xlsm.xlsx -> sheet:MirrorLevel
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class MirrorLevelData
	{
		[Key(0)]
		public Dictionary<int, MirrorLevelDataCell> StaticMirrorLevelDatas = new Dictionary<int, MirrorLevelDataCell>();
	}
	[MessagePackObject]
	public class MirrorLevelDataCell
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
		//副本UI
		[Key(4)]
		public string LevelPage;
		//副本阻挡物
		[Key(5)]
		public string LevelCollider;
		//等级类型
		[Key(6)]
		public int LevelType;
		public int GetLevelType()
		{
			return LevelType;
		}
		//等级参数
		[Key(7)]
		public int LevelNumber;
		public int GetLevelNumber()
		{
			return LevelNumber;
		}
		//副本持续时间
		[Key(8)]
		public int LimitTime;
		public int GetLimitTime()
		{
			return LimitTime;
		}
		//关联行为树
		[Key(9)]
		public int BehaviorTree;
		public int GetBehaviorTree()
		{
			return BehaviorTree;
		}
		//通用条件
		[Key(10)]
		public int CommonConditionTid;
		public int GetCommonConditionTid()
		{
			return CommonConditionTid;
		}
		//复活模板
		[Key(11)]
		public int ReviveTid;
		public int GetReviveTid()
		{
			return ReviveTid;
		}
		//通关奖励
		[Key(12)]
		public int RewardTid;
		public int GetRewardTid()
		{
			return RewardTid;
		}
		//是否自动战斗
		[Key(13)]
		public bool AutoBattle;
		public bool GetAutoBattle()
		{
			return AutoBattle;
		}
		//是否显示波纹效果
		[Key(14)]
		public bool IsShowEffect;
		public bool GetIsShowEffect()
		{
			return IsShowEffect;
		}
		//进入重置CD
		[Key(15)]
		public bool EnterCleanCD;
		public bool GetEnterCleanCD()
		{
			return EnterCleanCD;
		}
		//退出重置CD
		[Key(16)]
		public bool ExitCleanCD;
		public bool GetExitCleanCD()
		{
			return ExitCleanCD;
		}
		//是否隐藏伙伴
		[Key(17)]
		public bool IsHidePartner;
		public bool GetIsHidePartner()
		{
			return IsHidePartner;
		}
		//是否隐藏其他玩家
		[Key(18)]
		public bool IsHideOtherPlayer;
		public bool GetIsHideOtherPlayer()
		{
			return IsHideOtherPlayer;
		}
		//是否可旋转镜头
		[Key(19)]
		public bool IsCameraRotate;
		public bool GetIsCameraRotate()
		{
			return IsCameraRotate;
		}
	}
}
