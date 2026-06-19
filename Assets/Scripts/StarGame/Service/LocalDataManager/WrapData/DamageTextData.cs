//来源表文字表现_DamageText.xlsx -> sheet:DamageText
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class DamageTextData
	{
		[Key(0)]
		public Dictionary<int, DamageTextDataCell> StaticDamageTextDatas = new Dictionary<int, DamageTextDataCell>();
	}
	[MessagePackObject]
	public class DamageTextDataCell
	{
		//伤害类型ID
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//上投影
		[Key(1)]
		public string UpperColor;
		//下投影
		[Key(2)]
		public string LowerColor;
		//字体颜色
		[Key(3)]
		public string Outline;
		//字体颜色
		[Key(4)]
		public int ColorIndex;
		public int GetColorIndex()
		{
			return ColorIndex;
		}
		//字体下标
		[Key(5)]
		public int TextIndex;
		public int GetTextIndex()
		{
			return TextIndex;
		}
		//字号
		[Key(6)]
		public int FontSize;
		public int GetFontSize()
		{
			return FontSize;
		}
		//挂点区域
		[Key(7)]
		public int DisplayArea;
		public int GetDisplayArea()
		{
			return DisplayArea;
		}
		//动画类型
		[Key(8)]
		public int ActionType;
		public int GetActionType()
		{
			return ActionType;
		}
	}
}
