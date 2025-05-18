namespace Reconnect.Menu
{
    public enum MenuState
    {
        None,
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
        NewLesson
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