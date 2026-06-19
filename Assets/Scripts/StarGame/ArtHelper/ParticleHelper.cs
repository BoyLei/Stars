using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[System.Serializable]
public class ParticleItem
{
    public ParticleSystem particleSystem;

    [LabelText("特效发射速度,按当前速率的百分比")]
    [Range(0, 1)]
    public float Speed = 1;

#if UNITY_EDITOR
    private void OnValidate()
    {
        Refresh();
    }
#endif

    public void Refresh()
    {
        if (particleSystem == null)
        {
            return;
        }
        ParticleSystem.MainModule main = particleSystem.main;
        main.startSpeed = Speed;
        main.simulationSpeed = Speed;
    }

}
public class ParticleHelper : MonoBehaviour
{

    [SerializeField]
    public List<ParticleItem> ParticleItems;


    private void Awake()
    {
        ParticleItems.ForEach((item) =>
        {
            item.Refresh();
        });
    }

}
