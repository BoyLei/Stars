//来源表抽卡表_Gacha.xlsm.xlsx -> sheet:Probability
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class ProbabilityData
	{
		[Key(0)]
		public Dictionary<int, ProbabilityDataCell> StaticProbabilityDatas = new Dictionary<int, ProbabilityDataCell>();
	}
	[MessagePackObject]
	public class ProbabilityDataCell
	{
		//概率组ID
		[Key(0)]
		public int ProbGroupID;
		public int GetProbGroupID()
		{
			return ProbGroupID;
		}
		//N卡抽出概率
		[Key(1)]
		public int NProb;
		public int GetNProb()
		{
			return NProb;
		}
		//R卡抽出概率
		[Key(2)]
		public int RProb;
		public int GetRProb()
		{
			return RProb;
		}
		//SR卡抽出概率
		[Key(3)]
		public int SRProb;
		public int GetSRProb()
		{
			return SRProb;
		}
		//SSR卡抽出概率
		[Key(4)]
		public int SSRProb;
		public int GetSSRProb()
		{
			return SSRProb;
		}
		//SR卡额外up概率
		[Key(5)]
		public int UPSRProb;
		public int GetUPSRProb()
		{
			return UPSRProb;
		}
		//SSR卡额外up概率
		[Key(6)]
		public int UPSSRProb;
		public int GetUPSSRProb()
		{
			return UPSSRProb;
		}
		//开始SSR概率增加时抽数
		[Key(7)]
		public int StartUpTime;
		public int GetStartUpTime()
		{
			return StartUpTime;
		}
		//概率增加时每抽增加概率
		[Key(8)]
		public int PerIncreaseProb;
		public int GetPerIncreaseProb()
		{
			return PerIncreaseProb;
		}
		//强制保底次数
		[Key(9)]
		public int ForceSSRTimes;
		public int GetForceSSRTimes()
		{
			return ForceSSRTimes;
		}
	}
}
