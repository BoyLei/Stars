//来源表单服副本_Level.xlsm.xlsx -> sheet:GNGLevel
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class GNGLevelData
	{
		[Key(0)]
		public Dictionary<int, GNGLevelDataCell> StaticGNGLevelDatas = new Dictionary<int, GNGLevelDataCell>();
	}
	[MessagePackObject]
	public class GNGLevelDataCell
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
		//地图的基础信息
		[Key(2)]
		public int BaseID;
		public int GetBaseID()
		{
			return BaseID;
		}
		//副本阻挡物
		[Key(3)]
		public string LevelCollider;
		//等级类型
		[Key(4)]
		public int LevelType;
		public int GetLevelType()
		{
			return LevelType;
		}
		//等级参数
		[Key(5)]
		public int LevelNumber;
		public int GetLevelNumber()
		{
			return LevelNumber;
		}
		//副本持续时间
		[Key(6)]
		public int LimitTime;
		public int GetLimitTime()
		{
			return LimitTime;
		}
		//复活模板
		[Key(7)]
		public int ReviveTid;
		public int GetReviveTid()
		{
			return ReviveTid;
		}
		//关联行为树
		[Key(8)]
		public int BehaviorTree;
		public int GetBehaviorTree()
		{
			return BehaviorTree;
		}
		//是否自动战斗
		[Key(9)]
		public bool AutoBattle;
		public bool GetAutoBattle()
		{
			return AutoBattle;
		}
		//进入重置CD
		[Key(10)]
		public bool EnterCleanCD;
		public bool GetEnterCleanCD()
		{
			return EnterCleanCD;
		}
		//退出重置CD
		[Key(11)]
		public bool ExitCleanCD;
		public bool GetExitCleanCD()
		{
			return ExitCleanCD;
		}
		//是否隐藏伙伴
		[Key(12)]
		public bool IsHidePartner;
		public bool GetIsHidePartner()
		{
			return IsHidePartner;
		}
		//是否隐藏其他玩家
		[Key(13)]
		public bool IsHideOtherPlayer;
		public bool GetIsHideOtherPlayer()
		{
			return IsHideOtherPlayer;
		}
		//是否可旋转镜头
		[Key(14)]
		public bool IsCameraRotate;
		public bool GetIsCameraRotate()
		{
			return IsCameraRotate;
		}
	}
}
