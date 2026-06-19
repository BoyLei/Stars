//来源表道具合成表_Compose.xlsx -> sheet:Compose
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class ComposeData
	{
		[Key(0)]
		public Dictionary<long, ComposeDataCell> StaticComposeDatas = new Dictionary<long, ComposeDataCell>();
	}
	[MessagePackObject]
	public class ComposeDataCell
	{
		//需求道具ID
		[Key(0)]
		public long Id;
		public long GetId()
		{
			return Id;
		}
		//需求道具数量
		[Key(1)]
		public long StuffNum;
		public long GetStuffNum()
		{
			return StuffNum;
		}
		//目标道具ID
		[Key(2)]
		public long TargeID;
		public long GetTargeID()
		{
			return TargeID;
		}
	}
}
