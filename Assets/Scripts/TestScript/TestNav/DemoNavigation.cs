using UnityEngine;
using System.Collections;
public class DemoNavigation : MonoBehaviour
{
    public Transform target;
    void Start()
    {
        if (target != null)
        {
            this.gameObject.GetComponent<UnityEngine.AI.NavMeshAgent>().destination = target.position;
        }
    }
}
