//来源表抽卡表_Gacha.xlsm.xlsx -> sheet:GachaRedDot
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class GachaRedDotData
	{
		[Key(0)]
		public Dictionary<int, GachaRedDotDataCell> StaticGachaRedDotDatas = new Dictionary<int, GachaRedDotDataCell>();
	}
	[MessagePackObject]
	public class GachaRedDotDataCell
	{
		//卡池ID
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//行为类型
		[Key(1)]
		public int BehaviorType;
		public int GetBehaviorType()
		{
			return BehaviorType;
		}
	}
}
