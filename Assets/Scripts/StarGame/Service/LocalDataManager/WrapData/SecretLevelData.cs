//来源表单服副本_Level.xlsm.xlsx -> sheet:SecretLevel
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class SecretLevelData
	{
		[Key(0)]
		public Dictionary<int, SecretLevelDataCell> StaticSecretLevelDatas = new Dictionary<int, SecretLevelDataCell>();
	}
	[MessagePackObject]
	public class SecretLevelDataCell
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
		//关联行为树
		[Key(4)]
		public int BehaviorTree;
		public int GetBehaviorTree()
		{
			return BehaviorTree;
		}
		//是否自动战斗
		[Key(5)]
		public bool AutoBattle;
		public bool GetAutoBattle()
		{
			return AutoBattle;
		}
		//进入重置CD
		[Key(6)]
		public bool EnterCleanCD;
		public bool GetEnterCleanCD()
		{
			return EnterCleanCD;
		}
		//退出重置CD
		[Key(7)]
		public bool ExitCleanCD;
		public bool GetExitCleanCD()
		{
			return ExitCleanCD;
		}
		//是否隐藏伙伴
		[Key(8)]
		public bool IsHidePartner;
		public bool GetIsHidePartner()
		{
			return IsHidePartner;
		}
		//是否隐藏其他玩家
		[Key(9)]
		public bool IsHideOtherPlayer;
		public bool GetIsHideOtherPlayer()
		{
			return IsHideOtherPlayer;
		}
		//是否可旋转镜头
		[Key(10)]
		public bool IsCameraRotate;
		public bool GetIsCameraRotate()
		{
			return IsCameraRotate;
		}
	}
}
