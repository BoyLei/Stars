///--------------------------------------------------------------------
/// 文件名   :   ConverConfig
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/08/09 17:23:40
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------

using UnityEditor;

public class ConverConfig : Editor
{
    public static string WrapDataPath = @"\Scripts\StarGame\Service\LocalDataManager\WrapData\";
    public static string JsonDataPath = @"\StreamingAssets\Json\";
    public static string SvnupBatch = "转表准备.bat";
    public static string mainexe = "main.exe";

    [MenuItem("自动化工具/一键转策划表")]
    public static void Convert()
    {
        string svnupBatchPath = UnityEngine.Application.dataPath + "/../../Tables/" + SvnupBatch;
        MapEditor.MapEditorUtils.RunBat(svnupBatchPath, GetRunArgs());
    }

    public static string GetRunArgs()
    {
        string savePath = UnityEngine.Application.dataPath + WrapDataPath;
        string jsonPath = UnityEngine.Application.dataPath + JsonDataPath;
        return string.Format("{0} {1} {2}", savePath, jsonPath, jsonPath);
    }
}