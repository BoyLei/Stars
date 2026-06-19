//来源表MonsterAttrData
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
namespace StarProjectDef
{
	public class MonsterAttrData
	{
		public Dictionary<long, MonsterAttrDataCell> StaticMonsterAttrDatas = new Dictionary<long, MonsterAttrDataCell>();
	}
	public class MonsterAttrDataCell
	{
		//序号
		public string ID;
		private long _ID = -1;
		public long GetID()
		{
			if (_ID == -1 && long.TryParse(ID, out _ID))
			{
			}
			return _ID;
		}
		//称谓
		public string Appellation;
		//姓名
		public string Name;
		//等级
		public string Level;
		private int _Level = -1;
		public int GetLevel()
		{
			if (_Level == -1 && int.TryParse(Level, out _Level))
			{
			}
			return _Level;
		}
		//类型
		public string MonType;
		private int _MonType = -1;
		public int GetMonType()
		{
			if (_MonType == -1 && int.TryParse(MonType, out _MonType))
			{
			}
			return _MonType;
		}
		//阵营
		public string FactionID;
		private int _FactionID = -1;
		public int GetFactionID()
		{
			if (_FactionID == -1 && int.TryParse(FactionID, out _FactionID))
			{
			}
			return _FactionID;
		}
		//使用模板
		public string UseTemplate;
		private long _UseTemplate = -1;
		public long GetUseTemplate()
		{
			if (_UseTemplate == -1 && long.TryParse(UseTemplate, out _UseTemplate))
			{
			}
			return _UseTemplate;
		}
		//属性ID
		public List<int> AttrID = new List<int>();
		//属性值
		public List<int> AttrValue = new List<int>();
		//掉落经验
		public string DropExp;
		private long _DropExp = -1;
		public long GetDropExp()
		{
			if (_DropExp == -1 && long.TryParse(DropExp, out _DropExp))
			{
			}
			return _DropExp;
		}
		//掉落归属
		public string Owner;
		private int _Owner = -1;
		public int GetOwner()
		{
			if (_Owner == -1 && int.TryParse(Owner, out _Owner))
			{
			}
			return _Owner;
		}
		//掉落ID
		public string DropID;
		private long _DropID = -1;
		public long GetDropID()
		{
			if (_DropID == -1 && long.TryParse(DropID, out _DropID))
			{
			}
			return _DropID;
		}
		//血条段数
		public string HealthBarCount;
		private int _HealthBarCount = -1;
		public int GetHealthBarCount()
		{
			if (_HealthBarCount == -1 && int.TryParse(HealthBarCount, out _HealthBarCount))
			{
			}
			return _HealthBarCount;
		}
		//名字颜色
		public string NameColor;
		private int _NameColor = -1;
		public int GetNameColor()
		{
			if (_NameColor == -1 && int.TryParse(NameColor, out _NameColor))
			{
			}
			return _NameColor;
		}
		//日常本头像
		public string HeadIcon;

	}
}
