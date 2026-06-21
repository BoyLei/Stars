using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestAddressable : MonoBehaviour
{
    public Button btn1;
    public Button btn2;
    public Button btn3;
    public Image s1;
    // Start is called before the first frame update
    void Start()
    {//altas��Ҫ�ˣ�txtû�ã�scriptobjectҲ������
     //����[]��texture������sprite��altas����

       /* StarProject.Service.Resource.ResourceManager.Instance.Init();*/


        //��������ԣ��������� ��ע����
       // StarProject.Service.Resource.ResourceFormalManager.Instance.Init();

        btn1.onClick.AddListener(AA1);
        btn2.onClick.AddListener(AA2);
        btn3.onClick.AddListener(AA3);

    }


    private void AA1()
    {
        StarProject.Service.Resource.ResourceFormalManager.Instance.LoadResourceUniRefAsync<Sprite>("ui/common/textures/role/iconrole_l_91701",
             (Sprite img) =>
             {

                 s1.sprite = img;
             }, "IconRole_L_91701_Head");

    }

    private void AA2()
    {
        StarProject.Service.Resource.ResourceFormalManager.Instance.RemoveMiddleRef("ui/common/textures/role/iconrole_l_91701[IconRole_L_91701_Head]");
    }

    private void AA3()
    {
        /*  s1.sprite = null;
          s1 = null;*/
        s1.sprite = null;
        Resources.UnloadUnusedAssets();
        System.GC.Collect();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
