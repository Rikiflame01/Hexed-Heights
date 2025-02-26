public static class GameStateManager
{
    public enum GameState
    {
        MainMenu,
        Playing,
        ChooseCard,
        Paused
    }

    private static bool _initialized;
    private static GameState _currentState = GameState.MainMenu;

    public static GameState CurrentState
    {
        get => _currentState;
        set => _currentState = value;
    }

    public static bool IsMainMenu => _currentState == GameState.MainMenu;
    public static bool IsPlaying   => _currentState == GameState.Playing;
    public static bool IsChoosingCard  => _currentState == GameState.ChooseCard;
    public static bool IsPaused    => _currentState == GameState.Paused;

    public static void Initialize()
    {
        if (_initialized) return;

        ActionManager.OnPaused       += HandleGamePaused;
        ActionManager.OnChooseCard  += HandleChooseCard;
        ActionManager.OnPlaying      += HandlePlaying;
        ActionManager.OnMainMenu += HandleMainMenu;

        _initialized = true;
    }

    private static void HandleMainMenu()
    {
        SetMainMenu();
    }
    private static void HandlePlaying(){
        SetPlaying();
    }
    private static void HandleGamePaused()
    {
        SetPaused();
    }

    private static void HandleChooseCard()
    {
        SetChooseCard();
    }
    public static void SetPlaying()   => _currentState = GameState.Playing;
    public static void SetPaused()    => _currentState = GameState.Paused;
    public static void SetChooseCard()    => _currentState = GameState.ChooseCard;
    public static void SetMainMenu()  => _currentState = GameState.MainMenu;
}
