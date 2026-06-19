using SGF.Module.Framework;
using SGF.Unity;
using System.Collections.Generic;
using UnityEngine;

namespace StarProject.Game
{
    [XLua.LuaCallCSharp]
    ///1,小提高直接到对应的4M位置，如果很高直接设置
    ///2，小下降下降到4m数组右位，如果很低直接到位置
    ///3，如果下降只有小于4*3=12才立刻设置位置
    public class RenderManager : ServiceModule<RenderManager>
    {
        private const float MaxBudgetMemory4Texture = 2047.9f;//511.9f;//MB   1536   2048
        private const float InitBudget = 127.9f;//MB: 我默认编辑器必须1024  1保证美术能编辑，2渲染正确，3运行时再节省
        //预算在这个空间内

        private const float BlockBudget = 4;//MB 满足预算机制，逐渐增加

        private readonly List<int> MemCtrl = new List<int>();

        private const float DownMemoryPara = -3f;//下降不必频繁，且留有空间，保守一点 两倍的说明真不用了才释放

        private const int MBFromB = 1024 * 1024;

        //锁定峰值
        private int LockCurMemMBCacheIndex;

        private readonly int currentMemoryMBl;
        private int desiredMemoryMBIndex, lastCache;
        private readonly int totalMemoryMB;

        private readonly int WantChangeMemory;

        private readonly int MinIndex = (int)(InitBudget / BlockBudget);
        private readonly int MaxIndex = (int)(MaxBudgetMemory4Texture / BlockBudget);
        private float currentIndex;


        public bool isGacha = false;

        //允许内存下降
        private bool allowMemoryDecline = true;
        public bool AllowMemoryDecline
        {
            get => allowMemoryDecline; set
            {
                if (value != allowMemoryDecline)
                {
                    if (value == false)
                    {
                        //首次变成false，外面只能关闭
                        LockCurMemMBCacheIndex = (int)(Texture.currentTextureMemory / MBFromB / BlockBudget);
                        allowMemoryDecline = value;
                    }
                    else
                    {
                        //首次变成True
                        //外面不允许true

                    }

                }
            }
        }

        public void Init()
        {
            CheckSingleton();
            MonoHelper.AddUpdateListener(OnUpdate, MonoHelper.E_ModuleType.Render);
            QualitySettings.streamingMipmapsMemoryBudget = InitBudget;
        }
        public override void Release()
        {
            MonoHelper.RemoveUpdateListener(OnUpdate, MonoHelper.E_ModuleType.Render);
            base.Release();
        }
        //小于【floor】层以上
        private bool HowMuchLessThenBefore(int floor = 5)
        {
            return (desiredMemoryMBIndex < lastCache - floor);
        }
        //大于【floor】层以上，因为是int，所以1起步
        private bool HowMuchGreaterThenBefore(int floor = 0)
        {
            return (desiredMemoryMBIndex - lastCache > floor);
        }

        /*GameConfig.SEC_RATE*/
        //内存预算要管够->动态管理上限->提高上限需要即时，下限不需即时->关闭Camera下降就会导致即时
        //-->开关界面内存变化太快的问题需解决-->开启界面锁定降低--->关闭界面时瞬间降低问题--->到达历史线才允许他降低并且自我控制--->历史线必须高于我才允许降低因默认历史线确认时是等于当时--->
        //***当前还是可能突破历史线，并非如预期Des需要一定开始相等随后下降，他可能出现：1相机关闭减少很少，ui加载提供很大，整体上升，2：相互的时机问题没关闭，就加载ui了
        //1，出现这一次的时候会重新流式
        //2，但问题会逐渐减少，因为加载的ui会持久化为缓存，并且提高水位线，越到后期越趋近于减少，趋近于设计
        //本质：提高到历史高度后有概率下降即可，峰值确实保持起来没问题，提高上升频率减少下降频率
        //如果再深入处理，锁定瞬时，需要内存可能和你当前相同，也可能比你少，也可能比你高---你无法确认Des是直接突破你历史水位，还是下降后再突破历史水位所以不用处理了  ： 常年开界面如果GC了依然要走流式所以没关系
        private void OnUpdate()
        {

            //Tips；QualitySettings.streamingMipmapsMemoryBudget > currentMemoryMB > desiredMemoryMB
            //Thd cache

            desiredMemoryMBIndex = (int)(Texture.desiredTextureMemory / MBFromB / BlockBudget);
            //totalMemoryMB = Texture.desiredTextureMemory / MBFromB;


            if (desiredMemoryMBIndex <= LockCurMemMBCacheIndex)
            {
                return;
            }
            else
            {

                allowMemoryDecline = true;
                LockCurMemMBCacheIndex = 0;
            }

            if ((HowMuchLessThenBefore() && AllowMemoryDecline)//比之前少 && 允许下降; 下面一定会调整下降
                ||
                HowMuchGreaterThenBefore())
            {
                currentIndex = Mathf.Clamp(desiredMemoryMBIndex, MinIndex, MaxIndex);


                int tmpBudget = 0;
                if (isGacha)
                {
                    tmpBudget = (Mathf.FloorToInt(currentIndex) + 1) * (int)BlockBudget + 150;
                }
                else
                {
                    tmpBudget = (Mathf.FloorToInt(currentIndex) + 1) * (int)BlockBudget;
                }
                QualitySettings.streamingMipmapsMemoryBudget = tmpBudget;
                lastCache = desiredMemoryMBIndex;
            }
        }

        private void OnEventMessage()
        {

        }

        private void OffEventMessage()
        {

        }

        /// <summary>
        /// 开启体积雾气
        /// </summary>
        /// <param name="obj"></param>
        public void OnSetFog(GameObject VolumeFogGo)
        {
            //打开场景的体积雾气

            int diviceLevel = 1;//1.高 2.中 3.低(通过插件提供)


            int fogLevel = 2;//0不开雾气，1开Lighting雾气，2开后效雾

            if (diviceLevel == 1)
            {
                fogLevel = 2;
            }
            else if (diviceLevel == 2)
            {
                fogLevel = 1;
            }
            else if (diviceLevel == 3)
            {
                fogLevel = 0;
            }

            if (fogLevel == 0)
            {
                RenderSettings.fog = false;
                if (VolumeFogGo != null)
                {
                    VolumeFogGo.SetActive(false);
                }
            }
            else if (fogLevel == 1)
            {
                //打开场景的Lighting雾气
                RenderSettings.fog = true;
                if (VolumeFogGo != null)
                {
                    VolumeFogGo.SetActive(false);
                }
            }
            else if (fogLevel == 2)
            {
                RenderSettings.fog = false;
                if (VolumeFogGo != null)
                {
                    VolumeFogGo.SetActive(true);
                }
            }

        }




    }
}
