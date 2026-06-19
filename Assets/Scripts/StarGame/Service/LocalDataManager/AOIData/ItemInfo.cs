using StarProject.Service.LocalData;
namespace StarProjectDef
{
    public class ItemInfo 
    {
        public int startIndex { set; get; }

        public ItemPropInfo props { set; get; }
    }


    public class ItemPropInfo
    {
        
        public VitalSignAOIAttrs BaseID {get; set;}

        public VitalSignAOIAttrs DBID {get; set;}

        public VitalSignAOIAttrs Num {get; set;}

        public VitalSignAOIAttrs SpaceId {get; set;}

        public VitalSignAOIAttrs Level {get; set;}

        public VitalSignAOIAttrs Quality {get; set;}

        public VitalSignAOIAttrs UpdatePoint {get; set;}

        public VitalSignAOIAttrs CDTime {get; set;}

        public VitalSignAOIAttrs GetTime {get; set;}

        public VitalSignAOIAttrs EquipProps {get; set;}

        public VitalSignAOIAttrs IsBind {get; set;}

        public VitalSignAOIAttrs Durability {get; set;}

        public VitalSignAOIAttrs Heroid {get; set;}

        public VitalSignAOIAttrs CombatSkills {get; set;}

        public VitalSignAOIAttrs GemSlots {get; set;}

        public VitalSignAOIAttrs BelongEquipID {get; set;}

        public VitalSignAOIAttrs ExtractNum {get; set;}

        public VitalSignAOIAttrs TreasureMapID {get; set;}

        public VitalSignAOIAttrs TreasureInterID {get; set;}

        public VitalSignAOIAttrs TreasurePos {get; set;}

    }
}