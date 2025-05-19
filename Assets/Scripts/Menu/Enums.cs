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

    public static class MenuStateExtension
    {
        public static bool IsBriefMission(this MenuState menuState)
        {
            return menuState
                is MenuState.Level1
                or MenuState.Level2
                or MenuState.Level3
                or MenuState.Level4
                or MenuState.Level5
                or MenuState.Level6
                or MenuState.Level7
                or MenuState.Level8
                or MenuState.Level9;
        }
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