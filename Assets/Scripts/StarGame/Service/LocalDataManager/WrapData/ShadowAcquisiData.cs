//来源表Spectral.xlsx -> sheet:ShadowAcquisi
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
namespace StarProjectDef
{
	[MessagePackObject]
	public class ShadowAcquisiData
	{
		[Key(0)]
		public Dictionary<int, ShadowAcquisiDataCell> StaticShadowAcquisiDatas = new Dictionary<int, ShadowAcquisiDataCell>();
	}
	[MessagePackObject]
	public class ShadowAcquisiDataCell
	{
		//职业技能类型
		[Key(0)]
		public int JSType;
		public int GetJSType()
		{
			return JSType;
		}
		//可以获得的阴影值
		[Key(1)]
		public int CanShadow;
		public int GetCanShadow()
		{
			return CanShadow;
		}
	}
}
