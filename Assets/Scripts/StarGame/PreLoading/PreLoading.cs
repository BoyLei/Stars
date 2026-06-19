using SGF.Module.Framework;

public class PreLoading : ServiceModule<PreLoading>
{
    private RolePreLoading m_RolePreLoading;
    private TexturePreload m_TexturePreLoading;
    public void Init()
    {
        if (m_RolePreLoading == null)
        {
            m_RolePreLoading = new RolePreLoading();
            m_RolePreLoading.Init();
        }
        if (m_TexturePreLoading == null)
        {
            m_TexturePreLoading = new TexturePreload();
        }
    }

    public void Reset()
    {
        if (m_RolePreLoading == null)
        {
            m_RolePreLoading.Reset();
        }
    }

    public bool CanPreLoad()
    {
        if (m_RolePreLoading != null)
        {
            return m_RolePreLoading.CanPreLoad();
        }
        return false;
    }

    public void Loading(System.Action<string, float> process, System.Action complete)
    {
        if (m_RolePreLoading != null)
        {
            m_RolePreLoading.Loading(process, complete);
        }
    }
}
