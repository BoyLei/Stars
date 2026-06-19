//来源表个人爬塔表_PersonalTower.xlsx -> sheet:TowerStage
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class TowerStageData
	{
		[Key(0)]
		public Dictionary<int, TowerStageDataCell> StaticTowerStageDatas = new Dictionary<int, TowerStageDataCell>();
	}
	[MessagePackObject]
	public class TowerStageDataCell
	{
		//层数ID
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//阶段奖励1
		[Key(1)]
		public List<long> StageAward = new List<long>();
		//阶段奖励数量1
		[Key(2)]
		public List<long> StageAwardNum = new List<long>();
	}
}
