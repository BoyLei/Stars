//来源表道具配置表_Item.xlsm.xlsx -> sheet:Itemfilter
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class ItemfilterData
	{
		[Key(0)]
		public Dictionary<int, ItemfilterDataCell> StaticItemfilterDatas = new Dictionary<int, ItemfilterDataCell>();
	}
	[MessagePackObject]
	public class ItemfilterDataCell
	{
		//筛选器ID
		[Key(0)]
		public int Filter;
		public int GetFilter()
		{
			return Filter;
		}
		//清除规则
		[Key(1)]
		public int CleanRule;
		public int GetCleanRule()
		{
			return CleanRule;
		}
		//排序优先级
		[Key(2)]
		public int SortPriority;
		public int GetSortPriority()
		{
			return SortPriority;
		}
		//筛选器类别
		[Key(3)]
		public int FilterType;
		public int GetFilterType()
		{
			return FilterType;
		}
		//加载选项
		[Key(4)]
		public List<int> OperaOption = new List<int>();
	}
}
