#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;


namespace MapEditor
{
    public class MapEditorMainPlayerView : MapEditorBaseView
    {
        public MapEditorMainPlayerView(string viewName, Action<string> action) : base(viewName, action)
        {
        }
        private MainPlayer mPlayer;
        public override bool Load(SceneJsonData jsonData,Transform Parent)
        {
            if (base.Load(jsonData,Parent))
            {
                
                mPlayer = Root.gameObject.AddComponent<MainPlayer>();
                if (mPlayer.TriggerGroups == null)
                {
                    mPlayer.TriggerGroups = new List<TriggerGroup>();
                }
                mPlayer.TriggerGroups.Clear();
             
                if (jsonData!=null && jsonData.MainPlayer!=null&& jsonData.MainPlayer.TriggerGroups!=null && jsonData.MainPlayer.TriggerGroups.Count>0)
                {
                    foreach (var item in jsonData.MainPlayer.TriggerGroups)
                    {
                        mPlayer.TriggerGroups.Add(CreateTriggerGroup(item));
                    }
                }
                
                return true;
            }

            return false;
        }
        public TrrigerBase GetTrrigerBase(int TriggerID)
        {
            TrrigerBase[] trrigerBases = Parent.GetComponentsInChildren<TrrigerBase>();
            if (trrigerBases != null && trrigerBases.Length > 0)
            {
                for (int i = 0; i < trrigerBases.Length; i++)
                {
                    if (trrigerBases[i].ID == TriggerID)
                    {
                        return trrigerBases[i];
                    }
                }
            }

            return null;
        }
        
        private TriggerGroup CreateTriggerGroup(TriggerGroupJsonData jsonData)
        {
            TriggerGroup  group = new TriggerGroup();
            group.TriggerID = jsonData.TriggerID;
            group.trriger = GetTrrigerBase(jsonData.TriggerID);
            if( group.TrrigerEffects==null)
            {
                group.TrrigerEffects = new List<TrrigerEffect>();
            }
            group.TrrigerEffects.Clear();
       
            if(jsonData.Effects!=null && jsonData.Effects.Count>0)
            {
 
                foreach (var item in jsonData.Effects)
                {
                    group.TrrigerEffects.Add(item.GetTrrigerEffect());
                }
            }
            return group;
        }

        public override void OnGUI()
        {
        }

        public override void CreateChild()
        {
        }

        public override void CreateChild(object userData)
        {
        }

        public override void ExportJson(SceneJsonData sceneJson)
        {
            sceneJson.MainPlayer.Save(mPlayer);
        }
    }
}
#endif
