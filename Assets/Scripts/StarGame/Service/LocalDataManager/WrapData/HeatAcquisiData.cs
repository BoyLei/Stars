//来源表Spectral.xlsx -> sheet:HeatAcquisi
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
namespace StarProjectDef
{
	[MessagePackObject]
	public class HeatAcquisiData
	{
		[Key(0)]
		public Dictionary<int, HeatAcquisiDataCell> StaticHeatAcquisiDatas = new Dictionary<int, HeatAcquisiDataCell>();
	}
	[MessagePackObject]
	public class HeatAcquisiDataCell
	{
		//职业技能类型
		[Key(0)]
		public int JSType;
		public int GetJSType()
		{
			return JSType;
		}
		//可以获得的热量值
		[Key(1)]
		public int CanHeat;
		public int GetCanHeat()
		{
			return CanHeat;
		}
	}
}
