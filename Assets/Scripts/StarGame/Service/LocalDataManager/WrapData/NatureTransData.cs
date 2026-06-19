//来源表伙伴配置表_Partner.xlsm.xlsx -> sheet:NatureTrans
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class NatureTransData
	{
		[Key(0)]
		public Dictionary<int, NatureTransDataCell> StaticNatureTransDatas = new Dictionary<int, NatureTransDataCell>();
	}
	[MessagePackObject]
	public class NatureTransDataCell
	{
		//编号id
		[Key(0)]
		public int Id;
		public int GetId()
		{
			return Id;
		}
		//伙伴属性
		[Key(1)]
		public int Natures;
		public int GetNatures()
		{
			return Natures;
		}
		//要求
		[Key(2)]
		public int Require;
		public int GetRequire()
		{
			return Require;
		}
		//转化比例
		[Key(3)]
		public int Ratio;
		public int GetRatio()
		{
			return Ratio;
		}
	}
}
