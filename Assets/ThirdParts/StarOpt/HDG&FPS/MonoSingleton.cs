using UnityEngine;

public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
{
    public static bool HasInstance {
        get
        {
            return instance != null;
        }
    }

    protected static T instance = null;
    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                instance = GameObject.FindObjectOfType(typeof(T)) as T;
                if (FindObjectsOfType(typeof(T)).Length > 1)
                {
                    return instance;
                }

                if (instance == null)
                {
                    instance = new GameObject("Singleton of " + typeof(T).ToString(), typeof(T)).GetComponent<T>();
                }
            }

            return instance;
        }
    }

    protected virtual void InitInstance()
    {

    }

    protected virtual void Awake()
    {
        if (instance == null)
        {
            instance = this as T;
        }
    }

    protected virtual void Init()
    {

    }


    protected virtual void OnDestroy()
    {

    }
}