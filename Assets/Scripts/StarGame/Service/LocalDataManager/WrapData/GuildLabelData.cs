//来源表公会表_Guild.xlsm.xlsx -> sheet:GuildLabel
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class GuildLabelData
	{
		[Key(0)]
		public Dictionary<int, GuildLabelDataCell> StaticGuildLabelDatas = new Dictionary<int, GuildLabelDataCell>();
	}
	[MessagePackObject]
	public class GuildLabelDataCell
	{
		//标签ID
		[Key(0)]
		public int LabelID;
		public int GetLabelID()
		{
			return LabelID;
		}
		//标签描述
		[Key(1)]
		public string LabelDec
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_LabelDec); }
			set { _LabelDec = value; }
        }
		[IgnoreMember]
		private string _LabelDec;
	}
}
