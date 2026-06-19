//来源表随机姓名_RandomName.xlsx -> sheet:LastName
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class LastNameData
	{
		[Key(0)]
		public Dictionary<int, LastNameDataCell> StaticLastNameDatas = new Dictionary<int, LastNameDataCell>();
	}
	[MessagePackObject]
	public class LastNameDataCell
	{
		//ID
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//文本key
		[Key(1)]
		public string Text
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_Text); }
			set { _Text = value; }
        }
		[IgnoreMember]
		private string _Text;
	}
}
