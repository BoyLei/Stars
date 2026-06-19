//来源表伙伴配置表_Partner.xlsm.xlsx -> sheet:UpStarItemTransToken
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class UpStarItemTransTokenData
	{
		[Key(0)]
		public Dictionary<int, UpStarItemTransTokenDataCell> StaticUpStarItemTransTokenDatas = new Dictionary<int, UpStarItemTransTokenDataCell>();
	}
	[MessagePackObject]
	public class UpStarItemTransTokenDataCell
	{
		//编号id
		[Key(0)]
		public int Id;
		public int GetId()
		{
			return Id;
		}
		//伙伴品质
		[Key(1)]
		public int Quality;
		public int GetQuality()
		{
			return Quality;
		}
		//转换伙伴商店代币ID
		[Key(2)]
		public long TransTokenID;
		public long GetTransTokenID()
		{
			return TransTokenID;
		}
		//转换代币数量
		[Key(3)]
		public long TransTokenNum;
		public long GetTransTokenNum()
		{
			return TransTokenNum;
		}
	}
}
