//来源表角色升级表_CharacterAttr.xlsx -> sheet:ExpDecay
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class ExpDecayData
	{
		[Key(0)]
		public Dictionary<int, ExpDecayDataCell> StaticExpDecayDatas = new Dictionary<int, ExpDecayDataCell>();
	}
	[MessagePackObject]
	public class ExpDecayDataCell
	{
		//ID
		[Key(0)]
		public int Id;
		public int GetId()
		{
			return Id;
		}
		//超出该值以后百分比
		[Key(1)]
		public int OverLimit;
		public int GetOverLimit()
		{
			return OverLimit;
		}
		//区间上限
		[Key(2)]
		public int OverLimitMax;
		public int GetOverLimitMax()
		{
			return OverLimitMax;
		}
		//衰减至百分比
		[Key(3)]
		public int Decay;
		public int GetDecay()
		{
			return Decay;
		}
	}
}
