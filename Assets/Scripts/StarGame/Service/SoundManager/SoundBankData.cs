using System.Collections.Generic;
using StarProjectDef;
using MessagePack;
namespace StarProjectDef
{
	[MessagePackObject]
	public class SoundBankData
	{
		[Key(0)]
		public Dictionary<string, List<string>> BankInfoDict = new();
		[Key(1)]
		public Dictionary<uint, List<string>> ID_BankInfoDict = new();
		[Key(2)]
		public HashSet<string> SettingBanksSet = new();
    }
}
