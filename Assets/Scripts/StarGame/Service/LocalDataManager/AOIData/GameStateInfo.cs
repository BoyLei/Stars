using StarProject.Service.LocalData;
namespace StarProjectDef
{
    public class GameStateInfo 
    {
        public int startIndex { set; get; }

        public GameStatePropInfo props { set; get; }
    }


    public class GameStatePropInfo
    {
        
        public VitalSignAOIAttrs LevelTarget {get; set;}

    }
}