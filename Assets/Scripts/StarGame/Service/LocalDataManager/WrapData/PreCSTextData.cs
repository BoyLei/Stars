//来源表静态文本_PreCSText.xlsx -> sheet:PreCSText
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class PreCSTextData
	{
		[Key(0)]
		public Dictionary<string, PreCSTextDataCell> StaticPreCSTextDatas = new Dictionary<string, PreCSTextDataCell>();
	}
	[MessagePackObject]
	public class PreCSTextDataCell
	{
		//多语言Key
		[Key(0)]
		public string Key;
		//中文
		[Key(1)]
		public string Cn;
		//英文
		[Key(2)]
		public string En;
	}
}
