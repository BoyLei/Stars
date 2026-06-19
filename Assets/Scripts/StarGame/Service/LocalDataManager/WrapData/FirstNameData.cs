//来源表随机姓名_RandomName.xlsx -> sheet:FirstName
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class FirstNameData
	{
		[Key(0)]
		public Dictionary<int, FirstNameDataCell> StaticFirstNameDatas = new Dictionary<int, FirstNameDataCell>();
	}
	[MessagePackObject]
	public class FirstNameDataCell
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
