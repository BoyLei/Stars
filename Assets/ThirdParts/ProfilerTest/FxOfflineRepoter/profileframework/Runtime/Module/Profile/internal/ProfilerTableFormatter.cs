// -----------------------------------------------------------------------
// This file is part of  
//
// (c) ninglei   (2023/2/21 16:53:31)
// 
// For the full copyright and license information, please view the LICENSE
// file that was distributed with this source code.
// -----------------------------------------------------------------------
#if EFFECT_PROFILER
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yoka.Galaxy.Configure;

namespace Yoka.Galaxy.Profile
{
    /// <summary>
    /// д������ʽ���ļ�
    /// </summary>
    public class ProfilerTableFormatter : IProfilerOutputFormatter
    {
        private string _currentTag;

        public void Begin(IProfilerOutputData outputData, IEnumerable<IProfilerSampler> samplers)
        {
            SaveExcelData.InsertData(0, "Tab");
            SaveExcelData.InsertData(0, "Frame");
            foreach (var samplerItem in samplers)
            {
                SaveExcelData.InsertData(0, samplerItem.Lable());
            }

            outputData.Write("Tab\tFrame");
            foreach (var samplerItem in samplers)
            {
                outputData.Write($"\t{samplerItem.Lable()}");
            }
            outputData.Write("\n");
        }

        public void End(IProfilerOutputData outputData)
        {
            outputData.Write("\n\n");
            outputData.Write($"\nSystemInfo.operatingSystem\t{SystemInfo.operatingSystem}");
            outputData.Write($"\nSystemInfo.deviceName\t{SystemInfo.deviceName}");
            outputData.Write($"\nSystemInfo.deviceModel\t{SystemInfo.deviceModel}");
            outputData.Write($"\nSystemInfo.processorType\t{SystemInfo.processorType}");
            outputData.Write($"\nSystemInfo.processorFrequency\t{SystemInfo.processorFrequency}");
            outputData.Write($"\nSystemInfo.processorCount\t{SystemInfo.processorCount}");
            outputData.Write($"\nSystemInfo.systemMemorySize\t{SystemInfo.systemMemorySize}");
            outputData.Write($"\nSystemInfo.graphicsDeviceName\t{SystemInfo.graphicsDeviceName}");
            outputData.Write($"\nSystemInfo.graphicsMemorySize\t{SystemInfo.graphicsMemorySize}");
            outputData.Write($"\nSystemInfo.graphicsDeviceType\t{SystemInfo.graphicsDeviceType}");
            outputData.Write($"\nSystemInfo.graphicsDeviceVersion\t{SystemInfo.graphicsDeviceVersion}");
            outputData.Write($"\nSystemInfo.graphicsShaderLevel\t{SystemInfo.graphicsShaderLevel}");
            outputData.Write($"\nSystemInfo.renderingThreadingMode\t{SystemInfo.renderingThreadingMode}");
        }

        public string GetCharacter(FormatterCharacter character, string inputString)
        {
            switch (character)
            {
                case FormatterCharacter.Child:
                    return "/";
                case FormatterCharacter.Sibling:
                    return "|";
                case FormatterCharacter.Count:
                    return $"[{inputString}]";
            }
            return string.Empty;
        }

        public void Tag(string tag)
        {
            _currentTag = tag;
        }

        public void TakeSample(IProfilerOutputData outputData, IEnumerable<IProfilerSampler> samplers, ProfilerUseCase rootCase, int frame)
        {
            List<string> strs = new List<string>();
            

            if (rootCase != null)
            {
                strs.Add($"{rootCase.ReportStatus(this)}");
                strs.Add($"{frame}");
            }
            else
            {
                strs.Add($"{_currentTag}");
                strs.Add($"{frame}");
            }
            foreach (var samplerItem in samplers)
            {
                strs.Add($"{samplerItem.TakeSample()}");
            }
            SaveExcelData.InsertRowData(strs);


            if (rootCase != null)
            {
                outputData.Write($"{rootCase.ReportStatus(this)}\t{frame}");
            }
            else
            {
                outputData.Write($"{_currentTag}\t{frame}");
            }
            foreach (var samplerItem in samplers)
            {
                outputData.Write($"\t{samplerItem.TakeSample()}");
            }
            outputData.Write("\n");
        }
    }

}
#endif