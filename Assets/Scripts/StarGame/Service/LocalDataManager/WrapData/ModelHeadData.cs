//来源表头像配置表_Head.xlsx -> sheet:ModelHead
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class ModelHeadData
	{
		[Key(0)]
		public Dictionary<int, ModelHeadDataCell> StaticModelHeadDatas = new Dictionary<int, ModelHeadDataCell>();
	}
	[MessagePackObject]
	public class ModelHeadDataCell
	{
		//模型ID
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//半身像
		[Key(1)]
		public string HalfDrawing;
		//头像图集
		[Key(2)]
		public string HeadAtlasName;
		//大头像
		[Key(3)]
		public string MaxHead;
		//中头像
		[Key(4)]
		public string MiddleHead;
		//小头像
		[Key(5)]
		public string MinHead;
		//职业icon
		[Key(6)]
		public string JobIcon;
		//抽卡用半身像
		[Key(7)]
		public string GachaHalfDraw;
	}
}
