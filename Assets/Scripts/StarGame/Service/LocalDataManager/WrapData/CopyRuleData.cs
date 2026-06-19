//来源表组队日常本表_TeamDailyCopy.xlsm.xlsx -> sheet:CopyRule
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class CopyRuleData
	{
		[Key(0)]
		public Dictionary<int, CopyRuleDataCell> StaticCopyRuleDatas = new Dictionary<int, CopyRuleDataCell>();
	}
	[MessagePackObject]
	public class CopyRuleDataCell
	{
		//ID
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//对应关卡机制ID
		[Key(1)]
		public int RuleID;
		public int GetRuleID()
		{
			return RuleID;
		}
		//对应关卡机制组ID
		[Key(2)]
		public int RuleGroupID;
		public int GetRuleGroupID()
		{
			return RuleGroupID;
		}
		//出现权重
		[Key(3)]
		public int Weight;
		public int GetWeight()
		{
			return Weight;
		}
	}
}
