using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "RampGeneratorData", menuName = "Data/RampGeneratorData", order = 0)]
public class RampGeneratorData : ScriptableObject
{
    public string _GradientName = "Ramp";
    public int _GradientWidth = 128;//每一条渐变的宽度
    public int _GradientHeight = 4;//每一条渐变的高度
    public List<Gradient> _RampList = new List<Gradient>();
    public List<string> _RampNameList = new List<string>();
}