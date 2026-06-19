//来源表个人秘境_PersonSecret.xlsm.xlsx -> sheet:SeasonAchieve
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class SeasonAchieveData
	{
		[Key(0)]
		public Dictionary<long, SeasonAchieveDataCell> StaticSeasonAchieveDatas = new Dictionary<long, SeasonAchieveDataCell>();
	}
	[MessagePackObject]
	public class SeasonAchieveDataCell
	{
		//成就ID
		[Key(0)]
		public long AchieveID;
		public long GetAchieveID()
		{
			return AchieveID;
		}
		//成就分类
		[Key(1)]
		public long AchieveType;
		public long GetAchieveType()
		{
			return AchieveType;
		}
		//成就子类型
		[Key(2)]
		public long SubType;
		public long GetSubType()
		{
			return SubType;
		}
		//成就标题
		[Key(3)]
		public string AchieveTitle
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_AchieveTitle); }
			set { _AchieveTitle = value; }
        }
		[IgnoreMember]
		private string _AchieveTitle;
		//成就描述
		[Key(4)]
		public string AchieveDec
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_AchieveDec); }
			set { _AchieveDec = value; }
        }
		[IgnoreMember]
		private string _AchieveDec;
		//目标数量
		[Key(5)]
		public long TargetNum;
		public long GetTargetNum()
		{
			return TargetNum;
		}
		//目标参数
		[Key(6)]
		public List<string> Paramas = new List<string>();
		//成就奖励
		[Key(7)]
		public long AchieveAward;
		public long GetAchieveAward()
		{
			return AchieveAward;
		}
		//奖励1
		[Key(8)]
		public List<long> RewardID = new List<long>();
		//数量1
		[Key(9)]
		public List<long> RewardNum = new List<long>();
	}
}
