//来源表通用效果表_CommonEffect.xlsm.xlsx -> sheet:CMeffect
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class CMeffectData
	{
		[Key(0)]
		public Dictionary<int, CMeffectDataCell> StaticCMeffectDatas = new Dictionary<int, CMeffectDataCell>();
	}
	[MessagePackObject]
	public class CMeffectDataCell
	{
		//效果id
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//下一个效果ID
		[Key(1)]
		public List<int> NextID = new List<int>();
		//服务显示文本多语言Key
		[Key(2)]
		public string Name
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_Name); }
			set { _Name = value; }
        }
		[IgnoreMember]
		private string _Name;
		//效果是否等结束
		[Key(3)]
		public bool WaitEnd;
		public bool GetWaitEnd()
		{
			return WaitEnd;
		}
		//超时时间（毫秒）
		[Key(4)]
		public int Outtime;
		public int GetOuttime()
		{
			return Outtime;
		}
		//效果类型
		[Key(5)]
		public int Effecttype;
		public int GetEffecttype()
		{
			return Effecttype;
		}
		//参数1
		[Key(6)]
		public string Args1;
		//参数2
		[Key(7)]
		public string Args2;
		//参数3
		[Key(8)]
		public string Args3;
		//参数4
		[Key(9)]
		public string Args4;
		//参数5
		[Key(10)]
		public string Args5;
		//参数6
		[Key(11)]
		public string Args6;
		//参数7
		[Key(12)]
		public string Args7;
		//参数8
		[Key(13)]
		public string Args8;
		//参数9
		[Key(14)]
		public string Args9;
		//参数10
		[Key(15)]
		public string Args10;
	}
}
