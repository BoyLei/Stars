//来源表个人秘境_PersonSecret.xlsm.xlsx -> sheet:SecretRank
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class SecretRankData
	{
		[Key(0)]
		public Dictionary<int, SecretRankDataCell> StaticSecretRankDatas = new Dictionary<int, SecretRankDataCell>();
	}
	[MessagePackObject]
	public class SecretRankDataCell
	{
		//排行ID
		[Key(0)]
		public int RankID;
		public int GetRankID()
		{
			return RankID;
		}
		//排名要求
		[Key(1)]
		public int Rank;
		public int GetRank()
		{
			return Rank;
		}
		//排行标题
		[Key(2)]
		public string RankTitle
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_RankTitle); }
			set { _RankTitle = value; }
        }
		[IgnoreMember]
		private string _RankTitle;
		//排行奖励组ID
		[Key(3)]
		public long RankAward;
		public long GetRankAward()
		{
			return RankAward;
		}
		//奖励1
		[Key(4)]
		public List<long> RewardID = new List<long>();
		//数量1
		[Key(5)]
		public List<long> RewardNum = new List<long>();
	}
}
