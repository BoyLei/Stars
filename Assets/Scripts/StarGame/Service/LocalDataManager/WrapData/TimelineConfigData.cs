//来源表Timeline配置表.xlsx -> sheet:TimelineConfig
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class TimelineConfigData
	{
		[Key(0)]
		public Dictionary<int, TimelineConfigDataCell> StaticTimelineConfigDatas = new Dictionary<int, TimelineConfigDataCell>();
	}
	[MessagePackObject]
	public class TimelineConfigDataCell
	{
		//ID
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//Timeline路径
		[Key(1)]
		public string TimelinePath;
		//类型
		[Key(2)]
		public int TimelineType;
		public int GetTimelineType()
		{
			return TimelineType;
		}
		//结束隐藏Timeline
		[Key(3)]
		public bool FinishHide;
		public bool GetFinishHide()
		{
			return FinishHide;
		}
		//播放时隐藏UI
		[Key(4)]
		public bool HideUI;
		public bool GetHideUI()
		{
			return HideUI;
		}
		//播放时隐藏主角
		[Key(5)]
		public bool HideRole;
		public bool GetHideRole()
		{
			return HideRole;
		}
		//播放时隐藏伙伴
		[Key(6)]
		public bool HidePartner;
		public bool GetHidePartner()
		{
			return HidePartner;
		}
		//是否使用主角模型
		[Key(7)]
		public bool UseRoleModel;
		public bool GetUseRoleModel()
		{
			return UseRoleModel;
		}
		//是否可移动
		[Key(8)]
		public bool IsCanMove;
		public bool GetIsCanMove()
		{
			return IsCanMove;
		}
		//是否隐藏地图NPC
		[Key(9)]
		public bool HideMapNPC;
		public bool GetHideMapNPC()
		{
			return HideMapNPC;
		}
		//是否隐藏其他玩家
		[Key(10)]
		public bool HideOtherPlayer;
		public bool GetHideOtherPlayer()
		{
			return HideOtherPlayer;
		}
		//是否可跳过
		[Key(11)]
		public bool IsSkip;
		public bool GetIsSkip()
		{
			return IsSkip;
		}
		//是否进入闪黑
		[Key(12)]
		public bool IsInBalack;
		public bool GetIsInBalack()
		{
			return IsInBalack;
		}
		//是否退出闪黑
		[Key(13)]
		public bool IsOutBlack;
		public bool GetIsOutBlack()
		{
			return IsOutBlack;
		}
		//是否隐藏怪物
		[Key(14)]
		public bool HideMon;
		public bool GetHideMon()
		{
			return HideMon;
		}
		//是否隐藏交互物
		[Key(15)]
		public bool HideObj;
		public bool GetHideObj()
		{
			return HideObj;
		}
		//是否隐藏空气墙
		[Key(16)]
		public bool HideAirWall;
		public bool GetHideAirWall()
		{
			return HideAirWall;
		}
	}
}
