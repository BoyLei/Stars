#if EFFECT_PROFILER
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Yoka.Galaxy.Configure
{
    public class SaveExcelData
    {
        public static Dictionary<int, List<string>> g_excelData = new Dictionary<int, List<string>>();
        public static void ClearData()
        {
            g_excelData.Clear();
        }

        public static void InsertData(int row, string str)
        {
            if (!g_excelData.ContainsKey(row))
            {
                g_excelData[row] = new List<string>();
            }
            g_excelData[row].Add(str);
        }

        public static void InsertRowData(List<string> strs)
        {
            g_excelData[g_excelData.Count] = strs;
        }


        public static void SaveDataToCSV()
        {
            string filePath = Application.dataPath + "/MyExcelFile.csv";
            StreamWriter writer = new StreamWriter(filePath);

            foreach (var pair in g_excelData)
            {
                string row = string.Join(",", pair.Value);
                writer.WriteLine(row);
            }

            writer.Close();
        }
    }
}
#endif