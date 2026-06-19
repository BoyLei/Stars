using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class WantedTask : MonoBehaviour
{
    [ReadOnly]
    [LabelText("唯一ID")]
    public int Index;
    
    [LabelText("通缉类型")]
    public int Type;
}
