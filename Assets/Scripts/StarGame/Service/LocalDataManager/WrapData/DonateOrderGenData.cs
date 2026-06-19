//来源表公会捐献_GuildDonate.xlsm.xlsx -> sheet:DonateOrderGen
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class DonateOrderGenData
	{
		[Key(0)]
		public Dictionary<int, DonateOrderGenDataCell> StaticDonateOrderGenDatas = new Dictionary<int, DonateOrderGenDataCell>();
	}
	[MessagePackObject]
	public class DonateOrderGenDataCell
	{
		//ID
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//难度组
		[Key(1)]
		public int DiffcultyGroup;
		public int GetDiffcultyGroup()
		{
			return DiffcultyGroup;
		}
		//生成订单总量
		[Key(2)]
		public int TotalOrderNum;
		public int GetTotalOrderNum()
		{
			return TotalOrderNum;
		}
	}
}
