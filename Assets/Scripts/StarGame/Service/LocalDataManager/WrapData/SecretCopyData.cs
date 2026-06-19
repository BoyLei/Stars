//来源表个人秘境_PersonSecret.xlsm.xlsx -> sheet:SecretCopy
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class SecretCopyData
	{
		[Key(0)]
		public Dictionary<int, SecretCopyDataCell> StaticSecretCopyDatas = new Dictionary<int, SecretCopyDataCell>();
	}
	[MessagePackObject]
	public class SecretCopyDataCell
	{
		//层数
		[Key(0)]
		public int Floor;
		public int GetFloor()
		{
			return Floor;
		}
		//对应门票
		[Key(1)]
		public long TicketID;
		public long GetTicketID()
		{
			return TicketID;
		}
		//冒险等级
		[Key(2)]
		public int AdvGrade;
		public int GetAdvGrade()
		{
			return AdvGrade;
		}
		//冒险等级上限
		[Key(3)]
		public int AdvGradeUpper;
		public int GetAdvGradeUpper()
		{
			return AdvGradeUpper;
		}
		//角色等级
		[Key(4)]
		public int Grade;
		public int GetGrade()
		{
			return Grade;
		}
		//角色等级上限
		[Key(5)]
		public int GradeUpper;
		public int GetGradeUpper()
		{
			return GradeUpper;
		}
		//奖励1
		[Key(6)]
		public long Award;
		public long GetAward()
		{
			return Award;
		}
		//奖励2
		[Key(7)]
		public long Award2;
		public long GetAward2()
		{
			return Award2;
		}
		//奖励3
		[Key(8)]
		public long Award3;
		public long GetAward3()
		{
			return Award3;
		}
		//首通特殊掉落
		[Key(9)]
		public long FirstSpecialReward;
		public long GetFirstSpecialReward()
		{
			return FirstSpecialReward;
		}
		//支持场景
		[Key(10)]
		public List<int> Maps = new List<int>();
		//推荐战力
		[Key(11)]
		public int RecommendAbility;
		public int GetRecommendAbility()
		{
			return RecommendAbility;
		}
		//最大跳过层数
		[Key(12)]
		public int Skip;
		public int GetSkip()
		{
			return Skip;
		}
		//最小等级
		[Key(13)]
		public int MinLv;
		public int GetMinLv()
		{
			return MinLv;
		}
		//最大等级
		[Key(14)]
		public int MaxLv;
		public int GetMaxLv()
		{
			return MaxLv;
		}
		//小怪群落
		[Key(15)]
		public List<int> Lackey = new List<int>();
		//小怪群落
		[Key(16)]
		public List<int> Lackey1 = new List<int>();
		//小怪群落
		[Key(17)]
		public List<int> Lackey2 = new List<int>();
		//精英群落
		[Key(18)]
		public List<int> Elite = new List<int>();
		//boss群落
		[Key(19)]
		public List<int> BOSS = new List<int>();
		//奖励怪
		[Key(20)]
		public List<int> RewardMon = new List<int>();
		//奖励怪出现权重
		[Key(21)]
		public List<int> RewardMonWeight = new List<int>();
		//奖励怪buffid
		[Key(22)]
		public List<int> RewardMonBuff = new List<int>();
		//奖励怪buff随机权重
		[Key(23)]
		public List<int> RewardMonBuffWeight = new List<int>();
		//奖励1
		[Key(24)]
		public List<long> RewardID = new List<long>();
		//首通奖励1
		[Key(25)]
		public List<long> FirstAward = new List<long>();
		//首通奖励数量1
		[Key(26)]
		public List<long> FirstAwardNum = new List<long>();
	}
}
