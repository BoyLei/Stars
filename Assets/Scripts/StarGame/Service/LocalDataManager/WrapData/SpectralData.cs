//来源表Spectral.xlsx -> sheet:Spectral
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
namespace StarProjectDef
{
	[MessagePackObject]
	public class SpectralData
	{
		[Key(0)]
		public Dictionary<int, SpectralDataCell> StaticSpectralDatas = new Dictionary<int, SpectralDataCell>();
	}
	[MessagePackObject]
	public class SpectralDataCell
	{
		//职业ID
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//量谱名
		[Key(1)]
		public string SpectralNames;
	}
}
