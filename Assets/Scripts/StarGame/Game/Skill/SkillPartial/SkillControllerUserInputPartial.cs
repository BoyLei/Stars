using StarProjectDef;
using ProtoMsg;
using SGF.Time;
using SGF.Unity;

namespace StarProject.Game.Skill
{
    // 技能控制器  关于 用户输入相关的逻辑
    public partial class SkillController
    {
        /// <summary>
        /// 缓存当前技能的用户输入
        /// </summary>
        /// <param name="skillID"></param>
        public E_UseSkillResult CacheUserInput(SkillUseReq skillUseReq, E_UseSkillType usingSkillType)
        {
            // SGF.Debuger.Log($"{TagFlag}  ClientUseSkill skillId  {skillInfo.skillId} , runtimeID {skillUseReq.RuntimeID} ----> CacheUserInput ");
            // 缓存之前点击的 技能的skillID
            // 点击不同技能产生的 输入轴效果cache 只有一份,相互覆盖.
            // 当每次 活跃技能 -----> 非活跃的时候,触发 这个 输入效果轴的缓存逻辑

            int skillId = skillUseReq.SkillID;

            /// 执行缓存用户操作有几种情况
            /// 1.使用不同技能,但是当前存在活跃技能,且优先级不够,
            ///   此时,缓存下来的同时,给服务器发送的是 预使用技能(SendUPreUseSkillReq) ;
            /// 
            /// 2.使用相同的技能,但是当前活跃,且输入轴效果没开启的时候,
            ///   同情况1, 使用 SendUPreUseSkillReq;
            /// 
            /// 3.使用相同的技能,此时输入轴开启,但是输入轴为 预输入,
            ///   此时,需要缓存下来的同时，发送  技能的预操作(SendPreSkillUseInput)
            /// 
            /// 4.蓄力技能的抬起,取消蓄力的操作. 
            ///   由于蓄力也是走的 输入轴逻辑,对于客户端来说,蓄力技能如果是预输入的话,
            ///   需要再蓄力按钮抬起的时候,单独缓存 一份 蓄力抬起的用户操作数据,
            ///   当服务器返回蓄力技能时,使用这个 蓄力抬起的缓存数据.
            /// 

            ulong runtimeID;

            // 判断当前是否有正在使用的用户输入轴效果的技能
            {
                SkillEntity userInputSkill = GetRunningUserInputSkill(skillId);

                // 1.如果 runningInputSkill 存在,则执行的是用户输入.
                //   如果HasUserInputCache,判断 userInputCache的 runtimeID 是否一致，如果一致,则不发送相同预输入操作给服务器.
                //   如果没有缓存,那就发，并且缓存
                if (userInputSkill != null)
                {
                    runtimeID = userInputSkill.RuntimeID;
                    // xx  LogUtils.LogError(LogUtils.LogEnum.Skill,$"{TagFlag} [预输入] CacheUserInput,存在 活跃的 skillID: {skillId}输入轴,准备 缓存 和发送 用户输入runtimeID: {runtimeID} ");

                    return CachePreSkillUserInput(runtimeID, skillUseReq, E_UseSkillType.UsingSkill);
                }
            }


            // 如果不存在 输入轴效果 的技能,那就是预输入 技能
            // 如果不存在正在运行的skillId的技能,那就是预输入一个新的技能,发送SendUPreUseSkillReq
            SkillEntity runningSkillEntity = GetRunningSkillID(skillId);

            if (runningSkillEntity == null)
            {
                // xx  LogUtils.LogError(LogUtils.LogEnum.Skill,$"{TagFlag} [预输入] CacheUserInput,没有 活跃的 skillID: {skillId} 和输入轴, 准备发送预输入 技能");

                return CachePreUseSkill(skillUseReq, usingSkillType);
            }

            // 如果 存在 正在运行的 技能 ,但是 又没有任何预输入开启, 那就可能是 一个技能 正在运行,但是技能无cd
            // 此时 需要 预输入 新的技能, 但此时的 runtimeID 需要判断 是否已经 发送过.
            // 如果发送过, 那就用缓存 预输入 的 runtimeID
            // 如果没发送过, 那就 更新为 新的 runtimeID
            {
                // 先判断 是否存在 预输入 缓存的记录
                bool hasUserInputCache = HasUserInputCache();
                if (!hasUserInputCache)
                {
                    //skillUseReq.RuntimeID = GetNextID();
                    // xx  LogUtils.LogError(LogUtils.LogEnum.Skill,$"{TagFlag} [预输入] CacheUserInput, 存在skillID的技能,但是不存在 输入轴 和 预输入 记录, 那就是 预输入技能  ");

                    return CachePreUseSkill(skillUseReq, usingSkillType);
                }
                SkillInputCache userInputCache = GetUserInputCache();

                // 如果此时 存在了 预输入的记录, 判断 缓存预输入的技能 是否 就是本 技能, 如果是 的话, 那就是 已经发送过了 预输入技能, runtimeID 不变
                if (userInputCache.SkillID == skillId)
                {
                    // 判断 技能预输入的 runtimeID 是否就等于 这个技能的 runtimeID , 如果是 一样, 说明是一个失效的历史缓存记录
                    bool isSame = runningSkillEntity.RuntimeID == userInputCache.RuntimeID;
                    if (isSame)
                    {
                        // 如果活跃 技能 的 runtimeID 与 当前缓存的 预输入 runtimeID 一致, 那基本就是以下情况:
                        // 1.普工 存在了 输入轴的时候, 使用了这个技能, 存下了一份 缓存。 等预输入 结束的时候, 再次 使用这个技能, 此时 缓存存在, 且 runtimeID 与 活跃技能一样. 
                        // 2.预输入 使用了一个 技能,  存下一份缓存. 然后 在这个技能的 活跃阶段, 再次 使用这个技能, 此时 缓存 记录与此 技能runtimeID 一致.
                        // 
                        // 基于以上 两种情况, 再次使用技能的时候， 都是需要使用新的技能， 所以 runtimeID 需要换个新的
                        runtimeID = GetNextID();
                        // xx  LogUtils.LogError(LogUtils.LogEnum.Skill,$"{TagFlag} [预输入] CacheUserInput, 存在skillID的技能且 缓存中存在 这个 runtimeID {userInputCache.RuntimeID} 的记录, 可能是 普工预输入轴结束之后的缓存未关闭,此时 需要预输入技能,采用新的 id: {runtimeID} ");
                    }
                    else
                    {
                        // 如果此时 活跃技能 就是 需要使用的技能, 并且 活跃技能的 runtimeID 与 预输入 缓存中的 runtimeID 不一致, 
                        // 那么 就说明 之前已经 发送过 一次 预输入 技能, 且缓存了 一份 预输入的记录.
                        // 所以此处 再次 释放技能的时候, 就是 采用 之前 缓存的 runtimeID
                        // xx  LogUtils.LogError(LogUtils.LogEnum.Skill,$"{TagFlag} [预输入] CacheUserInput, 存在skillID的技能且 缓存中存在 这个 runtimeID {userInputCache.RuntimeID} 记录, 与正在运行的 技能 runtimeID {runningSkillEntity.RuntimeID} 不一致, 那就 采用 缓存的 runtimeID 记录重新发送技能预输入 ");
                        runtimeID = userInputCache.RuntimeID;
                    }
                }
                else
                {
                    // 如果 是一个新的技能 ID , 那么 就需要使用新的 runtTimeID
                    runtimeID = GetNextID();
                    // xx  LogUtils.LogError(LogUtils.LogEnum.Skill,$"{TagFlag} [预输入] CacheUserInput, 存在一个 不同 skillID的技能 , 所以 此时 应该采用新的技能 预输入, runtimeID: {runtimeID}");
                }


                SendUserInput(runtimeID, skillUseReq, usingSkillType, true);
            }

            return E_UseSkillResult.Succeed;
        }

        private E_UseSkillResult CachePreSkillUserInput(ulong runtimeID, SkillUseReq skillUseReq, E_UseSkillType usingSkillType)
        {
            //   SGF.Debuger.Log($"{TagFlag} [client] [input0] OnCachePreSkillUserInput skillId  {skillUseReq.SkillID}, runTimeID {runtimeID} , has runningInputSkill : {true}");

            SendUserInput(runtimeID, skillUseReq, usingSkillType, false);
            return E_UseSkillResult.Succeed;
        }

        /// <summary>
        /// 缓存预输入 使用技能 SendUPreUseSkillReq , 并提前发送
        /// </summary>
        /// <param name="skillUseReq"></param>
        /// <param name="usingSkillType"></param>
        /// <returns></returns>
        private E_UseSkillResult CachePreUseSkill(SkillUseReq skillUseReq, E_UseSkillType usingSkillType)
        {
            SkillInputCache userInputCache = GetUserInputCache();
            bool hasUserInputCache = userInputCache != null;

            ulong runtimeID;
            int skillId = skillUseReq.SkillID;

            // xx  LogUtils.LogError(LogUtils.LogEnum.Skill,$"{TagFlag} [预输入] CachePreUseSkill: skillId  {skillId}, hasUserInputCache {hasUserInputCache} ");
            if (hasUserInputCache && userInputCache.SkillID == skillId)
            {

                runtimeID = userInputCache.RuntimeID;
                // xx  LogUtils.LogError(LogUtils.LogEnum.Skill,$"{TagFlag} [预输入] 拥有 预输入的缓存, 且技能 id相同, 采用用户预输入的缓存 runtimeID: {runtimeID} ");
            }
            else
            {
                runtimeID = GetNextID();
                // xx  LogUtils.LogError(LogUtils.LogEnum.Skill,$"{TagFlag} [预输入] 没有 预输入的缓存,  采用新的 runtimeID: {runtimeID} ");
            }

            SendUserInput(runtimeID, skillUseReq, usingSkillType, true);
            return E_UseSkillResult.Succeed;
        }

        /// <summary>
        /// 客户端的输入缓存
        /// </summary>
        /// <returns></returns>
        private SkillInputCache _clientInputCache = new SkillInputCache();

        /// <summary>
        /// 等待 发送的 用户 预输入,包含 预输入 技能 和 预输入 操作
        /// 2023/4/4
        ///     当客户端 预输入 的时候,如果 当前的活跃阶段 没有被服务器通知创建,那么此次的 输入操作暂时缓存.
        ///     缓存 的唤醒 时机:
        ///     1. 当 收到 服务器同步了 当前活跃阶段的创建的时候,检查本地是否有 _waitSendInputCache, 有的话,发送给服务器;
        ///     2. 当 收到 服务器同步了 技能结束的时候, 检查 同上;
        /// 
        ///     清除时机:
        ///     1.上面 的唤醒 发送协议后,清除;
        ///     2.收到 服务器 回复 技能 使用成功, 清除;
        /// </summary>
        /// <returns></returns>
        private SkillInputCache _waitSendInputCache = new SkillInputCache();

        /// <summary>
        /// 更新 客户端的 预输入缓存
        /// note:
        ///     预输入的 缓存 一定 是 在活跃阶段发送给 服务器 的.
        /// </summary>
        /// <param name="skillUseReq"></param>
        /// <param name="usingSkillType"></param>
        /// <returns></returns>
        private bool UpdateClientInputCache(SkillUseReq skillUseReq, E_UseSkillType usingSkillType, bool sendUseSkill)
        {
            /// 2023/4/4
            /// 在 更新 玩家的 预输入 缓存之前,跟夏哥 沟通, 客户端 需要先判断当前的 活跃阶段服务器 是否已经创建，
            /// 如果还没有创建, 那客户端此时 按钮释放的 预输入, 应该只是 缓存下来, 而不是 发送给服务器.
            /// 等服务器 将这个 阶段 同步过来的 时候, 再 通知给服务器.
            /// 而且 客户端的预演, 也是需要先 判断 客户端的 预输入 是否同步给 服务器, 如果像上面这样的缓存数据, 就不应该提前预演
            {
                // 如果当前的 活跃 阶段 服务器并没有创建, 客户端 只是缓存此次的预输入
                if (activeSkillEntity != null && activeSkillEntity.CurSkillStage != null && !activeSkillEntity.CurSkillStage.ServerCreateStage)
                {
                    _waitSendInputCache.UpdateInputCache(skillUseReq, usingSkillType, sendUseSkill);
                    //DB_Close    LogUtils.LogError(LogUtils.LogEnum.Skill, $"{TagFlag} [预输入] 当前阶段:{activeSkillEntity.CurSkillStage.StageIDStr} 服务器未创建,客户端 先缓存RuntimeID: {skillUseReq.RuntimeID}, usingSkillType: {usingSkillType}");
                    return false;
                }
            }
            // 如果 不存在缓存, 那 可能是 第一次发送用户输入 也可能是 用户输入 已经被服务器执行而清除
            // 更新用户的预输入缓存
            // 更新之前,需要比较玩家的预输入是否发生改变,如果发生改变,那需要取消之前的用户预输入
            if (_clientInputCache.HasInputCache && _clientInputCache.RuntimeID != skillUseReq.RuntimeID)
            {
                //DB_Close    LogUtils.LogError(LogUtils.LogEnum.Skill, $"{TagFlag} [预输入] SendUserInput 取消用户预输入缓存runtimeID: {_clientInputCache.RuntimeID}, 缓存技能: {_clientInputCache.SkillID}, skillID: {skillUseReq.SkillID} , runtimeID: {skillUseReq.RuntimeID} ");
                CancelUserInputReq();
            }
            _clientInputCache.UpdateInputCache(skillUseReq, usingSkillType, sendUseSkill);

            return true;
        }


        private void SendUserInput(ulong runtimeID, SkillUseReq skillUseReq, E_UseSkillType usingSkillType, bool sendUseSkill)
        {
            /// 2023/4/4
            /// 发送 预输入 操作/ 预 释放技能
            /// 问题描述:
            ///     普工连点,在普工的 第三段用户输入中 输入了 用户 操作, 紧接着进入第四段 普工挥刀,此时再次点击普工,
            ///     由于此处没有 用户输入轴, 且 是活跃 阶段, 此处发送 技能的 预输入.
            ///     而 服务器 收到 客户端的请求可能都在 第三段用户输入中, 所以对于服务器来说, 可能就是在 第三段用户输入
            ///     过程中,先收到 了一次 普工的 用户操作,紧接着 收到了 一次 技能的 预输入. 
            ///     等服务器 由活跃 ---> 非活跃 阶段, 服务器 只会触发 技能的预输入. 
            ///     而 客户端的表现 就是 普工 挥第四刀 的 期间, 收到服务器 新的技能使用,转而打断挥第一刀.
            /// 
            /// 阶段方案:
            ///     目前 与夏哥的 沟通如下:
            ///     1.客户端 在 发送 用户预输入/ 技能预输入 之前, 要先确定 服务器是否走到了这个阶段(通过服务器
            ///       的阶段同步,能够知道 服务器阶段是否创建).
            ///     2.当 服务器 的阶段还未 创建的时候, 客户端 此时释放的 预输入 应当 另外缓存一份, 而不是直接给
            ///       服务器 发.
            ///     3.当 服务器  通知 客户端 阶段 创建后, 检查 本地 是否有这个 缓存, 有的话, 再给 服务器 发送
            /// 
            {
                skillUseReq.RuntimeID = runtimeID;
                FormatSkillUseReq(skillUseReq, false);

                bool updateCacheResult = UpdateClientInputCache(skillUseReq, usingSkillType, sendUseSkill);
                if (!updateCacheResult)
                {
                    //DB_Close    LogUtils.LogError(LogUtils.LogEnum.Skill, $"{TagFlag} [预输入] [====] 使用 [预输入sendUseSkill: {sendUseSkill}]  skillID: {skillUseReq.SkillID} , runtimeID: {skillUseReq.RuntimeID} ,usingSkillType: {usingSkillType} 被锁住了,不给服务器发送 ");
                    return;
                }

                if (sendUseSkill)
                {
                    // 如果当前存在活跃技能, 这个时候发送的就是
                    /// 2023/4/7
                    /// bug描述:
                    ///     冲刺技能的途中 放 普工, 会有概率出现 冲刺后 回拉释放技能的情况.
                    /// 原因:
                    ///     客户端的 预输入使用的是 客户端的坐标, 所有冲刺途中 的预输入坐标不对.
                    /// 方案:
                    ///     与夏哥沟通, 服务器目前 收到客户端的技能预输入后,会检查是否存在活跃阶段,
                    ///     存在,就用他们自己坐标,不存在,就用客户端坐标.
                    ///     对于客户端来说, 如果不存在 活跃技能,就用 客户端坐标;
                    ///     如果存在活跃技能, 就用 服务器坐标
                    /// 
                    /// 4/7 17:23
                    ///     经测试 发现 aoi 属性同步过来的坐标 并没有立即 同步过来.
                    ///     与夏哥沟通 如下:
                    ///     服务器 技能的 协议是收到 客户端的 协议后立即处理的,而 属性同步在帧尾, 这样理论上 属性同步的坐标 会晚一帧.
                    ///     note:
                    ///         但目前 测试 发现 普工 的第四段的 位移的冲刺的属性 坐标同步 延迟了 0.5s 才发过来, 应该是有bug, 需要服务器查.
                    /// 
                    ///     目前与夏哥 约定如下：
                    ///     1. 客户端 在使用技能的时候，根据本地是否有 活跃技能,判断是否是 预输入技能；
                    ///     2. 如果是 预输入技能, 那就 由服务器 决定技能的坐标位置;
                    ///        如果不是预输入技能, 那就由客户端 自己决定坐标位置;
                    ///     3. 技能的预输入 暂时不考虑
                    if (activeSkillEntity != null)
                    {
                        //DB_Close    LogUtils.LogError(LogUtils.LogEnum.Skill, $"{TagFlag} [预输入] [c-c] 时活跃技能存在, curPos: {skillUseReq.Pos} 服务器坐标: {playerData.myOwnerNtt.ServerPosition}, 让服务器使用自己坐标");

                        skillUseReq.IsUseClientPos = false;
                        // ProtoUtils.SetUnityVec3ToProtoVec3(playerData.myOwnerNtt.ServerPosition, skillUseReq.Pos);
                    }
                    //DB_Close    LogUtils.LogError(LogUtils.LogEnum.Skill, $"{TagFlag} [预输入] [====] 使用 [技能预输入] skillID: {skillUseReq.SkillID} , runtimeID: {skillUseReq.RuntimeID} ,usingSkillType: {usingSkillType} ");

                    SendPreUseSkillReq(skillUseReq);
                }
                else
                {
                    //DB_Close    LogUtils.LogError(LogUtils.LogEnum.Skill, $"{TagFlag} [预输入] [====] 使用 [预输入操作] skillID: {skillUseReq.SkillID} , runtimeID: {skillUseReq.RuntimeID} ,usingSkillType: {usingSkillType} ");

                    EffectParam userInput = GetSkillRunningInput(runtimeID);
                    if (userInput == null)
                    {
                        return;
                    }
                    SkillMsgUtils.SendPreSkillUseInput(runtimeID, skillUseReq, userInput.EffectID);
                }


                StartDelayCancelUserInput();
            }
        }

        private void SendWaitSendInputCache()
        {
            /// 2023/4/4
            /// 1.如果 本地的 缓存 是 用户 预输入操作的缓存,那就是 客户端在输入操作的时候,本地阶段 服务器还未同步创建.
            ///   等一会 服务器 同步了阶段创建后, 本地存在 预输入的 操作,那 此时 应该不管 有没有输入轴,都应该给服务器发送.
            ///   如果此时 已经 由 活跃 ---> 非活跃, 因为本地 之前的 操作缓存并没有发送(存入了waitInputCache),所以 也不会预播.
            ///   此时, 用户 操作能否 触发, 由 服务器的通知 决定.
            /// 
            /// 2.如果 本地缓存的 是 预输入技能, 那此处就是直接发送

            ulong runtimeID = _waitSendInputCache.RuntimeID;
            SkillUseReq skillUseReq = _waitSendInputCache.SkillUseReq;
            int skillId = skillUseReq.SkillID;
            E_UseSkillType useSkillType = _waitSendInputCache.UseSkillType;
            bool sendUseSkill = _waitSendInputCache.SendUseSkill;

            //DB_Close    LogUtils.LogError(LogUtils.LogEnum.Skill, $"{TagFlag} [预输入] 触发发送 缓存的预输入RuntimeID: {runtimeID}, skillId: {skillId}, useSkillType: {useSkillType}, sendUseSkill: {sendUseSkill}, 清空缓存");
            SendUserInput(runtimeID, skillUseReq, useSkillType, sendUseSkill);
            _waitSendInputCache.Reset();
        }

        /// <summary>
        /// 开启 延迟取消 用户输入
        /// note:
        ///     目前 策划 需求:
        ///     1. 预输入后0.5s 内如果没有执行这个预输入,且没有后续预输入,那就要取消这个预输入
        ///     2. 左摇杆的 滑动 与 预输入的 |角度的绝对值| > 30度, 则需要取消这个预输入
        /// </summary>
        private void StartDelayCancelUserInput()
        {
            StopDelayCancelUserInput();

            string tag = $"delayCancel_{playerData.M_EntityID}";
            DelayInvoker.DelayInvoke(tag, 0.5f, ExecuteCancelUserInputReq);
        }

        private void StopDelayCancelUserInput()
        {
            string tag = $"delayCancel_{playerData.M_EntityID}";
            DelayInvoker.CancelInvoke(tag);
        }

        private void ExecuteCancelUserInputReq(object[] args)
        {
            // xx  LogUtils.LogError(LogUtils.LogEnum.Skill,$"{TagFlag} [预输入] 延迟 执行 取消用户预输入 缓存");

            ////LogUtils.LogError(LogUtils.LogEnum.Skill,$"ExecuteCancelUserInputReq");
            CancelUserInputReq();
        }

        /// <summary>
        /// 发送取消用户输入
        /// </summary>
        private void CancelUserInputReq()
        {
            SkillInputCache userInputCache = GetUserInputCache();
            if (userInputCache == null)
            {
                return;
            }

            ulong runTimeID = userInputCache.RuntimeID;
            int skillID = userInputCache.SkillID;
            //E_UseSkillType useSkillType = userInputCache.UseSkillType;

            LogUtils.LogError(LogUtils.LogEnum.Skill, $"{TagFlag} [client] [input] 取消用户输入 CancelUserInputReq skillID : {skillID} , runtimeID : {runTimeID} ");

            // 取消 缓存的预输入,客户端也不确定取消缓存预输入到底需要取消技能还是用户预输入,所以全取消
            {
                sendPreSkillUseInputCancelReq(runTimeID, skillID);
            }

            CleanCacheUserInput();
        }

        private bool HasSkillUserInputCache(int skillID)
        {
            return _clientInputCache.SkillID == skillID;
        }

        private bool HasUserInputCache()
        {
            return _clientInputCache.HasInputCache;
        }

        private bool HasRuntimeIDUserInputCache(ulong runtimeID)
        {
            if (!_clientInputCache.HasInputCache)
            {
                return false;
            }
            return _clientInputCache.RuntimeID == runtimeID;
        }

        private SkillInputCache GetUserInputCache()
        {
            SkillInputCache userInputCache = _clientInputCache.HasInputCache ? _clientInputCache : null;
            return userInputCache;
        }

        public void CleanCacheUserInput()
        {
            StopDelayCancelUserInput();
            _clientInputCache.Reset();
        }

        public void CleanWaitSendInputCache()
        {
            _waitSendInputCache.Reset();
        }

        private ServerInputCache serverInputCache = new ServerInputCache();

        private void UpdateServerInputCache(ulong runtimeID, I_EffectParam effectParam, CustomBlackBoardNode blackBoardNode)
        {
            serverInputCache.UpdateCache(runtimeID, effectParam, blackBoardNode);
        }

        private void ClearServerInputCache()
        {
            serverInputCache.Reset();
        }

        /// <summary>
        /// 触发用户输入 
        /// </summary>
        /// <param name="enterStageTime">触发用户输入 缓存的时候, 进入后面 技能的 补帧时间</param>
        private void TriggerExecuteCacheUserInput(double enterStageTime)
        {
            if (activeSkillEntity != null)
            {
                return;
            }

            // 主角自己的时候, 在活跃---> 非活跃 触发用户输入的时候,需要判断是否禁止 攻击,
            // 如果禁止 攻击, 那就不触发.
            if (playerData.isMainPlayer)
            {
                if (playerData.IsForbidAttack)
                {
                    return;
                }
                TriggerMainPlayerUserInput(enterStageTime);
            }
            else
            {
                // 2023/4/11
                // 其它玩家的 输入轴效果 需要 找 夏哥 确认是否需要执行 这块的逻辑
                // 而对于 其它人的 用户输入, 目前服务器的输入效果 都是在触发的时候发给客户端,
                // 那么 很大的可能就是 网络延迟 导致发过来的时候客户端 已经进入了非活跃阶段,不会走这段逻辑
                TriggerServerInput();
            }
        }

        private long tempTime = 0;
        /// <summary>
        /// 触发 主角的 用户输入
        /// </summary>
        private void TriggerMainPlayerUserInput(double enterStageTime)
        {
            // 如果 不预播 客户端技能, 那就是 等待 服务器输入 来触发 用户输入
            if (!IsPrePlayClientSkill)
            {
                TriggerServerInput();
                return;
            }

            {
                /// 2023/3/2
                /// 重新整理 用户输入后,发现客户端 提前预播技能 之前是只依赖客户端缓存数据的问题.
                /// 在后续 的逻辑逻辑重构 时, 需要考虑一个问题, 客户端的 预播 到底是采用客户端数据
                /// 还是 采用 服务器的数据.
                /// 
                /// 2023/4/3
                /// 接 上面的话, 目前 技能的预输入 和 操作的预输入 客户端/服务器都是用同一个缓存，
                /// 即: 使用 了 A的 技能预输入 后， 在输入 B 的 用户输入操作, 生效的 只有 B , A的会被替换.
                /// 
                /// 由此, 客户端 在由 活跃 ---> 非活跃 阶段 触发 用户输入轴的时候, 也就只需要 关心当前
                /// 缓存的输入 是否存在, 如果存在, 就用 缓存的 输入执行 用户输入效果.
                /// 如果 不存在, 那 一般来说 服务器也不会有( 目前 还没有 服务器主动触发主角用户输入的情况).
                /// 
                /// 同样, 也基于上, 当 客户端缓存不存在的时候, 客户端 触发服务器缓存的 必要性 不大
                /// (但流程 可以补全).

                bool hasUserInputCache = HasUserInputCache();
                ////LogUtils.LogError(LogUtils.LogEnum.Skill,$"{TagFlag}  TriggerExecuteCacheUserInput activeSkillEntity null , ready  ExecuteCacheUserInput hasUserInputCache : {hasUserInputCache} ");
                if (!hasUserInputCache)
                {
                    TriggerServerInput();
                    return;
                }
            }




            SkillInputCache userInputCache = GetUserInputCache();



            int skillId = userInputCache.SkillID;
            ulong cacheRuntimeID = userInputCache.RuntimeID;

            SkillUseReq skillUseReq = userInputCache.SkillUseReq;

            E_UseSkillType useSkillType = userInputCache.UseSkillType;



            // 更新当前 需要发送的 skillUseReq 的时间
            FormatSkillUseReq(skillUseReq, false);


            SkillEntity runningInputSkill = GetRunningUserInputSkill(skillId);



            {
                /// 目前已知的是 收到服务器的useSkillRet后，客户端删掉用户预输入的缓存
                /// 但是如果客户端存在预播放,那客户端使用技能的时候，是不是也应该删掉呢？？？？？
                /// 
                /// 2023/4/3
                /// 接上面:  可以 删掉. 如果客户端收到 服务器技能A 使用之前 , 客户端恰好有一个新的节能输入B 缓存发送给服务器,
                /// 那这个 技能 预输入B 也是在 A 执行完成之后执行. 在客户端 执行 服务器使用A 之后, 清除 B的预输入. 然后 B 是否
                /// 能够使用, 完全 可以依赖 服务器 回复即可.
                CleanCacheUserInput();
            }

            // 存在当前预输入 技能的用户操作轴,那就执行它的用户预输入操作逻辑
            if (runningInputSkill != null)
            {
                EffectParam userInput = runningInputSkill.GetUserInput();

                if (!CheckCanTrigglePreUserInput(runningInputSkill.RuntimeID, userInput))
                {
                    //DB_Close    LogUtils.LogError(LogUtils.LogEnum.Skill, $"[预输入] [a-a] 准备提前触发预输入操作,但是 服务器没有 返回过 成功的用户输入EffectID: {userInput.EffectID}");
                    return;
                }

                if (cacheRuntimeID != runningInputSkill.RuntimeID)
                {
#if (UNITY_EDITOR && BATTLE_DEBUG)
                    LogUtils.LogError(LogUtils.LogEnum.Skill, $"{TagFlag} [预输入] 暂停: TriggerMainPlayerUserInput 触发主角缓存预输入: runtimeID: {runningInputSkill.RuntimeID}, skillID: {skillId}, 缓存的RuntimeID: {cacheRuntimeID}, 此时不触发用户输入轴效果!!!");
#endif
#if UNITY_EDITOR
                    //UnityEngine.Debug.Break();
#endif
                    return;
                }
                ////LogUtils.LogError(LogUtils.LogEnum.Skill,$"{TagFlag}  ClientUseSkill skillID  {skillId} , ExecuteUserInput ");

                // 执行 主角的用户预输入操作, 先同步之前 预输入的 朝向
                {
                    // runningInputSkill.UpdateSkillReqRot(skillUseReq); // 更新客户端/服务器角度/同步显示层
                }

                // xx  LogUtils.LogError(LogUtils.LogEnum.Skill,$"{TagFlag} [预输入] TriggerMainPlayerUserInput 触发主句缓存预输入 , runtimeID: {runningInputSkill.RuntimeID} , skillID: {skillId} ");

                runningInputSkill.ExecuteUserInput(true, E_BlackBoardNodeState.ClientUsed);
            }
            else
            {

                return;
                // 如果不存在 正在运行的用户输入轴, 那 从活跃 --> 非活跃 触发的 就是 新的技能的 预输入. 此时直接 使用使用新的技能即可
                // xx  LogUtils.LogError(LogUtils.LogEnum.Skill,$"{TagFlag} [预输入] TriggerMainPlayerUserInput 触发缓存预输入 , runtimeID: {cacheRuntimeID} , skillID: {skillId}, 使用新的技能 ");

                SkillContainer skillContainer = GetSkillContainer(skillId);
                if (skillContainer != null)
                {
                    // 当执行 用户 输入缓存逻辑的时候, 需要考虑 之前是否有 给服务器发送过与输入请求.
                    // 如果,当前 用户输入技能 A 100, 如果之前 预输入了技能 B 101，此时，就需要使用 B 101;
                    // 如果 预输入 技能 A 100(类似于普工第四段 输入了 普工), 接下来执行预输入就需要 使用 A 101;
                    // 对于 UseSkill 来说, 在 UseNewSkill 的时候,需要判断 是否存在正在运行的 技能 ,如果有，则用新的 runtimeID
                    // FormatSkillUseReq(skillUseReq, true);
                    //SGF.Debuger.Log($"{TagFlag}  ClientUseSkill skillID  {skillId} , ExecuteCacheUserInput ---> UseNewSkill ");

                    if (tempTime != 0)
                    {
                        var cost = TimeUtils.ClientNowStampMilli - tempTime;
                        //DB_Close    LogUtils.LogError(LogUtils.LogEnum.Skill, $"{TagFlag} [v-v] 客户端准备 使用技能 UseSkill: enterStageTime: {enterStageTime}, 间隔时间: {cost}ms ");
                    }
                    UseSkill(skillContainer, skillUseReq, useSkillType, (int)enterStageTime);
                }
            }
        }

        /// <summary>
        ///  触发服务器 同步的 用户操作
        /// </summary>
        private void TriggerServerInput()
        {
            // 没有服务器的 输入缓存数据
            ulong runtimeID = serverInputCache.RuntimeID;
            ////LogUtils.LogError(LogUtils.LogEnum.Skill,$"{TagFlag}  TriggerOtherServerInput runtimeID  {runtimeID} ");
            //LogUtils.LogError(LogUtils.LogEnum.Skill,$"{TagFlag} 技能进入非活跃阶段,准备触发 服务器下发的用户操作缓存, 技能 runtimeID  {runtimeID} ");

            if (runtimeID == 0)
            {
                return;
            }

            SkillEntity runningInputSkill = GetRunningSkill(runtimeID);
            if (runningInputSkill == null)
            {
                return;
            }
            // xx  LogUtils.LogError(LogUtils.LogEnum.Skill,$"{TagFlag} [预输入] TriggerServerInput 触发服务器用户操作缓存 的预输入 , runtimeID: {runningInputSkill.RuntimeID} , skillID: {runningInputSkill.skillId} ");

            runningInputSkill.ExecuteServerUserInput(serverInputCache, E_BlackBoardNodeState.ServerUsed);
            ClearServerInputCache();
        }

        /// <summary>
        /// 技能 提前 预演 用户输入的执行
        /// </summary>
        /// <param name="skillEntity"></param>
        public E_UseSkillResult PrePlayUserInput(SkillEntity skillEntity, SkillUseReq skillUseReq, float arg = -999)
        {
            LogUtils.LogError(LogUtils.LogEnum.Skill, $"{TagFlag} [client]  PrePlayUserInput skillID  {skillEntity.skillId} , runtimeID {skillEntity.RuntimeID} , arg: {arg} ");

            ulong userInputRuntimeID = skillEntity.RuntimeID;
            skillUseReq.RuntimeID = userInputRuntimeID;
            EffectParam userInput = GetSkillRunningInput(userInputRuntimeID);
            if (userInput == null)
            {
                return E_UseSkillResult.User_Input_Not_Open;
            }



            // 当gm 设置 不预播放 技能的时候, 就只给服务器发送协议
            if (!IsPrePlayClientSkill)
            {
                LogUtils.LogError(LogUtils.LogEnum.Skill, $"{TagFlag} 关闭了预播, 此时 发送预输入操作: skillID: {skillUseReq.SkillID},RuntimeID: {userInputRuntimeID} ");

                return E_UseSkillResult.Succeed;
            }


            E_UseSkillResult triggleResult = skillEntity.CheckCanPreTriggerUserInput(skillUseReq, E_BlackBoardNodeState.ClientUsed);

            LogUtils.LogError(LogUtils.LogEnum.Skill, $"[energy] 检查是否可以触发的 结果: {triggleResult}");

            // 如果是 蓄力秒放这种, 那就不 在按钮抬起的时候就 给服务器发送 输入取消的逻辑
            if (triggleResult != E_UseSkillResult.DealyInvokeSkill)
            {
                // 不管是不是 预输入, 在可以提前预播的时候,都给服务器发送协议.
                SkillMsgUtils.SendPreSkillUseInput(userInputRuntimeID, skillUseReq, userInput.EffectID);
            }


            // 如果 用户输入触发 失败, 直接 返回 触发的结果
            if (triggleResult != E_UseSkillResult.Succeed && triggleResult != E_UseSkillResult.DealyInvokeSkill)
            {
                LogUtils.LogError(LogUtils.LogEnum.Skill, $"{TagFlag} 提前预播 PrePlayUserInput runtimeID: {userInputRuntimeID} 失败, triggleResult: {triggleResult}");
                return triggleResult;
            }

            skillEntity.UpdateSkillReqRot(skillUseReq); // 更新客户端角度/同步显示层

            // SkillMsgUtils.SendPreSkillUseInput(userInputRuntimeID, skillUseReq, userInput.EffectID);

            //LogUtils.LogError(LogUtils.LogEnum.Skill,$"{TagFlag} 准备预 播 用户输入: skillID: {skillUseReq.SkillID}");
            triggleResult = skillEntity.OnTriggerUserInput(skillUseReq, true, E_BlackBoardNodeState.ClientUsed, arg);

            LogUtils.LogError(LogUtils.LogEnum.Skill, $"{TagFlag} [预输入] 提前播放预输入, runtimeID: {userInputRuntimeID}  , triggleResult: {triggleResult}");


            //SGF.Debuger.Log($"{TagFlag} 技能 自动转向---OnUserInput 角度={skillUseReq.Rot},,我的角度={playerData.myOwnerNtt.EulerAngles.y}");
            // 客户端立即执行 用户操作, 此时 先同步玩家的 朝向


            return E_UseSkillResult.Succeed;

        }

        /// <summary>
        /// 获取 skillID 正在运行的 用户输入 skillEntity
        /// </summary>
        /// <param name="skillID"></param>
        /// <returns></returns>
        private SkillEntity GetRunningUserInputSkill(int skillID)
        {
            // 如果活跃技能 就是skillID 且 开启了 用户输入,那就优先使用活跃技能
            if (activeSkillEntity != null && activeSkillEntity.skillId == skillID && activeSkillEntity.CheckIsOpenUserInput())
            {
                return activeSkillEntity;
            }

            // 如果不是活跃技能,那就直接找 第一个开启了 用户输入且skillID 相等的 skillEntity
            return skillEntities.Find((SkillEntity skillEntity) =>
            {
                return skillEntity.IsClientRunning && skillEntity.skillId == skillID && skillEntity.CheckIsOpenUserInput();
            });
        }


        /// <summary>
        /// 触发 等待用户输入的 缓存.
        /// 目前的 触发点如下:
        ///     1.服务器 通知了 当前 技能的 阶段进入活跃()
        ///     2. 
        /// </summary>
        private void TriggerWaitSendInputCache()
        {
            if (!_waitSendInputCache.HasInputCache)
            {
                return;
            }
            SendWaitSendInputCache();
        }

        ServerPreSkillUseInputResult serverPreSkillUseInputResult = new ServerPreSkillUseInputResult();

        /// <summary>
        /// 收到 服务器 用户预输入操作的 回复 
        /// note:
        ///     1.客户端预输入操作 由活跃 ---> 非活跃 的检查触发的时候, 先去检查服务器数据是否提前到(服务器跑的快一点);
        ///       如果已经提前到了,那客户端 直接跑 服务器数据;
        ///     2.如果 服务器数据 还没到,那就检查 服务器有没有 返回这个输入轴的 预输入操作效果结果是否存在。
        ///       目前存在一个问题, 客户端的预输入操作 是每次 点击 都会发送一次. 那么由于 网络延迟 或者 服务器提前跑,
        ///       就会存在 前面成钢,但 后面几次的 预输入操作 服务器返回的 可能失败的情况.
        ///       对于这种情况, 客户端  只需要能收到一次 预输入操作,即可!!!
        ///     3. 在收到了 用户预输入操作的 回复的时候, 检查 当前的 输入轴 是否有效 且当前的 阶段 是否为 非活跃阶段.
        ///        如果是 非活跃 阶段, 其实也可以主动触发 一次用户的 与输入操作. 
        ///        但 现在 是服务器回复输入输入的时候,也会触发 ，所以 , 这一条可以不去管.
        /// </summary>
        /// <param name="preSkillUseInputRet"></param>
        public void OnPreSkillUseInputRet(PreSkillUseInputRet preSkillUseInputRet)
        {
            ulong runtimeID = preSkillUseInputRet.RuntimeID;
            int effectID = preSkillUseInputRet.EffectID;
            //DB_Close    LogUtils.LogError(LogUtils.LogEnum.Skill, $"{TagFlag} [a-a] 收到服务器返回的技能预输入结果:runtimeID {runtimeID}, effectID: {effectID}, result: {preSkillUseInputRet.Ret}");
            SkillEntity skillEntity = GetRunningSkill(preSkillUseInputRet.RuntimeID);
            if (skillEntity == null)
            {
                return;
            }

            // 检查 技能是否开启effectID 对应的输入轴, 没开启就啥都不管
            if (!skillEntity.CheckIsOpenEffectInput(effectID))
            {
                return;
            }

            serverPreSkillUseInputResult.UpdateData(preSkillUseInputRet);
        }

        /// <summary>
        /// 检查是否可以触发用户预输入操作
        /// </summary>
        /// <returns></returns>
        public bool CheckCanTrigglePreUserInput(ulong runtimeID, EffectParam userInput)
        {
            return serverPreSkillUseInputResult.CheckUserInputResult(runtimeID, userInput);
        }

        public EffectParam GetSkillRunningInput(ulong runtimeID)
        {
            SkillEntity runningSkill = GetRunningSkill(runtimeID); ;
            if (runningSkill == null)
            {
                return null;
            }

            EffectParam userInput = runningSkill.GetUserInput();
            if (userInput == null)
            {
                return null;
            }

            return userInput;
        }

        public bool HasRunningSkillUserInput(int skillID)
        {
            var input = skillEntities.Find((SkillEntity skillEntity) =>
            {
                // SGF.Debuger.Log($" HasRunningSkillUserInput skillid={skillID}, ReadyRelease: {skillEntity.ReadyRelease} , CheckIsOpenUserInput: {skillEntity.CheckIsOpenUserInput()} ,IsUserInputValid: {skillEntity.IsUserInputValid} ");

                return !skillEntity.ReadyRelease && skillEntity.skillId == skillID && skillEntity.CheckIsOpenUserInput();
            });

            return input != null;
        }

        /// <summary>
        /// 如果输入轴 开启的话，把其它的 输入轴关闭
        /// </summary>
        /// <param name="triggerSkillEntity"></param> <summary>
        public void TriggerUserInputStart(SkillEntity triggerSkillEntity, I_EffectParam effectParam)
        {

            triggerSkillEntity.skillContainer.OnUserInputStart(effectParam);
            skillEntities.ForEach((skillEntity) =>
            {
                if (skillEntity.skillContainer == triggerSkillEntity.skillContainer && skillEntity != triggerSkillEntity)
                {
                    skillEntity.MarkUserInputClose();
                }
            });

            GameManager.Instance.TriggerEvent(TriggerEventType.StartUserInput, null);
        }

    }
}
