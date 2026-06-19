//来源表装备强化表_Intensify.xlsm.xlsx -> sheet:EquipSlotResonance
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class EquipSlotResonanceData
	{
		[Key(0)]
		public Dictionary<int, EquipSlotResonanceDataCell> StaticEquipSlotResonanceDatas = new Dictionary<int, EquipSlotResonanceDataCell>();
	}
	[MessagePackObject]
	public class EquipSlotResonanceDataCell
	{
		//编号id
		[Key(0)]
		public int Id;
		public int GetId()
		{
			return Id;
		}
		//共鸣等级
		[Key(1)]
		public int ResonanceLv;
		public int GetResonanceLv()
		{
			return ResonanceLv;
		}
		//共鸣描述
		[Key(2)]
		public string Desc
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_Desc); }
			set { _Desc = value; }
        }
		[IgnoreMember]
		private string _Desc;
		//职业
		[Key(3)]
		public List<int> Job_id = new List<int>();
		//强化等级
		[Key(4)]
		public int Level;
		public int GetLevel()
		{
			return Level;
		}
		//共鸣属性
		[Key(5)]
		public List<int> Nature = new List<int>();
		//共鸣属性值
		[Key(6)]
		public List<int> Value = new List<int>();
		//战力
		[Key(7)]
		public long Power;
		public long GetPower()
		{
			return Power;
		}
		//共鸣图标
		[Key(8)]
		public string Icon;
	}
}
