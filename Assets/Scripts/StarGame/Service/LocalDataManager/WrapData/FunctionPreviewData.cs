//来源表新手引导表_PlayerGuidance.xlsm.xlsx -> sheet:FunctionPreview
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class FunctionPreviewData
	{
		[Key(0)]
		public Dictionary<int, FunctionPreviewDataCell> StaticFunctionPreviewDatas = new Dictionary<int, FunctionPreviewDataCell>();
	}
	[MessagePackObject]
	public class FunctionPreviewDataCell
	{
		//ID
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//解锁表ID
		[Key(1)]
		public int GuidSysOpenID;
		public int GetGuidSysOpenID()
		{
			return GuidSysOpenID;
		}
		//功能名字
		[Key(2)]
		public string SysName
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_SysName); }
			set { _SysName = value; }
        }
		[IgnoreMember]
		private string _SysName;
		//功能标识
		[Key(3)]
		public string SysIcon;
		//功能描述
		[Key(4)]
		public string SysDesc
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_SysDesc); }
			set { _SysDesc = value; }
        }
		[IgnoreMember]
		private string _SysDesc;
		//解锁奖励
		[Key(5)]
		public long Reward;
		public long GetReward()
		{
			return Reward;
		}
		//奖励数量
		[Key(6)]
		public long RewardNum;
		public long GetRewardNum()
		{
			return RewardNum;
		}
	}
}
