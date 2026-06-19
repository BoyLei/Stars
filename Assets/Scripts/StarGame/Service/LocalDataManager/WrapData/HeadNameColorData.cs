//来源表头顶名字颜色_HeadNameColor.xlsx -> sheet:HeadNameColor
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class HeadNameColorData
	{
		[Key(0)]
		public Dictionary<int, HeadNameColorDataCell> StaticHeadNameColorDatas = new Dictionary<int, HeadNameColorDataCell>();
	}
	[MessagePackObject]
	public class HeadNameColorDataCell
	{
		//ID
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//字体颜色
		[Key(1)]
		public string Color;
		//描边
		[Key(2)]
		public string Outline;
		//字号
		[Key(3)]
		public int FontSize;
		public int GetFontSize()
		{
			return FontSize;
		}
	}
}
