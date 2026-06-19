namespace CokingNodeEditor
{
    public enum ConnectionPointTypeEnum 
    { 
        In,
        TrueOut,
        AlwaysOut,
        FalseOut,
    }

    public enum ConnectionType
    {
        TrueConnect,
        AlwaysConnect,
        FalseConnect
    }

    public enum NodeTypeEnum
    {
        Normal,
        Decision
    }

    public enum KeyTypeEnum
    {
        Int,
        String,
        Bool,
        Target,
        Pos,
        Toward
    }
}

