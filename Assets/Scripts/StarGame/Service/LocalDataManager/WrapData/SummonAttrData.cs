//来源表Monster.xlsm.xlsx -> sheet:SummonAttr
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class SummonAttrData
	{
		[Key(0)]
		public Dictionary<int, SummonAttrDataCell> StaticSummonAttrDatas = new Dictionary<int, SummonAttrDataCell>();
	}
	[MessagePackObject]
	public class SummonAttrDataCell
	{
		//序号
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
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
		//等级
		[Key(2)]
		public int Level;
		public int GetLevel()
		{
			return Level;
		}
		//存活时间
		[Key(3)]
		public int LiveTime;
		public int GetLiveTime()
		{
			return LiveTime;
		}
		//距离召唤者长度
		[Key(4)]
		public int Len;
		public int GetLen()
		{
			return Len;
		}
		//是否跟随召唤者死亡
		[Key(5)]
		public bool DieWithOwnerDie;
		public bool GetDieWithOwnerDie()
		{
			return DieWithOwnerDie;
		}
		//是否跟随召唤者脱战死亡
		[Key(6)]
		public bool DieWithOwnerOutBattle;
		public bool GetDieWithOwnerOutBattle()
		{
			return DieWithOwnerOutBattle;
		}
		//使用模板
		[Key(7)]
		public int UseTemplate;
		public int GetUseTemplate()
		{
			return UseTemplate;
		}
		//属性ID
		[Key(8)]
		public List<int> AttrID = new List<int>();
		//属性值
		[Key(9)]
		public List<long> AttrValue = new List<long>();
		//血条段数
		[Key(10)]
		public int HealthBarCount;
		public int GetHealthBarCount()
		{
			return HealthBarCount;
		}
	}
}
