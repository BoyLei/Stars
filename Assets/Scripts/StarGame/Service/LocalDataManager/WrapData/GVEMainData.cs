//来源表玩法晚间GVE_PlayModeGVE.xlsx -> sheet:GVEMain
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class GVEMainData
	{
		[Key(0)]
		public Dictionary<int, GVEMainDataCell> StaticGVEMainDatas = new Dictionary<int, GVEMainDataCell>();
	}
	[MessagePackObject]
	public class GVEMainDataCell
	{
		//玩法ID
		[Key(0)]
		public int Id;
		public int GetId()
		{
			return Id;
		}
		//控制器组ID
		[Key(1)]
		public int ControGroup;
		public int GetControGroup()
		{
			return ControGroup;
		}
		//活动怪物索敌范围
		[Key(2)]
		public int ScanRange;
		public int GetScanRange()
		{
			return ScanRange;
		}
		//活动怪物脱战范围
		[Key(3)]
		public int OutwarRange;
		public int GetOutwarRange()
		{
			return OutwarRange;
		}
		//活动怪物被动
		[Key(4)]
		public List<int> PassiveSkills = new List<int>();
	}
}
