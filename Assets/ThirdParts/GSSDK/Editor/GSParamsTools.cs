using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.IO.Compression;

namespace GSSDKEditor
{
    public class GSParamsTools
    {

        public static void CreateAndWritePropertiesFile(string paramsPath, Dictionary<string, string> keyValues)
        {
            using (StreamWriter writer = new StreamWriter(paramsPath))
            {
                foreach (var kvp in keyValues)
                {
                    writer.WriteLine($"{kvp.Key}={kvp.Value}");
                }
            }
        }

        public static Dictionary<string, string> ReadPropertiesFile(string filePath)
        {
            Dictionary<string, string> properties = new Dictionary<string, string>();

            if (File.Exists(filePath))
            {

                try
                {
                    // 读取文件内容
                    string[] lines = File.ReadAllLines(filePath);

                    // 解析每一行并添加到字典中
                    foreach (string line in lines)
                    {
                        // 使用Split方法将行拆分为键和值
                        string[] keyValue = line.Split('=');

                        // 如果行包含至少一个等号，并且键不为空，则添加到字典中
                        if (keyValue.Length >= 2 && !string.IsNullOrWhiteSpace(keyValue[0]))
                        {
                            // 去除键和值两边的空白字符
                            string key = keyValue[0].Trim();
                            string value = keyValue.Length > 1 ? keyValue[1].Trim() : string.Empty;

                            // 将键值对添加到字典中
                            properties.Add(key, value);
                        }
                    }
                }
                catch (Exception e)
                {
                    // 处理可能出现的异常，例如文件读取错误
                    Debug.LogError("Error reading properties file: " + e.Message);
                }
            }
            else
            {
                Debug.LogError("Properties file not found at: " + filePath);
            }

            return properties;

        }


        // public static void UpdateZipContent(string zipPath, string extractPath, string newFilePath, string updatedZipPath)
        // {
        //     // 确保解压路径存在
        //     Directory.CreateDirectory(extractPath);

        //     // 解压ZIP文件
        //     ZipFile.ExtractToDirectory(zipPath, extractPath);
        //     // 删除ZIP源文件
        //     File.Delete(zipPath);

        //     // 替换文件
        //     string targetFilePath = Path.Combine(extractPath, "assets",Path.GetFileName(newFilePath));
        //     File.Copy(newFilePath, targetFilePath, true); // 复制新文件到解压目录，覆盖旧文件

        //     // 清理解压目录，删除不必要的文件（可选）
        //     // 你可以根据需要选择保留哪些文件或文件夹
        //     // Directory.Delete(extractPath, true); // 注意：这会删除整个解压目录及其内容，慎用！

        //     // 重新压缩文件
        //     ZipFile.CreateFromDirectory(extractPath, updatedZipPath);

        //     // 删除解压目录（可选）
        //     Directory.Delete(extractPath, true);
        // }

    }
}
