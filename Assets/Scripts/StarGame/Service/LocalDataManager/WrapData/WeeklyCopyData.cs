//来源表周常挑战本_WeeklyCopy.xlsm.xlsx -> sheet:WeeklyCopy
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class WeeklyCopyData
	{
		[Key(0)]
		public Dictionary<int, WeeklyCopyDataCell> StaticWeeklyCopyDatas = new Dictionary<int, WeeklyCopyDataCell>();
	}
	[MessagePackObject]
	public class WeeklyCopyDataCell
	{
		//副本ID
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//副本名
		[Key(1)]
		public string Name
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_Name); }
			set { _Name = value; }
        }
		[IgnoreMember]
		private string _Name;
		//副本描述
		[Key(2)]
		public string Decs
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_Decs); }
			set { _Decs = value; }
        }
		[IgnoreMember]
		private string _Decs;
		//副本背景图资源
		[Key(3)]
		public string BG;
		//关卡入口背景图
		[Key(4)]
		public string EntranceBG;
		//BOSS模型ID
		[Key(5)]
		public long ModelID;
		public long GetModelID()
		{
			return ModelID;
		}
		//BOSS模型位置
		[Key(6)]
		public string ModelPos;
		//BOSS模型旋转
		[Key(7)]
		public string ModelRot;
		//BOSS模型缩放
		[Key(8)]
		public string ModelScale;
	}
}
