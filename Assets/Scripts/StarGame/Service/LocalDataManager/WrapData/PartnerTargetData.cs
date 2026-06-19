//来源表伙伴目标_PartnerTarget.xlsm.xlsx -> sheet:PartnerTarget
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class PartnerTargetData
	{
		[Key(0)]
		public Dictionary<int, PartnerTargetDataCell> StaticPartnerTargetDatas = new Dictionary<int, PartnerTargetDataCell>();
	}
	[MessagePackObject]
	public class PartnerTargetDataCell
	{
		//ID
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//任务ID
		[Key(1)]
		public List<int> TaskId = new List<int>();
		//目标阶段
		[Key(2)]
		public int Stage;
		public int GetStage()
		{
			return Stage;
		}
		//角色描述文本-KEY
		[Key(3)]
		public string RewardDecs
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_RewardDecs); }
			set { _RewardDecs = value; }
        }
		[IgnoreMember]
		private string _RewardDecs;
		//任务介绍文本-KEY
		[Key(4)]
		public string Decs
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_Decs); }
			set { _Decs = value; }
        }
		[IgnoreMember]
		private string _Decs;
		//图片路径
		[Key(5)]
		public string Pic;
	}
}
