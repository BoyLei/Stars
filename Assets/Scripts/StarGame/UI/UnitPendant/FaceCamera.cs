using StarProject.Game;
using StarProject.Game.Entity.RemoteDynamic;
using UnityEngine;

public class FaceCamera : MonoBehaviour
{

    void LateUpdate()
    {
        if (GameManager.Instance.M_GameCamera)
        {
            transform.forward = GameManager.Instance.M_GameCamera.transform.forward;
        }
        //transform.forward = Camera.main.transform.forward;
    }

    // copying transform.forward is relatively expensive and slows things down
    // for large amounts of entities, so we only want to do it while the mesh
    // is actually visible
    void Awake()
    {
        enabled = true;
    } 
    // disabled by default until visible
    //void OnBecameVisible()
    //{
    //    enabled = true;
    //}
    //void OnBecameInvisible()
    //{
    //    enabled = false;
    //}
}
