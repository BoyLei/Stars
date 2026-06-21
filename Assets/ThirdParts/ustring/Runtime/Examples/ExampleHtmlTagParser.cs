using System;
using Yoka.UnityString.Core;

namespace Yoka.UnityString.Examples
{
    public class ExampleHtmlTagParser
    {
        private string source1 = @"天下<quad width=4/><pc=quality,5><c>aa<c><c><v>asdf</v></c></c></c><s><oc=red>玩家</oc></s><color=#6be48b><b><i>【@KTPO62J8】</i></b></color>在<u>世界中<size=44><quad asset=Default name=attr02 width=2 color=#ff0000 /></size>发现了</u><color=#d56df7>【深水<oc=green>废墟</oc>】</color>遗迹，请广大<s>猎魔人</s>前往一同探索！</pc><color=#d56df7><a href=[8_549756670003_549756670003_459561500673_8IF3rj99OI_@KTPO62J8_]?1017><rate=10>点击<oc=#ff00ff>五杀<quad attr03 />前往<c>目<pc=quality,5>的</pc>地</c>球<quad name=attr03 />中国</oc></rate></a></color>飞 行";

        private string source2 = @"天下<rate=10>点击<oc=#ff00ff>五杀<quad attr03 />前往<c>目<pc=quality,5>的<quad name=attr03 /></pc>地</c>球中国</oc></rate>宇宙的<oc=#ff00ff>五杀<quad attr03 />前往<c>目<oc>的</oc>地</c>梦<quad name=attr03 />中国</oc>飞行";

        private HtmlTagParser parser = new HtmlTagParser();

        public void Start()
        {
            parser.htmlTagHandler += OnParseHtmlTag;
            parser.htmlTagCustomHandler += OnParseCustomTag;
            parser.htmlTagSuffixHandler += OnParseHtmlTagSuffix;

			parser.AddOrUpdateCustomTagSpace("quad", 1);    //自定义占位标记
			parser.AddCustomTag("<quad>", "<quad />");      //自定义规则标记，没有任何规律的合法标记
			parser.AddCustomIncludeTag("oc", "quad");       //关联占位标记
			parser.AddCustomIncludeTag("pc", "quad");       //关联占位标记

            var result = parser.StartParseHtmlTag(source1);
            DebugPrint(result);
        }

        private void DebugPrint<T>(T argv)
        {
            UnityEngine.Debug.Log(argv);
        }

        private void DebugPrint<T1, T2>(T1 argv1, T2 argv2)
        {
            UnityEngine.Debug.Log(string.Format("{0},{1}", argv1, argv2));
        }

        private void DebugPrint<T1, T2, T3>(T1 argv1, T2 argv2, T3 argv3)
        {
            UnityEngine.Debug.Log(string.Format("{0},{1},{2}", argv1, argv2, argv3));
        }

        /// <summary>
        /// 解析潜规则标记
        /// <returns></returns>
        private void OnParseCustomTag(UString tagStr, ref HtmlTag tag)
        {
            if (tagStr.StartsWith("quad") && tagStr.Contains(" "))
            {
                var strs = tagStr.Split(' ');
                var value = strs.Count == 2 ? strs[1].ToString() : string.Empty;

                tag.tagName = strs[0].ToString();
                tag.attrs.Add("name", value);
            }
        }

        /// <summary>
        /// 解析HTMLTag对象，返回值是最终形式添加到解析队列中
        /// </summary>
        string OnParseHtmlTag(HtmlTag tag)
        {
            //switch (tag.tagName)
            //{
            //    case "quad": return OnTagQuad(tag);
            //    case "pc": return OnTagPc(tag);
            //    case "color": return OnTagColor(tag);
            //    case "oc": return OnTagOc(tag);
            //}
            //DebugPrint(tag.ToString());
            return tag.ToString();
        }

        /// <summary>
        /// 描边Tag
        /// </summary>
        private string OnTagOc(HtmlTag tag)
        {
            //DebugPrint(tag.tagName, tag.startIndex, tag.endIndex);
            return tag.ToString();
        }

        /// <summary>
        /// 颜色Tag
        /// </summary>
        string OnTagColor(HtmlTag tag)
        {
            //DebugPrint(tag.tagValue);
            return tag.ToString();
        }

        /// <summary>
        /// 预设颜色Tag
        /// </summary>
        string OnTagPc(HtmlTag tag)
        {
            UString tagValue = tag.tagValue;
            var strs = tagValue.Split(',', StringSplitOptions.RemoveEmptyEntries);
            var quality = strs[0].Value;
            var value = strs[1].Value;

            //DebugPrint(quality, value);
            return tag.ToString();
        }

        /// <summary>
        /// 图片Tag
        /// </summary>
        string OnTagQuad(HtmlTag tag)
        {
            //DebugPrint(tag.attrs.Count);
            return tag.ToString();
        }

        string OnParseHtmlTagSuffix(HtmlTag tag)
        {
            switch (tag)
            {
                //case "</pc>": return "</color>";
            }
            //Debug.Log(tag);
            return tag.ToString();
        }

        private void Update()
        {
            //parser?.StartParseHtmlTag(source);
        }

        private void OnDestroy()
        {
            if (parser != null)
            {
                parser.htmlTagHandler -= OnParseHtmlTag;
                parser.htmlTagCustomHandler -= OnParseCustomTag;
                parser.htmlTagSuffixHandler -= OnParseHtmlTagSuffix;
            }
        }
    }
}