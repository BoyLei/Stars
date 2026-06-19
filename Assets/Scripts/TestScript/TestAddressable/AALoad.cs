using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class AALoad : MonoBehaviour
{
    public Button btn;
    //public GameObject goPerfab;
    //public AssetReference refPerfab;

    private void Awake()
    {
        btn.onClick.AddListener(async ()=> {//本方法也要支持异步调用，多线程
            //GameObject.Instantiate(goPerfab, Random.insideUnitSphere, Quaternion.identity);
            //Addressables.Instantiate(refPerfab, Random.insideUnitSphere, Quaternion.identity);
            //Addressables.Instantiate("RemoteBox", Random.insideUnitSphere, Quaternion.identity);
            //是异步的
            //GameObject go = Addressables.Instantiate("RemoteBox", Random.insideUnitSphere, Quaternion.identity);

            // Addressables.Instantiate("RemoteBox", Random.insideUnitSphere, Quaternion.identity).Completed += AALoadComp;
            //方法1
            //Addressables.InstantiateAsync("RemoteBox", Random.insideUnitSphere, Quaternion.identity).Completed += AALoadComp;
            //方法2:
            GameObject gob =  await Addressables.InstantiateAsync("RemoteBox123", Random.insideUnitSphere, Quaternion.identity).Task;//本方法也要异步
            print(gob.name);
            //1，我觉得当时是不存在的
            //2，但是你可以提前持有引用
            //3，开始必然为空，当然有缓存的时候不会为空
            //4，在回调的时候打印不为空，所以需要项目启动的时候提前干这件事儿
        });
    }

    //强制转换但是回调太多
    //private void AALoadComp(AsyncOperationHandle<GameObject> obj)
    //{
    //    //你实例化了一个RemoteBox(Clone) (UnityEngine.GameObject),无论下载，还是缓存，每次创建都会报
    //    print($"你实例化了一个{obj.Result}");
    //}

    // Start is called before the first frame update


}
