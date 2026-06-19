//来源表通用说明栏_Common_Info.xlsm.xlsx -> sheet:Intro
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class IntroData
	{
		[Key(0)]
		public Dictionary<int, IntroDataCell> StaticIntroDatas = new Dictionary<int, IntroDataCell>();
	}
	[MessagePackObject]
	public class IntroDataCell
	{
		//ID
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//模块名（程序用）
		[Key(1)]
		public string Name;
		//标题多语言Key
		[Key(2)]
		public string IntroTitle
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_IntroTitle); }
			set { _IntroTitle = value; }
        }
		[IgnoreMember]
		private string _IntroTitle;
		//文本内容多语言Key
		[Key(3)]
		public string IntroTxt
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_IntroTxt); }
			set { _IntroTxt = value; }
        }
		[IgnoreMember]
		private string _IntroTxt;
	}
}
