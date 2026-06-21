using System;

namespace GameDLL.Hdg
{
	public class Settings
	{
		//public static int BROADCAST_TIME = 1;
		static float _BROADCAST_SECOND = 1f;
		public static float BROADCAST_SECOND
        {
			get
            {
				return _BROADCAST_SECOND;
			}
			set
            {
				_BROADCAST_SECOND = value;
				_BROADCAST_TIME = (int)(_BROADCAST_SECOND * 1000);
			}
        }

		static int _BROADCAST_TIME = 1000;
		public static int BROADCAST_TIME
        {
            get
            {
				return _BROADCAST_TIME;

			}
        }

		public static int DEFAULT_BROADCAST_PORT = 12000;

		public static int DEFAULT_SERVER_PORT = 12000;
    }
}
