using DG.Tweening;
using StarProject.Game.Entity.View.VitalSign;
using StarProject.Service.Cam;
using StarProject.Service.Cam.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponTemp : MonoBehaviour
{
    public Tweener tr;
    //public AK.Wwise.Event ColliderSound = new AK.Wwise.Event();  //碰撞声音
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    public Dictionary<string, Color> S_Color = new Dictionary<string, Color>();
    private void OnTriggerEnter(Collider other)
    {
        return;//======================================
        //=============================================
        //=============================================


        //SGF.Debuger.LogError("Trigger" + other.name);
        //if (other.GetComponent<VVitalMonsterAnim>() != null)
        //{
        //    VVitalMonsterAnim vvm = other.GetComponent<VVitalMonsterAnim>();
        /*Tweener a = */
        //other.transform.DOShakeScale(0.2f, 0.1f).SetAutoKill(true).SetEase(Ease.InOutElastic).onComplete += () =>
        //{
        //     other.transform.SetLocalScale(Vector3.one);
        //    this.enabled = false;

        //};
        //    //other.transform.DOMove(Vector3.back, 0.3f).SetAutoKill(true);
        //}

        //应该用基础记录一下（anim初始化的时候）

        //other.GetComponent<VVitalMonsterAnim>().GetComponent<Rigidbody>().AddRelativeForce(Vector3.one * 20, ForceMode.Impulse);
        /*if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            ColliderSound.Post(gameObject);
        }
*/


        //if (other.transform.Find("ModelOffset/B_LangRen_01@Skin/LangRen_Body") == null)
        //{
        //    return;
        //}
        //Transform body = other.transform.Find("ModelOffset/B_LangRen_01@Skin/LangRen_Body");

        

        //SkinnedMeshRenderer meshRenderer = body.GetComponent<SkinnedMeshRenderer>();
        //if (meshRenderer == null)
        //{
        //    return;
        //}
        //Material[] ms = meshRenderer.materials;

        ////for (int i = 0; i < ms.Length; i++)
        //{
        //    Color c = Color.black;
        //    if (S_Color.ContainsKey(other.transform.parent.name))
        //    {
        //        c = S_Color[other.transform.parent.name];
        //    }
        //    else
        //    {
        //        c = ms[0].GetColor("_EmissionColor");
        //        S_Color.Add(other.transform.parent.name, c);
        //    }


        //    float intensity;
        //    float _x = 0;
        //    float factor;
        //    Color color = Color.red;
        //    factor = Mathf.Pow(2, 10);
        //    //开始还原
        //    ms[0].SetColor("_EmissionColor", S_Color[other.transform.parent.name]);


        //    DOTween.To(() => _x, x => _x = x, 5, 0.19f).OnUpdate(() =>
        //    {
        //        intensity = _x;
        //        factor = Mathf.Pow(2, intensity);
        //        color.r = c.r * factor * 5f;
        //        color.g = c.g * factor;
        //        color.b = c.b * factor;
        //        ms[0].SetColor("_EmissionColor", color);
        //    }).SetAutoKill(true).SetLoops(2, LoopType.Yoyo).SetEase(Ease.InOutQuint).onComplete += () =>
        //    {
        //        ms[0].SetColor("_EmissionColor", S_Color[other.transform.parent.name]); //结尾好还原
        //        //this.GetComponent<BoxCollider>().enabled = false;
        //    };

        //    //CameraManager.Instance.CurrentPlayCamera.transform.DOShakePosition(0.1f, 0.5f,1, 90, false, true).SetEase( Ease.InOutQuint);
        //}
        ////DoTweenFactory.DOColor(body.gameObject, Color.white, Color.red, 0.2f, Ease.InCubic, null, null);

    }






    private void OnCollisionEnter(Collision collision)
    {
        SGF.Debuger.LogError("CollisionCollisionCollisionCollisionCollisionCollisionCollisionCollisionCollision");
    }
}
