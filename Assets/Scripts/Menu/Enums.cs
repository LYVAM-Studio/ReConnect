namespace Reconnect.Menu
{
    public enum MenuState
    {
        None,
        Level1,
        Level2,
        Level3,
        Level4,
        Level5,
        Level6,
        Level7,
        Level8,
        Level9,
        Main,
        Singleplayer,
        Multiplayer,
        Settings,
        Pause,
        BreadBoard,
        Connection,
        ConnectionFailed,
        Lessons,
        ImageViewer,
        KnockOut,
        Quit,
        NewLesson,
     }

    public enum CursorState
    {
        Shown,
        Locked
    }

    public enum PlayMode
    {
        Single,
        MultiHost,
        MultiServer
    }
}