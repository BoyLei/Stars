using AssetChecker.Define;
using AssetChecker.ShaderAssetCheck;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace AssetChecker
{
    public class AssetBaseCheck
    {
        private static readonly Type ShaderUtilType = typeof(ShaderUtil);

        protected static string AssetPath
        {
            get { return Application.dataPath; }
        }

        public static string GetSelectedPathOrFallback()
        {
            string path = "Assets";
            foreach (UnityEngine.Object obj in Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets))
            {
                path = AssetDatabase.GetAssetPath(obj);
                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                {
                    path = Path.GetDirectoryName(path);
                    break;
                }
            }
            return path;
        }

        public static void WriteReportFile(string template, string content, string title, string webroot, string filename, string addtitle = "")
        {
            title = title + "    <b><font color='red'>[" + DateTime.Now.ToString("F") + "]</font></b>" + addtitle;
            var txtContent = template.Replace("[TEXT_CONTENT]", content);
            txtContent = txtContent.Replace("[TITLE]", title);
            File.WriteAllText(webroot + filename + ".html", txtContent, Encoding.UTF8);
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public static FileVerInfo GetFileVerInfo(string filePath)
        {
            var fileInfo = new FileVerInfo();
            try
            {
                ParseSVNLogData(filePath, fileInfo);
            }
            catch (Exception ex)
            {
                //UnityEngine.Debug.LogError("get file version info exception!!:>" + filePath + " ex:" + ex.Message);
            }
            return fileInfo;
        }

        static void ParseGitLogData(string filePath, FileVerInfo fileInfo)
        {
            var appData = Application.dataPath;
            var fullPath = appData.Replace("Assets", filePath);
            var dirName = Path.GetDirectoryName(fullPath).Replace('\\', '/');

            filePath = Path.GetFileName(filePath);
            var list = PullLogData("cmd.exe", "/c cd " + dirName + " & git log -1 " + filePath, dirName);
            if (list != null)
            {
                foreach (var item in list)
                {
                    if (item.StartsWith("Author:"))
                    {
                        fileInfo._author = item.Remove(0, "Author:".Length);
                    }
                    else if (item.StartsWith("commit"))
                    {
                        fileInfo._orderid = item.Remove(0, "commit".Length);
                    }
                }
            }
        }

        static void ParseSVNLogData(string filePath, FileVerInfo fileInfo)
        {
            var list = PullLogData("svn.exe", "--username=" + ShaderAssetChecker.globalSettings.SVNUser + 
                " --password=" + ShaderAssetChecker.globalSettings.SVNPass + " log -l 1 \"" + filePath + "\"");
            if (list != null && list.Count == 2)
            {
                var strs1 = list[0].Split('|');
                var strs2 = list[1].Split(' ');

                fileInfo._author = strs1[1];
                fileInfo._orderid = strs2[0];
            }
        }

        public static List<string> PullLogData(string proc, string args = "", string workDir = "")
        {
            List<string> strList = null;
            var info = GetCommandLine(proc, args);
            if (!string.IsNullOrEmpty(workDir))
            {
                info.WorkingDirectory = workDir;
            }
            using (var process = Process.Start(info))
            {
                process.WaitForExit();

                strList = new List<string>();
                string msg = process.StandardOutput.ReadToEnd();
                if (!string.IsNullOrEmpty(msg))
                {
                    var strs = msg.Split('\n');
                    foreach (var item in strs)
                    {
                        if (string.IsNullOrEmpty(item.Trim()) || item.StartsWith("----"))
                        {
                            continue;
                        }
                        strList.Add(item);
                    }
                }
                process.Close();
            }
            return strList;
        }

        public static ProcessStartInfo GetCommandLine(string proc, string args)
        {
            var info = new ProcessStartInfo();
            info.FileName = proc;
            info.Arguments = args;
            info.WindowStyle = ProcessWindowStyle.Hidden;
            info.UseShellExecute = false;
            info.CreateNoWindow = true;
            info.RedirectStandardOutput = true;
            info.RedirectStandardError = true;
            return info;
        }

        public static string GetPackageAssetFullPath(string compath)
        {
            var packagePath = Path.GetFullPath(Path.Combine("Packages", "com.yoka.assetchecker", compath));
            if (File.Exists(packagePath))
            {
                return packagePath;
            }
            packagePath = Path.GetFullPath("Assets");
            return Path.Combine(packagePath, compath);
        }

        public static (string, string) PreprocAssets()
        {
            var webroot = Application.dataPath + "/../report/";
            var reportPath = GetPackageAssetFullPath("Editor/3rd/report.html");

            if (!File.Exists(reportPath))
            {
                throw new Exception(reportPath + " not exist!!");
            }
            var template = File.ReadAllText(reportPath);
            if (!Directory.Exists(webroot))
            {
                Directory.CreateDirectory(webroot);
            }
            return (webroot, template);
        }

        public static void SaveOverview(string webroot, bool isOpenUrl = false)
        {
            var srcPath = GetPackageAssetFullPath("Editor/3rd/overview.html");
            var destPath = webroot + "overview.html";

            if (!File.Exists(srcPath))
            {
                throw new Exception(srcPath + " not exist!!");
            }

            File.Copy(srcPath, destPath, true);
            if (isOpenUrl)
            {
                Application.OpenURL(destPath);
            }
        }

        protected static bool isNumberic(string message, out float result)
        {
            result = -1;
            try
            {
                result = Convert.ToSingle(message);
                return true;
            }
            catch
            {
                return false;
            }
        }

        protected static void CloseVSEditor()
        {
            var process = Process.GetProcesses();
            foreach (var item in process)
            {
                if ("devenv" == item.ProcessName)
                {
                    item.Kill();
                }
            }
        }

        protected static void OpenCompiledShader(Shader s, int mode, int customPlatformsMask, bool includeAllVariants, bool preprocessOnly, bool stripLineDirectives)
        {
            ShaderUtilType.InvokeMember("OpenCompiledShader",
                BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.InvokeMethod, null, null,
                new object[] { s, mode, customPlatformsMask, includeAllVariants, preprocessOnly, stripLineDirectives });
        }

        public static void ExportExcel(string path, List<ExcelTitle> titles, List<List<string>> values)
        {
            //ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            var file = new FileInfo(path);
            if (file.Exists) file.Delete();

            using (var package = new ExcelPackage(file))
            {
                var worksheet = package.Workbook.Worksheets.Add("sheet1");

                var asciiBase = 65; //A
                for (int i = 0; i < titles.Count; i++)
                {
                    char c = (char)(asciiBase + i);
                    var cellName = c + "1";

                    worksheet.Column(i + 1).Width = titles[i]._width;
                    worksheet.Cells[cellName].Value = titles[i]._title;

                    //worksheet.Cells[cellName].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    //worksheet.Cells[cellName].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(1, 2, 3));
                }

                for (int i = 0; i < values.Count; i++)
                {
                    for (int j = 0; j < values[i].Count; j++)
                    {
                        char c = (char)(asciiBase + j);
                        var cellName = c + "" + (i + 2);
                        worksheet.Cells[cellName].Value = values[i][j];
                    }
                }
                package.Save();
            }
        }

        protected static string GetRandomName(string prefix)
        {
            return prefix + DateTime.Now.ToString("yyyyMMddHHmmss");
        }

        public static DateTime GetAssetTime(string path)
        {
            return new FileInfo(path).LastWriteTimeUtc;
        }

        public static uint GetTimeStamp(DateTime time)
        {
            var ts = time.Ticks - new DateTime(1970, 1, 1, 0, 0, 0).Ticks;
            return (uint)(new TimeSpan(ts).TotalSeconds);
        }

        public uint GetLastWriteTime(string path)
        {
            var dateTime = GetAssetTime(path);
            return GetTimeStamp(dateTime);
        }

        public string GetReportTitle(List<TableTitle> titles)
        {
            var title = "<tr bgcolor='#F0FF0F'>{0}</tr>";
            var bodyStr = string.Empty;
            if (titles != null)
            {
                foreach (var item in titles)
                {
                    var align = string.Empty;
                    if (!string.IsNullOrEmpty(item._align))
                    {
                        align = " align=" + item._align;
                    }
                    bodyStr += $"<td{align}>[ {item._title} ]</td>";
                }
                title = string.Format(title, bodyStr);
            }
            return title;
        }

        public string CreateMutiLineLink(List<string> lines, string filePath, string hrefUrl)
        {
            if (lines.Count > 0)
            {
                var urlName = lines[0];
                if (urlName.Contains("//"))
                {
                    var strs = urlName.Split(new string[] { "//" }, StringSplitOptions.None);
                    urlName = strs[0];
                }
                if (lines.Count == 1)
                {
                    return urlName;
                }
                else
                {
                    var content = string.Format("<font size='1'>{0}</font>", string.Join("<br>\n", lines));
                    File.WriteAllText(filePath, content);

                    string strMsg = "<a href='{0}' target='_blank'>{1}</a>";
                    if (urlName.Contains("<font color='red'>"))
                    {
                        urlName = urlName.Replace("<font color='red'>", "").Replace("</font>", "");
                    }
                    if (urlName.Contains(":"))
                    {
                        var strs = urlName.Split(':');
                        urlName = strs[1];
                    }
                    return string.Format(strMsg, hrefUrl, urlName);
                }
            }
            return string.Empty;
        }

        public string ClearFocusText(string text)
        {
            return text.Replace("<font color='red'>", string.Empty).Replace("</font>", string.Empty);
        }

        public string FormatFocusText(string text)
        {
            return "<font color='red'>" + text + "</font>";
        }

        public string FormatFuncName(string text, string funcName)
        {
            return text.Replace(funcName + "(", string.Format("<font color='red'>{0}</font>(", funcName));
        }

        public string GetRowFormat(int count)
        {
            var tdStr = string.Empty;
            for (int i = 1; i <= count; i++)
            {
                tdStr += "<td>{" + (i - 1) + "}</td>";
            }
            return string.Format("<tr>{0}</tr>", tdStr);
        }

        public string GetExcelLink(string filename)
        {
            return $" <a href='{filename}.xlsx'>下载Excel</a>";
        }

        public int GetStorageMemorySize(Texture texture)
        {
            return (int)InvokeInternalAPI("UnityEditor.TextureUtil", "GetStorageMemorySize", texture);
        }

        private static object InvokeInternalAPI(string type, string method, params object[] parameters)
        {
            var assembly = typeof(AssetDatabase).Assembly;
            var custom = assembly.GetType(type);
            var methodInfo = custom.GetMethod(method, BindingFlags.Public | BindingFlags.Static);
            return methodInfo != null ? methodInfo.Invoke(null, parameters) : 0;
        }
    }
}