/// <summary>
/// 关于特效的一些 工具类方法, 避免在各个脚本中重复CV
/// </summary>
public static class FxUtils
{
    static System.Text.StringBuilder sb = new();

    /// <summary>
    /// 延迟 播放 站立动画 的 tag , 通过 GetDelayStandFlag 封装为 一个唯一的key
    /// </summary>
    private static string DELAY_STAND_FLAG = "delay_stand";

    public static string GetDelayStandFlag(object ob)
    {
        // 目前不确定 sb 的这种写法是否会 减少字符串的生成, 先这样试下
        return sb.Clear().Append(DELAY_STAND_FLAG).Append(ob.GetHashCode().ToString()).ToString();
    }


}
