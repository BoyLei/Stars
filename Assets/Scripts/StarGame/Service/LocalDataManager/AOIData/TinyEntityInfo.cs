using StarProject.Service.LocalData;
namespace StarProjectDef
{
    public class TinyEntityInfo 
    {
        public int startIndex { set; get; }

        public TinyEntityPropInfo props { set; get; }
    }


    public class TinyEntityPropInfo
    {
        
        public VitalSignAOIAttrs Index {get; set;}

        public VitalSignAOIAttrs Position {get; set;}

        public VitalSignAOIAttrs Rot {get; set;}

        public VitalSignAOIAttrs AIState {get; set;}

        public VitalSignAOIAttrs ShowLimit {get; set;}

        public VitalSignAOIAttrs TargetId {get; set;}

        public VitalSignAOIAttrs InfluenceID {get; set;}

        public VitalSignAOIAttrs TinyEntityFlag {get; set;}

        public VitalSignAOIAttrs DropCount {get; set;}

        public VitalSignAOIAttrs SpawnInfluenceID {get; set;}

        public VitalSignAOIAttrs MonsterAIState {get; set;}

        public VitalSignAOIAttrs CreateTime {get; set;}

        public VitalSignAOIAttrs IsInteract {get; set;}

        public VitalSignAOIAttrs InteractUseList {get; set;}

        public VitalSignAOIAttrs SummonHostID {get; set;}

        public VitalSignAOIAttrs OffestRota {get; set;}

        public VitalSignAOIAttrs BelongTeamID {get; set;}

        public VitalSignAOIAttrs Alias {get; set;}

        public VitalSignAOIAttrs AvatarID {get; set;}

        public VitalSignAOIAttrs EntityLevel {get; set;}

        public VitalSignAOIAttrs SpaceIndex {get; set;}

        public VitalSignAOIAttrs Title {get; set;}

        public VitalSignAOIAttrs ControlID {get; set;}

        public VitalSignAOIAttrs ActMark {get; set;}

        public VitalSignAOIAttrs ShowLevel {get; set;}

    }
}