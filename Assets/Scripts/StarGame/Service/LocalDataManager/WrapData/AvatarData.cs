//来源表Avatar.xlsx -> sheet:Avatar
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class AvatarData
	{
		[Key(0)]
		public Dictionary<int, AvatarDataCell> StaticAvatarDatas = new Dictionary<int, AvatarDataCell>();
	}
	[MessagePackObject]
	public class AvatarDataCell
	{
		//化身ID
		[Key(0)]
		public int AvatarID;
		public int GetAvatarID()
		{
			return AvatarID;
		}
		//策划备注
		[Key(1)]
		public string Desc;
		//模型ID
		[Key(2)]
		public int ModelId;
		public int GetModelId()
		{
			return ModelId;
		}
		//高模ID
		[Key(3)]
		public int HighModelId;
		public int GetHighModelId()
		{
			return HighModelId;
		}
		//模型基础动作
		[Key(4)]
		public int BaseAnims;
		public int GetBaseAnims()
		{
			return BaseAnims;
		}
		//待机展示判断间隔
		[Key(5)]
		public List<int> StandRandTime = new List<int>();
		//每次待机展示概率
		[Key(6)]
		public int StandProbability;
		public int GetStandProbability()
		{
			return StandProbability;
		}
		//判断次数
		[Key(7)]
		public int StandCount;
		public int GetStandCount()
		{
			return StandCount;
		}
		//头像ID
		[Key(8)]
		public int HeadID;
		public int GetHeadID()
		{
			return HeadID;
		}
		//转身速度
		[Key(9)]
		public int TurnAroundSpeed;
		public int GetTurnAroundSpeed()
		{
			return TurnAroundSpeed;
		}
		//模型放缩
		[Key(10)]
		public int CollierRadius;
		public int GetCollierRadius()
		{
			return CollierRadius;
		}
		//模型放缩
		[Key(11)]
		public int ModelScaling;
		public int GetModelScaling()
		{
			return ModelScaling;
		}
		//模型偏移
		[Key(12)]
		public List<int> ModelPosOffset = new List<int>();
		//模型旋转
		[Key(13)]
		public int ModelRotOffset;
		public int GetModelRotOffset()
		{
			return ModelRotOffset;
		}
		//动作路径
		[Key(14)]
		public string AnimsPath;
		//特效路径
		[Key(15)]
		public string EffectsPath;
		//音频包
		[Key(16)]
		public string SoundBank;
	}
}
