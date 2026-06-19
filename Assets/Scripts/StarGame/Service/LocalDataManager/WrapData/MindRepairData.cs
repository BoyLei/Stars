//来源表心灵修复_MindRepair.xlsx -> sheet:MindRepair
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class MindRepairData
	{
		[Key(0)]
		public Dictionary<int, MindRepairDataCell> StaticMindRepairDatas = new Dictionary<int, MindRepairDataCell>();
	}
	[MessagePackObject]
	public class MindRepairDataCell
	{
		//序号
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//步数限制
		[Key(1)]
		public int StepLimitCount;
		public int GetStepLimitCount()
		{
			return StepLimitCount;
		}
		//第一行格子
		[Key(2)]
		public List<string> Grids = new List<string>();
	}
}
