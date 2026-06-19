//来源表商业化_Commerce.xlsm.xlsx -> sheet:SignIn
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class SignInData
	{
		[Key(0)]
		public Dictionary<int, SignInDataCell> StaticSignInDatas = new Dictionary<int, SignInDataCell>();
	}
	[MessagePackObject]
	public class SignInDataCell
	{
		//ID
		[Key(0)]
		public int ID;
		public int GetID()
		{
			return ID;
		}
		//数据组
		[Key(1)]
		public int Group;
		public int GetGroup()
		{
			return Group;
		}
		//类型
		[Key(2)]
		public int SignInType;
		public int GetSignInType()
		{
			return SignInType;
		}
		//道具1
		[Key(3)]
		public List<long> Item = new List<long>();
		//数量
		[Key(4)]
		public List<long> Num = new List<long>();
		//标题多语言Key
		[Key(5)]
		public List<string> Title = new List<string>();
	}
}
