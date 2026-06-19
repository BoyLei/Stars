//来源表Pv表_PvList.xlsx -> sheet:PvList
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class PvListData
	{
		[Key(0)]
		public Dictionary<int, PvListDataCell> StaticPvListDatas = new Dictionary<int, PvListDataCell>();
	}
	[MessagePackObject]
	public class PvListDataCell
	{
		//ID
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//音频名
		[Key(1)]
		public string SoundEventName;
		//PV文件路径
		[Key(2)]
		public string PvPath;
		//PV文件路径
		[Key(3)]
		public string PvPathEN;
		//是否可跳过
		[Key(4)]
		public bool IsSkip;
		public bool GetIsSkip()
		{
			return IsSkip;
		}
		//是否进入闪黑
		[Key(5)]
		public bool IsInBalack;
		public bool GetIsInBalack()
		{
			return IsInBalack;
		}
		//是否退出闪黑
		[Key(6)]
		public bool IsOutBlack;
		public bool GetIsOutBlack()
		{
			return IsOutBlack;
		}
	}
}
