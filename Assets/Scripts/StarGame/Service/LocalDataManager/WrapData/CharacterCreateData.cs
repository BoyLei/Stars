//来源表Job.xlsm.xlsx -> sheet:CharacterCreate
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class CharacterCreateData
	{
		[Key(0)]
		public Dictionary<int, CharacterCreateDataCell> StaticCharacterCreateDatas = new Dictionary<int, CharacterCreateDataCell>();
	}
	[MessagePackObject]
	public class CharacterCreateDataCell
	{
		//编号
		[Key(0)]
		public int Id;
		public int GetId()
		{
			return Id;
		}
		//职业ID
		[Key(1)]
		public int JobID;
		public int GetJobID()
		{
			return JobID;
		}
		//职业描述
		[Key(2)]
		public string Desc
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_Desc); }
			set { _Desc = value; }
        }
		[IgnoreMember]
		private string _Desc;
		//是否可以创建
		[Key(3)]
		public bool IsCanBuilder;
		public bool GetIsCanBuilder()
		{
			return IsCanBuilder;
		}
		//性别选择
		[Key(4)]
		public List<int> SexSelect = new List<int>();
		//职业标签
		[Key(5)]
		public string Label
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_Label); }
			set { _Label = value; }
        }
		[IgnoreMember]
		private string _Label;
		//四维能力值
		[Key(6)]
		public List<int> Power_Value = new List<int>();
		//转职方向
		[Key(7)]
		public List<string> Switch = new List<string>();
		//出场动作
		[Key(8)]
		public string AppreaceAnim;
		//职业3D贴图背景
		[Key(9)]
		public string BgPath;
		//职业3D贴图前景
		[Key(10)]
		public string FgPath;
	}
}
