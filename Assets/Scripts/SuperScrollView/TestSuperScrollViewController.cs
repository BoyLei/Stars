using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestSuperScrollViewController : MonoBehaviour
{
    public GameObject go_item;

    // Start is called before the first frame update
    void Start()
    {
        SuperScrollViewController SCView = transform.GetComponent<SuperScrollViewController>();
        SCView.NewScroll(go_item, 0, OnRefesh);

        SCView.ContinueScroll(50);
    }

    void OnRefesh(Transform tr,int id)
    {
        Debug.LogError(id);
    }

    // Update is called once per frame
   /* void Update()
    {
        
    }*/
}
