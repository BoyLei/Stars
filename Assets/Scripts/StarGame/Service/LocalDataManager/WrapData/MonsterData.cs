//来源表Monster.xlsm.xlsx -> sheet:Monster
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class MonsterData
	{
		[Key(0)]
		public Dictionary<long, MonsterDataCell> StaticMonsterDatas = new Dictionary<long, MonsterDataCell>();
	}
	[MessagePackObject]
	public class MonsterDataCell
	{
		//序号
		[Key(0)]
		public long ID;
		public long GetID()
		{
			return ID;
		}
		//称谓
		[Key(1)]
		public string Appellation
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_Appellation); }
			set { _Appellation = value; }
        }
		[IgnoreMember]
		private string _Appellation;
		//名称
		[Key(2)]
		public string Name
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_Name); }
			set { _Name = value; }
        }
		[IgnoreMember]
		private string _Name;
		//属性模板
		[Key(3)]
		public int AttrBase;
		public int GetAttrBase()
		{
			return AttrBase;
		}
		//掉落模板
		[Key(4)]
		public int DropBase;
		public int GetDropBase()
		{
			return DropBase;
		}
		//阵营
		[Key(5)]
		public int FactionID;
		public int GetFactionID()
		{
			return FactionID;
		}
		//类型
		[Key(6)]
		public int MonType;
		public int GetMonType()
		{
			return MonType;
		}
		//名字颜色
		[Key(7)]
		public int NameColor;
		public int GetNameColor()
		{
			return NameColor;
		}
		//战斗起手技能
		[Key(8)]
		public int StartSkill;
		public int GetStartSkill()
		{
			return StartSkill;
		}
		//主动技能
		[Key(9)]
		public List<int> ActiveSkills = new List<int>();
		//主动技能等级
		[Key(10)]
		public List<int> ActiveSkillsLv = new List<int>();
		//被动技能
		[Key(11)]
		public List<int> PassiveSkills = new List<int>();
		//被动技能等级
		[Key(12)]
		public List<int> PassiveSkillsLv = new List<int>();
		//视野外是否休眠
		[Key(13)]
		public bool IsSleep;
		public bool GetIsSleep()
		{
			return IsSleep;
		}
		//AI类型
		[Key(14)]
		public int AIType;
		public int GetAIType()
		{
			return AIType;
		}
		//AI表ID
		[Key(15)]
		public int Aiindex;
		public int GetAiindex()
		{
			return Aiindex;
		}
		//外观
		[Key(16)]
		public int AvatarID;
		public int GetAvatarID()
		{
			return AvatarID;
		}
		//模型半径
		[Key(17)]
		public int ModelRadius;
		public int GetModelRadius()
		{
			return ModelRadius;
		}
		//地图显示图标
		[Key(18)]
		public int MapLogo;
		public int GetMapLogo()
		{
			return MapLogo;
		}
	}
}
