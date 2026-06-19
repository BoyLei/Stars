using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StreamingMipMapLogger : MonoBehaviour
{
    public Text M_TextMipMapTest;
    public int masterTextureLimit;
    // Start is called before the first frame update
    void Start()
    {
        //QualitySettings.masterTextureLimit = masterTextureLimit;


    }

    // Update is called once per frame
    private void Update()
    {
        //DebugPanel
       /* M_TextMipMapTest.text =
            "所有纹理当前使用的内存量。:" + Texture.currentTextureMemory +
            "是否主动。:" + Texture.streamingTextureDiscardUnusedMips;
*/


    }
}
