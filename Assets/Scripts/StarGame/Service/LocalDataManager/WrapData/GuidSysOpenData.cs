//来源表新手引导表_PlayerGuidance.xlsm.xlsx -> sheet:GuidSysOpen
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class GuidSysOpenData
	{
		[Key(0)]
		public Dictionary<int, GuidSysOpenDataCell> StaticGuidSysOpenDatas = new Dictionary<int, GuidSysOpenDataCell>();
	}
	[MessagePackObject]
	public class GuidSysOpenDataCell
	{
		//编号
		[Key(0)]
		public int Id;
		public int GetId()
		{
			return Id;
		}
		//枚举类型
		[Key(1)]
		public string EnumName;
		//需求条件-角色等级
		[Key(2)]
		public int NeedLv;
		public int GetNeedLv()
		{
			return NeedLv;
		}
		//需求条件2-冒险等级
		[Key(3)]
		public int NeedAdvLv;
		public int GetNeedAdvLv()
		{
			return NeedAdvLv;
		}
		//需求条件3-任务完成
		[Key(4)]
		public int NeedQuestCom;
		public int GetNeedQuestCom()
		{
			return NeedQuestCom;
		}
		//是否有拍脸
		[Key(5)]
		public bool IsHaveOpenPic;
		public bool GetIsHaveOpenPic()
		{
			return IsHaveOpenPic;
		}
		//拍脸插入Icon
		[Key(6)]
		public string OpenIconRes;
		//拍脸大标题
		[Key(7)]
		public string OpenTitle
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_OpenTitle); }
			set { _OpenTitle = value; }
        }
		[IgnoreMember]
		private string _OpenTitle;
		//拍脸文本描述
		[Key(8)]
		public string OpenDesc
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_OpenDesc); }
			set { _OpenDesc = value; }
        }
		[IgnoreMember]
		private string _OpenDesc;
	}
}
