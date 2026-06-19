//来源表派对时刻_PartyTime.xlsm.xlsx -> sheet:PartyQA
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class PartyQAData
	{
		[Key(0)]
		public Dictionary<int, PartyQADataCell> StaticPartyQADatas = new Dictionary<int, PartyQADataCell>();
	}
	[MessagePackObject]
	public class PartyQADataCell
	{
		//问题ID
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//问题类型
		[Key(1)]
		public int Qtype;
		public int GetQtype()
		{
			return Qtype;
		}
		//问题内容多语言Key
		[Key(2)]
		public string Qcontent
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_Qcontent); }
			set { _Qcontent = value; }
        }
		[IgnoreMember]
		private string _Qcontent;
		//选项1多语言Key
		[Key(3)]
		public string Option1
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_Option1); }
			set { _Option1 = value; }
        }
		[IgnoreMember]
		private string _Option1;
		//选项2多语言Key
		[Key(4)]
		public string Option2
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_Option2); }
			set { _Option2 = value; }
        }
		[IgnoreMember]
		private string _Option2;
		//选项3多语言Key
		[Key(5)]
		public string Option3
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_Option3); }
			set { _Option3 = value; }
        }
		[IgnoreMember]
		private string _Option3;
		//选项4多语言Key
		[Key(6)]
		public string Option4
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_Option4); }
			set { _Option4 = value; }
        }
		[IgnoreMember]
		private string _Option4;
		//参与奖励
		[Key(7)]
		public List<long> JoinReward = new List<long>();
		//参与奖励数量
		[Key(8)]
		public List<long> JoinRewardNum = new List<long>();
		//答题正确奖励
		[Key(9)]
		public List<long> TrueReward = new List<long>();
		//答题正确奖励数量
		[Key(10)]
		public List<long> TrueRewardNum = new List<long>();
	}
}
