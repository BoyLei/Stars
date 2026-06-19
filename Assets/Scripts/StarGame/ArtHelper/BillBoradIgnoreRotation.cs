using UnityEngine;
[ExecuteInEditMode]
public class BillBoradIgnoreRotation : MonoBehaviour
{
    private Transform thisTransform;

    private Quaternion initialRotate;
    // Start is called before the first frame update
    void Start()
    {
        // Transform oldParent;
        // oldParent = thisTransform.parent;
        // thisTransform.parent = null;
        // initialRotate = new Quaternion();
        // initialRotate.eulerAngles = new Vector3(90,0,0);
        // thisTransform.parent = oldParent;
        initialRotate = new Quaternion();
    }
    
    
    // Update is called once per frame
    void Update()
    {
        thisTransform = this.transform;
        Transform oldParent;
        oldParent = thisTransform.parent;
        thisTransform.parent = null;
        initialRotate.eulerAngles = new Vector3(90,0,0);
        thisTransform.rotation = initialRotate;
        thisTransform.parent = oldParent;
    }
}
