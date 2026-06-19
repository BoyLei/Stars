//来源表个人秘境_PersonSecret.xlsm.xlsx -> sheet:SecretDailyAward
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class SecretDailyAwardData
	{
		[Key(0)]
		public Dictionary<int, SecretDailyAwardDataCell> StaticSecretDailyAwardDatas = new Dictionary<int, SecretDailyAwardDataCell>();
	}
	[MessagePackObject]
	public class SecretDailyAwardDataCell
	{
		//层数上限
		[Key(0)]
		public int Floor;
		public int GetFloor()
		{
			return Floor;
		}
		//每日奖励
		[Key(1)]
		public long Award;
		public long GetAward()
		{
			return Award;
		}
		//奖励1
		[Key(2)]
		public List<long> RewardID = new List<long>();
		//数量1
		[Key(3)]
		public List<long> RewardNum = new List<long>();
	}
}
