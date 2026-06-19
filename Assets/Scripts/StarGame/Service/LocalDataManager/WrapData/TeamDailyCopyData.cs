//来源表组队日常本表_TeamDailyCopy.xlsm.xlsx -> sheet:TeamDailyCopy
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class TeamDailyCopyData
	{
		[Key(0)]
		public Dictionary<int, TeamDailyCopyDataCell> StaticTeamDailyCopyDatas = new Dictionary<int, TeamDailyCopyDataCell>();
	}
	[MessagePackObject]
	public class TeamDailyCopyDataCell
	{
		//ID
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//解锁等级
		[Key(1)]
		public int UnlockLevel;
		public int GetUnlockLevel()
		{
			return UnlockLevel;
		}
		//解锁冒险等级
		[Key(2)]
		public int AdvGrade;
		public int GetAdvGrade()
		{
			return AdvGrade;
		}
		//区域名称-key
		[Key(3)]
		public string AreaName
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_AreaName); }
			set { _AreaName = value; }
        }
		[IgnoreMember]
		private string _AreaName;
		//副本入口场景
		[Key(4)]
		public int EntryMapID;
		public int GetEntryMapID()
		{
			return EntryMapID;
		}
		//对应怪物库
		[Key(5)]
		public int BOSSGroupID;
		public int GetBOSSGroupID()
		{
			return BOSSGroupID;
		}
		//对应关卡机制组
		[Key(6)]
		public int RuleGroupID;
		public int GetRuleGroupID()
		{
			return RuleGroupID;
		}
		//常规奖励
		[Key(7)]
		public long CommonAward;
		public long GetCommonAward()
		{
			return CommonAward;
		}
		//常规奖励展示
		[Key(8)]
		public List<int> CommonAwardsShow = new List<int>();
		//首通特殊掉落
		[Key(9)]
		public long FirstSpecialReward;
		public long GetFirstSpecialReward()
		{
			return FirstSpecialReward;
		}
		//额外奖励（奖励怪）
		[Key(10)]
		public long ExtraAward;
		public long GetExtraAward()
		{
			return ExtraAward;
		}
		//奖励怪ID
		[Key(11)]
		public long ExtraMonster;
		public long GetExtraMonster()
		{
			return ExtraMonster;
		}
		//背景图资源
		[Key(12)]
		public string BackGroud;
		//推荐战力
		[Key(13)]
		public int Fight;
		public int GetFight()
		{
			return Fight;
		}
		//文本描述-KEY
		[Key(14)]
		public string Desc
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_Desc); }
			set { _Desc = value; }
        }
		[IgnoreMember]
		private string _Desc;
		//玩法ID（快捷组队用）
		[Key(15)]
		public int PlayID;
		public int GetPlayID()
		{
			return PlayID;
		}
	}
}
