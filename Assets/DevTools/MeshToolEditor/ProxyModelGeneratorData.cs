using UnityEngine;
/// <summary>
/// 命名无所谓
/// editor无法访问
/// 中间加一层static，能想到，也应该这么做，必须是中间数据
/// 他数据共享数据，也不属于调用者，也不属于编辑器
/// </summary>
public class ProxyModelGeneratorData : MonoBehaviour
{
    // 定义需要共享的数据
    public static Mesh simMesh;
}