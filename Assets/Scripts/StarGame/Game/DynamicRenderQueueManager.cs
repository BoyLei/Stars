using SGF.Module.Framework;
using StarProject.Service.Cam;
using StarProjectDef;
using System.Collections.Generic;
using UnityEngine;
using Debug = UnityEngine.Debug;

///*删除也要告诉我,测试生效*/ /*80个*/ 关闭静态场景测试（ok因为变化很大） 更多的优化（一级）
/////策略根据人数超过20个以上（做到了）||||||知道是主角必须保留（做到了）||||||||||相机距离（没做到可以结合去掉描边贴图但是不重要是二级优化）||||||不要描边材质（做到了），和描边贴图（没做到，不重要是二级优化是处理留下来的）||||合批能合并但是面数翻倍（所以要做，也做到了，目前优化等级一级）
namespace StarProject.Game
{
    //静态设置好节省性能，但是量太大，可以editor；但是想思考维度是scene就够了；哪其实就是editor每个材质命名
    //这个也可以当成静态，只不过不全局，且不导入更新


    //list标准
    //list动态
    //dic动态标准 当前【动态或静态】
    //材质queue缓存 过程化

    //不同名字材质有可能核批么不可能：必须相同材质相同配置，所以名字就是唯一分类绝对无法合并的东西进行分组绝对利于核批   //打印设置的，查看结果数量，应该数值计算，赋予成功，材质数值还在
    //Release_首次+每次都会清空 +处理+ 赋予rqueue -> 规划材质rq:每次使用都有用Done
    //绑定Scene加载成功Done
    //不缓存材质Done
    //切换场景：空间可以多变少；ui消耗DC；还是比较少的379接近389，没有额外渲染片，同样没有因为分类问题导致峰值dc增加；切换没问题；时间不卡顿
    //Game下欺诈没问题，Scene欺诈没问题，欺诈一定统一材质，如果不同也不能核批，他的自动-1 反而不利于 不同材质组合组分离优化合并降低dc，所以欺诈asset不用还原-1规划
    //新加载的角色：TODO1
    //设置重新走了id0（设置走了只有一个），然后推断材质（3210），看材质是否一样（肯定一样看跟别人能否合并同一个有可能就合并不可能不打断别人（1组公用一定合并，【2多组公用也合并，3起码不打断别人）】，值正确不3210对，duandian初始化材质核批rq材质嘛，然后就可以性能了//核批测试不出来 ，怪物和人都算的，用怪物测试一样的，都是0说明只有1mat 他在边界+怪物+aOI；是否要走这个流程一定就跟初始化一样+dic就是池记录mat的+基础设置你不知道他历史出现过没+不消耗挺简单111
    //Release才开的标签TODO2Done

    //==TODOList
    //修改的话两处都要改，标准dic，如果新增shader的话--TODO
    //测试容纳脚本TODO2---现在还不能删除
    //带过来的角色都要处理TODO2---现在还不想做
    //UI材质没做，UI动静分离也没做TODO2---还没到时间
    //角色可能透明或者不透明：boss应该分离Shader，不然line这个不确定一定在后面就要设置两套:但是和统计没关系
    //



    public class DynamicRenderQueueManager : ServiceModule<DynamicRenderQueueManager>
    {
        /*  没有需要自增的，-1和固定数值是一样的，核批有效但是overDraw上来了给他分类是强制结果未必会好
         角色就不要在分rq打断合批了，角色同shader就能合并 ，除了rq不同不能合并外，不同材质球都能合并*/
        const bool NEED_AUTOADD = false;//是否需自增
        const bool NEED_CHECK_AUTOADD_ROOMSIZE = false;//自增情况看空间分配是否合理
        bool supportsEarlyZ;
        int SceneScore = 0;
        /*int NpcScore = 1;
        int MonsterScore = 1;
        int HeroScore = 1*/
        //记录材质就行了
        //一个角色4个part，4个描边
        //50个人最多 200~400
        //老大说20个描边就行 * 4个描边的
        public int MAT_OVER_SCORE = 80;//5;//增加/减少都可随时随意，减少需要等release到要求“水位”下后才会插入进来，不会立刻生效

        private Camera mainCamera;
        public Camera MainCamera
        {
            get
            {
                if (mainCamera == null)
                {
                    mainCamera = CameraManager.Instance.GetCamera(StarProjectDef.E_CameraType.StarWorldCam).Camera;
                }
                return mainCamera;
            }

        }

        public void Init()
        {
            SceneScore = 0;
            CheckSingleton();

            supportsEarlyZ = SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.Depth);
            Debug.Log("本设备是否支持supportsEarlyZ" + supportsEarlyZ);
            if (supportsEarlyZ)
            {
                MainCamera.depthTextureMode = DepthTextureMode.Depth;
            }

            InitMaterialRenderQueueSTAND();


            Other = MaterialRenderQueueSTAND["Other"];
            if (NEED_AUTOADD && NEED_CHECK_AUTOADD_ROOMSIZE)
            {
                /*foreach (var s in MaterialRenderQueueSTAND)
                {
                    if (s.Value != -2)
                    {
                        *//*LinkedListNode<int> linkedListNode = new LinkedListNode<int>(s.Value);
                        StandInitValues.AddLast(linkedListNode);*//*
                        StandInitValues.Add(s.Value);
                        InitValues.Add(s.Value);
                    }
                }*/
            }


            //1先异步加载场景，得不到过程。
            //2ui完成时被驱动 Scene加载结束。
            //【3加载配置都成功时】
            //不是Maproothelper Mono驱动
            GlobalEvent.OnMapConfigLoaded.AddListener(OnSceneLoaded);
     
           

        }

        private void InitMaterialRenderQueueSTAND()
        {
            MaterialRenderQueueSTAND = new Dictionary<string, int>{
            //基于一个场景出现的种类，切换Scene会换的。
            //物件 和植被区 互相遮盖，同一个自己合并，自己排布，没有描边，自己earlyZ

            { "SGAME/Item/Crystal",-1 },
            { "SGAME/SGAME_Scene_01",-2 },//200种
            { "SGAME/SGAME_Scene_01_Bak",-2 },//一般也不会出现这个
            { "GPUInstancer/SGAME/SGAME_Scene_01",-2 },//这个容易出现预留30个
            { "SGAME/Item/SGAME_Unlit",-2 },
            { "SGAME/URPSample",-1 },

            //剩余阴影区 管不了
            { "SGAME/SGAME_Scene_Shadow",-1 },
            { "SGAME/SimpleShadowShader",-1 },//推荐都用我这个，其实也就一个材质
            { "SGAME/SGAME_SkyBox_Boss",-1 },//推荐都用我这个，其实也就一个材质
            { "SGAME/DecalCloudShadow",-1 },//推荐都用我这个，其实也就一个材质
            { "GPUInstancer/SGAME/DecalCloudShadow",-1 },//推荐都用我这个，其实也就一个材质
         
            //节省区----EarlyZ-------queue越大越后渲染，越容易被遮挡的越后渲染-------------（-1和值一样）---------------------------------------------------
              //角色区域：可能有透明或者不透明
            { "SGAME/Roles/CartoonChar_4",-1 },//这里有深度，所以表现没问题，设置一样的话核批未必但是overdraw交给unity更好一点，所以不增加
            { "SGAME/Roles/CartoonChar_4_Line",-1 },
            //============================================================================================
            { "SGAME/Cartoon_NonoBall",/*2450 SkyBox*/2001 },
            { "Mirror_shader",/*2450 SkyBox*/2002 },

            { "Island_tree_leaf_01_Shadow",-1 },//不渲染，shadow不排序 

             /*
            建议不透明固定2500以下  半透明物体固定3000以上, 2500-3000 留给一些需要alpha clip的特殊shader用.
            */        
            { "SGAME/SGAME_Scene_leaf",2450 },
            { "SGAME/SGAME_Scene_leaf_Bak",2450 },
            { "GPUInstancer/SGAME/SGAME_Scene_leaf",2450 },//也就20种材质就够了
            


            //草地区，植被区 
            { "SGAME/Grass",2004 },//不自增自己排
            { "GPUInstancer/SGAME/Grass",2004 },

         
            //土地区 不透明2500
            { "SGame/Terrain/TerrainLit" ,2049},//2006
            { "SGame/TerrainLit_ADD" , 2049 },//2006
            { "SGame/Terrain/TerrainLit_TArray" ,2049},//2006
            

            //特殊地面区
            { "Universal Render Pipeline/Lit",2010 }, //混合没问题，接受水，并且最后渲染
              //=====================================================================================
     
            //被遮挡的放在后面
            //减少负优化分类是同一个值交给unity，反正物件没有描边
            //后渲染为了earlyZ,dc上去了overdraw下来了，           
            //3000之内是水采样不透明
            //节省区-----EarlyZ----------------------------------------------------------------------


            //水区域：大于3000融合3000之前的（2500作为标准就够了）
            { "SGAME/Scene/SimpleSea",3010 },
            { "SGAME_WaterFall_ASE",3015 },
            { "GPUInstancer/SGAME_WaterFall_ASE",3016 },//下一个还是3670就行

            {"VFX/Pandavfx_v2.3_URP",3020},//设置一样交给unity，几乎不能合批就，但也直接是gpu数据，起码要被雾气遮盖，之后这之后要有lut

            //后效果区域              
            {  "SGAME/DepthBasedHeightFog" , 3030},
            {  "SGame/SGameBloom" , /*3920*/-2 },
            {  "SGame/SGame_Fog" , 3031 },
        
            //lut校色

                  //插片区，预警圈
            { "SGame/SGAME_VFX_SkillControler",-2 },//warning 3100 地面之后；指示器是 3110 无论什么环境都应该看得见自己的技能指向

            // 其他的渲染队列值可以在这里继续添加

            //找到异常就4000开始，根据你材质转换成你shader，没有就默认2000
            { "Other" , -1 },

            //UI先不管
            {"Custom/ImageBlur",-2},
            {"Custom/Raw2DUrp",-2},
            {"Yoka/Stars/RectMask2D",-2},

            ///配置好得，配置不多的，不用管理，但是要列出来
            { "SGame_VFX_CharacterShadow",-2 },
            { "SGAME/VFX/SGAME_FresnelFadeChar",-2 },
            { "SGAME/FX/SGAME_VFX_CharFadeOut",-2 },
            { "SGAME_VFX_CloudShade",-2 },
            { "SGAME/FX/SGAME_VFX_Fire",-2 },
            { "SGAME/FX/SGAME_VFX_MagicFire_Plane",-2 },
            { "SGAME/FX/SGAME_VFX_RadialBlur",-2 },//不知道是什么就保持原样


            //UI文字 ：改管线后渲染奥。
        
    };
            //最浪费的方式前面是
            //EarlyZ会处理重复渲染，Ztext保证表现不会错误用于是否刷新像素点控制，Rq生效
            //HighZ+EarlyZ+Ztext
            if (supportsEarlyZ)
            {
                MaterialRenderQueueSTAND["Universal Render Pipeline/Lit"] = 2001;
                MaterialRenderQueueSTAND["SGame/Terrain/TerrainLit"] = 2049;
                MaterialRenderQueueSTAND["SGame/TerrainLit_ADD"] = 2049;
                MaterialRenderQueueSTAND["SGame/Terrain/TerrainLit_TArray"] = 2049;         
                MaterialRenderQueueSTAND["SGAME/Grass"] = 2005;
                MaterialRenderQueueSTAND["GPUInstancer/SGAME/Grass"] = 2005;
                MaterialRenderQueueSTAND["SGAME/SGAME_Scene_leaf"] = 2450;
                MaterialRenderQueueSTAND["SGAME/SGAME_Scene_leaf_Bak"] = 2450;
                MaterialRenderQueueSTAND["GPUInstancer/SGAME/SGAME_Scene_leaf"] = 2007;
                MaterialRenderQueueSTAND["Mirror_shader"] = 2007;
                MaterialRenderQueueSTAND["SGAME/Cartoon_NonoBall"] = 2009;
            }


        }

        public override void Release()
        {
            SceneScore = 0;
            GlobalEvent.OnMapConfigLoaded.RemoveListener(OnSceneLoaded);
            base.Release();
        }
        /// <summary>
        /// 为啥清理，因为每次材质不同，我希望量级是scene级别的
        /// 首先SRPBatch是通过Shader，因为Rq会导致不能合批的，材质球都无法影响合批，【所以动静都没有自增需求：按照shader合并力度更大，自增是排除影响，场景和个人通常不影响】，【所以描边可以分离】
        /// 那么-1 和 固定有没有区别，
        /// 2000,shader,材质优先级递增：1默认队列会表现不对，2shader会规划但是会漏，3和TA确定的材质相对准确：相对来说这个rq是组这个分组是更被开发人员确定的，和表现正确的，都是一组，至于固定组还是浮动组，越固定越谨慎
        /// </summary>
        public void ReInit()
        {
            if (NEED_AUTOADD)
            {
                InitMaterialRenderQueueSTAND();

                Other = MaterialRenderQueueSTAND["Other"];
            }

            if (NEED_CHECK_AUTOADD_ROOMSIZE && NEED_AUTOADD)
            {
                /*StandInitValues.Clear();
                InitValues.Clear();
                foreach (var s in MaterialRenderQueueSTAND)
                {
                    if (s.Value != -2)
                    {
                        StandInitValues.Add(s.Value);
                        InitValues.Add(s.Value);
                    }
                }*/
            }


            if (NEED_AUTOADD)
            {
                dynamicMetaAndQueueMaps.Clear();
            }

        }
        // 定义枚举，用于存储材质的 Shader 名称和渲染队列值
        //每次启动都是基础：新修改不要在编译的时候就初始化可以节省，外面也不会调用，1本类2sceneload或者创建角色3init最早甚至工程启动
        // /*    release时候才用*/
        /*虽然是标准但是也还是会变化的，定义阶段是标准，已经list存储了*/
        private Dictionary<string, int> MaterialRenderQueueSTAND;


        //辅助规划计算==============================规划合理可以去掉降低复杂度C#Test===
        /*    List<int> StandInitValues = new List<int>();
            List<int> InitValues = new List<int>();*/
        //辅助规划计算======================================================================
        /*要设置材质球都是这个值*/
        private Dictionary<string, int> dynamicMetaAndQueueMaps = new Dictionary<string, int>();

        //shader 对应当前标准  dic
        //名字   对应应该queue dic
        //静态直接就可以处理，不用缓存dic  


        //设置即可 新增加的需要处理
        //remove不用告诉我，因为我是场景处理的
        //表现层创建时候，基类所有render给我，广度一层一次就好
        //创建，切换
        //两个地方，创建和刷新，主角thd，伙伴npc，道具物件都有了|完美
        //策略根据人数超过20个以上||||||知道是主角必须保留；||||||||||相机距离||||||不要描边材质，和描边贴图||||合批能合并但是面数翻倍
        public int AddNewRender(Renderer[] renderers, E_OutlineEntityType dynamicItem)
        {
            //自己找好注意性能
            return DealDynamicMetrals(renderers, dynamicItem);
        }
        public void RemoveNewRender(int deCount)//记录再 ，view就是谁都有，我这边就是记录materials能少点，但本身就是谁都有
        {
            SceneScore -= deCount;


        }


        // 记录下一个可用的渲染队列值
        private int Other = 4000;
        private bool isFirst = true;
        private void OnSceneLoaded(string MapName)
        {//SHUOBUSHUI XI
            //只屏蔽静态不屏蔽动态这里是
#if !UNITY_EDITOR //所有平台的Editor，防止互相冲突，只有运行时的时候才会用到，也不用什么release_Tag了忘记还不生效了呢，实际上排除开发环境，一切都发布运行时都需要就好了不影响开发还自由提交meta我来处理
            if (isFirst)
            {
                isFirst = false;
            }
            else
            {
                ReInit();
            }
            SceneScore = 0;
            DealDynamicMetrals(GameObject.FindObjectsOfType<Renderer>(), E_OutlineEntityType.DynamicItem);//静态用不着也不给返回值
#endif//111  kaifang 222save 3revert 提交
        }

        private int DealDynamicMetrals(Renderer[] _renderers, E_OutlineEntityType dynamicItem)
        {
            int count = 0;
            Renderer[] renderers = _renderers;
            //MonoHelper.Instance.FindObjectsOfType<Renderer>();
            //这里会调用实例了GameObject.FindObjectsOfType<Renderer>();都一样 
            int passOutlineCount = 0; //一个人有几个outline数量
            //1一定是一个人

            //FindObjectsOfType<Renderer>();
            //2部件遍历多个部件
            foreach (Renderer renderer in renderers)
            {
                //3部件上多个材质遍历
                Material[] materials = renderer.sharedMaterials;
                List<Material> cacheDeleSignMateList = new List<Material>();
                foreach (Material material in materials)
                {
                    if (material != null)
                    {
                        //shader给参考，给shader标准值，递增shader标准
                        //材质球找到直接给材质球缓存历史阶段，没找到用递增过得shader标准
                        if (NEED_AUTOADD)
                        {
                            /*//数量正确很多个；数值设置成功；数值结果renderqueue也正确
                            if (!dynamicMetaAndQueueMaps.ContainsKey(material.name))
                            {
                                // 根据材质的 Shader 名称来获取渲染队列值
                                int currentRenderQueue = GetRenderQueueFromShaderName(material.shader.name);

                                //2（规划分成2.1具体和2.2不用管）包
                                //3所以一定要维护确定的
                                if (currentRenderQueue == -2  || material.name.Contains("Fx_UI_") || material.name.Contains("T_UI_"))
                                {
                                    //不需要管理按照他自己的材质球弄
                                    //内部控制外部continue就需要参了，参数是媒介
                                    continue;
                                }
                                else
                                {

                                    material.renderQueue = currentRenderQueue;
                                    material.enableInstancing = true;

                                    dynamicMetaAndQueueMaps.Add(material.name, currentRenderQueue);

                                    //给一个新增的值
                                    //bool a = material.renderQueue == currentRenderQueue;
                                    Debug.Log(material.name + "应该设置成：" + currentRenderQueue + "数量：" + count*//* + "结束后："+ material.renderQueue + "结果是否相同:" + a*//*); count++;
                                }
                                DealRenderQueueAdd(material.shader.name);



                            }
                            else
                            {
                                //给一个缓存的值
                                material.renderQueue = dynamicMetaAndQueueMaps[material.name];
                                material.enableInstancing = true;

                                // bool a = material.renderQueue == dynamicMetaAndQueueMaps[material.name];
                                Debug.Log(material.name + "应该设置成：" + dynamicMetaAndQueueMaps[material.name] + "数量：" + count*//* + "结束后：" + material.renderQueue + "结果是否相同:" + a*//*); count++;
                            }*/
                        }
                        else
                        {   //shader给参考，给shader标准值
                            //一直都是shader值部分历史，也部分过程
                            int currentRenderQueue = GetRenderQueueFromShaderName(material.shader.name);

                            if (currentRenderQueue == -2 || material.name.Contains("Fx_UI_") || material.name.Contains("T_UI_"))//材质球也就放在这里能排除了，这里是ui特效的材质，//公用shader但是他们不希望改变他们的rq其实无所谓3020； //他们怕太高了，会手动管理，可能会通过softRq管理，其实无所谓我注释掉材质也ok
                            {
                                //很有可能不控制rq但是还要统计描边的   UI材质球不设置数值了如果人也这么命名也不能不统计描边
                                //continue; //就不设置，但是统计描边，设置归设置统计归统计
                            }
                            else
                            {
                                //就设置，
                                material.renderQueue = currentRenderQueue;
                                material.enableInstancing = true;

                            }

                            //检测Scene01 shader的材质球是否是半透明, 是的话修改渲染队列.
                            if (material.shader.name == "SGAME/SGAME_Scene_01"||material.shader.name == "SGAME/SGAME_Scene_01_Bak"||material.shader.name == "GPUInstancer/SGAME/SGAME_Scene_01")
                            {
                                if (!CheckScene01ShaderAlpha(material))
                                {
                                    material.renderQueue = 3000;
                                }
                            }
                            //检测CartoonChar shader的材质球是否是半透明, 是的话修改渲染队列.
                            if (material.shader.name == "SGAME/Roles/CartoonChar_4")
                            {
                                if (!CheckCartoonCharacterShaderAlpha(material))
                                {
                                    material.renderQueue = 3000;
                                }
                            }
                            //Debug.Log(material.name + "现在的值是:" + material.renderQueue);
                            //如果真的需要自己增加
                            //1autoadd不走这里
                            //2不应该有得代码
                            //DealRenderQueueAdd(material.shader.name);
                            //3不删除，没有autoadd也不会增加
                            //可以确定了节省了一个队列，并且不增加，值也正确，也才能取得，就不需要了，同一个数据
                        }


                        //一次cpu减少很多gpu
                        //循环材质剔除无用
                        switch (dynamicItem)
                        {
                            case E_OutlineEntityType.NPC:
                            case E_OutlineEntityType.Monster:
                            case E_OutlineEntityType.Hero:
                                if (material.shader.name == "SGAME/Roles/CartoonChar_4_Line")
                                {
                                    if (SceneScore < MAT_OVER_SCORE)
                                    {
                                        //小于要求，性能没有压力
                                        //没排除，引入场景，开始计数
                                        SceneScore++;
                                        passOutlineCount++;//一个人一起增加，实际引入多少，你给删除的如果你记录了他没有占你份额但是删除却销毁你份额不对
                                    }
                                    else
                                    {
                                        //大于要求性能开始有压力
                                        //排除
                                        cacheDeleSignMateList.Add(material);
                                        //每一个render可能删除1个~2个；多个render都可能会排除后续；场景不换逻辑不换，不管合批顶点就有压力
                                    }
                                }
                                break;


                            case E_OutlineEntityType.Summon://子弹必须扣除
                                if (material.shader.name == "GJ/CartoonChar_4_Line")
                                {
                                    cacheDeleSignMateList.Add(material);//排
                                }
                                break;


                            case E_OutlineEntityType.DynamicItem://保留交互描边物件
                            case E_OutlineEntityType.StaticItem://压根不检查//不排
                            case E_OutlineEntityType.MainPlayer://主角儿必须有描边
                                break;
                            default:
                                break;
                        }
                    }

                }

                //材质循环后，决定材质剔除材质了，一个部件完事
                //一个部件一个缓存，多个部件多次，部件完事决定好一个人没问题

                //材质层剔除层次
                if (cacheDeleSignMateList.Count != 0)
                {
                    //删除描边批次
                    List<Material> materialsList = new List<Material>(materials);//转换list
                    for (int i = 0; i < cacheDeleSignMateList.Count; i++)
                    {

                        materialsList.Remove(cacheDeleSignMateList[i]);
                    }
                    renderer.materials = materialsList.ToArray();
                }
                //这一层次之后，下一层之前就结束（下一层是每个人了）



            }

            return passOutlineCount;
        }

        //检测使用Scene01 shader的材质球 是半透明还是不透明
        private bool CheckScene01ShaderAlpha(Material mat)
        {
            if (mat.GetFloat("_TotalAlpha") < 1f)
            {
                //半透明
                return false;
            }
            else
            {
                //不透明
                return true;
            }
            
        }
        
        //检测使用CartoonCharacter shader 的材质球 是半透明还是不透明
        private bool CheckCartoonCharacterShaderAlpha(Material mat)
        {
            if (mat.GetFloat("_IfVertexAlpha") == 1 || mat.GetFloat("_Alpha") < 1)
            {
                //半透明
                return false;
            }
            else
            {
                //不透明
                return true;
            }
            
        }
        //1非规划默认4k，2（规划分成2.1具体和2.2不用管）包
        private int GetRenderQueueFromShaderName(string shaderName)
        {
            // 根据 Shader 名称来获取渲染队列值
            int renderQueue = -1000;

            //值容易带来误导，其实经过筛选无非有和没有，一定可以确认一个具体的值
            //即规划和非规划，1非规划默认4k，2（规划分成2.1具体和2.2不用管）包，给别人处理
            if (MaterialRenderQueueSTAND.TryGetValue(shaderName, out renderQueue))
            {

            }
            else
            {
                //没规划shader
                renderQueue = Other;
                //Other++;//不着急这时候
            }

            return renderQueue;
        }
        private enum AddmsgType : byte//short
        {
            HaveReadySetting,//[-2]已经有设置了完全不用管
            NeedSetting,   //[-1,和其他值]
            NoControlSetting   //[other特殊值]不用管理别增加,材质都设置好了
        }


        //1非规划默认4k，2（规划分成2.1具体和2.2不用管）包
        private void DealRenderQueueAdd(string shaderName)
        {
            int renderQueue = -1000;
            AddmsgType needAdd = AddmsgType.HaveReadySetting;
            //规划外的Shader，不可能叫other，只能是对应设定标准值，其他全对Other
            if (MaterialRenderQueueSTAND.TryGetValue(shaderName, out renderQueue))
            {
                if (renderQueue == -2)
                {
                    needAdd = AddmsgType.HaveReadySetting;


                }
                else
                {
                    needAdd = AddmsgType.NeedSetting;
                    //规划空间增加：由于不增加所以暂时屏蔽
                    if (NEED_CHECK_AUTOADD_ROOMSIZE)
                    {
                        /* for (int i = 0; i < InitValues.Count; i++)
                         {
                             //都没变的时候
                             //报错就是出现没规划的情况,美术用了别尔德shader
                             //直接自增 ，加检查、；
                             if (InitValues[i] == MaterialRenderQueueSTAND[shaderName])
                             {//先增加，再检测，最终都增加，回来以后再对比查找
                                 InitValues[i]++;


                                 //异常空间检测片段
                                 //needAdd && 自己绝对不是-2&& 下一个不确定
                                 if (i < InitValues.Count - 1 && StandInitValues[i + 1] != -2)
                                 {
                                     //和自己list标准比较，前面dic是快
                                     if (InitValues[i] >= StandInitValues[i + 1])
                                     {
                                         Debug.Log("出现规划空间不足情况" + InitValues[i]);
                                     }
                                 }
                             }
                         }*/
                    }

                    if (NEED_AUTOADD)
                    {
                        MaterialRenderQueueSTAND[shaderName]++;
                    }

                    //很容易越别人空间的界的，所以值当key，那不也一样么循环判断是不是等于迭代器下一个
                    //那性能不损耗么，再来个空间吧
                }
            }
            else//-1000 也不用他
            {
                //没规划shader；默认空间给他自己增加
                needAdd = AddmsgType.NoControlSetting;

                Debug.Log("出现未规划shader:" + shaderName);

                if (NEED_CHECK_AUTOADD_ROOMSIZE)
                {
                    /*  for (int i = 0; i < InitValues.Count; i++)
                      {
                          if (InitValues[i] == Other)
                          {
                              InitValues[i]++;
                              Other++;//500个也要测试最后是后面是粒子系统

                              //异常空间检测片段
                              //needAdd && 自己绝对不是-2 && 下一个不确定
                              if (i < InitValues.Count - 1 && StandInitValues[i + 1] != -2)
                              {
                                  //和自己list标准比较，前面dic是快
                                  if (InitValues[i] >= StandInitValues[i + 1])
                                  {
                                      Debug.Log("Other都，出现规划空间不足情况" + InitValues[i]);
                                  }
                              }
                          }

                      }*/
                }

                if (NEED_AUTOADD)
                {
                    MaterialRenderQueueSTAND["Other"]++;
                }


            }

        }



        //谨防|| 联合 枚举 永真 --C陷阱和漏洞 ；不可能两个值只要两个就漏，多个更落下去，不是多个还是少个；只有&&
        /* if (dynamicItem != E_OutlineEntityType.StaticItem && dynamicItem != E_OutlineEntityType.MainPlayer)
         {
             if (material.shader.name == "GJ/CartoonChar_4_Line")
             {
                 cacheMaterialsList.Add(material);
             }
         }*/




    }
}
