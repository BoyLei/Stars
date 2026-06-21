// -----------------------------------------------------------------------
// This file is part of  
//
// (c) ninglei   (2023/2/20 13:25:35)
// 
// For the full copyright and license information, please view the LICENSE
// file that was distributed with this source code.
// -----------------------------------------------------------------------
#if EFFECT_PROFILER
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Yoka.Galaxy.Configure;

namespace Yoka.Galaxy.Profile
{
    public class ProfilerStreamOutputData : IProfilerOutputData
    {
        private string _outputFilePath;
        private StreamWriter _streamWriter;
        private Object _writerLock = new Object();
        private string _currentTag = string.Empty;

        public void Begin(string path)
        {
            _outputFilePath = path;
            _streamWriter = null;

            if (!string.IsNullOrEmpty(_outputFilePath))
            {
                string dirPath = Path.GetDirectoryName(_outputFilePath);
                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }
                _streamWriter = new StreamWriter(_outputFilePath, false);   //非append模式
            }
        }

        public void End()
        {
            lock(_writerLock)
            {
                if (_streamWriter != null)
                {
                    _streamWriter.Flush();
                    _streamWriter.Close();
                }

                SaveExcelData.SaveDataToCSV();
            }
        }

        public void Write(string content)
        {
            lock (_writerLock)
            {
                _streamWriter.Write(content);
            }
        }
    }

}
#endif