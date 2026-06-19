//来源表伙伴配置表_Partner.xlsm.xlsx -> sheet:PartnerAssistSlot
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class PartnerAssistSlotData
	{
		[Key(0)]
		public Dictionary<int, PartnerAssistSlotDataCell> StaticPartnerAssistSlotDatas = new Dictionary<int, PartnerAssistSlotDataCell>();
	}
	[MessagePackObject]
	public class PartnerAssistSlotDataCell
	{
		//编号id
		[Key(0)]
		public int Id;
		public int GetId()
		{
			return Id;
		}
		//职业id
		[Key(1)]
		public int Job_id;
		public int GetJob_id()
		{
			return Job_id;
		}
		//槽位id
		[Key(2)]
		public int Slot;
		public int GetSlot()
		{
			return Slot;
		}
		//槽位标记
		[Key(3)]
		public int Mark;
		public int GetMark()
		{
			return Mark;
		}
		//解锁条件
		[Key(4)]
		public int Condition;
		public int GetCondition()
		{
			return Condition;
		}
		//解锁描述
		[Key(5)]
		public string Desc
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_Desc); }
			set { _Desc = value; }
        }
		[IgnoreMember]
		private string _Desc;
		//槽位资质
		[Key(6)]
		public List<int> Qualifications = new List<int>();
		//转化属性
		[Key(7)]
		public List<int> Nature = new List<int>();
	}
}
