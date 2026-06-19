//来源表纹章养成表_Heraldry.xlsx -> sheet:HeraldryGrow
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class HeraldryGrowData
	{
		[Key(0)]
		public Dictionary<int, HeraldryGrowDataCell> StaticHeraldryGrowDatas = new Dictionary<int, HeraldryGrowDataCell>();
	}
	[MessagePackObject]
	public class HeraldryGrowDataCell
	{
		//目标等级
		[Key(0)]
		public int GrowLv;
		public int GetGrowLv()
		{
			return GrowLv;
		}
		//需求纹章1等级
		[Key(1)]
		public int NeedLv1;
		public int GetNeedLv1()
		{
			return NeedLv1;
		}
		//需求纹章1数量
		[Key(2)]
		public long NeedNum1;
		public long GetNeedNum1()
		{
			return NeedNum1;
		}
		//需求纹章2等级
		[Key(3)]
		public int NeedLv2;
		public int GetNeedLv2()
		{
			return NeedLv2;
		}
		//需求纹章2数量
		[Key(4)]
		public long NeedNum2;
		public long GetNeedNum2()
		{
			return NeedNum2;
		}
		//需求纹章3等级
		[Key(5)]
		public int NeedLv3;
		public int GetNeedLv3()
		{
			return NeedLv3;
		}
		//需求纹章3数量
		[Key(6)]
		public long NeedNum3;
		public long GetNeedNum3()
		{
			return NeedNum3;
		}
	}
}
