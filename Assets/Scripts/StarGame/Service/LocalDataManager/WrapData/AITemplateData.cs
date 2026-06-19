//来源表AI配置表.xlsx -> sheet:AITemplate
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class AITemplateData
	{
		[Key(0)]
		public Dictionary<int, AITemplateDataCell> StaticAITemplateDatas = new Dictionary<int, AITemplateDataCell>();
	}
	[MessagePackObject]
	public class AITemplateDataCell
	{
		//编号
		[Key(0)]
		public int Id;
		public int GetId()
		{
			return Id;
		}
		//最佳攻击距离
		[Key(1)]
		public int BestAttackRange;
		public int GetBestAttackRange()
		{
			return BestAttackRange;
		}
		//追击速度
		[Key(2)]
		public int RunSpeed;
		public int GetRunSpeed()
		{
			return RunSpeed;
		}
		//是否朝着目标
		[Key(3)]
		public bool IsLookAtTarget;
		public bool GetIsLookAtTarget()
		{
			return IsLookAtTarget;
		}
		//巡逻速度
		[Key(4)]
		public int WalkSpeed;
		public int GetWalkSpeed()
		{
			return WalkSpeed;
		}
		//巡逻类型
		[Key(5)]
		public int WanderType;
		public int GetWanderType()
		{
			return WanderType;
		}
		//巡逻范围
		[Key(6)]
		public int WanderRange;
		public int GetWanderRange()
		{
			return WanderRange;
		}
		//休息几率
		[Key(7)]
		public int RestChance;
		public int GetRestChance()
		{
			return RestChance;
		}
		//休息时间
		[Key(8)]
		public int RestTime;
		public int GetRestTime()
		{
			return RestTime;
		}
		//发呆几率
		[Key(9)]
		public int DumbChance;
		public int GetDumbChance()
		{
			return DumbChance;
		}
		//发呆时间
		[Key(10)]
		public List<int> DumbTime = new List<int>();
		//索敌范围半径
		[Key(11)]
		public int ScanEnemyRange;
		public int GetScanEnemyRange()
		{
			return ScanEnemyRange;
		}
		//索敌半角度（正面朝向左右半角）
		[Key(12)]
		public int ScanEnemyAngle;
		public int GetScanEnemyAngle()
		{
			return ScanEnemyAngle;
		}
		//索敌类型
		[Key(13)]
		public int ScanEnemyType;
		public int GetScanEnemyType()
		{
			return ScanEnemyType;
		}
		//技能全CD是否风筝
		[Key(14)]
		public bool IsKite;
		public bool GetIsKite()
		{
			return IsKite;
		}
		//风筝行为CD
		[Key(15)]
		public int KeepAwayCd;
		public int GetKeepAwayCd()
		{
			return KeepAwayCd;
		}
		//风筝触发距离
		[Key(16)]
		public int KeepAwayMaxRange;
		public int GetKeepAwayMaxRange()
		{
			return KeepAwayMaxRange;
		}
		//风筝移动最大时长
		[Key(17)]
		public int KeepAwayTime;
		public int GetKeepAwayTime()
		{
			return KeepAwayTime;
		}
		//风筝触发权重
		[Key(18)]
		public int KeepAwayChance;
		public int GetKeepAwayChance()
		{
			return KeepAwayChance;
		}
		//逃离CD
		[Key(19)]
		public int RunawayCd;
		public int GetRunawayCd()
		{
			return RunawayCd;
		}
		//逃离速度
		[Key(20)]
		public int RunawaySpeed;
		public int GetRunawaySpeed()
		{
			return RunawaySpeed;
		}
		//逃离最大时间
		[Key(21)]
		public int Time;
		public int GetTime()
		{
			return Time;
		}
		//逃离触发生命值百分比
		[Key(22)]
		public int RunawayHealthPct;
		public int GetRunawayHealthPct()
		{
			return RunawayHealthPct;
		}
		//脱战距离
		[Key(23)]
		public int HomeRange;
		public int GetHomeRange()
		{
			return HomeRange;
		}
		//强拉回主人距离
		[Key(24)]
		public int FlashRange;
		public int GetFlashRange()
		{
			return FlashRange;
		}
		//包含状态
		[Key(25)]
		public List<int> OwnStates = new List<int>();
	}
}
