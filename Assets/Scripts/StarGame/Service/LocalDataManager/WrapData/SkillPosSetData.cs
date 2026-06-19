//来源表Job.xlsm.xlsx -> sheet:SkillPosSet
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class SkillPosSetData
	{
		[Key(0)]
		public Dictionary<int, SkillPosSetDataCell> StaticSkillPosSetDatas = new Dictionary<int, SkillPosSetDataCell>();
	}
	[MessagePackObject]
	public class SkillPosSetDataCell
	{
		//技能位
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//技能类型
		[Key(1)]
		public int SkillType;
		public int GetSkillType()
		{
			return SkillType;
		}
		//技能类型
		[Key(2)]
		public int Sort;
		public int GetSort()
		{
			return Sort;
		}
		//技能位置
		[Key(3)]
		public List<int> SkillPos = new List<int>();
		//按钮缩放比例
		[Key(4)]
		public int Scale;
		public int GetScale()
		{
			return Scale;
		}
		//键盘key
		[Key(5)]
		public string KeyCode;
		//技能背景
		[Key(6)]
		public string SkillBg;
	}
}
