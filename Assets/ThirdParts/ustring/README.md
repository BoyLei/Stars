# Yoka Zero Allocation String for Unity.
## Zero Allocation String 2021

### Yoka UString Lib

- UString
  - 基于微软最新.NET CORE的System.Memory、System.Buffer技术构建的0GC的高效轻量级字符串库，本库在游戏启动时，固定占用1.5M内存空间。
  - 目前基于对频繁路径拼凑，对于128字节长度的临时短字符串具有很高的性能。代码要在UString.Block的范围内，会自动收集开辟的对象。
 ```csharp
	void Update()
	{
		UString externalStr = null;
		using (UString.Block())
		{
			UString ustr1 = "222Assets/As/As/";
			var ustr2 = ustr1.Replace("As", "A中国") + str4;

			ustr2.ToUpper();
			ustr2 = ustr2.Append(100).Append(3.14f).AppendLine("200");
			var index = ustr2.IndexOf("A");
			var lastIndex = ustr2.LastIndexOf("A");

			var result0 = !(ustr2 == null);
			var result1 = ustr2.Contains("A");
			var result2 = ustr2.StartsWith("222a", StringComparison.OrdinalIgnoreCase);
			var result3 = ustr2.EndsWith(".PREFAB");

			var result4 = ustr1 != ustr2 && !ustr1.Equals(ustr2) && !ustr1.Equals(str3);
			var result5 = ustr1 == ustr1.Value && ustr1 != str1;

			//Debug.Assert(index > 0 && lastIndex > 0 && result0 && result1 && result2 && result3 && result4 && result5);

			text1.text = ustr2.PadLeft(5, '&').PadRight(5, '*').ToString();
		}

		using (UString.Block())
		{
			text2.text = UString.Concat(str1, str2, str1, str2, str1, str2).ToString();

			if (frameCount == 0)
			{
				externalStr = UString.Format("v1:{0},v2:{1},v3:{2},v4:{3}", frameCount++, str3, 300, true).Intern();
			}
			else
			{
				text3.text = UString.Format("v1:{0},v2:{1},v3:{2},v4:{3}", frameCount++, str3, 300, false).ToString();
			}
		}

		using (UString.Block())
		{
			UString ustr3 = str5;
			text4.text = (ustr3.Trim() + UString.Join('&', strs).Substring(1)).ToString();

			UString ustr4 = str7;
			var splitStrs = ustr4.Split(splitChars, StringSplitOptions.RemoveEmptyEntries);
			//text1.text = splitStrs[0].ToString();
			//text2.text = splitStrs[1].ToString();
			//text3.text = splitStrs[2].ToString();
			//text4.text = splitStrs[3].ToString();

			var strCount = GetCacheCapacity();
			text5.text = UString.Format("CacheCapacity:{0}", strCount).ToString();
		}
		externalStr?.Dispose();
	}

	string GetCacheCapacity()
	{
		UString strCount = string.Empty;
		for (int i = 1; i < StrDefine.InitCacheNum; i++)
		{
			var count = UString.GetCacheCapacity(i);
			if (count != StrDefine.InitSingleCacheNum)
			{
				strCount = strCount + UString.Concat(i, "[", count, "] ");
			}
		}
		return strCount.ToString();
	}
 ```
