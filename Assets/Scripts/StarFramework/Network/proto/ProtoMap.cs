using System.Collections.Generic;

using Google.Protobuf;
using Google.Protobuf.Reflection;
using ProtoMsg;

//  ======= 生成消息ID枚举 ====
[XLua.LuaCallCSharp]
public enum MsgIDEnum{
	  SyncBaseInfoID = 1000,
	  RpcMsgID = 1001,
	  PropBaseSyncListID = 1002,
	  RawMsgID = 1003,
	  BattlePropDataID = 1004,
	  MsgRetID = 1020,
	  CommonRewardNtfID = 1021,
	  PlayerOffineSrvID = 1022,
	  PlayerReloadSrvID = 1023,
	  PlayerLogoutSrvID = 1024,
	  Client2ThirdReqID = 1030,
	  Client2ThirdRetID = 1031,
	  Client2LinkSrvID = 1032,
	  Client2CenterReqID = 1033,
	  Client2CenterSrvID = 1034,
	  RouteMsgToOtherSrvReqID = 1035,
	  RouteMsgToOtherSrvRetID = 1036,
	  UNUSED_Center2SrvReqID = 1037,
	  UNUSED_Center2SrvRetID = 1038,
	  Server2LinkSrvID = 1039,
	  Vector3ID = 1050,
	  ArrayVector3ID = 1051,
	  RespParamID = 1052,
	  BaseDataID = 1053,
	  BaseBinaryID = 1054,
	  BaseItemDataID = 1055,
	  DropSetID = 1056,
	  WaitMonNodeID = 1057,
	  UserBaseDataID = 1058,
	  I32WithBoolMapDataID = 1059,
	  I64WithBoolMapDataID = 1060,
	  UI64WithI32MapDataID = 1061,
	  UI64ArrayDataID = 1062,
	  I64ArrayDataID = 1063,
	  I32WithI32MapDataID = 1064,
	  I64WithI64MapDataID = 1065,
	  DBDataModelID = 2000,
	  MapModelID = 2001,
	  MsgDataID = 2002,
	  GlobalServerInfoMDID = 2003,
	  DBGetPlayerInfoReqID = 2010,
	  OtherGetPlayerInfoReqID = 2011,
	  DBGetPlayerInfoAckID = 2012,
	  DBGetPlayerInfoEndAckID = 2013,
	  DBUpUserDatasReqID = 2014,
	  OtherUpUserDatasRetID = 2015,
	  DBGetPlayerTableReqID = 2016,
	  AreaMDID = 2100,
	  GroupMDID = 2101,
	  UserMDID = 2102,
	  PlayerMDID = 2103,
	  WhiteListMDID = 2104,
	  ClientDeviceInfoID = 2128,
	  YoukaLoginNewReqID = 2129,
	  YoukaLoginReqID = 2130,
	  UserLoginReqID = 2131,
	  UserLoginRetID = 2132,
	  UpLevelReqID = 2133,
	  TransJobNtfID = 2134,
	  RechargeOrderMDID = 2200,
	  UserPayDataID = 2201,
	  UserPayDataMDID = 2202,
	  GetRechargeOrderReqID = 2210,
	  GetRechargeOrderRetID = 2211,
	  PaySuccessNtfID = 2212,
	  RechargeInfoNtfID = 2213,
	  FirstPaySignInfoNtfID = 2214,
	  FirstPaySignReqID = 2215,
	  SmallMonthCardSignReqID = 2216,
	  SmallMonthCardInfoNtfID = 2217,
	  BattlePassInfoNtfID = 2220,
	  BattlePassGetLevelRewardReqID = 2221,
	  BattlePassBuyLevelReqID = 2222,
	  CommonParamDataID = 3210,
	  NewEventsID = 3220,
	  EventNodeID = 3221,
	  NoticeEventsID = 3222,
	  NoticeEventID = 3223,
	  MonsterDataToMsgID = 3230,
	  BattleSkillDataToMsgID = 3231,
	  ItemDataToMsgID = 3232,
	  MapDataToMsgID = 3233,
	  InterDataToMsgID = 3234,
	  ControlDataID = 3235,
	  FriendApplyClientNtfID = 3236,
	  SearchUserReqID = 3400,
	  SearchUserRespID = 3401,
	  GetUserListReqID = 3402,
	  GetUserListRespID = 3403,
	  RecommendUserReqID = 3404,
	  RecommendUserRespID = 3405,
	  ApplyToAddFriendReqID = 3406,
	  ApplyToAddFriendRespID = 3407,
	  GetPageFriendsApplyToReqID = 3408,
	  GetPageFriendsApplyToRespID = 3409,
	  IsFriendReqID = 3410,
	  IsFriendRespID = 3411,
	  IsBlackReqID = 3412,
	  IsBlackRespID = 3413,
	  DeleteFriendReqID = 3414,
	  DeleteFriendRespID = 3415,
	  RespondFriendApplyReqID = 3416,
	  RespondFriendApplyRespID = 3417,
	  GetDesignatedFriendsReqID = 3418,
	  GetDesignatedFriendsRespID = 3419,
	  GetPageFriendsReqID = 3420,
	  GetPageFriendsRespID = 3421,
	  GetFriendIDsReqID = 3422,
	  GetFriendIDsRespID = 3423,
	  SetFriendRemarkReqID = 3424,
	  SetFriendRemarkRespID = 3425,
	  ImportFriendReqID = 3426,
	  ImportFriendRespID = 3427,
	  AddBlackReqID = 3428,
	  AddBlackRespID = 3429,
	  RemoveBlackReqID = 3430,
	  RemoveBlackRespID = 3431,
	  GetPageBlacksReqID = 3432,
	  GetPageBlacksRespID = 3433,
	  BlackInfoID = 3434,
	  PublicUserInfoID = 3435,
	  BaseRespID = 3436,
	  RequestQueryID = 3437,
	  UserInfoID = 3438,
	  FriendInfoID = 3439,
	  FriendRequestID = 3440,
	  ChatExtraInfosID = 3500,
	  ChatExtraInfoID = 3501,
	  ChatLocationInfoID = 3502,
	  RecruitNtfID = 3503,
	  JobSrvNtfID = 3520,
	  SendChatMsgReqID = 3521,
	  SendChatMsgRetID = 3522,
	  ChatMsgNoticeID = 3523,
	  ChatMsgAllNtfID = 3524,
	  ChatMsgNtfByConID = 3525,
	  ChatMsgNoticeSID = 3526,
	  RunHorseSrvReqID = 3527,
	  RunHorseSrvRetID = 3528,
	  RunHorseNoticeID = 3529,
	  AddCurHeroAttrID = 3600,
	  GmCmdReqID = 3610,
	  GmCmdAckID = 3611,
	  ServerInfoUpdateNtfID = 3620,
	  GCMemoryReqID = 3621,
	  SetServerTimeReqID = 3622,
	  ServerGmCmdReqID = 3623,
	  ServerGmCmdAckID = 3624,
	  HeroMDID = 3800,
	  TransJobRewardListID = 3801,
	  HeroJobSkillModelID = 3805,
	  HeroTalentModelID = 3806,
	  HeroTalentTreeID = 3807,
	  HeroBaseInfoID = 3808,
	  OfflineBattleSaveID = 3809,
	  SingleSkillInfoID = 3810,
	  SkillPosID = 3811,
	  HeroInstanceDataID = 3812,
	  OfflinePartnerDataID = 3813,
	  HeroDailyInstanceDataID = 3814,
	  HeroPersonSecretDataID = 3815,
	  HeroTeamDailyDataID = 3816,
	  HeroSkillCaseDataID = 3817,
	  LobbyHeroInfoID = 3820,
	  AllSkillNoticeID = 3821,
	  SwitchSkillPosReqID = 3822,
	  SwitchSkillPosRetID = 3823,
	  SwitchTalentReqID = 3824,
	  SwitchTalentRetID = 3825,
	  UpdateJobSkillReqID = 3826,
	  UpdateJobSkillRetID = 3827,
	  UpdateJobSkillNoticeID = 3828,
	  UpSkillCaseDataReqID = 3829,
	  UpSkillCaseDataRetID = 3830,
	  GetHeroBattleInfoListID = 3840,
	  HeroBattleInfoID = 3841,
	  SwitchSkillPosSrvReqID = 3842,
	  SwitchSkillPosSrvRetID = 3843,
	  SkillUpgradeSrvReqID = 3844,
	  SkillUpgradeSrvRetID = 3845,
	  NotifyLeveChangeSrvID = 3846,
	  SwitchTalentNoticeID = 3847,
	  TransJobReqID = 3850,
	  TransJobRetID = 3851,
	  TalentNodeActiveReqID = 3852,
	  TalentNodeActiveRetID = 3853,
	  TalentTreeResetReqID = 3854,
	  TalentTreeResetRetID = 3855,
	  TalentNodeAutoReqID = 3856,
	  TalentNodeAutoRetID = 3857,
	  TransJobCondRewardReqID = 3858,
	  TransJobCondRewardRetID = 3859,
	  OfflineBattleDataID = 3860,
	  ResetJobSkillPointReqID = 3861,
	  ResetJobSkillPointRetID = 3862,
	  WorldLevelReqID = 3863,
	  WorldLevelRetID = 3864,
	  WorldLevelConfirmReportID = 3865,
	  InterEeqID = 4000,
	  InterRetID = 4001,
	  InterStateID = 4002,
	  AllInterStateID = 4003,
	  UserCurEffNotifyID = 4004,
	  CurEffOverReqID = 4005,
	  EffectDataID = 4006,
	  BreakInterReqID = 4007,
	  ItemSpaceMDID = 4200,
	  ItemMDID = 4201,
	  EquipSlotMDID = 4202,
	  EntityPropEntryMDID = 4203,
	  EqRecastMDID = 4204,
	  TweeterMDID = 4205,
	  CommonEntryPropID = 4210,
	  EquipPropListID = 4211,
	  DropinfoID = 4212,
	  RewardInfoID = 4213,
	  StorageDrugsMdID = 4214,
	  CombatSkillID = 4215,
	  CombatSkillListID = 4216,
	  GemSlotID = 4217,
	  GemSlotListID = 4218,
	  ExtractaAmuletMDID = 4219,
	  RecastModelID = 4220,
	  ReEventInfoID = 4221,
	  ReHistoryModelID = 4222,
	  TreasureDataID = 4223,
	  CommonEntryPropsModelID = 4224,
	  ItemUseReqID = 4230,
	  ItemUseSrvID = 4231,
	  ItemAddDelReqID = 4232,
	  ItemAddDelSrvID = 4233,
	  DropNotifyID = 4234,
	  WearEquipReqID = 4235,
	  WearEquipRetID = 4236,
	  TakeOffEquipReqID = 4237,
	  TakeOffEquipRetID = 4238,
	  DropListSrvID = 4239,
	  UseItemCDInfoNtfID = 4240,
	  DelItemReqID = 4241,
	  DelItemRetID = 4242,
	  OneUseDrugReqID = 4243,
	  UseDrugSrvReqID = 4244,
	  UseDrugSrvRetID = 4245,
	  UpdateStorageDrugsReqID = 4246,
	  EquipSlotUpgradeReqID = 4247,
	  EquipSlotUpgradeRetID = 4248,
	  EquipSlotDataID = 4249,
	  EquipSlotAllNoticeID = 4250,
	  EquipSlotDifferNoticeID = 4251,
	  EquipRefineReqID = 4252,
	  EquipRefineRetID = 4253,
	  CommonEntityPropChangeSrvID = 4254,
	  WearParEquipReqID = 4255,
	  WearParEquipRetID = 4256,
	  TakeOffParEquipReqID = 4257,
	  TakeOffParEquipRetID = 4258,
	  GemSynthesizedReqID = 4259,
	  GemSynthesizedRetID = 4260,
	  WearAmuletReqID = 4261,
	  WearAmuletRetID = 4262,
	  TakeOffAmuletReqID = 4263,
	  TakeOffAmuletRetID = 4264,
	  PolishAmuletReqID = 4265,
	  PolishAmuletRetID = 4266,
	  ExtractAmuletReqID = 4267,
	  ExtractAmuletRetID = 4268,
	  ChooseExtractReqID = 4269,
	  ChooseExtractRetID = 4270,
	  RandHeraldryReqID = 4271,
	  RandHeraldryRetID = 4272,
	  HeraldryWearReqID = 4273,
	  HeraldryWearRetID = 4274,
	  HeraldryTakeOffReqID = 4275,
	  HeraldryTakeOffRetID = 4276,
	  ItemResolveReqID = 4277,
	  ItemResolveRetID = 4278,
	  TempItemListID = 4279,
	  EqRecastReqID = 4280,
	  EqRecastRetID = 4281,
	  ChooseEqRecastReqID = 4282,
	  ChooseEqRecastRetID = 4283,
	  OpenBoxReqID = 4284,
	  OpenBoxRetID = 4285,
	  ReItemSrvID = 4286,
	  AddItemSrvID = 4287,
	  WearTweeterReqID = 4288,
	  WearTweeterRetID = 4289,
	  TakeOffTweeterReqID = 4290,
	  TakeOffTweeterRetID = 4291,
	  UpTweeterReqID = 4292,
	  UpTweeterRetID = 4293,
	  SynthesizedItemReqID = 4294,
	  BoxRareItemBroadNtfID = 4295,
	  MailMDID = 4400,
	  MailListNtfID = 4401,
	  MailGetInfosReqID = 4402,
	  MailGetInfosRetID = 4403,
	  MailDeleteReqID = 4404,
	  MailDeleteReadReqID = 4405,
	  MailDeleteRetID = 4406,
	  MailGetRewardReqID = 4407,
	  MailGetRewardAllReqID = 4408,
	  MailGetRewardRetID = 4409,
	  MailMarkReadedReqID = 4410,
	  MailsMarkReadedReqID = 4411,
	  MailsMarkReadedRetID = 4412,
	  MailAttachedContentID = 4413,
	  GetMailAttachedContentNtfID = 4414,
	  GetMailAttachedContentDataID = 4415,
	  NewMailReqSrvID = 4416,
	  SendGlobalGMMailReqID = 4450,
	  BankGoodsMDID = 4600,
	  BankGoodsEntryMDID = 4601,
	  BankItemIDMDID = 4602,
	  BankLogMDID = 4603,
	  BankGoodsBaseID = 4610,
	  BankLogBaseID = 4611,
	  BankItemBaseID = 4612,
	  BankSubTypeListReqID = 4620,
	  BankTypeListReqID = 4621,
	  BankItemListReqID = 4622,
	  BankQueryListReqID = 4623,
	  BankGoodsListRetID = 4624,
	  BankItemIDListRetID = 4625,
	  BankMyFollowReqID = 4630,
	  BankMyFollowSrvID = 4631,
	  BankMyFollowRetID = 4632,
	  BankMyGoodsReqID = 4633,
	  BankMyGoodsRetID = 4634,
	  BankAddGoodsReqID = 4635,
	  BankAddGoodsSrvID = 4636,
	  BankAddGoodsRetID = 4637,
	  BankOffShelfGoodsReqID = 4638,
	  BankGetIncomeReqID = 4639,
	  BankLogByItemIDReqID = 4640,
	  BankLogByItemIDRetID = 4641,
	  BankLogByBuyReqID = 4642,
	  BankLogByBuyRetID = 4643,
	  BankLogBySellReqID = 4644,
	  BankLogBySellRetID = 4645,
	  BankBuyGoodsReqID = 4646,
	  BankBuyGoodsRetID = 4647,
	  BankSetMyFollowReqID = 4648,
	  BankSetMyFollowSrvID = 4649,
	  BankSetMyFollowRetID = 4650,
	  BankSetMyFollowItemIDReqID = 4651,
	  BankSetMyFollowItemIDRetID = 4652,
	  BankGetItemPriceReqID = 4653,
	  BankGetItemPriceRetID = 4654,
	  BankBuyGoodsMaxReqID = 4655,
	  BankPlayerMDID = 4670,
	  FollowGoodsFieldID = 4671,
	  FollowItemFieldID = 4672,
	  Money2MoneyMDID = 4680,
	  Money2MoneyReqID = 4681,
	  Money2MoneyRetID = 4682,
	  Money2MoneyNtfID = 4683,
	  Money2MoneyInfoReqID = 4684,
	  Money2MoneyInfoRetID = 4685,
	  TradeBankMaySystemRestockNtfID = 4686,
	  EnterAOIID = 4800,
	  LeaveAOIID = 4801,
	  UpdateAOIID = 4802,
	  AOIMsgID = 4803,
	  PropSyncListID = 4804,
	  RepeatedPropSyncListID = 4805,
	  UserMainDataNotifyID = 4806,
	  EnterSceneReqID = 4820,
	  MapPreloadNoticeID = 4821,
	  EnterSpaceNtfID = 4822,
	  LeaveSpaceID = 4823,
	  MapChangeReqID = 4824,
	  MapChangeRetID = 4825,
	  MapLeaveAckID = 4826,
	  MapLeaveID = 4827,
	  MapEnterAckID = 4828,
	  MapEnterID = 4829,
	  ClientInsReadyReqID = 4830,
	  SpaceLoadEndNtfID = 4831,
	  ChangeDeadStateID = 4840,
	  MoveMsg2ID = 4841,
	  ServerTimeReqID = 4842,
	  ServerTimeRetID = 4843,
	  RoleReviveReqID = 4844,
	  RoleReviveRetID = 4845,
	  PropPanelReqID = 4846,
	  PropPanelRetID = 4847,
	  AutoFindPathReqID = 4848,
	  AutoFindPathStartID = 4849,
	  AutoFindPathEndID = 4850,
	  MapLinesReqID = 4851,
	  MapLinesRetID = 4852,
	  ServerMapLoadInfoID = 4853,
	  GameStartID = 4860,
	  GameEndID = 4861,
	  InsFlagNtfID = 4862,
	  InstanceMonDataID = 4863,
	  InstanceDataReqID = 4864,
	  InstanceDataRetID = 4865,
	  LeaveInstanceReqID = 4866,
	  CaptainNewRoomRetSrvID = 4867,
	  RemoveTinyReqSrvID = 4868,
	  TreasureEndNtfID = 4869,
	  MTInstanceDataID = 4870,
	  MapUserDataID = 4871,
	  TeamEnterMapID = 4872,
	  CreateMapI2INtfID = 4873,
	  CreateMapI2IRetID = 4874,
	  GiveUpInstanceReqID = 4875,
	  MotionNtfID = 4880,
	  ServerSetRotNTFID = 4881,
	  ServerSetPosNTFID = 4882,
	  ServerNotifyPosNTFID = 4883,
	  ObstacleInfoNtfID = 4884,
	  DialogNotifyID = 4885,
	  DropTreasureChestID = 4886,
	  TreasureOpeNtfID = 4887,
	  SingleBlackBoardID = 4888,
	  ClientNoticeBevStartID = 4889,
	  Scene2InterSpaceCloseNtfID = 4890,
	  JumpSpaceReqID = 4891,
	  JumpSpaceRetID = 4892,
	  ShopMDID = 5000,
	  ShopLimMDID = 5001,
	  AllLimMDID = 5002,
	  AllLimInfoReqID = 5020,
	  AllLimInfoRetID = 5021,
	  ShopBuyReqID = 5022,
	  ShopBuyRetID = 5023,
	  CurrencyExchangeReqID = 5024,
	  CurrencyExchangeRetID = 5025,
	  BlackBoardNodeID = 5200,
	  ArrayUint64ID = 5201,
	  ArrayInt32ID = 5202,
	  MapInt64ID = 5203,
	  HurtNodeMsgID = 5204,
	  HurtDataID = 5205,
	  CureNodeMsgID = 5206,
	  CureDataID = 5207,
	  ManaNodeMsgID = 5208,
	  ManaDataID = 5209,
	  SkillTarsMsgID = 5210,
	  SkillTarDataID = 5211,
	  PosArrayID = 5212,
	  OffsetNodeMsgID = 5213,
	  OffsetDataID = 5214,
	  UpRotaNodeMsgID = 5215,
	  UpRotaDataID = 5216,
	  BlackHoleNodeMsgID = 5217,
	  PreUserInputDataID = 5218,
	  SkillUseReqID = 5230,
	  PreSkillUseReqID = 5231,
	  PreSkillUseInputReqID = 5232,
	  PreSkillUseInputCancelReqID = 5233,
	  PreSkillCancelReqID = 5234,
	  SkillQuitReqID = 5235,
	  CDUpdateNoticeID = 5250,
	  CDDataID = 5251,
	  RunLineDataID = 5252,
	  RunStageRetID = 5253,
	  RuntimeSyncRetID = 5254,
	  SkillUseRetID = 5255,
	  SkillEndRetID = 5256,
	  BuffCreateRetID = 5257,
	  BuffEndRetID = 5258,
	  BulletCreateRetID = 5259,
	  BulletEndRetID = 5260,
	  PassiveSkillUseRetID = 5261,
	  PassiveSkillEndRetID = 5262,
	  RunStageForceEndRetID = 5263,
	  PreSkillUseInputRetID = 5264,
	  ReadyDeadNtfID = 5265,
	  RoleAllCDListNtfID = 5266,
	  PartnerAllCDListNtfID = 5267,
	  AllPartnerCDListNtfID = 5268,
	  PartnerSwitchEndTimeNtfID = 5269,
	  GetMoveBuffReqID = 5270,
	  GetMoveBuffRetID = 5271,
	  TaskMDID = 5400,
	  RingTaskConMDID = 5401,
	  TaskingModelID = 5405,
	  TaskRingModelID = 5406,
	  TaskBinaryID = 5407,
	  RingTaskInfoMdID = 5408,
	  RingTaskExRewMdID = 5409,
	  LobbyServiceReqID = 5415,
	  SpaceServiceReqID = 5416,
	  LobbyServiceAckID = 5417,
	  BrieflyTaskDataID = 5418,
	  TaskProgressDataID = 5419,
	  TaskPropgressID = 5420,
	  SingleTaskDataID = 5421,
	  TasksID = 5422,
	  NoticeTasksID = 5423,
	  TaskLiveUpdateID = 5424,
	  AcceptTaskReqID = 5425,
	  AcceptTaskAckID = 5426,
	  DropTaskReqID = 5427,
	  DropTaskAckID = 5428,
	  SubmitTaskReqID = 5429,
	  SubmitTaskAckID = 5430,
	  GetTaskRewardReqID = 5431,
	  GetTaskRewardAckID = 5432,
	  TaskLiveOperatorID = 5433,
	  TaskPropgressChangeID = 5434,
	  NoticeTaskDataID = 5435,
	  BatchSubmitTaskReqID = 5436,
	  BatchSubmitTaskAckID = 5437,
	  RingTaskNtfID = 5450,
	  RingTaskExReqID = 5451,
	  RingTaskExRetID = 5452,
	  TaskRewardNtfID = 5453,
	  RTRewardNtfID = 5454,
	  WorldLineRewardReqID = 5455,
	  WorldLineRewardRetID = 5456,
	  RingTaskFinishedNtfID = 5457,
	  ComCountMDID = 5600,
	  UpComCountInfoSrvID = 5620,
	  CheckInMDID = 5700,
	  CheckInCountRewardModelID = 5701,
	  CheckInRewardReqID = 5710,
	  CheckInRewardRetID = 5711,
	  FixCheckInReqID = 5712,
	  FixCheckInRetID = 5713,
	  CheckInCountRewardReqID = 5714,
	  CheckInCountRewardRetID = 5715,
	  PartnerMDID = 5800,
	  PartnerAptitudeMDID = 5801,
	  PartnerAptitudesMDID = 5802,
	  PartnerCDsID = 5803,
	  PartnerBattleReqID = 5804,
	  PartnerBattleRetID = 5805,
	  PartnerAssistReqID = 5806,
	  PartnerAssistRetID = 5807,
	  PartnerFallReqID = 5808,
	  PartnerFallRetID = 5809,
	  AllPartnerListID = 5810,
	  NoticePartnerListID = 5811,
	  PartnerSkillUseReqID = 5812,
	  PartnerConcretizeReqID = 5813,
	  PartnerConcretizeRetID = 5814,
	  PartnerSelectCDReqID = 5815,
	  PartnerSelectCDRetID = 5816,
	  PartnerUpgradeReqID = 5817,
	  PartnerUpgradeRetID = 5818,
	  PartnerChristenReqID = 5819,
	  PartnerChristenRetID = 5820,
	  PartnerChristenSaveReqID = 5821,
	  PartnerChristenSaveRetID = 5822,
	  PartnerLimitBreakReqID = 5823,
	  PartnerLimitBreakRetID = 5824,
	  PartnerUpStarReqID = 5825,
	  PartnerUpStarRetID = 5826,
	  CreatePartnerSrvID = 5827,
	  AskCreatePartnerSrvID = 5828,
	  PartnerCurPropUpdateID = 5829,
	  PartnerEquipUPReqID = 5830,
	  PartnerEquipUPRetID = 5831,
	  PartnerCallStateNtfID = 5832,
	  PartnerSystemMDID = 5833,
	  PartnerTeamCaseModelID = 5834,
	  PartnerUpdateTeamCaseReqID = 5835,
	  PartnerUpdateTeamCaseRetID = 5836,
	  PartnerUpgradeNewReqID = 5837,
	  PartnerUpgradeNewRetID = 5838,
	  TempPartnerCreateNtfID = 5839,
	  AddFirstSpacePartnerNtfID = 5840,
	  DailyActivityMDID = 5900,
	  DailyActivityModelID = 5901,
	  DailyActivityTaskModelID = 5902,
	  DailyActivityTaskNodeModelID = 5903,
	  DailyOnlineTimeAwardFieldID = 5904,
	  DailyActivityRewardReqID = 5910,
	  DailyActivityRewardRetID = 5911,
	  TransTimeNtfID = 5912,
	  WeekActivityMDID = 5913,
	  ActiveValueUpdateNtfID = 5914,
	  SevenDayGoalMDID = 6000,
	  SevenDayRewardModelID = 6001,
	  SevenDayGoalRewardReqID = 6010,
	  SevenDayGoalRewardRetID = 6011,
	  AchieveMDID = 6100,
	  AchieveFinishedModelID = 6101,
	  AchieveRunningModelID = 6102,
	  AchieveRunningDataID = 6103,
	  LobbyGamePlayMDID = 6200,
	  NewbieTargetFieldID = 6220,
	  NewbieGetStageAwardReqID = 6240,
	  NewbieGetStageAwardRetID = 6241,
	  PersonSecretMDID = 6300,
	  PSStateModelID = 6301,
	  PSInstanceDataID = 6302,
	  PSRewardNtfID = 6310,
	  PersonSecretDataRetID = 6311,
	  PersonSecretDayAwardReqID = 6312,
	  PersonSecretDayAwardRetID = 6313,
	  PersonSecretAchieveAwardReqID = 6314,
	  PersonSecretAchieveAwardRetID = 6315,
	  PersonSecretProgressDirtyAchieveRetID = 6316,
	  PersonSecretStateDirtyAchieveRetID = 6317,
	  PersonSecretSeasonDirtyRetID = 6318,
	  EnterPersonSercetReqID = 6330,
	  EnterPersonSercetRetID = 6331,
	  PersonSecretLevelDataRetID = 6332,
	  GameEndPersonSecretRetID = 6333,
	  PersonSecretReviveReqID = 6334,
	  PersonSecretReviveRetID = 6335,
	  PersonSecretRankMDID = 6336,
	  PersonSecretRankReqID = 6337,
	  PersonSecretRankRetID = 6338,
	  PersonSecretLastRankReqID = 6339,
	  PersonSecretLastRankRetID = 6340,
	  PersonSecretLastRewardReqID = 6341,
	  PersonSecretLastRewardRetID = 6342,
	  PersonSecretGMNtfID = 6343,
	  PSGetDailyRewardReqID = 6344,
	  PSGetDailyRewardRetID = 6345,
	  PSPassGameParamsID = 6346,
	  PSResetMonsterReqID = 6347,
	  PSResetMonsterRetID = 6348,
	  AuctionItemMDID = 6350,
	  AuctionSubItemMDID = 6360,
	  AuctionDivMDID = 6361,
	  AuctionItemID = 6362,
	  AuctionStartNtfID = 6370,
	  AuctionReqID = 6371,
	  AuctionItemNtfID = 6372,
	  AuctionGuildReqID = 6373,
	  AuctionGuildRetID = 6374,
	  AuctionBidReqID = 6375,
	  AuctionBidRetID = 6376,
	  AuctionGuildRewardNtfID = 6377,
	  AuctionStarTSrvNtfID = 6378,
	  AuctionBidReqSrvID = 6379,
	  AuctionGuildReqSrvID = 6380,
	  AuctionReqSrvID = 6381,
	  AuctionStartGMNtfID = 6382,
	  TDReviveReqID = 6400,
	  TDReviveRetID = 6401,
	  TDGameStartNtfID = 6402,
	  TDLevelDataNtfID = 6403,
	  TDGameEndNtfID = 6404,
	  TDOpenChallengeReqID = 6405,
	  TDOpenChallengeRetID = 6406,
	  TDEntryOpenNtfID = 6407,
	  TDStartChanllengeReqID = 6408,
	  TDStartChanllengeRetID = 6409,
	  TDMemberJoinReqID = 6410,
	  TDMemberJoinRetID = 6411,
	  TDMemberJoinNtfID = 6412,
	  TDCaptainNewRoomReqSrvID = 6413,
	  TDCaptainStartReqID = 6414,
	  TDCaptainStartRetID = 6415,
	  TDChallangeWindowReqID = 6416,
	  TDChallangeWindowRetID = 6417,
	  TeamUserInfoID = 6500,
	  TeamInfoID = 6501,
	  CreateTeamReqID = 6502,
	  CreateTeamRetID = 6503,
	  JoinTeamReqID = 6504,
	  JoinTeamReqSrvID = 6505,
	  AgreeJoinReqID = 6506,
	  JoinTeamRetID = 6507,
	  SignOutTeamReqID = 6508,
	  SignOutTeamRetID = 6509,
	  DesOutTeamSrvReqID = 6510,
	  GetTeamInfoSrvNtfID = 6511,
	  GetTeamInfoSrvReqID = 6512,
	  GetTeamInfoSrvRetID = 6513,
	  ChangeTeamCaptainReqID = 6514,
	  ChangeTeamCaptainRetID = 6515,
	  TeamInfoSrvNtfID = 6516,
	  InviteJoinTeamReqID = 6517,
	  AgreeJoinTeamReqID = 6518,
	  InviteJoinTeamRetID = 6519,
	  GetTargetTeamInfoReqID = 6520,
	  GetTargetTeamInfoRetID = 6521,
	  TeamSetPlayIDReqID = 6522,
	  TeamSetPlayIDRetID = 6523,
	  GetAllTeamInfoReqID = 6524,
	  GetAllTeamRetID = 6525,
	  RequestCaptainReqID = 6526,
	  RequestCaptainReqSrvID = 6527,
	  AgreeCaptainReqID = 6528,
	  RequestCaptainRetID = 6529,
	  KickOutTeamCapReqID = 6530,
	  KickOutTeamCapRetID = 6531,
	  KickOutTeamRetID = 6532,
	  KickOutTeamNtfID = 6533,
	  UpdateTeamDescriptionReqID = 6534,
	  UpdateTeamDescriptionRetID = 6535,
	  UpTeamInfoSrvID = 6536,
	  TeamInfoAllNtfID = 6537,
	  TDSetNeedRobotNtfID = 6538,
	  TeamReSyncReqID = 6539,
	  TeamReSyncRetID = 6540,
	  PDinfoMDID = 6600,
	  DailyInstanceInfoID = 6601,
	  DailyInstanceDiffcultInfoID = 6602,
	  AllDailyInstanceListID = 6603,
	  DailyInstanceSyncListID = 6604,
	  DailyInstanceEnterReqID = 6605,
	  DailyInstanceEnterRetID = 6606,
	  GameEndPersonDailyRetID = 6607,
	  PDDataID = 6608,
	  TeamDailyPassListModelID = 6609,
	  GuildDonateMDID = 6626,
	  GuildDonateCurGetModelID = 6627,
	  GuildDonateOrderModelID = 6628,
	  GuildDonateOrderID = 6629,
	  GuildDonateSubmitReqID = 6630,
	  GuildDonateSubmitRetID = 6631,
	  RefreshGuildDonateOrderReqID = 6632,
	  RefreshGuildDonateOrderRetID = 6633,
	  GetGuildDonateCountRewardReqID = 6634,
	  GetGuildDonateCountRewardRetID = 6635,
	  WildBossActOpenNtfID = 6650,
	  WildBossCreateNtfID = 6651,
	  WildBossBeHurtNtfID = 6652,
	  WildBossDeadNtfID = 6653,
	  WildBossBoxDropNtfID = 6654,
	  WildBossGiveAwardNtfID = 6655,
	  WildBossHPNtfID = 6656,
	  WildBossHurtReqID = 6657,
	  WildBossHurtRetID = 6658,
	  WildBossNoticeNtfID = 6659,
	  MapBossListReqID = 6660,
	  MapBossListRetID = 6661,
	  MapBossListSrvReqID = 6662,
	  WildBossCreatedNtfID = 6663,
	  WildBossGMNtfID = 6664,
	  WildBossOnDeadNtfID = 6665,
	  PersonTowerMDID = 6700,
	  PersonTowerAwardListID = 6701,
	  PersonTowerLevelReqID = 6706,
	  PersonTowerLevelRetID = 6707,
	  PersonTowerAwardReqID = 6708,
	  PersonTowerAwardRetID = 6709,
	  PersonTowerLobbyModelID = 6710,
	  PersonTowerGameEndNtfID = 6711,
	  SpaceOfflineMDID = 6800,
	  SO_BattleDatasID = 6801,
	  SO_CDDatasID = 6802,
	  GachaMDID = 6850,
	  CardLogMDID = 6851,
	  GachaIDDataID = 6860,
	  GachaListByTypeID = 6861,
	  GachaReqID = 6870,
	  GachaRetID = 6871,
	  SendCardLogSrvID = 6872,
	  GetCardLogReqID = 6873,
	  GetCardLogRetID = 6874,
	  GachaLogNodeID = 6875,
	  LotteryCardListReqID = 6900,
	  LotteryCardListRespID = 6901,
	  LotteryCardID = 6902,
	  RequestPaginationID = 6903,
	  GVEGuildMDID = 7000,
	  GVEBonusID = 7003,
	  GuildHurtListID = 7004,
	  GuildHurtID = 7005,
	  GVEMonSelID = 7006,
	  GuildGVEDataID = 7007,
	  MemberGVEDataID = 7008,
	  GVEBossCreateID = 7010,
	  GVEBossCreateSetID = 7011,
	  GVEBossCreateListID = 7012,
	  GVEDonateReqID = 7013,
	  GVEDonateRetID = 7014,
	  GVEActMsgReqID = 7015,
	  GVEBossHurtSortReqID = 7016,
	  GVEMyGuildDataReqID = 7017,
	  GVEMyGuildDataRetID = 7018,
	  GVEBossBeHurtNtfID = 7020,
	  GVEBossBeDeadNtfID = 7021,
	  GVEBossGuildNtfID = 7022,
	  GVEBossHPNtfID = 7023,
	  GVEStartI2SID = 7024,
	  GVEDonateNtfID = 7025,
	  GVEClientGetNtfID = 7026,
	  GVEBossListNtfID = 7027,
	  GVEBossHurtSortRet1ID = 7028,
	  GVEBossRewardNtfID = 7029,
	  GVEEndNtfID = 7030,
	  GVEGetBounsNtfID = 7031,
	  GVEModelID = 7032,
	  GVEMonsterModelID = 7033,
	  ActgveGuildMDID = 7040,
	  ActgvePlayerMDID = 7041,
	  ActgveBossModelID = 7050,
	  GVEMonNodeID = 7051,
	  GveGuildHurtID = 7052,
	  ActgveStartSrvNtfID = 7060,
	  ActgveEndSrvNtfID = 7061,
	  ActgveCreateBossSrvID = 7062,
	  ActgveUpBossListSrvID = 7063,
	  ActgveBossBeHurtSrvID = 7064,
	  ActgveBossSyncHPSrvID = 7065,
	  ActgveBossHurtRankReqID = 7066,
	  ActgveBossHurtRankRetID = 7067,
	  ActgveBossRewardNtfID = 7068,
	  ActgvePlayerUpIntSrvID = 7080,
	  ActgvePlayerDonateReqID = 7081,
	  ActgvePopWinReqID = 7082,
	  ActgveGetInfoReqID = 7083,
	  ActgveMyGuildDataReqID = 7084,
	  ActgveMyGuildDataRetID = 7085,
	  GNGLobbyInfoID = 7110,
	  GNGUserDataID = 7111,
	  GNGRankNodeID = 7112,
	  GNGActDataModelID = 7113,
	  GNGOpenPlayReqID = 7120,
	  GNGOpenPlayRetID = 7121,
	  GNGChooseBuffReqID = 7122,
	  GNGChooseBuffRetID = 7123,
	  GNGRankReqID = 7124,
	  GNGRankRetID = 7125,
	  GNGPlayStartI2LID = 7140,
	  GNGStartNtfID = 7141,
	  GNGPlayEndNtfID = 7142,
	  GNGRunNtfID = 7143,
	  GNGEndNtfID = 7144,
	  GNGPersonRewardNtfID = 7145,
	  GNGGuildRewardNtfID = 7146,
	  GNGScoreNodeNtfID = 7147,
	  GNGHorseNtfID = 7148,
	  GNGGetActInfoSrvReqID = 7149,
	  GuildInfoMDID = 7200,
	  GuEventMDID = 7201,
	  GuildPlayerMDID = 7202,
	  GuildApplyMDID = 7203,
	  GuildInfoBaseID = 7204,
	  GuildBaseFieldID = 7210,
	  PlayerGuildInfoID = 7211,
	  GuPlayerInfoID = 7212,
	  GuildApplyPlayerInfoID = 7213,
	  GuildDayFieldID = 7214,
	  GuildCreateReqID = 7230,
	  GuildCreateRetID = 7231,
	  GuildMyGetBaseReqID = 7232,
	  GuildMyGetBaseRetID = 7233,
	  GuildMyGetPlayerListReqID = 7234,
	  GuildMyGetPlayerListRetID = 7235,
	  GuildMyGetEventReqID = 7236,
	  GuildMyGetEventRetID = 7237,
	  GuildQuitReqID = 7238,
	  GuildMySrvReqID = 7239,
	  GuildMySrvRetID = 7240,
	  GuildInviteReqID = 7241,
	  GuildInviteNtfID = 7242,
	  GuildInviteApplyReqID = 7243,
	  GuildMyApplyListReqID = 7244,
	  GuildMyApplyListRetID = 7245,
	  GuildMySetApplyReqID = 7246,
	  GuildMyKickReqID = 7247,
	  GuildMyBankChatReqID = 7248,
	  GuildMyUnbankChatReqID = 7249,
	  GuildMySetPosLvReqID = 7250,
	  GuildMyUpNameReqID = 7251,
	  GuildMyUpNameRetID = 7252,
	  GuildMyUpJoinLvReqID = 7253,
	  GuildMyUpAutoReqID = 7254,
	  GuildMyChangeInfoReqID = 7255,
	  AddGuildExpSrvReqID = 7256,
	  GuMgrGetListReqID = 7280,
	  GuMgrGetListRetID = 7281,
	  GuMgrApplyReqID = 7282,
	  GuMgrApplyRetID = 7283,
	  GuMgrCelApplyReqID = 7284,
	  GuMgrCelApplyRetID = 7285,
	  GuCreateTerritorySrvRetID = 7290,
	  GuDestroyTerritorySrvNtfID = 7291,
	  EnterGuildTerritoryReqID = 7292,
	  EnterGuildTerritorySrvReqID = 7293,
	  GuGuildDestroyNtfID = 7294,
	  PlayerLeaveGuildNtfID = 7295,
	  GetPlayerListReqID = 7296,
	  Battle10RegisterMDID = 7300,
	  Battle10V10ModelID = 7301,
	  Battle10V10MatchSpaceModelID = 7302,
	  B10InstanceDataID = 7305,
	  B10PersonModelID = 7306,
	  BFMatchedNtfID = 7320,
	  B10RunDataNtfID = 7321,
	  BFNoticDataNtfID = 7322,
	  BFGameEndNtfID = 7323,
	  B10PlayStartI2LID = 7330,
	  B10PlayStartI2LReqID = 7331,
	  B10PlayStartI2LRetID = 7332,
	  B10RegisterReqID = 7333,
	  B10RegisterRetID = 7334,
	  BFMatchAgreeReqID = 7335,
	  BFCreateMapI2IRetID = 7336,
	  BFGetStatesReqID = 7337,
	  BFGetStatesRetID = 7338,
	  BFTeamModelID = 7339,
	  BFRegisterDataReqID = 7340,
	  BFRegisterDataRetID = 7341,
	  BFGameStartI2IRetID = 7342,
	  ArenaRankDataMDID = 7400,
	  ArenaLogDataMDID = 7401,
	  ArenaRobotInfoID = 7402,
	  ArenaRunInfoID = 7403,
	  ArenaMyMatchRankItemID = 7404,
	  ArenaFightOneLogID = 7405,
	  ArenaGetMyBaseInfoReqID = 7410,
	  ArenaGetMyBaseInfoRetID = 7411,
	  ArenaBuyChallengeTimesReqID = 7414,
	  ArenaBuyChallengeTimesRetID = 7415,
	  ArenaGetFightListReqID = 7416,
	  ArenaGetFightListRetID = 7417,
	  ArenaGetMyFightLogReqID = 7418,
	  ArenaGetMyFightLogRetID = 7419,
	  ArenaChallengeTargetReqID = 7420,
	  ArenaLevelDataID = 7421,
	  ArenaCreateRetID = 7422,
	  ArenaNoticeDataNtfID = 7423,
	  ArenaGameEndNtfID = 7424,
	  ArenaChallengeTargetGameEndNtfID = 7425,
	  ArenaAddFightLogNtfID = 7426,
	  WantTaskMDID = 7500,
	  WTaskPointTarInfoID = 7501,
	  WTaskSpMonTarInfoID = 7502,
	  WTaskSrvTarInfoID = 7503,
	  WTaskTarInfoID = 7504,
	  WTaskAcceptReqID = 7505,
	  WTaskAcceptRetID = 7506,
	  WTaskChallReqID = 7507,
	  WTaskChallRetID = 7508,
	  WTaskStartReqID = 7509,
	  WTaskStartRetID = 7510,
	  WTaskStartSrvReqID = 7511,
	  WTaskCreMonSrvNtfID = 7512,
	  WTaskCreLevSrvReqID = 7513,
	  WTaskCreLevSrvRetID = 7514,
	  WTaskGetAllInfoReqID = 7515,
	  WTaskGetAllInfoSrvReqID = 7516,
	  WTaskGetAllInfoRetID = 7517,
	  WTaskPointNtfID = 7518,
	  WTaskStageNtfID = 7519,
	  WTaskLevSrvNtfID = 7520,
	  WTaskMonRewSrvReqID = 7521,
	  WTaskLevMonHurtID = 7522,
	  WTaskLevMonHurtNtfID = 7523,
	  WTaskLevMonResNtfID = 7524,
	  WTaskLevRemSrvReqID = 7525,
	  WTaskEndNtfID = 7526,
	  WTaskChangeNtfID = 7527,
	  LifeSkillUserDataMDID = 7600,
	  UserSceneLogicDataMDID = 7601,
	  LifeSkillMapMineDataFieldID = 7610,
	  LifeSkillOneMapMineDataID = 7611,
	  LifeSkillOneMineDataID = 7612,
	  LifeSkillFormulationFieldID = 7613,
	  LifeSkillQTEItemInfoID = 7614,
	  MapNpcOrInterShowHideDataFieldID = 7615,
	  MapNpcOrInterHideDataID = 7616,
	  LifeSkillCollectNtfSrvID = 7620,
	  LifeSkillProduceItemReqID = 7621,
	  LifeSkillQTEInfoNtfID = 7622,
	  LifeSkillQTEGameResultReqID = 7623,
	  LifeSkillQueryMapGatherObjInfoReqID = 7624,
	  LifeSkillQueryMapGatherMapInfoRetID = 7625,
	  LifeSkillUnlockFormulationReqID = 7626,
	  FightMDID = 7700,
	  GetFightRewardReqID = 7710,
	  GetFightRewardRetID = 7711,
	  TeamMatePowerUpdateNtfID = 7712,
	  QAPlayStartNtfID = 7800,
	  QAPlayEndNtfID = 7801,
	  QAQestionToUserNtfID = 7802,
	  QAUserSelectNtfID = 7803,
	  QAAnswerToUserNtfID = 7804,
	  QARewardNtfID = 7805,
	  PartyTimeHangUpRewardReqID = 7806,
	  PartyTimeGuildScoreRewardReqID = 7807,
	  PartyTimeGuildScoreSrvReqID = 7808,
	  PartyTimeGuildScoreSrvRetID = 7809,
	  PartyTimeQADataNtfID = 7810,
	  GetPartyTimeDataReqID = 7811,
	  UpdateRankMetaDataSrvReqID = 7910,
	  UpdateRankMetaDataSrvRetID = 7911,
	  UpdateRankAwardStatusSrvNtfID = 7912,
	  GenericPlayerRankItemID = 7921,
	  GetGenericRankDataReqID = 7922,
	  GetGenericRankDataRetID = 7923,
	  GetGenericRankGetAwardReqID = 7924,
	  GetGenericRankGetAwardRetID = 7925,
	  UserSundryMDID = 9000,
	  OfflineUserMDID = 9001,
	  RiskLevelMDID = 9002,
	  GloAuntionMDID = 9003,
	  GloRiskInfoMDID = 9004,
	  GloInfoSrvRetID = 9005,
	  UserSettingReqID = 9006,
	  UserSettingBatchReqID = 9007,
	  UserSettingBatchRetID = 9008,
	  UserSundryNtfID = 9009,
	  UpReplyHpReqID = 9010,
	  TickNtfID = 9011,
	  OfflineUserMDSrvNtfID = 9012,
	  CraftingTreasureMapReqID = 9013,
	  CraftingTreasureMapRetID = 9014,
	  OpenTreasureMapReqID = 9015,
	  OpenTreasureMapRetID = 9016,
	  DigTreasureEndNtfID = 9017,
	  BreakDigTreasureReqID = 9018,
	  RiskLevelUpReqID = 9019,
	  RiskLevelUpRetID = 9020,
	  GloInfoSrvReqID = 9021,
	  ChangeSrvRiskInfoReqID = 9022,
	  UpRiskLevelSrvNtfID = 9023,
	  TreMonIDID = 9024,
	  GuideSignSetReqID = 9025,
	  GuideSignSetRetID = 9026,
	  ServerSignNtfID = 9027,
	  GetUserOfflineEntrysSrvReqID = 9028,
	  GetUserOfflineEntrysSrvRetID = 9029,
	  CreateOfflineRobotSrvReqID = 9030,
	  CreateOfflineRobotSrvRetID = 9031,
	  GetSystemOpenRewardReqID = 9032,
	  GetSystemOpenRewardRetID = 9033,
	  GoodsExchangeReqID = 9034,
	  GoodsExchangeRetID = 9035,
	  PlayerNameplateMDID = 9040,
	  GetOtherPlayerNameplateReqID = 9041,
	  GetOtherPlayerNameplateRetID = 9042,
	  FightPowerModelID = 9043,
	  ArrayItemModelID = 9044,
	  ArrayPartnerModelID = 9045,
	  ArraySlotModelID = 9046,
	  ServerRedPointNtfID = 9050,
	  GlobalGameRecordMDID = 9051,
	  OTPersonMailSendReqID = 10000,
	  OTPersonMailSendRespID = 10001,
	  OTManyMailsSendReqID = 10002,
	  OTManyMailsSendRespID = 10003,
	  OTServerMailSendReqID = 10004,
	  OTServerMailSendRespID = 10005,
	  ManyMailsID = 10006,
	  UserListID = 10007,
	  GetUserMailsReqID = 10008,
	  GetUserMailsRespID = 10009,
	  GetServerMailsReqID = 10010,
	  GetServerMailRespID = 10011,
	  OPTGetUserRoleInfoReqID = 10012,
	  OPTUserRoleBaseInfoID = 10013,
	  OPTGetUserRoleInfoRespID = 10014,
	  OPTUserRoleGetBagItemsReqID = 10015,
	  OPTUserRoleGetBagItemsRespID = 10016,
	  ServerMailGetConditionDataID = 10017,
	  ServerMailsMDID = 10018,
	  ServerMailGetNtfID = 10019,
	  GetServerMailReqID = 10020,
	  MarkServerMailReqID = 10021,
	  SendMailByQTReqID = 10022,
	  OPTUserRoleGetModuleDataReqID = 10023,
	  OPTUserRoleGetModuleDataRespID = 10024,
	  MarkServerMailRespID = 10025,
	  NtfServerMailReqID = 10026,
	  NtfServerMailRespID = 10027,
	  OPTChangeSrvOpenDayID = 10028,
	  SendPersonMailReqID = 10029,
	  SendPersonMailRespID = 10030,
	  GetPersonMailReqID = 10031,
	  GetPersonMailRespID = 10032,
	  PersonalMailsMDID = 10033,
	
}

//  ======= 初始化ProtoMap字典 ====
public class ProtoMap
{
	public Dictionary<int, System.Type> ProtoMapDic = new Dictionary<int, System.Type>();

	public void Init()
	{
		ProtoMapDic.Add(((int)MsgIDEnum.SyncBaseInfoID), typeof(SyncBaseInfo));
		ProtoMapDic.Add(((int)MsgIDEnum.RpcMsgID), typeof(RpcMsg));
		ProtoMapDic.Add(((int)MsgIDEnum.PropBaseSyncListID), typeof(PropBaseSyncList));
		ProtoMapDic.Add(((int)MsgIDEnum.RawMsgID), typeof(RawMsg));
		ProtoMapDic.Add(((int)MsgIDEnum.BattlePropDataID), typeof(BattlePropData));
		ProtoMapDic.Add(((int)MsgIDEnum.MsgRetID), typeof(MsgRet));
		ProtoMapDic.Add(((int)MsgIDEnum.CommonRewardNtfID), typeof(CommonRewardNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.PlayerOffineSrvID), typeof(PlayerOffineSrv));
		ProtoMapDic.Add(((int)MsgIDEnum.PlayerReloadSrvID), typeof(PlayerReloadSrv));
		ProtoMapDic.Add(((int)MsgIDEnum.PlayerLogoutSrvID), typeof(PlayerLogoutSrv));
		ProtoMapDic.Add(((int)MsgIDEnum.Client2ThirdReqID), typeof(Client2ThirdReq));
		ProtoMapDic.Add(((int)MsgIDEnum.Client2ThirdRetID), typeof(Client2ThirdRet));
		ProtoMapDic.Add(((int)MsgIDEnum.Client2LinkSrvID), typeof(Client2LinkSrv));
		ProtoMapDic.Add(((int)MsgIDEnum.Client2CenterReqID), typeof(Client2CenterReq));
		ProtoMapDic.Add(((int)MsgIDEnum.Client2CenterSrvID), typeof(Client2CenterSrv));
		ProtoMapDic.Add(((int)MsgIDEnum.RouteMsgToOtherSrvReqID), typeof(RouteMsgToOtherSrvReq));
		ProtoMapDic.Add(((int)MsgIDEnum.RouteMsgToOtherSrvRetID), typeof(RouteMsgToOtherSrvRet));
		ProtoMapDic.Add(((int)MsgIDEnum.UNUSED_Center2SrvReqID), typeof(UNUSED_Center2SrvReq));
		ProtoMapDic.Add(((int)MsgIDEnum.UNUSED_Center2SrvRetID), typeof(UNUSED_Center2SrvRet));
		ProtoMapDic.Add(((int)MsgIDEnum.Server2LinkSrvID), typeof(Server2LinkSrv));
		ProtoMapDic.Add(((int)MsgIDEnum.Vector3ID), typeof(Vector3));
		ProtoMapDic.Add(((int)MsgIDEnum.ArrayVector3ID), typeof(ArrayVector3));
		ProtoMapDic.Add(((int)MsgIDEnum.RespParamID), typeof(RespParam));
		ProtoMapDic.Add(((int)MsgIDEnum.BaseDataID), typeof(BaseData));
		ProtoMapDic.Add(((int)MsgIDEnum.BaseBinaryID), typeof(BaseBinary));
		ProtoMapDic.Add(((int)MsgIDEnum.BaseItemDataID), typeof(BaseItemData));
		ProtoMapDic.Add(((int)MsgIDEnum.DropSetID), typeof(DropSet));
		ProtoMapDic.Add(((int)MsgIDEnum.WaitMonNodeID), typeof(WaitMonNode));
		ProtoMapDic.Add(((int)MsgIDEnum.UserBaseDataID), typeof(UserBaseData));
		ProtoMapDic.Add(((int)MsgIDEnum.I32WithBoolMapDataID), typeof(I32WithBoolMapData));
		ProtoMapDic.Add(((int)MsgIDEnum.I64WithBoolMapDataID), typeof(I64WithBoolMapData));
		ProtoMapDic.Add(((int)MsgIDEnum.UI64WithI32MapDataID), typeof(UI64WithI32MapData));
		ProtoMapDic.Add(((int)MsgIDEnum.UI64ArrayDataID), typeof(UI64ArrayData));
		ProtoMapDic.Add(((int)MsgIDEnum.I64ArrayDataID), typeof(I64ArrayData));
		ProtoMapDic.Add(((int)MsgIDEnum.I32WithI32MapDataID), typeof(I32WithI32MapData));
		ProtoMapDic.Add(((int)MsgIDEnum.I64WithI64MapDataID), typeof(I64WithI64MapData));
		ProtoMapDic.Add(((int)MsgIDEnum.DBDataModelID), typeof(DBDataModel));
		ProtoMapDic.Add(((int)MsgIDEnum.MapModelID), typeof(MapModel));
		ProtoMapDic.Add(((int)MsgIDEnum.MsgDataID), typeof(MsgData));
		ProtoMapDic.Add(((int)MsgIDEnum.GlobalServerInfoMDID), typeof(GlobalServerInfoMD));
		ProtoMapDic.Add(((int)MsgIDEnum.DBGetPlayerInfoReqID), typeof(DBGetPlayerInfoReq));
		ProtoMapDic.Add(((int)MsgIDEnum.OtherGetPlayerInfoReqID), typeof(OtherGetPlayerInfoReq));
		ProtoMapDic.Add(((int)MsgIDEnum.DBGetPlayerInfoAckID), typeof(DBGetPlayerInfoAck));
		ProtoMapDic.Add(((int)MsgIDEnum.DBGetPlayerInfoEndAckID), typeof(DBGetPlayerInfoEndAck));
		ProtoMapDic.Add(((int)MsgIDEnum.DBUpUserDatasReqID), typeof(DBUpUserDatasReq));
		ProtoMapDic.Add(((int)MsgIDEnum.OtherUpUserDatasRetID), typeof(OtherUpUserDatasRet));
		ProtoMapDic.Add(((int)MsgIDEnum.DBGetPlayerTableReqID), typeof(DBGetPlayerTableReq));
		ProtoMapDic.Add(((int)MsgIDEnum.AreaMDID), typeof(AreaMD));
		ProtoMapDic.Add(((int)MsgIDEnum.GroupMDID), typeof(GroupMD));
		ProtoMapDic.Add(((int)MsgIDEnum.UserMDID), typeof(UserMD));
		ProtoMapDic.Add(((int)MsgIDEnum.PlayerMDID), typeof(PlayerMD));
		ProtoMapDic.Add(((int)MsgIDEnum.WhiteListMDID), typeof(WhiteListMD));
		ProtoMapDic.Add(((int)MsgIDEnum.ClientDeviceInfoID), typeof(ClientDeviceInfo));
		ProtoMapDic.Add(((int)MsgIDEnum.YoukaLoginNewReqID), typeof(YoukaLoginNewReq));
		ProtoMapDic.Add(((int)MsgIDEnum.YoukaLoginReqID), typeof(YoukaLoginReq));
		ProtoMapDic.Add(((int)MsgIDEnum.UserLoginReqID), typeof(UserLoginReq));
		ProtoMapDic.Add(((int)MsgIDEnum.UserLoginRetID), typeof(UserLoginRet));
		ProtoMapDic.Add(((int)MsgIDEnum.UpLevelReqID), typeof(UpLevelReq));
		ProtoMapDic.Add(((int)MsgIDEnum.TransJobNtfID), typeof(TransJobNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.RechargeOrderMDID), typeof(RechargeOrderMD));
		ProtoMapDic.Add(((int)MsgIDEnum.UserPayDataID), typeof(UserPayData));
		ProtoMapDic.Add(((int)MsgIDEnum.UserPayDataMDID), typeof(UserPayDataMD));
		ProtoMapDic.Add(((int)MsgIDEnum.GetRechargeOrderReqID), typeof(GetRechargeOrderReq));
		ProtoMapDic.Add(((int)MsgIDEnum.GetRechargeOrderRetID), typeof(GetRechargeOrderRet));
		ProtoMapDic.Add(((int)MsgIDEnum.PaySuccessNtfID), typeof(PaySuccessNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.RechargeInfoNtfID), typeof(RechargeInfoNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.FirstPaySignInfoNtfID), typeof(FirstPaySignInfoNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.FirstPaySignReqID), typeof(FirstPaySignReq));
		ProtoMapDic.Add(((int)MsgIDEnum.SmallMonthCardSignReqID), typeof(SmallMonthCardSignReq));
		ProtoMapDic.Add(((int)MsgIDEnum.SmallMonthCardInfoNtfID), typeof(SmallMonthCardInfoNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.BattlePassInfoNtfID), typeof(BattlePassInfoNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.BattlePassGetLevelRewardReqID), typeof(BattlePassGetLevelRewardReq));
		ProtoMapDic.Add(((int)MsgIDEnum.BattlePassBuyLevelReqID), typeof(BattlePassBuyLevelReq));
		ProtoMapDic.Add(((int)MsgIDEnum.CommonParamDataID), typeof(CommonParamData));
		ProtoMapDic.Add(((int)MsgIDEnum.NewEventsID), typeof(NewEvents));
		ProtoMapDic.Add(((int)MsgIDEnum.EventNodeID), typeof(EventNode));
		ProtoMapDic.Add(((int)MsgIDEnum.NoticeEventsID), typeof(NoticeEvents));
		ProtoMapDic.Add(((int)MsgIDEnum.NoticeEventID), typeof(NoticeEvent));
		ProtoMapDic.Add(((int)MsgIDEnum.MonsterDataToMsgID), typeof(MonsterDataToMsg));
		ProtoMapDic.Add(((int)MsgIDEnum.BattleSkillDataToMsgID), typeof(BattleSkillDataToMsg));
		ProtoMapDic.Add(((int)MsgIDEnum.ItemDataToMsgID), typeof(ItemDataToMsg));
		ProtoMapDic.Add(((int)MsgIDEnum.MapDataToMsgID), typeof(MapDataToMsg));
		ProtoMapDic.Add(((int)MsgIDEnum.InterDataToMsgID), typeof(InterDataToMsg));
		ProtoMapDic.Add(((int)MsgIDEnum.ControlDataID), typeof(ControlData));
		ProtoMapDic.Add(((int)MsgIDEnum.FriendApplyClientNtfID), typeof(FriendApplyClientNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.SearchUserReqID), typeof(SearchUserReq));
		ProtoMapDic.Add(((int)MsgIDEnum.SearchUserRespID), typeof(SearchUserResp));
		ProtoMapDic.Add(((int)MsgIDEnum.GetUserListReqID), typeof(GetUserListReq));
		ProtoMapDic.Add(((int)MsgIDEnum.GetUserListRespID), typeof(GetUserListResp));
		ProtoMapDic.Add(((int)MsgIDEnum.RecommendUserReqID), typeof(RecommendUserReq));
		ProtoMapDic.Add(((int)MsgIDEnum.RecommendUserRespID), typeof(RecommendUserResp));
		ProtoMapDic.Add(((int)MsgIDEnum.ApplyToAddFriendReqID), typeof(ApplyToAddFriendReq));
		ProtoMapDic.Add(((int)MsgIDEnum.ApplyToAddFriendRespID), typeof(ApplyToAddFriendResp));
		ProtoMapDic.Add(((int)MsgIDEnum.GetPageFriendsApplyToReqID), typeof(GetPageFriendsApplyToReq));
		ProtoMapDic.Add(((int)MsgIDEnum.GetPageFriendsApplyToRespID), typeof(GetPageFriendsApplyToResp));
		ProtoMapDic.Add(((int)MsgIDEnum.IsFriendReqID), typeof(IsFriendReq));
		ProtoMapDic.Add(((int)MsgIDEnum.IsFriendRespID), typeof(IsFriendResp));
		ProtoMapDic.Add(((int)MsgIDEnum.IsBlackReqID), typeof(IsBlackReq));
		ProtoMapDic.Add(((int)MsgIDEnum.IsBlackRespID), typeof(IsBlackResp));
		ProtoMapDic.Add(((int)MsgIDEnum.DeleteFriendReqID), typeof(DeleteFriendReq));
		ProtoMapDic.Add(((int)MsgIDEnum.DeleteFriendRespID), typeof(DeleteFriendResp));
		ProtoMapDic.Add(((int)MsgIDEnum.RespondFriendApplyReqID), typeof(RespondFriendApplyReq));
		ProtoMapDic.Add(((int)MsgIDEnum.RespondFriendApplyRespID), typeof(RespondFriendApplyResp));
		ProtoMapDic.Add(((int)MsgIDEnum.GetDesignatedFriendsReqID), typeof(GetDesignatedFriendsReq));
		ProtoMapDic.Add(((int)MsgIDEnum.GetDesignatedFriendsRespID), typeof(GetDesignatedFriendsResp));
		ProtoMapDic.Add(((int)MsgIDEnum.GetPageFriendsReqID), typeof(GetPageFriendsReq));
		ProtoMapDic.Add(((int)MsgIDEnum.GetPageFriendsRespID), typeof(GetPageFriendsResp));
		ProtoMapDic.Add(((int)MsgIDEnum.GetFriendIDsReqID), typeof(GetFriendIDsReq));
		ProtoMapDic.Add(((int)MsgIDEnum.GetFriendIDsRespID), typeof(GetFriendIDsResp));
		ProtoMapDic.Add(((int)MsgIDEnum.SetFriendRemarkReqID), typeof(SetFriendRemarkReq));
		ProtoMapDic.Add(((int)MsgIDEnum.SetFriendRemarkRespID), typeof(SetFriendRemarkResp));
		ProtoMapDic.Add(((int)MsgIDEnum.ImportFriendReqID), typeof(ImportFriendReq));
		ProtoMapDic.Add(((int)MsgIDEnum.ImportFriendRespID), typeof(ImportFriendResp));
		ProtoMapDic.Add(((int)MsgIDEnum.AddBlackReqID), typeof(AddBlackReq));
		ProtoMapDic.Add(((int)MsgIDEnum.AddBlackRespID), typeof(AddBlackResp));
		ProtoMapDic.Add(((int)MsgIDEnum.RemoveBlackReqID), typeof(RemoveBlackReq));
		ProtoMapDic.Add(((int)MsgIDEnum.RemoveBlackRespID), typeof(RemoveBlackResp));
		ProtoMapDic.Add(((int)MsgIDEnum.GetPageBlacksReqID), typeof(GetPageBlacksReq));
		ProtoMapDic.Add(((int)MsgIDEnum.GetPageBlacksRespID), typeof(GetPageBlacksResp));
		ProtoMapDic.Add(((int)MsgIDEnum.BlackInfoID), typeof(BlackInfo));
		ProtoMapDic.Add(((int)MsgIDEnum.PublicUserInfoID), typeof(PublicUserInfo));
		ProtoMapDic.Add(((int)MsgIDEnum.BaseRespID), typeof(BaseResp));
		ProtoMapDic.Add(((int)MsgIDEnum.RequestQueryID), typeof(RequestQuery));
		ProtoMapDic.Add(((int)MsgIDEnum.UserInfoID), typeof(UserInfo));
		ProtoMapDic.Add(((int)MsgIDEnum.FriendInfoID), typeof(FriendInfo));
		ProtoMapDic.Add(((int)MsgIDEnum.FriendRequestID), typeof(FriendRequest));
		ProtoMapDic.Add(((int)MsgIDEnum.ChatExtraInfosID), typeof(ChatExtraInfos));
		ProtoMapDic.Add(((int)MsgIDEnum.ChatExtraInfoID), typeof(ChatExtraInfo));
		ProtoMapDic.Add(((int)MsgIDEnum.ChatLocationInfoID), typeof(ChatLocationInfo));
		ProtoMapDic.Add(((int)MsgIDEnum.RecruitNtfID), typeof(RecruitNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.JobSrvNtfID), typeof(JobSrvNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.SendChatMsgReqID), typeof(SendChatMsgReq));
		ProtoMapDic.Add(((int)MsgIDEnum.SendChatMsgRetID), typeof(SendChatMsgRet));
		ProtoMapDic.Add(((int)MsgIDEnum.ChatMsgNoticeID), typeof(ChatMsgNotice));
		ProtoMapDic.Add(((int)MsgIDEnum.ChatMsgAllNtfID), typeof(ChatMsgAllNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.ChatMsgNtfByConID), typeof(ChatMsgNtfByCon));
		ProtoMapDic.Add(((int)MsgIDEnum.ChatMsgNoticeSID), typeof(ChatMsgNoticeS));
		ProtoMapDic.Add(((int)MsgIDEnum.RunHorseSrvReqID), typeof(RunHorseSrvReq));
		ProtoMapDic.Add(((int)MsgIDEnum.RunHorseSrvRetID), typeof(RunHorseSrvRet));
		ProtoMapDic.Add(((int)MsgIDEnum.RunHorseNoticeID), typeof(RunHorseNotice));
		ProtoMapDic.Add(((int)MsgIDEnum.AddCurHeroAttrID), typeof(AddCurHeroAttr));
		ProtoMapDic.Add(((int)MsgIDEnum.GmCmdReqID), typeof(GmCmdReq));
		ProtoMapDic.Add(((int)MsgIDEnum.GmCmdAckID), typeof(GmCmdAck));
		ProtoMapDic.Add(((int)MsgIDEnum.ServerInfoUpdateNtfID), typeof(ServerInfoUpdateNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.GCMemoryReqID), typeof(GCMemoryReq));
		ProtoMapDic.Add(((int)MsgIDEnum.SetServerTimeReqID), typeof(SetServerTimeReq));
		ProtoMapDic.Add(((int)MsgIDEnum.ServerGmCmdReqID), typeof(ServerGmCmdReq));
		ProtoMapDic.Add(((int)MsgIDEnum.ServerGmCmdAckID), typeof(ServerGmCmdAck));
		ProtoMapDic.Add(((int)MsgIDEnum.HeroMDID), typeof(HeroMD));
		ProtoMapDic.Add(((int)MsgIDEnum.TransJobRewardListID), typeof(TransJobRewardList));
		ProtoMapDic.Add(((int)MsgIDEnum.HeroJobSkillModelID), typeof(HeroJobSkillModel));
		ProtoMapDic.Add(((int)MsgIDEnum.HeroTalentModelID), typeof(HeroTalentModel));
		ProtoMapDic.Add(((int)MsgIDEnum.HeroTalentTreeID), typeof(HeroTalentTree));
		ProtoMapDic.Add(((int)MsgIDEnum.HeroBaseInfoID), typeof(HeroBaseInfo));
		ProtoMapDic.Add(((int)MsgIDEnum.OfflineBattleSaveID), typeof(OfflineBattleSave));
		ProtoMapDic.Add(((int)MsgIDEnum.SingleSkillInfoID), typeof(SingleSkillInfo));
		ProtoMapDic.Add(((int)MsgIDEnum.SkillPosID), typeof(SkillPos));
		ProtoMapDic.Add(((int)MsgIDEnum.HeroInstanceDataID), typeof(HeroInstanceData));
		ProtoMapDic.Add(((int)MsgIDEnum.OfflinePartnerDataID), typeof(OfflinePartnerData));
		ProtoMapDic.Add(((int)MsgIDEnum.HeroDailyInstanceDataID), typeof(HeroDailyInstanceData));
		ProtoMapDic.Add(((int)MsgIDEnum.HeroPersonSecretDataID), typeof(HeroPersonSecretData));
		ProtoMapDic.Add(((int)MsgIDEnum.HeroTeamDailyDataID), typeof(HeroTeamDailyData));
		ProtoMapDic.Add(((int)MsgIDEnum.HeroSkillCaseDataID), typeof(HeroSkillCaseData));
		ProtoMapDic.Add(((int)MsgIDEnum.LobbyHeroInfoID), typeof(LobbyHeroInfo));
		ProtoMapDic.Add(((int)MsgIDEnum.AllSkillNoticeID), typeof(AllSkillNotice));
		ProtoMapDic.Add(((int)MsgIDEnum.SwitchSkillPosReqID), typeof(SwitchSkillPosReq));
		ProtoMapDic.Add(((int)MsgIDEnum.SwitchSkillPosRetID), typeof(SwitchSkillPosRet));
		ProtoMapDic.Add(((int)MsgIDEnum.SwitchTalentReqID), typeof(SwitchTalentReq));
		ProtoMapDic.Add(((int)MsgIDEnum.SwitchTalentRetID), typeof(SwitchTalentRet));
		ProtoMapDic.Add(((int)MsgIDEnum.UpdateJobSkillReqID), typeof(UpdateJobSkillReq));
		ProtoMapDic.Add(((int)MsgIDEnum.UpdateJobSkillRetID), typeof(UpdateJobSkillRet));
		ProtoMapDic.Add(((int)MsgIDEnum.UpdateJobSkillNoticeID), typeof(UpdateJobSkillNotice));
		ProtoMapDic.Add(((int)MsgIDEnum.UpSkillCaseDataReqID), typeof(UpSkillCaseDataReq));
		ProtoMapDic.Add(((int)MsgIDEnum.UpSkillCaseDataRetID), typeof(UpSkillCaseDataRet));
		ProtoMapDic.Add(((int)MsgIDEnum.GetHeroBattleInfoListID), typeof(GetHeroBattleInfoList));
		ProtoMapDic.Add(((int)MsgIDEnum.HeroBattleInfoID), typeof(HeroBattleInfo));
		ProtoMapDic.Add(((int)MsgIDEnum.SwitchSkillPosSrvReqID), typeof(SwitchSkillPosSrvReq));
		ProtoMapDic.Add(((int)MsgIDEnum.SwitchSkillPosSrvRetID), typeof(SwitchSkillPosSrvRet));
		ProtoMapDic.Add(((int)MsgIDEnum.SkillUpgradeSrvReqID), typeof(SkillUpgradeSrvReq));
		ProtoMapDic.Add(((int)MsgIDEnum.SkillUpgradeSrvRetID), typeof(SkillUpgradeSrvRet));
		ProtoMapDic.Add(((int)MsgIDEnum.NotifyLeveChangeSrvID), typeof(NotifyLeveChangeSrv));
		ProtoMapDic.Add(((int)MsgIDEnum.SwitchTalentNoticeID), typeof(SwitchTalentNotice));
		ProtoMapDic.Add(((int)MsgIDEnum.TransJobReqID), typeof(TransJobReq));
		ProtoMapDic.Add(((int)MsgIDEnum.TransJobRetID), typeof(TransJobRet));
		ProtoMapDic.Add(((int)MsgIDEnum.TalentNodeActiveReqID), typeof(TalentNodeActiveReq));
		ProtoMapDic.Add(((int)MsgIDEnum.TalentNodeActiveRetID), typeof(TalentNodeActiveRet));
		ProtoMapDic.Add(((int)MsgIDEnum.TalentTreeResetReqID), typeof(TalentTreeResetReq));
		ProtoMapDic.Add(((int)MsgIDEnum.TalentTreeResetRetID), typeof(TalentTreeResetRet));
		ProtoMapDic.Add(((int)MsgIDEnum.TalentNodeAutoReqID), typeof(TalentNodeAutoReq));
		ProtoMapDic.Add(((int)MsgIDEnum.TalentNodeAutoRetID), typeof(TalentNodeAutoRet));
		ProtoMapDic.Add(((int)MsgIDEnum.TransJobCondRewardReqID), typeof(TransJobCondRewardReq));
		ProtoMapDic.Add(((int)MsgIDEnum.TransJobCondRewardRetID), typeof(TransJobCondRewardRet));
		ProtoMapDic.Add(((int)MsgIDEnum.OfflineBattleDataID), typeof(OfflineBattleData));
		ProtoMapDic.Add(((int)MsgIDEnum.ResetJobSkillPointReqID), typeof(ResetJobSkillPointReq));
		ProtoMapDic.Add(((int)MsgIDEnum.ResetJobSkillPointRetID), typeof(ResetJobSkillPointRet));
		ProtoMapDic.Add(((int)MsgIDEnum.WorldLevelReqID), typeof(WorldLevelReq));
		ProtoMapDic.Add(((int)MsgIDEnum.WorldLevelRetID), typeof(WorldLevelRet));
		ProtoMapDic.Add(((int)MsgIDEnum.WorldLevelConfirmReportID), typeof(WorldLevelConfirmReport));
		ProtoMapDic.Add(((int)MsgIDEnum.InterEeqID), typeof(InterEeq));
		ProtoMapDic.Add(((int)MsgIDEnum.InterRetID), typeof(InterRet));
		ProtoMapDic.Add(((int)MsgIDEnum.InterStateID), typeof(InterState));
		ProtoMapDic.Add(((int)MsgIDEnum.AllInterStateID), typeof(AllInterState));
		ProtoMapDic.Add(((int)MsgIDEnum.UserCurEffNotifyID), typeof(UserCurEffNotify));
		ProtoMapDic.Add(((int)MsgIDEnum.CurEffOverReqID), typeof(CurEffOverReq));
		ProtoMapDic.Add(((int)MsgIDEnum.EffectDataID), typeof(EffectData));
		ProtoMapDic.Add(((int)MsgIDEnum.BreakInterReqID), typeof(BreakInterReq));
		ProtoMapDic.Add(((int)MsgIDEnum.ItemSpaceMDID), typeof(ItemSpaceMD));
		ProtoMapDic.Add(((int)MsgIDEnum.ItemMDID), typeof(ItemMD));
		ProtoMapDic.Add(((int)MsgIDEnum.EquipSlotMDID), typeof(EquipSlotMD));
		ProtoMapDic.Add(((int)MsgIDEnum.EntityPropEntryMDID), typeof(EntityPropEntryMD));
		ProtoMapDic.Add(((int)MsgIDEnum.EqRecastMDID), typeof(EqRecastMD));
		ProtoMapDic.Add(((int)MsgIDEnum.TweeterMDID), typeof(TweeterMD));
		ProtoMapDic.Add(((int)MsgIDEnum.CommonEntryPropID), typeof(CommonEntryProp));
		ProtoMapDic.Add(((int)MsgIDEnum.EquipPropListID), typeof(EquipPropList));
		ProtoMapDic.Add(((int)MsgIDEnum.DropinfoID), typeof(Dropinfo));
		ProtoMapDic.Add(((int)MsgIDEnum.RewardInfoID), typeof(RewardInfo));
		ProtoMapDic.Add(((int)MsgIDEnum.StorageDrugsMdID), typeof(StorageDrugsMd));
		ProtoMapDic.Add(((int)MsgIDEnum.CombatSkillID), typeof(CombatSkill));
		ProtoMapDic.Add(((int)MsgIDEnum.CombatSkillListID), typeof(CombatSkillList));
		ProtoMapDic.Add(((int)MsgIDEnum.GemSlotID), typeof(GemSlot));
		ProtoMapDic.Add(((int)MsgIDEnum.GemSlotListID), typeof(GemSlotList));
		ProtoMapDic.Add(((int)MsgIDEnum.ExtractaAmuletMDID), typeof(ExtractaAmuletMD));
		ProtoMapDic.Add(((int)MsgIDEnum.RecastModelID), typeof(RecastModel));
		ProtoMapDic.Add(((int)MsgIDEnum.ReEventInfoID), typeof(ReEventInfo));
		ProtoMapDic.Add(((int)MsgIDEnum.ReHistoryModelID), typeof(ReHistoryModel));
		ProtoMapDic.Add(((int)MsgIDEnum.TreasureDataID), typeof(TreasureData));
		ProtoMapDic.Add(((int)MsgIDEnum.CommonEntryPropsModelID), typeof(CommonEntryPropsModel));
		ProtoMapDic.Add(((int)MsgIDEnum.ItemUseReqID), typeof(ItemUseReq));
		ProtoMapDic.Add(((int)MsgIDEnum.ItemUseSrvID), typeof(ItemUseSrv));
		ProtoMapDic.Add(((int)MsgIDEnum.ItemAddDelReqID), typeof(ItemAddDelReq));
		ProtoMapDic.Add(((int)MsgIDEnum.ItemAddDelSrvID), typeof(ItemAddDelSrv));
		ProtoMapDic.Add(((int)MsgIDEnum.DropNotifyID), typeof(DropNotify));
		ProtoMapDic.Add(((int)MsgIDEnum.WearEquipReqID), typeof(WearEquipReq));
		ProtoMapDic.Add(((int)MsgIDEnum.WearEquipRetID), typeof(WearEquipRet));
		ProtoMapDic.Add(((int)MsgIDEnum.TakeOffEquipReqID), typeof(TakeOffEquipReq));
		ProtoMapDic.Add(((int)MsgIDEnum.TakeOffEquipRetID), typeof(TakeOffEquipRet));
		ProtoMapDic.Add(((int)MsgIDEnum.DropListSrvID), typeof(DropListSrv));
		ProtoMapDic.Add(((int)MsgIDEnum.UseItemCDInfoNtfID), typeof(UseItemCDInfoNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.DelItemReqID), typeof(DelItemReq));
		ProtoMapDic.Add(((int)MsgIDEnum.DelItemRetID), typeof(DelItemRet));
		ProtoMapDic.Add(((int)MsgIDEnum.OneUseDrugReqID), typeof(OneUseDrugReq));
		ProtoMapDic.Add(((int)MsgIDEnum.UseDrugSrvReqID), typeof(UseDrugSrvReq));
		ProtoMapDic.Add(((int)MsgIDEnum.UseDrugSrvRetID), typeof(UseDrugSrvRet));
		ProtoMapDic.Add(((int)MsgIDEnum.UpdateStorageDrugsReqID), typeof(UpdateStorageDrugsReq));
		ProtoMapDic.Add(((int)MsgIDEnum.EquipSlotUpgradeReqID), typeof(EquipSlotUpgradeReq));
		ProtoMapDic.Add(((int)MsgIDEnum.EquipSlotUpgradeRetID), typeof(EquipSlotUpgradeRet));
		ProtoMapDic.Add(((int)MsgIDEnum.EquipSlotDataID), typeof(EquipSlotData));
		ProtoMapDic.Add(((int)MsgIDEnum.EquipSlotAllNoticeID), typeof(EquipSlotAllNotice));
		ProtoMapDic.Add(((int)MsgIDEnum.EquipSlotDifferNoticeID), typeof(EquipSlotDifferNotice));
		ProtoMapDic.Add(((int)MsgIDEnum.EquipRefineReqID), typeof(EquipRefineReq));
		ProtoMapDic.Add(((int)MsgIDEnum.EquipRefineRetID), typeof(EquipRefineRet));
		ProtoMapDic.Add(((int)MsgIDEnum.CommonEntityPropChangeSrvID), typeof(CommonEntityPropChangeSrv));
		ProtoMapDic.Add(((int)MsgIDEnum.WearParEquipReqID), typeof(WearParEquipReq));
		ProtoMapDic.Add(((int)MsgIDEnum.WearParEquipRetID), typeof(WearParEquipRet));
		ProtoMapDic.Add(((int)MsgIDEnum.TakeOffParEquipReqID), typeof(TakeOffParEquipReq));
		ProtoMapDic.Add(((int)MsgIDEnum.TakeOffParEquipRetID), typeof(TakeOffParEquipRet));
		ProtoMapDic.Add(((int)MsgIDEnum.GemSynthesizedReqID), typeof(GemSynthesizedReq));
		ProtoMapDic.Add(((int)MsgIDEnum.GemSynthesizedRetID), typeof(GemSynthesizedRet));
		ProtoMapDic.Add(((int)MsgIDEnum.WearAmuletReqID), typeof(WearAmuletReq));
		ProtoMapDic.Add(((int)MsgIDEnum.WearAmuletRetID), typeof(WearAmuletRet));
		ProtoMapDic.Add(((int)MsgIDEnum.TakeOffAmuletReqID), typeof(TakeOffAmuletReq));
		ProtoMapDic.Add(((int)MsgIDEnum.TakeOffAmuletRetID), typeof(TakeOffAmuletRet));
		ProtoMapDic.Add(((int)MsgIDEnum.PolishAmuletReqID), typeof(PolishAmuletReq));
		ProtoMapDic.Add(((int)MsgIDEnum.PolishAmuletRetID), typeof(PolishAmuletRet));
		ProtoMapDic.Add(((int)MsgIDEnum.ExtractAmuletReqID), typeof(ExtractAmuletReq));
		ProtoMapDic.Add(((int)MsgIDEnum.ExtractAmuletRetID), typeof(ExtractAmuletRet));
		ProtoMapDic.Add(((int)MsgIDEnum.ChooseExtractReqID), typeof(ChooseExtractReq));
		ProtoMapDic.Add(((int)MsgIDEnum.ChooseExtractRetID), typeof(ChooseExtractRet));
		ProtoMapDic.Add(((int)MsgIDEnum.RandHeraldryReqID), typeof(RandHeraldryReq));
		ProtoMapDic.Add(((int)MsgIDEnum.RandHeraldryRetID), typeof(RandHeraldryRet));
		ProtoMapDic.Add(((int)MsgIDEnum.HeraldryWearReqID), typeof(HeraldryWearReq));
		ProtoMapDic.Add(((int)MsgIDEnum.HeraldryWearRetID), typeof(HeraldryWearRet));
		ProtoMapDic.Add(((int)MsgIDEnum.HeraldryTakeOffReqID), typeof(HeraldryTakeOffReq));
		ProtoMapDic.Add(((int)MsgIDEnum.HeraldryTakeOffRetID), typeof(HeraldryTakeOffRet));
		ProtoMapDic.Add(((int)MsgIDEnum.ItemResolveReqID), typeof(ItemResolveReq));
		ProtoMapDic.Add(((int)MsgIDEnum.ItemResolveRetID), typeof(ItemResolveRet));
		ProtoMapDic.Add(((int)MsgIDEnum.TempItemListID), typeof(TempItemList));
		ProtoMapDic.Add(((int)MsgIDEnum.EqRecastReqID), typeof(EqRecastReq));
		ProtoMapDic.Add(((int)MsgIDEnum.EqRecastRetID), typeof(EqRecastRet));
		ProtoMapDic.Add(((int)MsgIDEnum.ChooseEqRecastReqID), typeof(ChooseEqRecastReq));
		ProtoMapDic.Add(((int)MsgIDEnum.ChooseEqRecastRetID), typeof(ChooseEqRecastRet));
		ProtoMapDic.Add(((int)MsgIDEnum.OpenBoxReqID), typeof(OpenBoxReq));
		ProtoMapDic.Add(((int)MsgIDEnum.OpenBoxRetID), typeof(OpenBoxRet));
		ProtoMapDic.Add(((int)MsgIDEnum.ReItemSrvID), typeof(ReItemSrv));
		ProtoMapDic.Add(((int)MsgIDEnum.AddItemSrvID), typeof(AddItemSrv));
		ProtoMapDic.Add(((int)MsgIDEnum.WearTweeterReqID), typeof(WearTweeterReq));
		ProtoMapDic.Add(((int)MsgIDEnum.WearTweeterRetID), typeof(WearTweeterRet));
		ProtoMapDic.Add(((int)MsgIDEnum.TakeOffTweeterReqID), typeof(TakeOffTweeterReq));
		ProtoMapDic.Add(((int)MsgIDEnum.TakeOffTweeterRetID), typeof(TakeOffTweeterRet));
		ProtoMapDic.Add(((int)MsgIDEnum.UpTweeterReqID), typeof(UpTweeterReq));
		ProtoMapDic.Add(((int)MsgIDEnum.UpTweeterRetID), typeof(UpTweeterRet));
		ProtoMapDic.Add(((int)MsgIDEnum.SynthesizedItemReqID), typeof(SynthesizedItemReq));
		ProtoMapDic.Add(((int)MsgIDEnum.BoxRareItemBroadNtfID), typeof(BoxRareItemBroadNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.MailMDID), typeof(MailMD));
		ProtoMapDic.Add(((int)MsgIDEnum.MailListNtfID), typeof(MailListNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.MailGetInfosReqID), typeof(MailGetInfosReq));
		ProtoMapDic.Add(((int)MsgIDEnum.MailGetInfosRetID), typeof(MailGetInfosRet));
		ProtoMapDic.Add(((int)MsgIDEnum.MailDeleteReqID), typeof(MailDeleteReq));
		ProtoMapDic.Add(((int)MsgIDEnum.MailDeleteReadReqID), typeof(MailDeleteReadReq));
		ProtoMapDic.Add(((int)MsgIDEnum.MailDeleteRetID), typeof(MailDeleteRet));
		ProtoMapDic.Add(((int)MsgIDEnum.MailGetRewardReqID), typeof(MailGetRewardReq));
		ProtoMapDic.Add(((int)MsgIDEnum.MailGetRewardAllReqID), typeof(MailGetRewardAllReq));
		ProtoMapDic.Add(((int)MsgIDEnum.MailGetRewardRetID), typeof(MailGetRewardRet));
		ProtoMapDic.Add(((int)MsgIDEnum.MailMarkReadedReqID), typeof(MailMarkReadedReq));
		ProtoMapDic.Add(((int)MsgIDEnum.MailsMarkReadedReqID), typeof(MailsMarkReadedReq));
		ProtoMapDic.Add(((int)MsgIDEnum.MailsMarkReadedRetID), typeof(MailsMarkReadedRet));
		ProtoMapDic.Add(((int)MsgIDEnum.MailAttachedContentID), typeof(MailAttachedContent));
		ProtoMapDic.Add(((int)MsgIDEnum.GetMailAttachedContentNtfID), typeof(GetMailAttachedContentNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.GetMailAttachedContentDataID), typeof(GetMailAttachedContentData));
		ProtoMapDic.Add(((int)MsgIDEnum.NewMailReqSrvID), typeof(NewMailReqSrv));
		ProtoMapDic.Add(((int)MsgIDEnum.SendGlobalGMMailReqID), typeof(SendGlobalGMMailReq));
		ProtoMapDic.Add(((int)MsgIDEnum.BankGoodsMDID), typeof(BankGoodsMD));
		ProtoMapDic.Add(((int)MsgIDEnum.BankGoodsEntryMDID), typeof(BankGoodsEntryMD));
		ProtoMapDic.Add(((int)MsgIDEnum.BankItemIDMDID), typeof(BankItemIDMD));
		ProtoMapDic.Add(((int)MsgIDEnum.BankLogMDID), typeof(BankLogMD));
		ProtoMapDic.Add(((int)MsgIDEnum.BankGoodsBaseID), typeof(BankGoodsBase));
		ProtoMapDic.Add(((int)MsgIDEnum.BankLogBaseID), typeof(BankLogBase));
		ProtoMapDic.Add(((int)MsgIDEnum.BankItemBaseID), typeof(BankItemBase));
		ProtoMapDic.Add(((int)MsgIDEnum.BankSubTypeListReqID), typeof(BankSubTypeListReq));
		ProtoMapDic.Add(((int)MsgIDEnum.BankTypeListReqID), typeof(BankTypeListReq));
		ProtoMapDic.Add(((int)MsgIDEnum.BankItemListReqID), typeof(BankItemListReq));
		ProtoMapDic.Add(((int)MsgIDEnum.BankQueryListReqID), typeof(BankQueryListReq));
		ProtoMapDic.Add(((int)MsgIDEnum.BankGoodsListRetID), typeof(BankGoodsListRet));
		ProtoMapDic.Add(((int)MsgIDEnum.BankItemIDListRetID), typeof(BankItemIDListRet));
		ProtoMapDic.Add(((int)MsgIDEnum.BankMyFollowReqID), typeof(BankMyFollowReq));
		ProtoMapDic.Add(((int)MsgIDEnum.BankMyFollowSrvID), typeof(BankMyFollowSrv));
		ProtoMapDic.Add(((int)MsgIDEnum.BankMyFollowRetID), typeof(BankMyFollowRet));
		ProtoMapDic.Add(((int)MsgIDEnum.BankMyGoodsReqID), typeof(BankMyGoodsReq));
		ProtoMapDic.Add(((int)MsgIDEnum.BankMyGoodsRetID), typeof(BankMyGoodsRet));
		ProtoMapDic.Add(((int)MsgIDEnum.BankAddGoodsReqID), typeof(BankAddGoodsReq));
		ProtoMapDic.Add(((int)MsgIDEnum.BankAddGoodsSrvID), typeof(BankAddGoodsSrv));
		ProtoMapDic.Add(((int)MsgIDEnum.BankAddGoodsRetID), typeof(BankAddGoodsRet));
		ProtoMapDic.Add(((int)MsgIDEnum.BankOffShelfGoodsReqID), typeof(BankOffShelfGoodsReq));
		ProtoMapDic.Add(((int)MsgIDEnum.BankGetIncomeReqID), typeof(BankGetIncomeReq));
		ProtoMapDic.Add(((int)MsgIDEnum.BankLogByItemIDReqID), typeof(BankLogByItemIDReq));
		ProtoMapDic.Add(((int)MsgIDEnum.BankLogByItemIDRetID), typeof(BankLogByItemIDRet));
		ProtoMapDic.Add(((int)MsgIDEnum.BankLogByBuyReqID), typeof(BankLogByBuyReq));
		ProtoMapDic.Add(((int)MsgIDEnum.BankLogByBuyRetID), typeof(BankLogByBuyRet));
		ProtoMapDic.Add(((int)MsgIDEnum.BankLogBySellReqID), typeof(BankLogBySellReq));
		ProtoMapDic.Add(((int)MsgIDEnum.BankLogBySellRetID), typeof(BankLogBySellRet));
		ProtoMapDic.Add(((int)MsgIDEnum.BankBuyGoodsReqID), typeof(BankBuyGoodsReq));
		ProtoMapDic.Add(((int)MsgIDEnum.BankBuyGoodsRetID), typeof(BankBuyGoodsRet));
		ProtoMapDic.Add(((int)MsgIDEnum.BankSetMyFollowReqID), typeof(BankSetMyFollowReq));
		ProtoMapDic.Add(((int)MsgIDEnum.BankSetMyFollowSrvID), typeof(BankSetMyFollowSrv));
		ProtoMapDic.Add(((int)MsgIDEnum.BankSetMyFollowRetID), typeof(BankSetMyFollowRet));
		ProtoMapDic.Add(((int)MsgIDEnum.BankSetMyFollowItemIDReqID), typeof(BankSetMyFollowItemIDReq));
		ProtoMapDic.Add(((int)MsgIDEnum.BankSetMyFollowItemIDRetID), typeof(BankSetMyFollowItemIDRet));
		ProtoMapDic.Add(((int)MsgIDEnum.BankGetItemPriceReqID), typeof(BankGetItemPriceReq));
		ProtoMapDic.Add(((int)MsgIDEnum.BankGetItemPriceRetID), typeof(BankGetItemPriceRet));
		ProtoMapDic.Add(((int)MsgIDEnum.BankBuyGoodsMaxReqID), typeof(BankBuyGoodsMaxReq));
		ProtoMapDic.Add(((int)MsgIDEnum.BankPlayerMDID), typeof(BankPlayerMD));
		ProtoMapDic.Add(((int)MsgIDEnum.FollowGoodsFieldID), typeof(FollowGoodsField));
		ProtoMapDic.Add(((int)MsgIDEnum.FollowItemFieldID), typeof(FollowItemField));
		ProtoMapDic.Add(((int)MsgIDEnum.Money2MoneyMDID), typeof(Money2MoneyMD));
		ProtoMapDic.Add(((int)MsgIDEnum.Money2MoneyReqID), typeof(Money2MoneyReq));
		ProtoMapDic.Add(((int)MsgIDEnum.Money2MoneyRetID), typeof(Money2MoneyRet));
		ProtoMapDic.Add(((int)MsgIDEnum.Money2MoneyNtfID), typeof(Money2MoneyNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.Money2MoneyInfoReqID), typeof(Money2MoneyInfoReq));
		ProtoMapDic.Add(((int)MsgIDEnum.Money2MoneyInfoRetID), typeof(Money2MoneyInfoRet));
		ProtoMapDic.Add(((int)MsgIDEnum.TradeBankMaySystemRestockNtfID), typeof(TradeBankMaySystemRestockNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.EnterAOIID), typeof(EnterAOI));
		ProtoMapDic.Add(((int)MsgIDEnum.LeaveAOIID), typeof(LeaveAOI));
		ProtoMapDic.Add(((int)MsgIDEnum.UpdateAOIID), typeof(UpdateAOI));
		ProtoMapDic.Add(((int)MsgIDEnum.AOIMsgID), typeof(AOIMsg));
		ProtoMapDic.Add(((int)MsgIDEnum.PropSyncListID), typeof(PropSyncList));
		ProtoMapDic.Add(((int)MsgIDEnum.RepeatedPropSyncListID), typeof(RepeatedPropSyncList));
		ProtoMapDic.Add(((int)MsgIDEnum.UserMainDataNotifyID), typeof(UserMainDataNotify));
		ProtoMapDic.Add(((int)MsgIDEnum.EnterSceneReqID), typeof(EnterSceneReq));
		ProtoMapDic.Add(((int)MsgIDEnum.MapPreloadNoticeID), typeof(MapPreloadNotice));
		ProtoMapDic.Add(((int)MsgIDEnum.EnterSpaceNtfID), typeof(EnterSpaceNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.LeaveSpaceID), typeof(LeaveSpace));
		ProtoMapDic.Add(((int)MsgIDEnum.MapChangeReqID), typeof(MapChangeReq));
		ProtoMapDic.Add(((int)MsgIDEnum.MapChangeRetID), typeof(MapChangeRet));
		ProtoMapDic.Add(((int)MsgIDEnum.MapLeaveAckID), typeof(MapLeaveAck));
		ProtoMapDic.Add(((int)MsgIDEnum.MapLeaveID), typeof(MapLeave));
		ProtoMapDic.Add(((int)MsgIDEnum.MapEnterAckID), typeof(MapEnterAck));
		ProtoMapDic.Add(((int)MsgIDEnum.MapEnterID), typeof(MapEnter));
		ProtoMapDic.Add(((int)MsgIDEnum.ClientInsReadyReqID), typeof(ClientInsReadyReq));
		ProtoMapDic.Add(((int)MsgIDEnum.SpaceLoadEndNtfID), typeof(SpaceLoadEndNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.ChangeDeadStateID), typeof(ChangeDeadState));
		ProtoMapDic.Add(((int)MsgIDEnum.MoveMsg2ID), typeof(MoveMsg2));
		ProtoMapDic.Add(((int)MsgIDEnum.ServerTimeReqID), typeof(ServerTimeReq));
		ProtoMapDic.Add(((int)MsgIDEnum.ServerTimeRetID), typeof(ServerTimeRet));
		ProtoMapDic.Add(((int)MsgIDEnum.RoleReviveReqID), typeof(RoleReviveReq));
		ProtoMapDic.Add(((int)MsgIDEnum.RoleReviveRetID), typeof(RoleReviveRet));
		ProtoMapDic.Add(((int)MsgIDEnum.PropPanelReqID), typeof(PropPanelReq));
		ProtoMapDic.Add(((int)MsgIDEnum.PropPanelRetID), typeof(PropPanelRet));
		ProtoMapDic.Add(((int)MsgIDEnum.AutoFindPathReqID), typeof(AutoFindPathReq));
		ProtoMapDic.Add(((int)MsgIDEnum.AutoFindPathStartID), typeof(AutoFindPathStart));
		ProtoMapDic.Add(((int)MsgIDEnum.AutoFindPathEndID), typeof(AutoFindPathEnd));
		ProtoMapDic.Add(((int)MsgIDEnum.MapLinesReqID), typeof(MapLinesReq));
		ProtoMapDic.Add(((int)MsgIDEnum.MapLinesRetID), typeof(MapLinesRet));
		ProtoMapDic.Add(((int)MsgIDEnum.ServerMapLoadInfoID), typeof(ServerMapLoadInfo));
		ProtoMapDic.Add(((int)MsgIDEnum.GameStartID), typeof(GameStart));
		ProtoMapDic.Add(((int)MsgIDEnum.GameEndID), typeof(GameEnd));
		ProtoMapDic.Add(((int)MsgIDEnum.InsFlagNtfID), typeof(InsFlagNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.InstanceMonDataID), typeof(InstanceMonData));
		ProtoMapDic.Add(((int)MsgIDEnum.InstanceDataReqID), typeof(InstanceDataReq));
		ProtoMapDic.Add(((int)MsgIDEnum.InstanceDataRetID), typeof(InstanceDataRet));
		ProtoMapDic.Add(((int)MsgIDEnum.LeaveInstanceReqID), typeof(LeaveInstanceReq));
		ProtoMapDic.Add(((int)MsgIDEnum.CaptainNewRoomRetSrvID), typeof(CaptainNewRoomRetSrv));
		ProtoMapDic.Add(((int)MsgIDEnum.RemoveTinyReqSrvID), typeof(RemoveTinyReqSrv));
		ProtoMapDic.Add(((int)MsgIDEnum.TreasureEndNtfID), typeof(TreasureEndNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.MTInstanceDataID), typeof(MTInstanceData));
		ProtoMapDic.Add(((int)MsgIDEnum.MapUserDataID), typeof(MapUserData));
		ProtoMapDic.Add(((int)MsgIDEnum.TeamEnterMapID), typeof(TeamEnterMap));
		ProtoMapDic.Add(((int)MsgIDEnum.CreateMapI2INtfID), typeof(CreateMapI2INtf));
		ProtoMapDic.Add(((int)MsgIDEnum.CreateMapI2IRetID), typeof(CreateMapI2IRet));
		ProtoMapDic.Add(((int)MsgIDEnum.GiveUpInstanceReqID), typeof(GiveUpInstanceReq));
		ProtoMapDic.Add(((int)MsgIDEnum.MotionNtfID), typeof(MotionNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.ServerSetRotNTFID), typeof(ServerSetRotNTF));
		ProtoMapDic.Add(((int)MsgIDEnum.ServerSetPosNTFID), typeof(ServerSetPosNTF));
		ProtoMapDic.Add(((int)MsgIDEnum.ServerNotifyPosNTFID), typeof(ServerNotifyPosNTF));
		ProtoMapDic.Add(((int)MsgIDEnum.ObstacleInfoNtfID), typeof(ObstacleInfoNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.DialogNotifyID), typeof(DialogNotify));
		ProtoMapDic.Add(((int)MsgIDEnum.DropTreasureChestID), typeof(DropTreasureChest));
		ProtoMapDic.Add(((int)MsgIDEnum.TreasureOpeNtfID), typeof(TreasureOpeNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.SingleBlackBoardID), typeof(SingleBlackBoard));
		ProtoMapDic.Add(((int)MsgIDEnum.ClientNoticeBevStartID), typeof(ClientNoticeBevStart));
		ProtoMapDic.Add(((int)MsgIDEnum.Scene2InterSpaceCloseNtfID), typeof(Scene2InterSpaceCloseNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.JumpSpaceReqID), typeof(JumpSpaceReq));
		ProtoMapDic.Add(((int)MsgIDEnum.JumpSpaceRetID), typeof(JumpSpaceRet));
		ProtoMapDic.Add(((int)MsgIDEnum.ShopMDID), typeof(ShopMD));
		ProtoMapDic.Add(((int)MsgIDEnum.ShopLimMDID), typeof(ShopLimMD));
		ProtoMapDic.Add(((int)MsgIDEnum.AllLimMDID), typeof(AllLimMD));
		ProtoMapDic.Add(((int)MsgIDEnum.AllLimInfoReqID), typeof(AllLimInfoReq));
		ProtoMapDic.Add(((int)MsgIDEnum.AllLimInfoRetID), typeof(AllLimInfoRet));
		ProtoMapDic.Add(((int)MsgIDEnum.ShopBuyReqID), typeof(ShopBuyReq));
		ProtoMapDic.Add(((int)MsgIDEnum.ShopBuyRetID), typeof(ShopBuyRet));
		ProtoMapDic.Add(((int)MsgIDEnum.CurrencyExchangeReqID), typeof(CurrencyExchangeReq));
		ProtoMapDic.Add(((int)MsgIDEnum.CurrencyExchangeRetID), typeof(CurrencyExchangeRet));
		ProtoMapDic.Add(((int)MsgIDEnum.BlackBoardNodeID), typeof(BlackBoardNode));
		ProtoMapDic.Add(((int)MsgIDEnum.ArrayUint64ID), typeof(ArrayUint64));
		ProtoMapDic.Add(((int)MsgIDEnum.ArrayInt32ID), typeof(ArrayInt32));
		ProtoMapDic.Add(((int)MsgIDEnum.MapInt64ID), typeof(MapInt64));
		ProtoMapDic.Add(((int)MsgIDEnum.HurtNodeMsgID), typeof(HurtNodeMsg));
		ProtoMapDic.Add(((int)MsgIDEnum.HurtDataID), typeof(HurtData));
		ProtoMapDic.Add(((int)MsgIDEnum.CureNodeMsgID), typeof(CureNodeMsg));
		ProtoMapDic.Add(((int)MsgIDEnum.CureDataID), typeof(CureData));
		ProtoMapDic.Add(((int)MsgIDEnum.ManaNodeMsgID), typeof(ManaNodeMsg));
		ProtoMapDic.Add(((int)MsgIDEnum.ManaDataID), typeof(ManaData));
		ProtoMapDic.Add(((int)MsgIDEnum.SkillTarsMsgID), typeof(SkillTarsMsg));
		ProtoMapDic.Add(((int)MsgIDEnum.SkillTarDataID), typeof(SkillTarData));
		ProtoMapDic.Add(((int)MsgIDEnum.PosArrayID), typeof(PosArray));
		ProtoMapDic.Add(((int)MsgIDEnum.OffsetNodeMsgID), typeof(OffsetNodeMsg));
		ProtoMapDic.Add(((int)MsgIDEnum.OffsetDataID), typeof(OffsetData));
		ProtoMapDic.Add(((int)MsgIDEnum.UpRotaNodeMsgID), typeof(UpRotaNodeMsg));
		ProtoMapDic.Add(((int)MsgIDEnum.UpRotaDataID), typeof(UpRotaData));
		ProtoMapDic.Add(((int)MsgIDEnum.BlackHoleNodeMsgID), typeof(BlackHoleNodeMsg));
		ProtoMapDic.Add(((int)MsgIDEnum.PreUserInputDataID), typeof(PreUserInputData));
		ProtoMapDic.Add(((int)MsgIDEnum.SkillUseReqID), typeof(SkillUseReq));
		ProtoMapDic.Add(((int)MsgIDEnum.PreSkillUseReqID), typeof(PreSkillUseReq));
		ProtoMapDic.Add(((int)MsgIDEnum.PreSkillUseInputReqID), typeof(PreSkillUseInputReq));
		ProtoMapDic.Add(((int)MsgIDEnum.PreSkillUseInputCancelReqID), typeof(PreSkillUseInputCancelReq));
		ProtoMapDic.Add(((int)MsgIDEnum.PreSkillCancelReqID), typeof(PreSkillCancelReq));
		ProtoMapDic.Add(((int)MsgIDEnum.SkillQuitReqID), typeof(SkillQuitReq));
		ProtoMapDic.Add(((int)MsgIDEnum.CDUpdateNoticeID), typeof(CDUpdateNotice));
		ProtoMapDic.Add(((int)MsgIDEnum.CDDataID), typeof(CDData));
		ProtoMapDic.Add(((int)MsgIDEnum.RunLineDataID), typeof(RunLineData));
		ProtoMapDic.Add(((int)MsgIDEnum.RunStageRetID), typeof(RunStageRet));
		ProtoMapDic.Add(((int)MsgIDEnum.RuntimeSyncRetID), typeof(RuntimeSyncRet));
		ProtoMapDic.Add(((int)MsgIDEnum.SkillUseRetID), typeof(SkillUseRet));
		ProtoMapDic.Add(((int)MsgIDEnum.SkillEndRetID), typeof(SkillEndRet));
		ProtoMapDic.Add(((int)MsgIDEnum.BuffCreateRetID), typeof(BuffCreateRet));
		ProtoMapDic.Add(((int)MsgIDEnum.BuffEndRetID), typeof(BuffEndRet));
		ProtoMapDic.Add(((int)MsgIDEnum.BulletCreateRetID), typeof(BulletCreateRet));
		ProtoMapDic.Add(((int)MsgIDEnum.BulletEndRetID), typeof(BulletEndRet));
		ProtoMapDic.Add(((int)MsgIDEnum.PassiveSkillUseRetID), typeof(PassiveSkillUseRet));
		ProtoMapDic.Add(((int)MsgIDEnum.PassiveSkillEndRetID), typeof(PassiveSkillEndRet));
		ProtoMapDic.Add(((int)MsgIDEnum.RunStageForceEndRetID), typeof(RunStageForceEndRet));
		ProtoMapDic.Add(((int)MsgIDEnum.PreSkillUseInputRetID), typeof(PreSkillUseInputRet));
		ProtoMapDic.Add(((int)MsgIDEnum.ReadyDeadNtfID), typeof(ReadyDeadNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.RoleAllCDListNtfID), typeof(RoleAllCDListNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.PartnerAllCDListNtfID), typeof(PartnerAllCDListNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.AllPartnerCDListNtfID), typeof(AllPartnerCDListNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.PartnerSwitchEndTimeNtfID), typeof(PartnerSwitchEndTimeNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.GetMoveBuffReqID), typeof(GetMoveBuffReq));
		ProtoMapDic.Add(((int)MsgIDEnum.GetMoveBuffRetID), typeof(GetMoveBuffRet));
		ProtoMapDic.Add(((int)MsgIDEnum.TaskMDID), typeof(TaskMD));
		ProtoMapDic.Add(((int)MsgIDEnum.RingTaskConMDID), typeof(RingTaskConMD));
		ProtoMapDic.Add(((int)MsgIDEnum.TaskingModelID), typeof(TaskingModel));
		ProtoMapDic.Add(((int)MsgIDEnum.TaskRingModelID), typeof(TaskRingModel));
		ProtoMapDic.Add(((int)MsgIDEnum.TaskBinaryID), typeof(TaskBinary));
		ProtoMapDic.Add(((int)MsgIDEnum.RingTaskInfoMdID), typeof(RingTaskInfoMd));
		ProtoMapDic.Add(((int)MsgIDEnum.RingTaskExRewMdID), typeof(RingTaskExRewMd));
		ProtoMapDic.Add(((int)MsgIDEnum.LobbyServiceReqID), typeof(LobbyServiceReq));
		ProtoMapDic.Add(((int)MsgIDEnum.SpaceServiceReqID), typeof(SpaceServiceReq));
		ProtoMapDic.Add(((int)MsgIDEnum.LobbyServiceAckID), typeof(LobbyServiceAck));
		ProtoMapDic.Add(((int)MsgIDEnum.BrieflyTaskDataID), typeof(BrieflyTaskData));
		ProtoMapDic.Add(((int)MsgIDEnum.TaskProgressDataID), typeof(TaskProgressData));
		ProtoMapDic.Add(((int)MsgIDEnum.TaskPropgressID), typeof(TaskPropgress));
		ProtoMapDic.Add(((int)MsgIDEnum.SingleTaskDataID), typeof(SingleTaskData));
		ProtoMapDic.Add(((int)MsgIDEnum.TasksID), typeof(Tasks));
		ProtoMapDic.Add(((int)MsgIDEnum.NoticeTasksID), typeof(NoticeTasks));
		ProtoMapDic.Add(((int)MsgIDEnum.TaskLiveUpdateID), typeof(TaskLiveUpdate));
		ProtoMapDic.Add(((int)MsgIDEnum.AcceptTaskReqID), typeof(AcceptTaskReq));
		ProtoMapDic.Add(((int)MsgIDEnum.AcceptTaskAckID), typeof(AcceptTaskAck));
		ProtoMapDic.Add(((int)MsgIDEnum.DropTaskReqID), typeof(DropTaskReq));
		ProtoMapDic.Add(((int)MsgIDEnum.DropTaskAckID), typeof(DropTaskAck));
		ProtoMapDic.Add(((int)MsgIDEnum.SubmitTaskReqID), typeof(SubmitTaskReq));
		ProtoMapDic.Add(((int)MsgIDEnum.SubmitTaskAckID), typeof(SubmitTaskAck));
		ProtoMapDic.Add(((int)MsgIDEnum.GetTaskRewardReqID), typeof(GetTaskRewardReq));
		ProtoMapDic.Add(((int)MsgIDEnum.GetTaskRewardAckID), typeof(GetTaskRewardAck));
		ProtoMapDic.Add(((int)MsgIDEnum.TaskLiveOperatorID), typeof(TaskLiveOperator));
		ProtoMapDic.Add(((int)MsgIDEnum.TaskPropgressChangeID), typeof(TaskPropgressChange));
		ProtoMapDic.Add(((int)MsgIDEnum.NoticeTaskDataID), typeof(NoticeTaskData));
		ProtoMapDic.Add(((int)MsgIDEnum.BatchSubmitTaskReqID), typeof(BatchSubmitTaskReq));
		ProtoMapDic.Add(((int)MsgIDEnum.BatchSubmitTaskAckID), typeof(BatchSubmitTaskAck));
		ProtoMapDic.Add(((int)MsgIDEnum.RingTaskNtfID), typeof(RingTaskNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.RingTaskExReqID), typeof(RingTaskExReq));
		ProtoMapDic.Add(((int)MsgIDEnum.RingTaskExRetID), typeof(RingTaskExRet));
		ProtoMapDic.Add(((int)MsgIDEnum.TaskRewardNtfID), typeof(TaskRewardNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.RTRewardNtfID), typeof(RTRewardNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.WorldLineRewardReqID), typeof(WorldLineRewardReq));
		ProtoMapDic.Add(((int)MsgIDEnum.WorldLineRewardRetID), typeof(WorldLineRewardRet));
		ProtoMapDic.Add(((int)MsgIDEnum.RingTaskFinishedNtfID), typeof(RingTaskFinishedNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.ComCountMDID), typeof(ComCountMD));
		ProtoMapDic.Add(((int)MsgIDEnum.UpComCountInfoSrvID), typeof(UpComCountInfoSrv));
		ProtoMapDic.Add(((int)MsgIDEnum.CheckInMDID), typeof(CheckInMD));
		ProtoMapDic.Add(((int)MsgIDEnum.CheckInCountRewardModelID), typeof(CheckInCountRewardModel));
		ProtoMapDic.Add(((int)MsgIDEnum.CheckInRewardReqID), typeof(CheckInRewardReq));
		ProtoMapDic.Add(((int)MsgIDEnum.CheckInRewardRetID), typeof(CheckInRewardRet));
		ProtoMapDic.Add(((int)MsgIDEnum.FixCheckInReqID), typeof(FixCheckInReq));
		ProtoMapDic.Add(((int)MsgIDEnum.FixCheckInRetID), typeof(FixCheckInRet));
		ProtoMapDic.Add(((int)MsgIDEnum.CheckInCountRewardReqID), typeof(CheckInCountRewardReq));
		ProtoMapDic.Add(((int)MsgIDEnum.CheckInCountRewardRetID), typeof(CheckInCountRewardRet));
		ProtoMapDic.Add(((int)MsgIDEnum.PartnerMDID), typeof(PartnerMD));
		ProtoMapDic.Add(((int)MsgIDEnum.PartnerAptitudeMDID), typeof(PartnerAptitudeMD));
		ProtoMapDic.Add(((int)MsgIDEnum.PartnerAptitudesMDID), typeof(PartnerAptitudesMD));
		ProtoMapDic.Add(((int)MsgIDEnum.PartnerCDsID), typeof(PartnerCDs));
		ProtoMapDic.Add(((int)MsgIDEnum.PartnerBattleReqID), typeof(PartnerBattleReq));
		ProtoMapDic.Add(((int)MsgIDEnum.PartnerBattleRetID), typeof(PartnerBattleRet));
		ProtoMapDic.Add(((int)MsgIDEnum.PartnerAssistReqID), typeof(PartnerAssistReq));
		ProtoMapDic.Add(((int)MsgIDEnum.PartnerAssistRetID), typeof(PartnerAssistRet));
		ProtoMapDic.Add(((int)MsgIDEnum.PartnerFallReqID), typeof(PartnerFallReq));
		ProtoMapDic.Add(((int)MsgIDEnum.PartnerFallRetID), typeof(PartnerFallRet));
		ProtoMapDic.Add(((int)MsgIDEnum.AllPartnerListID), typeof(AllPartnerList));
		ProtoMapDic.Add(((int)MsgIDEnum.NoticePartnerListID), typeof(NoticePartnerList));
		ProtoMapDic.Add(((int)MsgIDEnum.PartnerSkillUseReqID), typeof(PartnerSkillUseReq));
		ProtoMapDic.Add(((int)MsgIDEnum.PartnerConcretizeReqID), typeof(PartnerConcretizeReq));
		ProtoMapDic.Add(((int)MsgIDEnum.PartnerConcretizeRetID), typeof(PartnerConcretizeRet));
		ProtoMapDic.Add(((int)MsgIDEnum.PartnerSelectCDReqID), typeof(PartnerSelectCDReq));
		ProtoMapDic.Add(((int)MsgIDEnum.PartnerSelectCDRetID), typeof(PartnerSelectCDRet));
		ProtoMapDic.Add(((int)MsgIDEnum.PartnerUpgradeReqID), typeof(PartnerUpgradeReq));
		ProtoMapDic.Add(((int)MsgIDEnum.PartnerUpgradeRetID), typeof(PartnerUpgradeRet));
		ProtoMapDic.Add(((int)MsgIDEnum.PartnerChristenReqID), typeof(PartnerChristenReq));
		ProtoMapDic.Add(((int)MsgIDEnum.PartnerChristenRetID), typeof(PartnerChristenRet));
		ProtoMapDic.Add(((int)MsgIDEnum.PartnerChristenSaveReqID), typeof(PartnerChristenSaveReq));
		ProtoMapDic.Add(((int)MsgIDEnum.PartnerChristenSaveRetID), typeof(PartnerChristenSaveRet));
		ProtoMapDic.Add(((int)MsgIDEnum.PartnerLimitBreakReqID), typeof(PartnerLimitBreakReq));
		ProtoMapDic.Add(((int)MsgIDEnum.PartnerLimitBreakRetID), typeof(PartnerLimitBreakRet));
		ProtoMapDic.Add(((int)MsgIDEnum.PartnerUpStarReqID), typeof(PartnerUpStarReq));
		ProtoMapDic.Add(((int)MsgIDEnum.PartnerUpStarRetID), typeof(PartnerUpStarRet));
		ProtoMapDic.Add(((int)MsgIDEnum.CreatePartnerSrvID), typeof(CreatePartnerSrv));
		ProtoMapDic.Add(((int)MsgIDEnum.AskCreatePartnerSrvID), typeof(AskCreatePartnerSrv));
		ProtoMapDic.Add(((int)MsgIDEnum.PartnerCurPropUpdateID), typeof(PartnerCurPropUpdate));
		ProtoMapDic.Add(((int)MsgIDEnum.PartnerEquipUPReqID), typeof(PartnerEquipUPReq));
		ProtoMapDic.Add(((int)MsgIDEnum.PartnerEquipUPRetID), typeof(PartnerEquipUPRet));
		ProtoMapDic.Add(((int)MsgIDEnum.PartnerCallStateNtfID), typeof(PartnerCallStateNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.PartnerSystemMDID), typeof(PartnerSystemMD));
		ProtoMapDic.Add(((int)MsgIDEnum.PartnerTeamCaseModelID), typeof(PartnerTeamCaseModel));
		ProtoMapDic.Add(((int)MsgIDEnum.PartnerUpdateTeamCaseReqID), typeof(PartnerUpdateTeamCaseReq));
		ProtoMapDic.Add(((int)MsgIDEnum.PartnerUpdateTeamCaseRetID), typeof(PartnerUpdateTeamCaseRet));
		ProtoMapDic.Add(((int)MsgIDEnum.PartnerUpgradeNewReqID), typeof(PartnerUpgradeNewReq));
		ProtoMapDic.Add(((int)MsgIDEnum.PartnerUpgradeNewRetID), typeof(PartnerUpgradeNewRet));
		ProtoMapDic.Add(((int)MsgIDEnum.TempPartnerCreateNtfID), typeof(TempPartnerCreateNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.AddFirstSpacePartnerNtfID), typeof(AddFirstSpacePartnerNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.DailyActivityMDID), typeof(DailyActivityMD));
		ProtoMapDic.Add(((int)MsgIDEnum.DailyActivityModelID), typeof(DailyActivityModel));
		ProtoMapDic.Add(((int)MsgIDEnum.DailyActivityTaskModelID), typeof(DailyActivityTaskModel));
		ProtoMapDic.Add(((int)MsgIDEnum.DailyActivityTaskNodeModelID), typeof(DailyActivityTaskNodeModel));
		ProtoMapDic.Add(((int)MsgIDEnum.DailyOnlineTimeAwardFieldID), typeof(DailyOnlineTimeAwardField));
		ProtoMapDic.Add(((int)MsgIDEnum.DailyActivityRewardReqID), typeof(DailyActivityRewardReq));
		ProtoMapDic.Add(((int)MsgIDEnum.DailyActivityRewardRetID), typeof(DailyActivityRewardRet));
		ProtoMapDic.Add(((int)MsgIDEnum.TransTimeNtfID), typeof(TransTimeNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.WeekActivityMDID), typeof(WeekActivityMD));
		ProtoMapDic.Add(((int)MsgIDEnum.ActiveValueUpdateNtfID), typeof(ActiveValueUpdateNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.SevenDayGoalMDID), typeof(SevenDayGoalMD));
		ProtoMapDic.Add(((int)MsgIDEnum.SevenDayRewardModelID), typeof(SevenDayRewardModel));
		ProtoMapDic.Add(((int)MsgIDEnum.SevenDayGoalRewardReqID), typeof(SevenDayGoalRewardReq));
		ProtoMapDic.Add(((int)MsgIDEnum.SevenDayGoalRewardRetID), typeof(SevenDayGoalRewardRet));
		ProtoMapDic.Add(((int)MsgIDEnum.AchieveMDID), typeof(AchieveMD));
		ProtoMapDic.Add(((int)MsgIDEnum.AchieveFinishedModelID), typeof(AchieveFinishedModel));
		ProtoMapDic.Add(((int)MsgIDEnum.AchieveRunningModelID), typeof(AchieveRunningModel));
		ProtoMapDic.Add(((int)MsgIDEnum.AchieveRunningDataID), typeof(AchieveRunningData));
		ProtoMapDic.Add(((int)MsgIDEnum.LobbyGamePlayMDID), typeof(LobbyGamePlayMD));
		ProtoMapDic.Add(((int)MsgIDEnum.NewbieTargetFieldID), typeof(NewbieTargetField));
		ProtoMapDic.Add(((int)MsgIDEnum.NewbieGetStageAwardReqID), typeof(NewbieGetStageAwardReq));
		ProtoMapDic.Add(((int)MsgIDEnum.NewbieGetStageAwardRetID), typeof(NewbieGetStageAwardRet));
		ProtoMapDic.Add(((int)MsgIDEnum.PersonSecretMDID), typeof(PersonSecretMD));
		ProtoMapDic.Add(((int)MsgIDEnum.PSStateModelID), typeof(PSStateModel));
		ProtoMapDic.Add(((int)MsgIDEnum.PSInstanceDataID), typeof(PSInstanceData));
		ProtoMapDic.Add(((int)MsgIDEnum.PSRewardNtfID), typeof(PSRewardNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.PersonSecretDataRetID), typeof(PersonSecretDataRet));
		ProtoMapDic.Add(((int)MsgIDEnum.PersonSecretDayAwardReqID), typeof(PersonSecretDayAwardReq));
		ProtoMapDic.Add(((int)MsgIDEnum.PersonSecretDayAwardRetID), typeof(PersonSecretDayAwardRet));
		ProtoMapDic.Add(((int)MsgIDEnum.PersonSecretAchieveAwardReqID), typeof(PersonSecretAchieveAwardReq));
		ProtoMapDic.Add(((int)MsgIDEnum.PersonSecretAchieveAwardRetID), typeof(PersonSecretAchieveAwardRet));
		ProtoMapDic.Add(((int)MsgIDEnum.PersonSecretProgressDirtyAchieveRetID), typeof(PersonSecretProgressDirtyAchieveRet));
		ProtoMapDic.Add(((int)MsgIDEnum.PersonSecretStateDirtyAchieveRetID), typeof(PersonSecretStateDirtyAchieveRet));
		ProtoMapDic.Add(((int)MsgIDEnum.PersonSecretSeasonDirtyRetID), typeof(PersonSecretSeasonDirtyRet));
		ProtoMapDic.Add(((int)MsgIDEnum.EnterPersonSercetReqID), typeof(EnterPersonSercetReq));
		ProtoMapDic.Add(((int)MsgIDEnum.EnterPersonSercetRetID), typeof(EnterPersonSercetRet));
		ProtoMapDic.Add(((int)MsgIDEnum.PersonSecretLevelDataRetID), typeof(PersonSecretLevelDataRet));
		ProtoMapDic.Add(((int)MsgIDEnum.GameEndPersonSecretRetID), typeof(GameEndPersonSecretRet));
		ProtoMapDic.Add(((int)MsgIDEnum.PersonSecretReviveReqID), typeof(PersonSecretReviveReq));
		ProtoMapDic.Add(((int)MsgIDEnum.PersonSecretReviveRetID), typeof(PersonSecretReviveRet));
		ProtoMapDic.Add(((int)MsgIDEnum.PersonSecretRankMDID), typeof(PersonSecretRankMD));
		ProtoMapDic.Add(((int)MsgIDEnum.PersonSecretRankReqID), typeof(PersonSecretRankReq));
		ProtoMapDic.Add(((int)MsgIDEnum.PersonSecretRankRetID), typeof(PersonSecretRankRet));
		ProtoMapDic.Add(((int)MsgIDEnum.PersonSecretLastRankReqID), typeof(PersonSecretLastRankReq));
		ProtoMapDic.Add(((int)MsgIDEnum.PersonSecretLastRankRetID), typeof(PersonSecretLastRankRet));
		ProtoMapDic.Add(((int)MsgIDEnum.PersonSecretLastRewardReqID), typeof(PersonSecretLastRewardReq));
		ProtoMapDic.Add(((int)MsgIDEnum.PersonSecretLastRewardRetID), typeof(PersonSecretLastRewardRet));
		ProtoMapDic.Add(((int)MsgIDEnum.PersonSecretGMNtfID), typeof(PersonSecretGMNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.PSGetDailyRewardReqID), typeof(PSGetDailyRewardReq));
		ProtoMapDic.Add(((int)MsgIDEnum.PSGetDailyRewardRetID), typeof(PSGetDailyRewardRet));
		ProtoMapDic.Add(((int)MsgIDEnum.PSPassGameParamsID), typeof(PSPassGameParams));
		ProtoMapDic.Add(((int)MsgIDEnum.PSResetMonsterReqID), typeof(PSResetMonsterReq));
		ProtoMapDic.Add(((int)MsgIDEnum.PSResetMonsterRetID), typeof(PSResetMonsterRet));
		ProtoMapDic.Add(((int)MsgIDEnum.AuctionItemMDID), typeof(AuctionItemMD));
		ProtoMapDic.Add(((int)MsgIDEnum.AuctionSubItemMDID), typeof(AuctionSubItemMD));
		ProtoMapDic.Add(((int)MsgIDEnum.AuctionDivMDID), typeof(AuctionDivMD));
		ProtoMapDic.Add(((int)MsgIDEnum.AuctionItemID), typeof(AuctionItem));
		ProtoMapDic.Add(((int)MsgIDEnum.AuctionStartNtfID), typeof(AuctionStartNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.AuctionReqID), typeof(AuctionReq));
		ProtoMapDic.Add(((int)MsgIDEnum.AuctionItemNtfID), typeof(AuctionItemNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.AuctionGuildReqID), typeof(AuctionGuildReq));
		ProtoMapDic.Add(((int)MsgIDEnum.AuctionGuildRetID), typeof(AuctionGuildRet));
		ProtoMapDic.Add(((int)MsgIDEnum.AuctionBidReqID), typeof(AuctionBidReq));
		ProtoMapDic.Add(((int)MsgIDEnum.AuctionBidRetID), typeof(AuctionBidRet));
		ProtoMapDic.Add(((int)MsgIDEnum.AuctionGuildRewardNtfID), typeof(AuctionGuildRewardNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.AuctionStarTSrvNtfID), typeof(AuctionStarTSrvNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.AuctionBidReqSrvID), typeof(AuctionBidReqSrv));
		ProtoMapDic.Add(((int)MsgIDEnum.AuctionGuildReqSrvID), typeof(AuctionGuildReqSrv));
		ProtoMapDic.Add(((int)MsgIDEnum.AuctionReqSrvID), typeof(AuctionReqSrv));
		ProtoMapDic.Add(((int)MsgIDEnum.AuctionStartGMNtfID), typeof(AuctionStartGMNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.TDReviveReqID), typeof(TDReviveReq));
		ProtoMapDic.Add(((int)MsgIDEnum.TDReviveRetID), typeof(TDReviveRet));
		ProtoMapDic.Add(((int)MsgIDEnum.TDGameStartNtfID), typeof(TDGameStartNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.TDLevelDataNtfID), typeof(TDLevelDataNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.TDGameEndNtfID), typeof(TDGameEndNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.TDOpenChallengeReqID), typeof(TDOpenChallengeReq));
		ProtoMapDic.Add(((int)MsgIDEnum.TDOpenChallengeRetID), typeof(TDOpenChallengeRet));
		ProtoMapDic.Add(((int)MsgIDEnum.TDEntryOpenNtfID), typeof(TDEntryOpenNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.TDStartChanllengeReqID), typeof(TDStartChanllengeReq));
		ProtoMapDic.Add(((int)MsgIDEnum.TDStartChanllengeRetID), typeof(TDStartChanllengeRet));
		ProtoMapDic.Add(((int)MsgIDEnum.TDMemberJoinReqID), typeof(TDMemberJoinReq));
		ProtoMapDic.Add(((int)MsgIDEnum.TDMemberJoinRetID), typeof(TDMemberJoinRet));
		ProtoMapDic.Add(((int)MsgIDEnum.TDMemberJoinNtfID), typeof(TDMemberJoinNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.TDCaptainNewRoomReqSrvID), typeof(TDCaptainNewRoomReqSrv));
		ProtoMapDic.Add(((int)MsgIDEnum.TDCaptainStartReqID), typeof(TDCaptainStartReq));
		ProtoMapDic.Add(((int)MsgIDEnum.TDCaptainStartRetID), typeof(TDCaptainStartRet));
		ProtoMapDic.Add(((int)MsgIDEnum.TDChallangeWindowReqID), typeof(TDChallangeWindowReq));
		ProtoMapDic.Add(((int)MsgIDEnum.TDChallangeWindowRetID), typeof(TDChallangeWindowRet));
		ProtoMapDic.Add(((int)MsgIDEnum.TeamUserInfoID), typeof(TeamUserInfo));
		ProtoMapDic.Add(((int)MsgIDEnum.TeamInfoID), typeof(TeamInfo));
		ProtoMapDic.Add(((int)MsgIDEnum.CreateTeamReqID), typeof(CreateTeamReq));
		ProtoMapDic.Add(((int)MsgIDEnum.CreateTeamRetID), typeof(CreateTeamRet));
		ProtoMapDic.Add(((int)MsgIDEnum.JoinTeamReqID), typeof(JoinTeamReq));
		ProtoMapDic.Add(((int)MsgIDEnum.JoinTeamReqSrvID), typeof(JoinTeamReqSrv));
		ProtoMapDic.Add(((int)MsgIDEnum.AgreeJoinReqID), typeof(AgreeJoinReq));
		ProtoMapDic.Add(((int)MsgIDEnum.JoinTeamRetID), typeof(JoinTeamRet));
		ProtoMapDic.Add(((int)MsgIDEnum.SignOutTeamReqID), typeof(SignOutTeamReq));
		ProtoMapDic.Add(((int)MsgIDEnum.SignOutTeamRetID), typeof(SignOutTeamRet));
		ProtoMapDic.Add(((int)MsgIDEnum.DesOutTeamSrvReqID), typeof(DesOutTeamSrvReq));
		ProtoMapDic.Add(((int)MsgIDEnum.GetTeamInfoSrvNtfID), typeof(GetTeamInfoSrvNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.GetTeamInfoSrvReqID), typeof(GetTeamInfoSrvReq));
		ProtoMapDic.Add(((int)MsgIDEnum.GetTeamInfoSrvRetID), typeof(GetTeamInfoSrvRet));
		ProtoMapDic.Add(((int)MsgIDEnum.ChangeTeamCaptainReqID), typeof(ChangeTeamCaptainReq));
		ProtoMapDic.Add(((int)MsgIDEnum.ChangeTeamCaptainRetID), typeof(ChangeTeamCaptainRet));
		ProtoMapDic.Add(((int)MsgIDEnum.TeamInfoSrvNtfID), typeof(TeamInfoSrvNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.InviteJoinTeamReqID), typeof(InviteJoinTeamReq));
		ProtoMapDic.Add(((int)MsgIDEnum.AgreeJoinTeamReqID), typeof(AgreeJoinTeamReq));
		ProtoMapDic.Add(((int)MsgIDEnum.InviteJoinTeamRetID), typeof(InviteJoinTeamRet));
		ProtoMapDic.Add(((int)MsgIDEnum.GetTargetTeamInfoReqID), typeof(GetTargetTeamInfoReq));
		ProtoMapDic.Add(((int)MsgIDEnum.GetTargetTeamInfoRetID), typeof(GetTargetTeamInfoRet));
		ProtoMapDic.Add(((int)MsgIDEnum.TeamSetPlayIDReqID), typeof(TeamSetPlayIDReq));
		ProtoMapDic.Add(((int)MsgIDEnum.TeamSetPlayIDRetID), typeof(TeamSetPlayIDRet));
		ProtoMapDic.Add(((int)MsgIDEnum.GetAllTeamInfoReqID), typeof(GetAllTeamInfoReq));
		ProtoMapDic.Add(((int)MsgIDEnum.GetAllTeamRetID), typeof(GetAllTeamRet));
		ProtoMapDic.Add(((int)MsgIDEnum.RequestCaptainReqID), typeof(RequestCaptainReq));
		ProtoMapDic.Add(((int)MsgIDEnum.RequestCaptainReqSrvID), typeof(RequestCaptainReqSrv));
		ProtoMapDic.Add(((int)MsgIDEnum.AgreeCaptainReqID), typeof(AgreeCaptainReq));
		ProtoMapDic.Add(((int)MsgIDEnum.RequestCaptainRetID), typeof(RequestCaptainRet));
		ProtoMapDic.Add(((int)MsgIDEnum.KickOutTeamCapReqID), typeof(KickOutTeamCapReq));
		ProtoMapDic.Add(((int)MsgIDEnum.KickOutTeamCapRetID), typeof(KickOutTeamCapRet));
		ProtoMapDic.Add(((int)MsgIDEnum.KickOutTeamRetID), typeof(KickOutTeamRet));
		ProtoMapDic.Add(((int)MsgIDEnum.KickOutTeamNtfID), typeof(KickOutTeamNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.UpdateTeamDescriptionReqID), typeof(UpdateTeamDescriptionReq));
		ProtoMapDic.Add(((int)MsgIDEnum.UpdateTeamDescriptionRetID), typeof(UpdateTeamDescriptionRet));
		ProtoMapDic.Add(((int)MsgIDEnum.UpTeamInfoSrvID), typeof(UpTeamInfoSrv));
		ProtoMapDic.Add(((int)MsgIDEnum.TeamInfoAllNtfID), typeof(TeamInfoAllNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.TDSetNeedRobotNtfID), typeof(TDSetNeedRobotNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.TeamReSyncReqID), typeof(TeamReSyncReq));
		ProtoMapDic.Add(((int)MsgIDEnum.TeamReSyncRetID), typeof(TeamReSyncRet));
		ProtoMapDic.Add(((int)MsgIDEnum.PDinfoMDID), typeof(PDinfoMD));
		ProtoMapDic.Add(((int)MsgIDEnum.DailyInstanceInfoID), typeof(DailyInstanceInfo));
		ProtoMapDic.Add(((int)MsgIDEnum.DailyInstanceDiffcultInfoID), typeof(DailyInstanceDiffcultInfo));
		ProtoMapDic.Add(((int)MsgIDEnum.AllDailyInstanceListID), typeof(AllDailyInstanceList));
		ProtoMapDic.Add(((int)MsgIDEnum.DailyInstanceSyncListID), typeof(DailyInstanceSyncList));
		ProtoMapDic.Add(((int)MsgIDEnum.DailyInstanceEnterReqID), typeof(DailyInstanceEnterReq));
		ProtoMapDic.Add(((int)MsgIDEnum.DailyInstanceEnterRetID), typeof(DailyInstanceEnterRet));
		ProtoMapDic.Add(((int)MsgIDEnum.GameEndPersonDailyRetID), typeof(GameEndPersonDailyRet));
		ProtoMapDic.Add(((int)MsgIDEnum.PDDataID), typeof(PDData));
		ProtoMapDic.Add(((int)MsgIDEnum.TeamDailyPassListModelID), typeof(TeamDailyPassListModel));
		ProtoMapDic.Add(((int)MsgIDEnum.GuildDonateMDID), typeof(GuildDonateMD));
		ProtoMapDic.Add(((int)MsgIDEnum.GuildDonateCurGetModelID), typeof(GuildDonateCurGetModel));
		ProtoMapDic.Add(((int)MsgIDEnum.GuildDonateOrderModelID), typeof(GuildDonateOrderModel));
		ProtoMapDic.Add(((int)MsgIDEnum.GuildDonateOrderID), typeof(GuildDonateOrder));
		ProtoMapDic.Add(((int)MsgIDEnum.GuildDonateSubmitReqID), typeof(GuildDonateSubmitReq));
		ProtoMapDic.Add(((int)MsgIDEnum.GuildDonateSubmitRetID), typeof(GuildDonateSubmitRet));
		ProtoMapDic.Add(((int)MsgIDEnum.RefreshGuildDonateOrderReqID), typeof(RefreshGuildDonateOrderReq));
		ProtoMapDic.Add(((int)MsgIDEnum.RefreshGuildDonateOrderRetID), typeof(RefreshGuildDonateOrderRet));
		ProtoMapDic.Add(((int)MsgIDEnum.GetGuildDonateCountRewardReqID), typeof(GetGuildDonateCountRewardReq));
		ProtoMapDic.Add(((int)MsgIDEnum.GetGuildDonateCountRewardRetID), typeof(GetGuildDonateCountRewardRet));
		ProtoMapDic.Add(((int)MsgIDEnum.WildBossActOpenNtfID), typeof(WildBossActOpenNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.WildBossCreateNtfID), typeof(WildBossCreateNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.WildBossBeHurtNtfID), typeof(WildBossBeHurtNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.WildBossDeadNtfID), typeof(WildBossDeadNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.WildBossBoxDropNtfID), typeof(WildBossBoxDropNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.WildBossGiveAwardNtfID), typeof(WildBossGiveAwardNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.WildBossHPNtfID), typeof(WildBossHPNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.WildBossHurtReqID), typeof(WildBossHurtReq));
		ProtoMapDic.Add(((int)MsgIDEnum.WildBossHurtRetID), typeof(WildBossHurtRet));
		ProtoMapDic.Add(((int)MsgIDEnum.WildBossNoticeNtfID), typeof(WildBossNoticeNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.MapBossListReqID), typeof(MapBossListReq));
		ProtoMapDic.Add(((int)MsgIDEnum.MapBossListRetID), typeof(MapBossListRet));
		ProtoMapDic.Add(((int)MsgIDEnum.MapBossListSrvReqID), typeof(MapBossListSrvReq));
		ProtoMapDic.Add(((int)MsgIDEnum.WildBossCreatedNtfID), typeof(WildBossCreatedNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.WildBossGMNtfID), typeof(WildBossGMNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.WildBossOnDeadNtfID), typeof(WildBossOnDeadNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.PersonTowerMDID), typeof(PersonTowerMD));
		ProtoMapDic.Add(((int)MsgIDEnum.PersonTowerAwardListID), typeof(PersonTowerAwardList));
		ProtoMapDic.Add(((int)MsgIDEnum.PersonTowerLevelReqID), typeof(PersonTowerLevelReq));
		ProtoMapDic.Add(((int)MsgIDEnum.PersonTowerLevelRetID), typeof(PersonTowerLevelRet));
		ProtoMapDic.Add(((int)MsgIDEnum.PersonTowerAwardReqID), typeof(PersonTowerAwardReq));
		ProtoMapDic.Add(((int)MsgIDEnum.PersonTowerAwardRetID), typeof(PersonTowerAwardRet));
		ProtoMapDic.Add(((int)MsgIDEnum.PersonTowerLobbyModelID), typeof(PersonTowerLobbyModel));
		ProtoMapDic.Add(((int)MsgIDEnum.PersonTowerGameEndNtfID), typeof(PersonTowerGameEndNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.SpaceOfflineMDID), typeof(SpaceOfflineMD));
		ProtoMapDic.Add(((int)MsgIDEnum.SO_BattleDatasID), typeof(SO_BattleDatas));
		ProtoMapDic.Add(((int)MsgIDEnum.SO_CDDatasID), typeof(SO_CDDatas));
		ProtoMapDic.Add(((int)MsgIDEnum.GachaMDID), typeof(GachaMD));
		ProtoMapDic.Add(((int)MsgIDEnum.CardLogMDID), typeof(CardLogMD));
		ProtoMapDic.Add(((int)MsgIDEnum.GachaIDDataID), typeof(GachaIDData));
		ProtoMapDic.Add(((int)MsgIDEnum.GachaListByTypeID), typeof(GachaListByType));
		ProtoMapDic.Add(((int)MsgIDEnum.GachaReqID), typeof(GachaReq));
		ProtoMapDic.Add(((int)MsgIDEnum.GachaRetID), typeof(GachaRet));
		ProtoMapDic.Add(((int)MsgIDEnum.SendCardLogSrvID), typeof(SendCardLogSrv));
		ProtoMapDic.Add(((int)MsgIDEnum.GetCardLogReqID), typeof(GetCardLogReq));
		ProtoMapDic.Add(((int)MsgIDEnum.GetCardLogRetID), typeof(GetCardLogRet));
		ProtoMapDic.Add(((int)MsgIDEnum.GachaLogNodeID), typeof(GachaLogNode));
		ProtoMapDic.Add(((int)MsgIDEnum.LotteryCardListReqID), typeof(LotteryCardListReq));
		ProtoMapDic.Add(((int)MsgIDEnum.LotteryCardListRespID), typeof(LotteryCardListResp));
		ProtoMapDic.Add(((int)MsgIDEnum.LotteryCardID), typeof(LotteryCard));
		ProtoMapDic.Add(((int)MsgIDEnum.RequestPaginationID), typeof(RequestPagination));
		ProtoMapDic.Add(((int)MsgIDEnum.GVEGuildMDID), typeof(GVEGuildMD));
		ProtoMapDic.Add(((int)MsgIDEnum.GVEBonusID), typeof(GVEBonus));
		ProtoMapDic.Add(((int)MsgIDEnum.GuildHurtListID), typeof(GuildHurtList));
		ProtoMapDic.Add(((int)MsgIDEnum.GuildHurtID), typeof(GuildHurt));
		ProtoMapDic.Add(((int)MsgIDEnum.GVEMonSelID), typeof(GVEMonSel));
		ProtoMapDic.Add(((int)MsgIDEnum.GuildGVEDataID), typeof(GuildGVEData));
		ProtoMapDic.Add(((int)MsgIDEnum.MemberGVEDataID), typeof(MemberGVEData));
		ProtoMapDic.Add(((int)MsgIDEnum.GVEBossCreateID), typeof(GVEBossCreate));
		ProtoMapDic.Add(((int)MsgIDEnum.GVEBossCreateSetID), typeof(GVEBossCreateSet));
		ProtoMapDic.Add(((int)MsgIDEnum.GVEBossCreateListID), typeof(GVEBossCreateList));
		ProtoMapDic.Add(((int)MsgIDEnum.GVEDonateReqID), typeof(GVEDonateReq));
		ProtoMapDic.Add(((int)MsgIDEnum.GVEDonateRetID), typeof(GVEDonateRet));
		ProtoMapDic.Add(((int)MsgIDEnum.GVEActMsgReqID), typeof(GVEActMsgReq));
		ProtoMapDic.Add(((int)MsgIDEnum.GVEBossHurtSortReqID), typeof(GVEBossHurtSortReq));
		ProtoMapDic.Add(((int)MsgIDEnum.GVEMyGuildDataReqID), typeof(GVEMyGuildDataReq));
		ProtoMapDic.Add(((int)MsgIDEnum.GVEMyGuildDataRetID), typeof(GVEMyGuildDataRet));
		ProtoMapDic.Add(((int)MsgIDEnum.GVEBossBeHurtNtfID), typeof(GVEBossBeHurtNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.GVEBossBeDeadNtfID), typeof(GVEBossBeDeadNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.GVEBossGuildNtfID), typeof(GVEBossGuildNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.GVEBossHPNtfID), typeof(GVEBossHPNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.GVEStartI2SID), typeof(GVEStartI2S));
		ProtoMapDic.Add(((int)MsgIDEnum.GVEDonateNtfID), typeof(GVEDonateNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.GVEClientGetNtfID), typeof(GVEClientGetNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.GVEBossListNtfID), typeof(GVEBossListNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.GVEBossHurtSortRet1ID), typeof(GVEBossHurtSortRet1));
		ProtoMapDic.Add(((int)MsgIDEnum.GVEBossRewardNtfID), typeof(GVEBossRewardNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.GVEEndNtfID), typeof(GVEEndNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.GVEGetBounsNtfID), typeof(GVEGetBounsNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.GVEModelID), typeof(GVEModel));
		ProtoMapDic.Add(((int)MsgIDEnum.GVEMonsterModelID), typeof(GVEMonsterModel));
		ProtoMapDic.Add(((int)MsgIDEnum.ActgveGuildMDID), typeof(ActgveGuildMD));
		ProtoMapDic.Add(((int)MsgIDEnum.ActgvePlayerMDID), typeof(ActgvePlayerMD));
		ProtoMapDic.Add(((int)MsgIDEnum.ActgveBossModelID), typeof(ActgveBossModel));
		ProtoMapDic.Add(((int)MsgIDEnum.GVEMonNodeID), typeof(GVEMonNode));
		ProtoMapDic.Add(((int)MsgIDEnum.GveGuildHurtID), typeof(GveGuildHurt));
		ProtoMapDic.Add(((int)MsgIDEnum.ActgveStartSrvNtfID), typeof(ActgveStartSrvNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.ActgveEndSrvNtfID), typeof(ActgveEndSrvNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.ActgveCreateBossSrvID), typeof(ActgveCreateBossSrv));
		ProtoMapDic.Add(((int)MsgIDEnum.ActgveUpBossListSrvID), typeof(ActgveUpBossListSrv));
		ProtoMapDic.Add(((int)MsgIDEnum.ActgveBossBeHurtSrvID), typeof(ActgveBossBeHurtSrv));
		ProtoMapDic.Add(((int)MsgIDEnum.ActgveBossSyncHPSrvID), typeof(ActgveBossSyncHPSrv));
		ProtoMapDic.Add(((int)MsgIDEnum.ActgveBossHurtRankReqID), typeof(ActgveBossHurtRankReq));
		ProtoMapDic.Add(((int)MsgIDEnum.ActgveBossHurtRankRetID), typeof(ActgveBossHurtRankRet));
		ProtoMapDic.Add(((int)MsgIDEnum.ActgveBossRewardNtfID), typeof(ActgveBossRewardNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.ActgvePlayerUpIntSrvID), typeof(ActgvePlayerUpIntSrv));
		ProtoMapDic.Add(((int)MsgIDEnum.ActgvePlayerDonateReqID), typeof(ActgvePlayerDonateReq));
		ProtoMapDic.Add(((int)MsgIDEnum.ActgvePopWinReqID), typeof(ActgvePopWinReq));
		ProtoMapDic.Add(((int)MsgIDEnum.ActgveGetInfoReqID), typeof(ActgveGetInfoReq));
		ProtoMapDic.Add(((int)MsgIDEnum.ActgveMyGuildDataReqID), typeof(ActgveMyGuildDataReq));
		ProtoMapDic.Add(((int)MsgIDEnum.ActgveMyGuildDataRetID), typeof(ActgveMyGuildDataRet));
		ProtoMapDic.Add(((int)MsgIDEnum.GNGLobbyInfoID), typeof(GNGLobbyInfo));
		ProtoMapDic.Add(((int)MsgIDEnum.GNGUserDataID), typeof(GNGUserData));
		ProtoMapDic.Add(((int)MsgIDEnum.GNGRankNodeID), typeof(GNGRankNode));
		ProtoMapDic.Add(((int)MsgIDEnum.GNGActDataModelID), typeof(GNGActDataModel));
		ProtoMapDic.Add(((int)MsgIDEnum.GNGOpenPlayReqID), typeof(GNGOpenPlayReq));
		ProtoMapDic.Add(((int)MsgIDEnum.GNGOpenPlayRetID), typeof(GNGOpenPlayRet));
		ProtoMapDic.Add(((int)MsgIDEnum.GNGChooseBuffReqID), typeof(GNGChooseBuffReq));
		ProtoMapDic.Add(((int)MsgIDEnum.GNGChooseBuffRetID), typeof(GNGChooseBuffRet));
		ProtoMapDic.Add(((int)MsgIDEnum.GNGRankReqID), typeof(GNGRankReq));
		ProtoMapDic.Add(((int)MsgIDEnum.GNGRankRetID), typeof(GNGRankRet));
		ProtoMapDic.Add(((int)MsgIDEnum.GNGPlayStartI2LID), typeof(GNGPlayStartI2L));
		ProtoMapDic.Add(((int)MsgIDEnum.GNGStartNtfID), typeof(GNGStartNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.GNGPlayEndNtfID), typeof(GNGPlayEndNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.GNGRunNtfID), typeof(GNGRunNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.GNGEndNtfID), typeof(GNGEndNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.GNGPersonRewardNtfID), typeof(GNGPersonRewardNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.GNGGuildRewardNtfID), typeof(GNGGuildRewardNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.GNGScoreNodeNtfID), typeof(GNGScoreNodeNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.GNGHorseNtfID), typeof(GNGHorseNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.GNGGetActInfoSrvReqID), typeof(GNGGetActInfoSrvReq));
		ProtoMapDic.Add(((int)MsgIDEnum.GuildInfoMDID), typeof(GuildInfoMD));
		ProtoMapDic.Add(((int)MsgIDEnum.GuEventMDID), typeof(GuEventMD));
		ProtoMapDic.Add(((int)MsgIDEnum.GuildPlayerMDID), typeof(GuildPlayerMD));
		ProtoMapDic.Add(((int)MsgIDEnum.GuildApplyMDID), typeof(GuildApplyMD));
		ProtoMapDic.Add(((int)MsgIDEnum.GuildInfoBaseID), typeof(GuildInfoBase));
		ProtoMapDic.Add(((int)MsgIDEnum.GuildBaseFieldID), typeof(GuildBaseField));
		ProtoMapDic.Add(((int)MsgIDEnum.PlayerGuildInfoID), typeof(PlayerGuildInfo));
		ProtoMapDic.Add(((int)MsgIDEnum.GuPlayerInfoID), typeof(GuPlayerInfo));
		ProtoMapDic.Add(((int)MsgIDEnum.GuildApplyPlayerInfoID), typeof(GuildApplyPlayerInfo));
		ProtoMapDic.Add(((int)MsgIDEnum.GuildDayFieldID), typeof(GuildDayField));
		ProtoMapDic.Add(((int)MsgIDEnum.GuildCreateReqID), typeof(GuildCreateReq));
		ProtoMapDic.Add(((int)MsgIDEnum.GuildCreateRetID), typeof(GuildCreateRet));
		ProtoMapDic.Add(((int)MsgIDEnum.GuildMyGetBaseReqID), typeof(GuildMyGetBaseReq));
		ProtoMapDic.Add(((int)MsgIDEnum.GuildMyGetBaseRetID), typeof(GuildMyGetBaseRet));
		ProtoMapDic.Add(((int)MsgIDEnum.GuildMyGetPlayerListReqID), typeof(GuildMyGetPlayerListReq));
		ProtoMapDic.Add(((int)MsgIDEnum.GuildMyGetPlayerListRetID), typeof(GuildMyGetPlayerListRet));
		ProtoMapDic.Add(((int)MsgIDEnum.GuildMyGetEventReqID), typeof(GuildMyGetEventReq));
		ProtoMapDic.Add(((int)MsgIDEnum.GuildMyGetEventRetID), typeof(GuildMyGetEventRet));
		ProtoMapDic.Add(((int)MsgIDEnum.GuildQuitReqID), typeof(GuildQuitReq));
		ProtoMapDic.Add(((int)MsgIDEnum.GuildMySrvReqID), typeof(GuildMySrvReq));
		ProtoMapDic.Add(((int)MsgIDEnum.GuildMySrvRetID), typeof(GuildMySrvRet));
		ProtoMapDic.Add(((int)MsgIDEnum.GuildInviteReqID), typeof(GuildInviteReq));
		ProtoMapDic.Add(((int)MsgIDEnum.GuildInviteNtfID), typeof(GuildInviteNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.GuildInviteApplyReqID), typeof(GuildInviteApplyReq));
		ProtoMapDic.Add(((int)MsgIDEnum.GuildMyApplyListReqID), typeof(GuildMyApplyListReq));
		ProtoMapDic.Add(((int)MsgIDEnum.GuildMyApplyListRetID), typeof(GuildMyApplyListRet));
		ProtoMapDic.Add(((int)MsgIDEnum.GuildMySetApplyReqID), typeof(GuildMySetApplyReq));
		ProtoMapDic.Add(((int)MsgIDEnum.GuildMyKickReqID), typeof(GuildMyKickReq));
		ProtoMapDic.Add(((int)MsgIDEnum.GuildMyBankChatReqID), typeof(GuildMyBankChatReq));
		ProtoMapDic.Add(((int)MsgIDEnum.GuildMyUnbankChatReqID), typeof(GuildMyUnbankChatReq));
		ProtoMapDic.Add(((int)MsgIDEnum.GuildMySetPosLvReqID), typeof(GuildMySetPosLvReq));
		ProtoMapDic.Add(((int)MsgIDEnum.GuildMyUpNameReqID), typeof(GuildMyUpNameReq));
		ProtoMapDic.Add(((int)MsgIDEnum.GuildMyUpNameRetID), typeof(GuildMyUpNameRet));
		ProtoMapDic.Add(((int)MsgIDEnum.GuildMyUpJoinLvReqID), typeof(GuildMyUpJoinLvReq));
		ProtoMapDic.Add(((int)MsgIDEnum.GuildMyUpAutoReqID), typeof(GuildMyUpAutoReq));
		ProtoMapDic.Add(((int)MsgIDEnum.GuildMyChangeInfoReqID), typeof(GuildMyChangeInfoReq));
		ProtoMapDic.Add(((int)MsgIDEnum.AddGuildExpSrvReqID), typeof(AddGuildExpSrvReq));
		ProtoMapDic.Add(((int)MsgIDEnum.GuMgrGetListReqID), typeof(GuMgrGetListReq));
		ProtoMapDic.Add(((int)MsgIDEnum.GuMgrGetListRetID), typeof(GuMgrGetListRet));
		ProtoMapDic.Add(((int)MsgIDEnum.GuMgrApplyReqID), typeof(GuMgrApplyReq));
		ProtoMapDic.Add(((int)MsgIDEnum.GuMgrApplyRetID), typeof(GuMgrApplyRet));
		ProtoMapDic.Add(((int)MsgIDEnum.GuMgrCelApplyReqID), typeof(GuMgrCelApplyReq));
		ProtoMapDic.Add(((int)MsgIDEnum.GuMgrCelApplyRetID), typeof(GuMgrCelApplyRet));
		ProtoMapDic.Add(((int)MsgIDEnum.GuCreateTerritorySrvRetID), typeof(GuCreateTerritorySrvRet));
		ProtoMapDic.Add(((int)MsgIDEnum.GuDestroyTerritorySrvNtfID), typeof(GuDestroyTerritorySrvNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.EnterGuildTerritoryReqID), typeof(EnterGuildTerritoryReq));
		ProtoMapDic.Add(((int)MsgIDEnum.EnterGuildTerritorySrvReqID), typeof(EnterGuildTerritorySrvReq));
		ProtoMapDic.Add(((int)MsgIDEnum.GuGuildDestroyNtfID), typeof(GuGuildDestroyNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.PlayerLeaveGuildNtfID), typeof(PlayerLeaveGuildNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.GetPlayerListReqID), typeof(GetPlayerListReq));
		ProtoMapDic.Add(((int)MsgIDEnum.Battle10RegisterMDID), typeof(Battle10RegisterMD));
		ProtoMapDic.Add(((int)MsgIDEnum.Battle10V10ModelID), typeof(Battle10V10Model));
		ProtoMapDic.Add(((int)MsgIDEnum.Battle10V10MatchSpaceModelID), typeof(Battle10V10MatchSpaceModel));
		ProtoMapDic.Add(((int)MsgIDEnum.B10InstanceDataID), typeof(B10InstanceData));
		ProtoMapDic.Add(((int)MsgIDEnum.B10PersonModelID), typeof(B10PersonModel));
		ProtoMapDic.Add(((int)MsgIDEnum.BFMatchedNtfID), typeof(BFMatchedNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.B10RunDataNtfID), typeof(B10RunDataNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.BFNoticDataNtfID), typeof(BFNoticDataNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.BFGameEndNtfID), typeof(BFGameEndNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.B10PlayStartI2LID), typeof(B10PlayStartI2L));
		ProtoMapDic.Add(((int)MsgIDEnum.B10PlayStartI2LReqID), typeof(B10PlayStartI2LReq));
		ProtoMapDic.Add(((int)MsgIDEnum.B10PlayStartI2LRetID), typeof(B10PlayStartI2LRet));
		ProtoMapDic.Add(((int)MsgIDEnum.B10RegisterReqID), typeof(B10RegisterReq));
		ProtoMapDic.Add(((int)MsgIDEnum.B10RegisterRetID), typeof(B10RegisterRet));
		ProtoMapDic.Add(((int)MsgIDEnum.BFMatchAgreeReqID), typeof(BFMatchAgreeReq));
		ProtoMapDic.Add(((int)MsgIDEnum.BFCreateMapI2IRetID), typeof(BFCreateMapI2IRet));
		ProtoMapDic.Add(((int)MsgIDEnum.BFGetStatesReqID), typeof(BFGetStatesReq));
		ProtoMapDic.Add(((int)MsgIDEnum.BFGetStatesRetID), typeof(BFGetStatesRet));
		ProtoMapDic.Add(((int)MsgIDEnum.BFTeamModelID), typeof(BFTeamModel));
		ProtoMapDic.Add(((int)MsgIDEnum.BFRegisterDataReqID), typeof(BFRegisterDataReq));
		ProtoMapDic.Add(((int)MsgIDEnum.BFRegisterDataRetID), typeof(BFRegisterDataRet));
		ProtoMapDic.Add(((int)MsgIDEnum.BFGameStartI2IRetID), typeof(BFGameStartI2IRet));
		ProtoMapDic.Add(((int)MsgIDEnum.ArenaRankDataMDID), typeof(ArenaRankDataMD));
		ProtoMapDic.Add(((int)MsgIDEnum.ArenaLogDataMDID), typeof(ArenaLogDataMD));
		ProtoMapDic.Add(((int)MsgIDEnum.ArenaRobotInfoID), typeof(ArenaRobotInfo));
		ProtoMapDic.Add(((int)MsgIDEnum.ArenaRunInfoID), typeof(ArenaRunInfo));
		ProtoMapDic.Add(((int)MsgIDEnum.ArenaMyMatchRankItemID), typeof(ArenaMyMatchRankItem));
		ProtoMapDic.Add(((int)MsgIDEnum.ArenaFightOneLogID), typeof(ArenaFightOneLog));
		ProtoMapDic.Add(((int)MsgIDEnum.ArenaGetMyBaseInfoReqID), typeof(ArenaGetMyBaseInfoReq));
		ProtoMapDic.Add(((int)MsgIDEnum.ArenaGetMyBaseInfoRetID), typeof(ArenaGetMyBaseInfoRet));
		ProtoMapDic.Add(((int)MsgIDEnum.ArenaBuyChallengeTimesReqID), typeof(ArenaBuyChallengeTimesReq));
		ProtoMapDic.Add(((int)MsgIDEnum.ArenaBuyChallengeTimesRetID), typeof(ArenaBuyChallengeTimesRet));
		ProtoMapDic.Add(((int)MsgIDEnum.ArenaGetFightListReqID), typeof(ArenaGetFightListReq));
		ProtoMapDic.Add(((int)MsgIDEnum.ArenaGetFightListRetID), typeof(ArenaGetFightListRet));
		ProtoMapDic.Add(((int)MsgIDEnum.ArenaGetMyFightLogReqID), typeof(ArenaGetMyFightLogReq));
		ProtoMapDic.Add(((int)MsgIDEnum.ArenaGetMyFightLogRetID), typeof(ArenaGetMyFightLogRet));
		ProtoMapDic.Add(((int)MsgIDEnum.ArenaChallengeTargetReqID), typeof(ArenaChallengeTargetReq));
		ProtoMapDic.Add(((int)MsgIDEnum.ArenaLevelDataID), typeof(ArenaLevelData));
		ProtoMapDic.Add(((int)MsgIDEnum.ArenaCreateRetID), typeof(ArenaCreateRet));
		ProtoMapDic.Add(((int)MsgIDEnum.ArenaNoticeDataNtfID), typeof(ArenaNoticeDataNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.ArenaGameEndNtfID), typeof(ArenaGameEndNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.ArenaChallengeTargetGameEndNtfID), typeof(ArenaChallengeTargetGameEndNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.ArenaAddFightLogNtfID), typeof(ArenaAddFightLogNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.WantTaskMDID), typeof(WantTaskMD));
		ProtoMapDic.Add(((int)MsgIDEnum.WTaskPointTarInfoID), typeof(WTaskPointTarInfo));
		ProtoMapDic.Add(((int)MsgIDEnum.WTaskSpMonTarInfoID), typeof(WTaskSpMonTarInfo));
		ProtoMapDic.Add(((int)MsgIDEnum.WTaskSrvTarInfoID), typeof(WTaskSrvTarInfo));
		ProtoMapDic.Add(((int)MsgIDEnum.WTaskTarInfoID), typeof(WTaskTarInfo));
		ProtoMapDic.Add(((int)MsgIDEnum.WTaskAcceptReqID), typeof(WTaskAcceptReq));
		ProtoMapDic.Add(((int)MsgIDEnum.WTaskAcceptRetID), typeof(WTaskAcceptRet));
		ProtoMapDic.Add(((int)MsgIDEnum.WTaskChallReqID), typeof(WTaskChallReq));
		ProtoMapDic.Add(((int)MsgIDEnum.WTaskChallRetID), typeof(WTaskChallRet));
		ProtoMapDic.Add(((int)MsgIDEnum.WTaskStartReqID), typeof(WTaskStartReq));
		ProtoMapDic.Add(((int)MsgIDEnum.WTaskStartRetID), typeof(WTaskStartRet));
		ProtoMapDic.Add(((int)MsgIDEnum.WTaskStartSrvReqID), typeof(WTaskStartSrvReq));
		ProtoMapDic.Add(((int)MsgIDEnum.WTaskCreMonSrvNtfID), typeof(WTaskCreMonSrvNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.WTaskCreLevSrvReqID), typeof(WTaskCreLevSrvReq));
		ProtoMapDic.Add(((int)MsgIDEnum.WTaskCreLevSrvRetID), typeof(WTaskCreLevSrvRet));
		ProtoMapDic.Add(((int)MsgIDEnum.WTaskGetAllInfoReqID), typeof(WTaskGetAllInfoReq));
		ProtoMapDic.Add(((int)MsgIDEnum.WTaskGetAllInfoSrvReqID), typeof(WTaskGetAllInfoSrvReq));
		ProtoMapDic.Add(((int)MsgIDEnum.WTaskGetAllInfoRetID), typeof(WTaskGetAllInfoRet));
		ProtoMapDic.Add(((int)MsgIDEnum.WTaskPointNtfID), typeof(WTaskPointNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.WTaskStageNtfID), typeof(WTaskStageNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.WTaskLevSrvNtfID), typeof(WTaskLevSrvNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.WTaskMonRewSrvReqID), typeof(WTaskMonRewSrvReq));
		ProtoMapDic.Add(((int)MsgIDEnum.WTaskLevMonHurtID), typeof(WTaskLevMonHurt));
		ProtoMapDic.Add(((int)MsgIDEnum.WTaskLevMonHurtNtfID), typeof(WTaskLevMonHurtNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.WTaskLevMonResNtfID), typeof(WTaskLevMonResNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.WTaskLevRemSrvReqID), typeof(WTaskLevRemSrvReq));
		ProtoMapDic.Add(((int)MsgIDEnum.WTaskEndNtfID), typeof(WTaskEndNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.WTaskChangeNtfID), typeof(WTaskChangeNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.LifeSkillUserDataMDID), typeof(LifeSkillUserDataMD));
		ProtoMapDic.Add(((int)MsgIDEnum.UserSceneLogicDataMDID), typeof(UserSceneLogicDataMD));
		ProtoMapDic.Add(((int)MsgIDEnum.LifeSkillMapMineDataFieldID), typeof(LifeSkillMapMineDataField));
		ProtoMapDic.Add(((int)MsgIDEnum.LifeSkillOneMapMineDataID), typeof(LifeSkillOneMapMineData));
		ProtoMapDic.Add(((int)MsgIDEnum.LifeSkillOneMineDataID), typeof(LifeSkillOneMineData));
		ProtoMapDic.Add(((int)MsgIDEnum.LifeSkillFormulationFieldID), typeof(LifeSkillFormulationField));
		ProtoMapDic.Add(((int)MsgIDEnum.LifeSkillQTEItemInfoID), typeof(LifeSkillQTEItemInfo));
		ProtoMapDic.Add(((int)MsgIDEnum.MapNpcOrInterShowHideDataFieldID), typeof(MapNpcOrInterShowHideDataField));
		ProtoMapDic.Add(((int)MsgIDEnum.MapNpcOrInterHideDataID), typeof(MapNpcOrInterHideData));
		ProtoMapDic.Add(((int)MsgIDEnum.LifeSkillCollectNtfSrvID), typeof(LifeSkillCollectNtfSrv));
		ProtoMapDic.Add(((int)MsgIDEnum.LifeSkillProduceItemReqID), typeof(LifeSkillProduceItemReq));
		ProtoMapDic.Add(((int)MsgIDEnum.LifeSkillQTEInfoNtfID), typeof(LifeSkillQTEInfoNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.LifeSkillQTEGameResultReqID), typeof(LifeSkillQTEGameResultReq));
		ProtoMapDic.Add(((int)MsgIDEnum.LifeSkillQueryMapGatherObjInfoReqID), typeof(LifeSkillQueryMapGatherObjInfoReq));
		ProtoMapDic.Add(((int)MsgIDEnum.LifeSkillQueryMapGatherMapInfoRetID), typeof(LifeSkillQueryMapGatherMapInfoRet));
		ProtoMapDic.Add(((int)MsgIDEnum.LifeSkillUnlockFormulationReqID), typeof(LifeSkillUnlockFormulationReq));
		ProtoMapDic.Add(((int)MsgIDEnum.FightMDID), typeof(FightMD));
		ProtoMapDic.Add(((int)MsgIDEnum.GetFightRewardReqID), typeof(GetFightRewardReq));
		ProtoMapDic.Add(((int)MsgIDEnum.GetFightRewardRetID), typeof(GetFightRewardRet));
		ProtoMapDic.Add(((int)MsgIDEnum.TeamMatePowerUpdateNtfID), typeof(TeamMatePowerUpdateNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.QAPlayStartNtfID), typeof(QAPlayStartNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.QAPlayEndNtfID), typeof(QAPlayEndNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.QAQestionToUserNtfID), typeof(QAQestionToUserNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.QAUserSelectNtfID), typeof(QAUserSelectNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.QAAnswerToUserNtfID), typeof(QAAnswerToUserNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.QARewardNtfID), typeof(QARewardNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.PartyTimeHangUpRewardReqID), typeof(PartyTimeHangUpRewardReq));
		ProtoMapDic.Add(((int)MsgIDEnum.PartyTimeGuildScoreRewardReqID), typeof(PartyTimeGuildScoreRewardReq));
		ProtoMapDic.Add(((int)MsgIDEnum.PartyTimeGuildScoreSrvReqID), typeof(PartyTimeGuildScoreSrvReq));
		ProtoMapDic.Add(((int)MsgIDEnum.PartyTimeGuildScoreSrvRetID), typeof(PartyTimeGuildScoreSrvRet));
		ProtoMapDic.Add(((int)MsgIDEnum.PartyTimeQADataNtfID), typeof(PartyTimeQADataNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.GetPartyTimeDataReqID), typeof(GetPartyTimeDataReq));
		ProtoMapDic.Add(((int)MsgIDEnum.UpdateRankMetaDataSrvReqID), typeof(UpdateRankMetaDataSrvReq));
		ProtoMapDic.Add(((int)MsgIDEnum.UpdateRankMetaDataSrvRetID), typeof(UpdateRankMetaDataSrvRet));
		ProtoMapDic.Add(((int)MsgIDEnum.UpdateRankAwardStatusSrvNtfID), typeof(UpdateRankAwardStatusSrvNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.GenericPlayerRankItemID), typeof(GenericPlayerRankItem));
		ProtoMapDic.Add(((int)MsgIDEnum.GetGenericRankDataReqID), typeof(GetGenericRankDataReq));
		ProtoMapDic.Add(((int)MsgIDEnum.GetGenericRankDataRetID), typeof(GetGenericRankDataRet));
		ProtoMapDic.Add(((int)MsgIDEnum.GetGenericRankGetAwardReqID), typeof(GetGenericRankGetAwardReq));
		ProtoMapDic.Add(((int)MsgIDEnum.GetGenericRankGetAwardRetID), typeof(GetGenericRankGetAwardRet));
		ProtoMapDic.Add(((int)MsgIDEnum.UserSundryMDID), typeof(UserSundryMD));
		ProtoMapDic.Add(((int)MsgIDEnum.OfflineUserMDID), typeof(OfflineUserMD));
		ProtoMapDic.Add(((int)MsgIDEnum.RiskLevelMDID), typeof(RiskLevelMD));
		ProtoMapDic.Add(((int)MsgIDEnum.GloAuntionMDID), typeof(GloAuntionMD));
		ProtoMapDic.Add(((int)MsgIDEnum.GloRiskInfoMDID), typeof(GloRiskInfoMD));
		ProtoMapDic.Add(((int)MsgIDEnum.GloInfoSrvRetID), typeof(GloInfoSrvRet));
		ProtoMapDic.Add(((int)MsgIDEnum.UserSettingReqID), typeof(UserSettingReq));
		ProtoMapDic.Add(((int)MsgIDEnum.UserSettingBatchReqID), typeof(UserSettingBatchReq));
		ProtoMapDic.Add(((int)MsgIDEnum.UserSettingBatchRetID), typeof(UserSettingBatchRet));
		ProtoMapDic.Add(((int)MsgIDEnum.UserSundryNtfID), typeof(UserSundryNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.UpReplyHpReqID), typeof(UpReplyHpReq));
		ProtoMapDic.Add(((int)MsgIDEnum.TickNtfID), typeof(TickNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.OfflineUserMDSrvNtfID), typeof(OfflineUserMDSrvNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.CraftingTreasureMapReqID), typeof(CraftingTreasureMapReq));
		ProtoMapDic.Add(((int)MsgIDEnum.CraftingTreasureMapRetID), typeof(CraftingTreasureMapRet));
		ProtoMapDic.Add(((int)MsgIDEnum.OpenTreasureMapReqID), typeof(OpenTreasureMapReq));
		ProtoMapDic.Add(((int)MsgIDEnum.OpenTreasureMapRetID), typeof(OpenTreasureMapRet));
		ProtoMapDic.Add(((int)MsgIDEnum.DigTreasureEndNtfID), typeof(DigTreasureEndNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.BreakDigTreasureReqID), typeof(BreakDigTreasureReq));
		ProtoMapDic.Add(((int)MsgIDEnum.RiskLevelUpReqID), typeof(RiskLevelUpReq));
		ProtoMapDic.Add(((int)MsgIDEnum.RiskLevelUpRetID), typeof(RiskLevelUpRet));
		ProtoMapDic.Add(((int)MsgIDEnum.GloInfoSrvReqID), typeof(GloInfoSrvReq));
		ProtoMapDic.Add(((int)MsgIDEnum.ChangeSrvRiskInfoReqID), typeof(ChangeSrvRiskInfoReq));
		ProtoMapDic.Add(((int)MsgIDEnum.UpRiskLevelSrvNtfID), typeof(UpRiskLevelSrvNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.TreMonIDID), typeof(TreMonID));
		ProtoMapDic.Add(((int)MsgIDEnum.GuideSignSetReqID), typeof(GuideSignSetReq));
		ProtoMapDic.Add(((int)MsgIDEnum.GuideSignSetRetID), typeof(GuideSignSetRet));
		ProtoMapDic.Add(((int)MsgIDEnum.ServerSignNtfID), typeof(ServerSignNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.GetUserOfflineEntrysSrvReqID), typeof(GetUserOfflineEntrysSrvReq));
		ProtoMapDic.Add(((int)MsgIDEnum.GetUserOfflineEntrysSrvRetID), typeof(GetUserOfflineEntrysSrvRet));
		ProtoMapDic.Add(((int)MsgIDEnum.CreateOfflineRobotSrvReqID), typeof(CreateOfflineRobotSrvReq));
		ProtoMapDic.Add(((int)MsgIDEnum.CreateOfflineRobotSrvRetID), typeof(CreateOfflineRobotSrvRet));
		ProtoMapDic.Add(((int)MsgIDEnum.GetSystemOpenRewardReqID), typeof(GetSystemOpenRewardReq));
		ProtoMapDic.Add(((int)MsgIDEnum.GetSystemOpenRewardRetID), typeof(GetSystemOpenRewardRet));
		ProtoMapDic.Add(((int)MsgIDEnum.GoodsExchangeReqID), typeof(GoodsExchangeReq));
		ProtoMapDic.Add(((int)MsgIDEnum.GoodsExchangeRetID), typeof(GoodsExchangeRet));
		ProtoMapDic.Add(((int)MsgIDEnum.PlayerNameplateMDID), typeof(PlayerNameplateMD));
		ProtoMapDic.Add(((int)MsgIDEnum.GetOtherPlayerNameplateReqID), typeof(GetOtherPlayerNameplateReq));
		ProtoMapDic.Add(((int)MsgIDEnum.GetOtherPlayerNameplateRetID), typeof(GetOtherPlayerNameplateRet));
		ProtoMapDic.Add(((int)MsgIDEnum.FightPowerModelID), typeof(FightPowerModel));
		ProtoMapDic.Add(((int)MsgIDEnum.ArrayItemModelID), typeof(ArrayItemModel));
		ProtoMapDic.Add(((int)MsgIDEnum.ArrayPartnerModelID), typeof(ArrayPartnerModel));
		ProtoMapDic.Add(((int)MsgIDEnum.ArraySlotModelID), typeof(ArraySlotModel));
		ProtoMapDic.Add(((int)MsgIDEnum.ServerRedPointNtfID), typeof(ServerRedPointNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.GlobalGameRecordMDID), typeof(GlobalGameRecordMD));
		ProtoMapDic.Add(((int)MsgIDEnum.OTPersonMailSendReqID), typeof(OTPersonMailSendReq));
		ProtoMapDic.Add(((int)MsgIDEnum.OTPersonMailSendRespID), typeof(OTPersonMailSendResp));
		ProtoMapDic.Add(((int)MsgIDEnum.OTManyMailsSendReqID), typeof(OTManyMailsSendReq));
		ProtoMapDic.Add(((int)MsgIDEnum.OTManyMailsSendRespID), typeof(OTManyMailsSendResp));
		ProtoMapDic.Add(((int)MsgIDEnum.OTServerMailSendReqID), typeof(OTServerMailSendReq));
		ProtoMapDic.Add(((int)MsgIDEnum.OTServerMailSendRespID), typeof(OTServerMailSendResp));
		ProtoMapDic.Add(((int)MsgIDEnum.ManyMailsID), typeof(ManyMails));
		ProtoMapDic.Add(((int)MsgIDEnum.UserListID), typeof(UserList));
		ProtoMapDic.Add(((int)MsgIDEnum.GetUserMailsReqID), typeof(GetUserMailsReq));
		ProtoMapDic.Add(((int)MsgIDEnum.GetUserMailsRespID), typeof(GetUserMailsResp));
		ProtoMapDic.Add(((int)MsgIDEnum.GetServerMailsReqID), typeof(GetServerMailsReq));
		ProtoMapDic.Add(((int)MsgIDEnum.GetServerMailRespID), typeof(GetServerMailResp));
		ProtoMapDic.Add(((int)MsgIDEnum.OPTGetUserRoleInfoReqID), typeof(OPTGetUserRoleInfoReq));
		ProtoMapDic.Add(((int)MsgIDEnum.OPTUserRoleBaseInfoID), typeof(OPTUserRoleBaseInfo));
		ProtoMapDic.Add(((int)MsgIDEnum.OPTGetUserRoleInfoRespID), typeof(OPTGetUserRoleInfoResp));
		ProtoMapDic.Add(((int)MsgIDEnum.OPTUserRoleGetBagItemsReqID), typeof(OPTUserRoleGetBagItemsReq));
		ProtoMapDic.Add(((int)MsgIDEnum.OPTUserRoleGetBagItemsRespID), typeof(OPTUserRoleGetBagItemsResp));
		ProtoMapDic.Add(((int)MsgIDEnum.ServerMailGetConditionDataID), typeof(ServerMailGetConditionData));
		ProtoMapDic.Add(((int)MsgIDEnum.ServerMailsMDID), typeof(ServerMailsMD));
		ProtoMapDic.Add(((int)MsgIDEnum.ServerMailGetNtfID), typeof(ServerMailGetNtf));
		ProtoMapDic.Add(((int)MsgIDEnum.GetServerMailReqID), typeof(GetServerMailReq));
		ProtoMapDic.Add(((int)MsgIDEnum.MarkServerMailReqID), typeof(MarkServerMailReq));
		ProtoMapDic.Add(((int)MsgIDEnum.SendMailByQTReqID), typeof(SendMailByQTReq));
		ProtoMapDic.Add(((int)MsgIDEnum.OPTUserRoleGetModuleDataReqID), typeof(OPTUserRoleGetModuleDataReq));
		ProtoMapDic.Add(((int)MsgIDEnum.OPTUserRoleGetModuleDataRespID), typeof(OPTUserRoleGetModuleDataResp));
		ProtoMapDic.Add(((int)MsgIDEnum.MarkServerMailRespID), typeof(MarkServerMailResp));
		ProtoMapDic.Add(((int)MsgIDEnum.NtfServerMailReqID), typeof(NtfServerMailReq));
		ProtoMapDic.Add(((int)MsgIDEnum.NtfServerMailRespID), typeof(NtfServerMailResp));
		ProtoMapDic.Add(((int)MsgIDEnum.OPTChangeSrvOpenDayID), typeof(OPTChangeSrvOpenDay));
		ProtoMapDic.Add(((int)MsgIDEnum.SendPersonMailReqID), typeof(SendPersonMailReq));
		ProtoMapDic.Add(((int)MsgIDEnum.SendPersonMailRespID), typeof(SendPersonMailResp));
		ProtoMapDic.Add(((int)MsgIDEnum.GetPersonMailReqID), typeof(GetPersonMailReq));
		ProtoMapDic.Add(((int)MsgIDEnum.GetPersonMailRespID), typeof(GetPersonMailResp));
		ProtoMapDic.Add(((int)MsgIDEnum.PersonalMailsMDID), typeof(PersonalMailsMD));
	
	}
}
