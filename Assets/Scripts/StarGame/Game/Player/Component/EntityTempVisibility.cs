using StarProjectDef;

namespace StarProject.Game.Player.Component
{
    public class EntityTempVisibility
    {
        public E_EntityType EntryType { get; private set; }

        public int ConfigID { get; private set; }

        public bool Visibility { get; private set; }

        private System.Action<bool> _onVisibilityChange;

        public void Initialize(E_EntityType entryType, int configID, System.Action<bool> onChange)
        {
            this.EntryType = entryType;
            this.ConfigID = configID;
            this._onVisibilityChange = onChange;
            this.Visibility = false;

            GlobalEvent.OnEntityTempVisibityChange.AddListener(OnVisibityChange);
        }

        public void Release()
        {
            this.EntryType = 0;
            this.ConfigID = 0;
            this._onVisibilityChange = null;
            this.Visibility = false;
            GlobalEvent.OnEntityTempVisibityChange.RemoveListener(OnVisibityChange);
        }

        private void OnVisibityChange(E_EntityType entityType, int configid, bool visibity)
        {
            if (entityType == EntryType && ConfigID == configid)
            {
                if (Visibility != visibity)
                {
                    Visibility = visibity;
                    _onVisibilityChange?.Invoke(Visibility);
                }
            }
        }
    }
}
