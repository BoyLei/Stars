//来源表个人秘境_PersonSecret.xlsm.xlsx -> sheet:TicketSynthesize
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class TicketSynthesizeData
	{
		[Key(0)]
		public Dictionary<long, TicketSynthesizeDataCell> StaticTicketSynthesizeDatas = new Dictionary<long, TicketSynthesizeDataCell>();
	}
	[MessagePackObject]
	public class TicketSynthesizeDataCell
	{
		//门票ID
		[Key(0)]
		public long ID;
		public long GetID()
		{
			return ID;
		}
		//素材道具
		[Key(1)]
		public long MaterialID;
		public long GetMaterialID()
		{
			return MaterialID;
		}
		//数量
		[Key(2)]
		public long Num;
		public long GetNum()
		{
			return Num;
		}
	}
}
