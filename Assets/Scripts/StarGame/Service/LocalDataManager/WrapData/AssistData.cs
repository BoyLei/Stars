//来源表伙伴配置表_Partner.xlsm.xlsx -> sheet:Assist
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class AssistData
	{
		[Key(0)]
		public Dictionary<int, AssistDataCell> StaticAssistDatas = new Dictionary<int, AssistDataCell>();
	}
	[MessagePackObject]
	public class AssistDataCell
	{
		//编号id
		[Key(0)]
		public int Id;
		public int GetId()
		{
			return Id;
		}
		//资质描述
		[Key(1)]
		public string Desc
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_Desc); }
			set { _Desc = value; }
        }
		[IgnoreMember]
		private string _Desc;
		//职业id
		[Key(2)]
		public int Job_id;
		public int GetJob_id()
		{
			return Job_id;
		}
		//助战槽位
		[Key(3)]
		public int Slot;
		public int GetSlot()
		{
			return Slot;
		}
		//槽位资质
		[Key(4)]
		public List<int> Qualifications = new List<int>();
		//转化属性
		[Key(5)]
		public List<int> Nature = new List<int>();
		//资质要求
		[Key(6)]
		public List<int> Require = new List<int>();
		//转化比例
		[Key(7)]
		public List<int> Ratio = new List<int>();
		//战力
		[Key(8)]
		public long Power;
		public long GetPower()
		{
			return Power;
		}
		//战力系数
		[Key(9)]
		public long PowerCo;
		public long GetPowerCo()
		{
			return PowerCo;
		}
	}
}
