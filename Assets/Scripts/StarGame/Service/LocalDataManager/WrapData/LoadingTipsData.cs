//来源表loading.xlsm.xlsx -> sheet:LoadingTips
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class LoadingTipsData
	{
		[Key(0)]
		public Dictionary<int, LoadingTipsDataCell> StaticLoadingTipsDatas = new Dictionary<int, LoadingTipsDataCell>();
	}
	[MessagePackObject]
	public class LoadingTipsDataCell
	{
		//ID
		[Key(0)]
		public int Id;
		public int GetId()
		{
			return Id;
		}
		//文本-KEY
		[Key(1)]
		public string Desc
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_Desc); }
			set { _Desc = value; }
        }
		[IgnoreMember]
		private string _Desc;
		//Tips库ID
		[Key(2)]
		public int TipsID;
		public int GetTipsID()
		{
			return TipsID;
		}
	}
}
