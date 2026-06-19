//来源表战斗文本信息表_DescText.xlsm.xlsx -> sheet:BuffDesc
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class BuffDescData
	{
		[Key(0)]
		public Dictionary<int, BuffDescDataCell> StaticBuffDescDatas = new Dictionary<int, BuffDescDataCell>();
	}
	[MessagePackObject]
	public class BuffDescDataCell
	{
		//编号id
		[Key(0)]
		public int Id;
		public int GetId()
		{
			return Id;
		}
		//BUFFID
		[Key(1)]
		public int SkillID;
		public int GetSkillID()
		{
			return SkillID;
		}
		//状态等级
		[Key(2)]
		public int Level;
		public int GetLevel()
		{
			return Level;
		}
		//状态名
		[Key(3)]
		public string SkillName
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_SkillName); }
			set { _SkillName = value; }
        }
		[IgnoreMember]
		private string _SkillName;
		//状态描述
		[Key(4)]
		public string Decs
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_Decs); }
			set { _Decs = value; }
        }
		[IgnoreMember]
		private string _Decs;
		//状态图标
		[Key(5)]
		public string IconPath;
		//状态图标
		[Key(6)]
		public string IconPath2;
	}
}
