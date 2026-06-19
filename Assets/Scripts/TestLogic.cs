using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestLogic : MonoBehaviour
{
    // Start is called before the first frame update
    public string sceneName;
    void Start()
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName, UnityEngine.SceneManagement.LoadSceneMode.Additive);
           // UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        }
    }
    public GameObject go;
    public int num = 1;
    // Update is called once per frame
    int length = 0;
    void Update()
    {
        //int[] arr = new int[1024 * 1024];
        //GC.Collect();
        if (go == null)
            return;
        for(int i=0;i< num;i++)
        {
            length++;
            GameObject clone = GameObject.Instantiate(go,Vector3.zero, Quaternion.identity, this.transform);
            // clone.name = "0";
            GameObject.DestroyImmediate(clone);
        }
    }
}
