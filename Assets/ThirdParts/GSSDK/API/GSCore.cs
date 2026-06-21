namespace GSSDK
{
    public class GSCore:GSModule
    {

        public static void Init(GSRunMode mode) 
        {

            GSPlatform.GetInstance().SetGSPlatform(GetSDKPlatform());

            GSSDKMainThreadDispatcher.Instance();

            GSPlatform.GetInstance().GetPlatform().Init(mode);
        }


        private static IGSPlatform platform;

        private static readonly object platformLock = new object();

        internal static IGSPlatform GetSDKPlatform()
        {
            if (platform == null)
            {
                lock (platformLock)
                {
                    if (platform == null)
                    {
#if UNITY_IOS && !UNITY_EDITOR
                        platform = new GSiOS();
#elif UNITY_ANDROID && !UNITY_EDITOR
                        platform = new GSAndroid();
#else
                        platform = new GSUniversalPlatform();
#endif
                    }
                    return platform;
                }
            }
            return platform;
        }

    }
}
