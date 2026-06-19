using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Playables;

public class TimelineMenu : Editor
{

    [MenuItem("Assets/Timeline编辑器/ModifyTimeline")]
    public static void ModifyTimeline()
    {
        if (Selection.activeGameObject != null)
        {
            var PlayableDirector = Selection.activeGameObject.GetComponent<PlayableDirector>();
            if (PlayableDirector != null)
            {
                var window = EditorWindow.GetWindow<TimelineModifyWindow>("Timeline编辑器");
                window.TimelineObject = Selection.activeGameObject;
                window.Show();
            }
        }
    }
}
