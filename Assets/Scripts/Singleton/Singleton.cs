using System;
public abstract class Singleton<T> where T : class, new()
{
	private static volatile T m_instance;
	// 增加一个变量来保证线程安全
	private static readonly object syncRoot = new object();
	public static T Instance
	{
		get
		{
			if (Singleton<T>.m_instance == null)
			{
				// 加锁用来保证单例的线程安全
				lock (syncRoot)
				{
					if (Singleton<T>.m_instance == null)
					{
						Singleton<T>.m_instance = Activator.CreateInstance<T>();
						if (Singleton<T>.m_instance != null)
						{
							(Singleton<T>.m_instance as Singleton<T>).Init();
						}
					}
				}
			}

			return Singleton<T>.m_instance;
		}
	}

	public static void Release()
	{
		if (Singleton<T>.m_instance != null)
		{
			Singleton<T>.m_instance = (T)((object)null);
		}
	}

	public virtual void Init()
	{

	}

	public virtual void Dispose() { }

}
