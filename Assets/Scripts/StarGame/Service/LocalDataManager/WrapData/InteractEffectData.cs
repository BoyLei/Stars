using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
namespace StarProjectDef
{
	public class InteractEffectData
	{
		public Dictionary<int, InteractEffectDataCell> StaticInteractEffectDatas = new Dictionary<int, InteractEffectDataCell>();
	}
	public class InteractEffectDataCell
	{		//效果ID
		public string ID;
		private int _ID = -1;
		public int GetID()
		{
			if (_ID == -1 && int.TryParse(ID, out _ID))
			{
			}
			return _ID;
		}
		//效果类型
		public string EffectType;
		private int _EffectType = -1;
		public int GetEffectType()
		{
			if (_EffectType == -1 && int.TryParse(EffectType, out _EffectType))
			{
			}
			return _EffectType;
		}
		//参数1
		public string Args1;
		//参数2
		public string Args2;
		//参数3
		public string Args3;
		//参数4
		public string Args4;
		//参数5
		public string Args5;
		//参数6
		public string Args6;
		//参数7
		public string Args7;
		//参数8
		public string Args8;
		//参数9
		public string Args9;
		//参数10
		public string Args10;

	}
}
