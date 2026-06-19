//来源表属性天赋树表_AttriTalentTree.xlsm.xlsx -> sheet:AttriTTNode
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class AttriTTNodeData
	{
		[Key(0)]
		public Dictionary<int, AttriTTNodeDataCell> StaticAttriTTNodeDatas = new Dictionary<int, AttriTTNodeDataCell>();
	}
	[MessagePackObject]
	public class AttriTTNodeDataCell
	{
		//唯一KEY
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//所属天赋树区域
		[Key(1)]
		public int FromTree;
		public int GetFromTree()
		{
			return FromTree;
		}
		//节点类型
		[Key(2)]
		public int NodeType;
		public int GetNodeType()
		{
			return NodeType;
		}
		//前置节点1
		[Key(3)]
		public List<int> PredeNode = new List<int>();
		//最大等级
		[Key(4)]
		public int MaxLevel;
		public int GetMaxLevel()
		{
			return MaxLevel;
		}
		//增加属性ID
		[Key(5)]
		public int AttrID;
		public int GetAttrID()
		{
			return AttrID;
		}
		//合计点数对应道具
		[Key(6)]
		public long TotalCountItem;
		public long GetTotalCountItem()
		{
			return TotalCountItem;
		}
		//需要消耗总天赋点
		[Key(7)]
		public long TotalNeedNum;
		public long GetTotalNeedNum()
		{
			return TotalNeedNum;
		}
		//1级增加属性值
		[Key(8)]
		public long FirLvAttrValue;
		public long GetFirLvAttrValue()
		{
			return FirLvAttrValue;
		}
		//战力
		[Key(9)]
		public long Power1;
		public long GetPower1()
		{
			return Power1;
		}
		//消耗道具
		[Key(10)]
		public List<long> FirLvUseItem = new List<long>();
		//消耗道具数量
		[Key(11)]
		public List<long> FirLvNum = new List<long>();
		//2级增加属性值
		[Key(12)]
		public long SecLvAttrValue;
		public long GetSecLvAttrValue()
		{
			return SecLvAttrValue;
		}
		//战力
		[Key(13)]
		public long Power2;
		public long GetPower2()
		{
			return Power2;
		}
		//消耗道具
		[Key(14)]
		public List<long> SecUseItem = new List<long>();
		//消耗道具数量
		[Key(15)]
		public List<long> SecLvNum = new List<long>();
		//3级增加属性值
		[Key(16)]
		public long ThrLvAttrValue;
		public long GetThrLvAttrValue()
		{
			return ThrLvAttrValue;
		}
		//战力
		[Key(17)]
		public long Power3;
		public long GetPower3()
		{
			return Power3;
		}
		//消耗道具
		[Key(18)]
		public List<long> ThrLvUseItem = new List<long>();
		//消耗道具数量
		[Key(19)]
		public List<long> ThrLvNum = new List<long>();
		//解锁优先级
		[Key(20)]
		public int UnlockPrior;
		public int GetUnlockPrior()
		{
			return UnlockPrior;
		}
	}
}
