using Coffee.UIExtensions;
using SGF.UI.Framework;
using StarProjectDef;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// UI��������ǻ���
/// UI��ʵ�ǿ��Ը���Ԫ������ÿһ�������Ծ���λ�ƺͱ�������
/// ������Ч��ʵ��3D������е����Ʋ�ͼ�ķ�ʽ
/// ������3D�����Ȼ���ǲ��У����Ƚϼ򵥵ķ�ʽҲ�ǿ��Դ���ģ����������
/// 1,������Ч��Ҫ����Ŀ���ڵ���00���Ҿ�����������Ԫ��
/// 2,��Ч���ڵ��scale ����Ծ���Զ��������Ҫ������ԡ���autoScale��
/// 3,Ȼ��3Dscale���ڲ�Ԫ�����ţ��Ǹ��ڵ�ͼ���С��fittle������ͼƬ��С��/ͼƬ��СƱ     /scale  ���ֵ����3DScale
/// ���ű�������UI��Ч����
/// ���ȱ���ͼ��2048��ô��Ч��Ӧ����2048�ģ�
/// </summary>
public class UIFXBGAdaptive : UIBehaviour
{
    UIParticle uip;

    public void Init()
    {
        //1������
        uip = transform.GetComponent<UIParticle>();
        uip.positionMode = UIParticle.PositionMode.Relative;
        uip.autoScaling = true;
        uip.onUpdateScale += onUpdateScale;
        transform.parent.GetComponent<AspectRatioFitterEX>().aspectMode = AspectRatioFitter.AspectMode.HeightControlsWidth;
        transform.parent.GetComponent<AspectRatioFitterEX>().aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
        transform.parent.GetComponent<AspectRatioFitterEX>().SetDirty();
    }


    protected override void OnEnable()
    {
        transform.parent.GetComponent<AspectRatioFitterEX>().aspectMode = AspectRatioFitter.AspectMode.HeightControlsWidth;
        transform.parent.GetComponent<AspectRatioFitterEX>().aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
        transform.parent.GetComponent<AspectRatioFitterEX>().SetDirty();


    }
    private void Adaptive()
    {


    }
    float size;
    float para;
    protected override void OnRectTransformDimensionsChange()
    {
        Texture tx = transform.parent.GetComponent<RawImage>().texture;
        RectTransform rtf = transform.parent.GetComponent<RectTransform>();
        if (rtf.rect.width > 0)
        {
            size = rtf.rect.width;
            Debug.Log(size + "�õ�Size!!!");

            //2ȷ���Լ���С��ͼƬ��Сһ��
            transform.GetComponent<RectTransform>().sizeDelta = new Vector2(tx.width, tx.height);
            //3�����
            para = rtf.rect.width / tx.width;//����Ǳ�������1��1�ģ����޹ؽ�Ҫ
            Debug.Log(rtf.rect.width + "_" + tx.width + "_" + transform.localScale.x);




        }



    }
    /// <summary>
    /// 4��֪ͨ:˳��ͬ
    /// </summary>
    /// <param name="obj"></param>
    private void onUpdateScale(Vector3 obj)
    {
        //OnRectTransformDimensionsChange �����ڶ��β�����õ���û�տ��ˣ���TODO
        if (para != 0)
        {
            SaveValue(obj);//��ֵ�ʹ�

        }
        else
        {
            if (GameConfig.UIFxScale != 0)
            {
                uip.scale = GameConfig.UIFxScale;
            }
            else if (para != 0)
            {
                SaveValue(obj);//����ֵ���
            }
            else
            {

            }

        }

    }

    private void SaveValue(Vector3 obj)
    {
        ///���������ǳ��Ϳ��һ�� Ҫ����
        if (obj.x > 1)
        {
            //���Է�������õ����ű���
            float fxScale = para / obj.x;
            uip.scale = /*Vector3.one **/ fxScale;
            Debug.Log(fxScale + "_");
            GameConfig.UIFxScale = fxScale;
        }
        else
        {
            //���Է�������õ����ű���
            float fxScale = obj.x / para;
            uip.scale = /*Vector3.one **/ fxScale;
            Debug.Log(fxScale + "_");
            GameConfig.UIFxScale = fxScale;
        }
    }

    [ContextMenu("ˢ��")]
    public void ForceExcute()
    {
        Adaptive();
    }

    private void Update()
    {

    }

    protected override void OnDestroy()
    {
        if (uip != null)
        {
            uip.onUpdateScale -= onUpdateScale;
        }
        
    }
}
