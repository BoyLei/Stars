//来源表ModelAnimancer.xlsx -> sheet:AnimancerEnterTime
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class AnimancerEnterTimeData
	{
		[Key(0)]
		public Dictionary<int, AnimancerEnterTimeDataCell> StaticAnimancerEnterTimeDatas = new Dictionary<int, AnimancerEnterTimeDataCell>();
	}
	[MessagePackObject]
	public class AnimancerEnterTimeDataCell
	{
		//ID
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//状态名
		[Key(1)]
		public string StateName;
		//进入状态时间
		[Key(2)]
		public int StatrEnterTime;
		public int GetStatrEnterTime()
		{
			return StatrEnterTime;
		}
	}
}
