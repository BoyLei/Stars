//来源表SecretMonsterData
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
namespace StarProjectDef
{
	public class SecretMonsterData
	{
		public Dictionary<int, SecretMonsterDataCell> StaticSecretMonsterDatas = new Dictionary<int, SecretMonsterDataCell>();
	}
	public class SecretMonsterDataCell
	{
		//怪物ID
		public string MonID;
		private int _MonID = -1;
		public int GetMonID()
		{
			if (_MonID == -1 && int.TryParse(MonID, out _MonID))
			{
			}
			return _MonID;
		}
		//进度值
		public string Progress;
		private int _Progress = -1;
		public int GetProgress()
		{
			if (_Progress == -1 && int.TryParse(Progress, out _Progress))
			{
			}
			return _Progress;
		}

	}
}
