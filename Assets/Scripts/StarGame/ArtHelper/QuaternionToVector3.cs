using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class QuaternionToVector3 : MonoBehaviour
{
    [Header("将此XYZ填入SubstancePainter着色器设置的DirectionXYZ中")]
    public float DirectionX;
    public float DirectionY;
    public float DirectionZ;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Light light = this.transform.GetComponent<Light>();
        if (light != null)
        {
            //light.di
        }
        Quaternion q = this.transform.rotation;
        Vector3 v = this.transform.forward;
        DirectionX = -v.x;
        DirectionY = -v.y;
        DirectionZ = v.z;
    }
}
