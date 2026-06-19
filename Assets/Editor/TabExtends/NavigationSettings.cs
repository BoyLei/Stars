using UnityEditor;
using UnityEditor.AI;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 客户端和服务器的可以不一致，客户端是玩家自己发起的寻路移动可以让你不贴边；客户端永远小于服务器模型默认，客户端独立是加分移动补贴便用的
/// 服务器就让他配置裁剪0----客户端生成的蓝色模型非常贴近阻挡也是接近0-----并且蓝色就是服务器可行走【碰撞就是服务器可行走区域】
/// 客户端要0.2的原因是客户端  碰撞必须等于阻挡=碰撞是0.4了===1防止模型渲染有缝隙，2不要让客户端自己发起的寻路的地方客户端自己碰撞又走不到，
/// 【现在不可以的原因是客户端自己拦截了：走路的时候不用？？寻路时候用？？？？老实用路店？》？】
/// </summary>
public class NavigationSettings : EditorWindow
{
    private SerializedObject navMeshSettingsObject;

    [MenuItem("自动化工具/快捷NavMesh设置")]
    private static void Init()
    {
        var window = GetWindow<NavigationSettings>();
        window.Show();
    }

    private void OnGUI()
    {
        GUILayout.Label("服务器要的就是尽量蓝色模型贴合阻挡，客户端要的是寻路", EditorStyles.boldLabel);

        if (GUILayout.Button("给服务器的设置"))
        {
            SetServerSetting();
        }
        if (GUILayout.Button("客户端自己的设置：【就0.2吧客户端容易不让你走】"))
        {
            SetClientSetting();
        }
        if (GUILayout.Button("寻路还是自己点吧！！！"))
        {
            SetClientSetting();
        }
    }
    void PrintSerializedProperties(SerializedProperty property, string indent = "")
    {
        SerializedProperty iterator = property.Copy();
        bool enterChildren = true;

        while (iterator.NextVisible(enterChildren))
        {
            Debug.Log(indent + "Property Path: " + iterator.propertyPath);
            Debug.Log(indent + "Property Type: " + iterator.propertyType);

            if (iterator.hasVisibleChildren)
            {
                PrintSerializedProperties(iterator.Copy(), indent + "  ");
            }

            enterChildren = false;
        }
    }
    private void SetServerSetting()
    {
        //agents保持0.2就好了
        //对应数值放进去结果是不对的，反射找不到，要打印，然后判断

        // 获取当前打开场景的导航设置对象
        navMeshSettingsObject = new SerializedObject(UnityEditor.AI.NavMeshBuilder.navMeshSettingsObject);

        // 修改导航面板的配置参数
        SerializedProperty agentRadiusProperty1 = navMeshSettingsObject.FindProperty("m_BuildSettings.agentRadius");
        agentRadiusProperty1.floatValue = 0.05f;



        
                SerializedProperty voxelSizeProperty2 = navMeshSettingsObject.FindProperty("m_BuildSettings.cellSize");
                voxelSizeProperty2.floatValue = agentRadiusProperty1.floatValue/3f;  // 启用手动设置 Voxel 大小

        SerializedProperty voxelSizeProperty3 = navMeshSettingsObject.FindProperty("m_BuildSettings.agentClimb");
        voxelSizeProperty3.floatValue = 0.6f;  // 启用手动设置 Voxel 大小
        



        SerializedProperty agentRadiusProperty4 = navMeshSettingsObject.FindProperty("m_BuildSettings.agentHeight");
        agentRadiusProperty4.floatValue = 2f;


        SerializedProperty voxelSizeProperty5 = navMeshSettingsObject.FindProperty("m_BuildSettings.manualCellSize");

        voxelSizeProperty5.intValue = 1;  // 启用手动设置 Voxel 大小

        // 应用修改
        navMeshSettingsObject.ApplyModifiedProperties();




/*
        navMeshSettingsObject = new SerializedObject(UnityEditor.AI.NavMeshBuilder.navMeshSettingsObject);
        SerializedProperty buildSettingsProperty = navMeshSettingsObject.FindProperty("m_BuildSettings");

        if (buildSettingsProperty != null)
        {
            PrintSerializedProperties(buildSettingsProperty);
        }
        else
        {
            Debug.Log("m_BuildSettings property not found");
        }*/

        //运行时配置和editor名字不同

        //NavMeshBuildSettings buildSettings = NavMesh.GetSettingsByID(0);



        /* // 修改配置参数
         buildSettings.overrideVoxelSize = true;

         // 应用修改后的配置参数
         UnityEditor.AI.NavMeshBuilder.UpdateNavMeshSettings(buildSettings);



         Debug.Log("Successfully set default server navigation parameters.");*/
    }

    private void SetClientSetting()
    {
        //agents保持0.2就好了
        //对应数值放进去结果是不对的，反射找不到，要打印，然后判断

        // 获取当前打开场景的导航设置对象
        navMeshSettingsObject = new SerializedObject(UnityEditor.AI.NavMeshBuilder.navMeshSettingsObject);

        // 修改导航面板的配置参数
        SerializedProperty agentRadiusProperty1 = navMeshSettingsObject.FindProperty("m_BuildSettings.agentRadius");
        agentRadiusProperty1.floatValue = 0.2f;




        SerializedProperty voxelSizeProperty2 = navMeshSettingsObject.FindProperty("m_BuildSettings.cellSize");
        voxelSizeProperty2.floatValue = agentRadiusProperty1.floatValue / 3f;  // 启用手动设置 Voxel 大小

        SerializedProperty voxelSizeProperty3 = navMeshSettingsObject.FindProperty("m_BuildSettings.agentClimb");
        voxelSizeProperty3.floatValue = 0.6f;  // 启用手动设置 Voxel 大小




        SerializedProperty agentRadiusProperty4 = navMeshSettingsObject.FindProperty("m_BuildSettings.agentHeight");
        agentRadiusProperty4.floatValue = 2f;


        SerializedProperty voxelSizeProperty5 = navMeshSettingsObject.FindProperty("m_BuildSettings.manualCellSize");

        voxelSizeProperty5.intValue = 1;  // 启用手动设置 Voxel 大小

        // 应用修改
        navMeshSettingsObject.ApplyModifiedProperties();



    }
}