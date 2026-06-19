//来源表个人爬塔表_PersonalTower.xlsx -> sheet:TowerCopy
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class TowerCopyData
	{
		[Key(0)]
		public Dictionary<int, TowerCopyDataCell> StaticTowerCopyDatas = new Dictionary<int, TowerCopyDataCell>();
	}
	[MessagePackObject]
	public class TowerCopyDataCell
	{
		//层数ID
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//解锁条件
		[Key(1)]
		public int ConditionID;
		public int GetConditionID()
		{
			return ConditionID;
		}
		//怪物等级
		[Key(2)]
		public int MonLevel;
		public int GetMonLevel()
		{
			return MonLevel;
		}
		//关卡ID
		[Key(3)]
		public int CopyID;
		public int GetCopyID()
		{
			return CopyID;
		}
		//关卡类型
		[Key(4)]
		public int CopyType;
		public int GetCopyType()
		{
			return CopyType;
		}
		//刷怪点1
		[Key(5)]
		public int MonPos1;
		public int GetMonPos1()
		{
			return MonPos1;
		}
		//刷怪点2
		[Key(6)]
		public int MonPos2;
		public int GetMonPos2()
		{
			return MonPos2;
		}
		//刷怪点3
		[Key(7)]
		public int MonPos3;
		public int GetMonPos3()
		{
			return MonPos3;
		}
		//刷怪点4
		[Key(8)]
		public int MonPos4;
		public int GetMonPos4()
		{
			return MonPos4;
		}
		//刷怪点5
		[Key(9)]
		public int MonPos5;
		public int GetMonPos5()
		{
			return MonPos5;
		}
		//刷怪点6
		[Key(10)]
		public int MonPos6;
		public int GetMonPos6()
		{
			return MonPos6;
		}
		//刷怪点7
		[Key(11)]
		public int MonPos7;
		public int GetMonPos7()
		{
			return MonPos7;
		}
		//刷怪点8
		[Key(12)]
		public int MonPos8;
		public int GetMonPos8()
		{
			return MonPos8;
		}
		//刷怪点9
		[Key(13)]
		public int MonPos9;
		public int GetMonPos9()
		{
			return MonPos9;
		}
		//刷怪点10
		[Key(14)]
		public int MonPos10;
		public int GetMonPos10()
		{
			return MonPos10;
		}
		//关卡限时
		[Key(15)]
		public int TimeLimit;
		public int GetTimeLimit()
		{
			return TimeLimit;
		}
		//常规奖励1
		[Key(16)]
		public List<long> CommonAward = new List<long>();
		//常规奖励数量1
		[Key(17)]
		public List<long> CommonAwardNum = new List<long>();
	}
}
