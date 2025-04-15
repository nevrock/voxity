namespace Ngin
{
    public enum BlobifierState
    {
        Setup,
        Launch,

        Enable,
        Disable,

        Destroy,
        
        Tick,
        TickPhysics,
        TickLate,

        FirstFrame
    }
    public enum Direction
    {
        Up,
        Down,
        North,
        South,
        East,
        West
    }
    public enum GuiState
    {
        HoverIn,
        HoverOut,
        ClickIn,
        ClickOut,
        PressIn,
        PressOut
    }
}