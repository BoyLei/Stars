//来源表装备配置表_Equip.xlsm.xlsx -> sheet:EquipSub
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class EquipSubData
	{
		[Key(0)]
		public Dictionary<int, EquipSubDataCell> StaticEquipSubDatas = new Dictionary<int, EquipSubDataCell>();
	}
	[MessagePackObject]
	public class EquipSubDataCell
	{
		//编号
		[Key(0)]
		public int Id;
		public int GetId()
		{
			return Id;
		}
		//名称
		[Key(1)]
		public string Name
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_Name); }
			set { _Name = value; }
        }
		[IgnoreMember]
		private string _Name;
		//显示类型
		[Key(2)]
		public int ShowType;
		public int GetShowType()
		{
			return ShowType;
		}
		//属性类型
		[Key(3)]
		public int AttType;
		public int GetAttType()
		{
			return AttType;
		}
		//稀有度
		[Key(4)]
		public int Value;
		public int GetValue()
		{
			return Value;
		}
		//属性库
		[Key(5)]
		public int Group;
		public int GetGroup()
		{
			return Group;
		}
		//专属部件
		[Key(6)]
		public List<int> Exclusive = new List<int>();
		//属性id
		[Key(7)]
		public int NatureId;
		public int GetNatureId()
		{
			return NatureId;
		}
		//品质
		[Key(8)]
		public List<int> Quality = new List<int>();
		//基础值
		[Key(9)]
		public List<int> Num = new List<int>();
		//战力
		[Key(10)]
		public List<long> Power = new List<long>();
		//等级曲线
		[Key(11)]
		public int Curve;
		public int GetCurve()
		{
			return Curve;
		}
		//解锁等级
		[Key(12)]
		public int Arise;
		public int GetArise()
		{
			return Arise;
		}
		//封闭等级
		[Key(13)]
		public int Lock;
		public int GetLock()
		{
			return Lock;
		}
		//黑名单
		[Key(14)]
		public int Blacklist;
		public int GetBlacklist()
		{
			return Blacklist;
		}
		//产出标识
		[Key(15)]
		public int Mark;
		public int GetMark()
		{
			return Mark;
		}
	}
}
