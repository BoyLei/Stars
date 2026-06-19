#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEditor;
using UnityEngine.AI;

namespace MapEditor
{

    public class MapEditorObstacleView : MapEditorBaseView
    {
        public MapEditorObstacleView(string viewName, Action<string> action) : base(viewName, action)
        {
        }

        public override bool Load(SceneJsonData jsonData, Transform Parent)
        {
            if (base.Load(jsonData, Parent))
            {
                MaxIndex = 0;
                if (jsonData!=null && jsonData.Obstacles != null && jsonData.Obstacles.Count > 0)
                {
                    foreach (var obstacle in jsonData.Obstacles)
                    {
                        CreateObstacleGroup(obstacle.Value);
                        if (obstacle.Value.Index > MaxIndex)
                        {
                            MaxIndex = obstacle.Value.Index;
                        }
                    }
                }
                /*ObstacleGroup[] ObstacleGroups = Root.GetComponentsInChildren<ObstacleGroup>();
                if (ObstacleGroups != null && ObstacleGroups.Length > 0)
                {
                    foreach (var item in ObstacleGroups)
                    {
                        if (item.ID > MaxIndex)
                        {
                            MaxIndex = item.ID;
                        }
                    }
                }*/


                return true;
            }

            return false;
        }
        public void CreateObstacleGroup(ObstacleGroupJsonData jsonData)
        {
            var go = MapEditorUtils.CreatePort(jsonData.Desc, Root);
            go.position =Vector3.zero;
            go.localRotation = Quaternion.identity;
            ObstacleGroup group = go.gameObject.AddComponent<ObstacleGroup>();
            group.Index = jsonData.Index;
            group.ID=jsonData.GroupID;
            group.IsOpen=jsonData.IsOpen;
            var list = jsonData.Obstacles;
            if (list != null && list.Count > 0)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    CreateObstacle(i,list[i],go);
                }
            }

            if (group.Effects == null)
            {
                group.Effects = new List<EffectTransform>();
            }
            group.Effects.Clear();
            if (jsonData.Effects != null && jsonData.Effects.Count > 0)
            {
                for (int i = 0; i < jsonData.Effects.Count; i++)
                {

                    var effect = CreateEffectTransform(i, jsonData.Effects[i], go);
                    group.Effects.Add(effect);
                }
            }
        }

        public EffectTransform CreateEffectTransform(int index, ObstacleGroupJsonData.ObstacleEffectJsonData jsonData,
            Transform parent)
        {
            var go = MapEditorUtils.CreatePort(index.ToString(), parent);
            go.position =jsonData.Position.Convert();
            go.localRotation = Quaternion.Euler(0,jsonData.Rotation,0);
            go.localScale = jsonData.Scale.Convert();

            EffectTransform effectTransform = new EffectTransform()
            {
EffectTransfom = go,
EffectPath = jsonData.Path
            };
            if (!string.IsNullOrEmpty(effectTransform.EffectPath))
            {
              var  obj= AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/Res/{effectTransform.EffectPath}.prefab");
              if (obj != null)
              {
                 var ins= GameObject.Instantiate(obj);
                 ins.transform.SetParent(go);
                 ins.transform.localPosition = Vector3.zero;
                 ins.transform.localRotation=quaternion.identity;
                 ins.transform.localScale = Vector3.one;
              }
            }
            return effectTransform;
        }
        public NavMeshObstacle CreateObstacle(int index,ObstacleGroupJsonData.ObstacleJsonData jsonData,Transform parent)
        {
            var go = MapEditorUtils.CreatePort(index.ToString(), parent);
            go.position =jsonData.Position.Convert();
            go.localRotation = Quaternion.Euler(0,jsonData.Rotation,0);
            go.localScale = jsonData.Size.Convert();
            NavMeshObstacle obstacle = go.gameObject.AddComponent<NavMeshObstacle>();
            obstacle.shape=(NavMeshObstacleShape)jsonData.Shape;
            obstacle.center = jsonData.Center.Convert();
            return obstacle;
        }

    public override void OnGUI()
        {
            if (GUILayout.Button("创建动态阻挡"))
            {
                CreateChild();
            }
        }

        public override void CreateChild()
        {
            Vector3 position;
            bool suc = MapEditorUtils.GetScreenPosition(out position);
            if (suc)
            {
                MapEditor.SpawnerEditorData editorData = new MapEditor.SpawnerEditorData(position, true);
                CreateChild(editorData);
            }
        }

        public override void CreateChild(object userData)
        {
            if (Root == null)
            {
                EditorUtility.DisplayDialog("提示", "编辑环境已经破坏，请重新打开", "确定");
                return;
            }

            MapEditor.SpawnerEditorData data = userData as MapEditor.SpawnerEditorData;
            GameObject go = new GameObject();
            go.layer = LayerMask.NameToLayer("Entity");
            go.transform.SetParent(Root.transform);
            go.transform.position = data.Position;
            ObstacleGroup obstacle = go.AddComponent<ObstacleGroup>();
            MaxIndex++;
            obstacle.ID = MaxIndex;
            go.name = "ObstacleGroup_" + obstacle.ID;
            obstacle.CreateObstacle();

            obstacle.Effects = new List<EffectTransform>();
            obstacle.Effects.Add(new EffectTransform()
                { EffectPath = "Effects/Scene/Cm/Fx_scene_kongqiqiang_common_01" });
            go.layer = LayerMask.NameToLayer("Entity");
            Selection.activeGameObject = go;
        }

        public override void ExportJson(SceneJsonData sceneJson)
        {
            sceneJson.Obstacles.Clear();
            ObstacleGroup[] obstacles = Root.GetComponentsInChildren<ObstacleGroup>();
            if (obstacles != null && obstacles.Length > 0)
            {
                int index = 1;
                foreach (var item in obstacles)
                {
                    sceneJson.Obstacles.Add(item.ID, new ObstacleGroupJsonData(index++, item));
                }
            }
        }
    }
}
#endif
