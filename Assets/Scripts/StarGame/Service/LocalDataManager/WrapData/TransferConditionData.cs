//来源表Job.xlsm.xlsx -> sheet:TransferCondition
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class TransferConditionData
	{
		[Key(0)]
		public Dictionary<int, TransferConditionDataCell> StaticTransferConditionDatas = new Dictionary<int, TransferConditionDataCell>();
	}
	[MessagePackObject]
	public class TransferConditionDataCell
	{
		//ID
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//条件ID
		[Key(1)]
		public int ConditionID;
		public int GetConditionID()
		{
			return ConditionID;
		}
		//条件描述
		[Key(2)]
		public string AchieveDec
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_AchieveDec); }
			set { _AchieveDec = value; }
        }
		[IgnoreMember]
		private string _AchieveDec;
		//奖励1
		[Key(3)]
		public List<long> RewardID = new List<long>();
		//数量1
		[Key(4)]
		public List<long> RewardNum = new List<long>();
	}
}
