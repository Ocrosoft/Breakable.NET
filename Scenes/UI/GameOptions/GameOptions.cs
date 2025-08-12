using Godot;

public partial class GameOptions : Node2D
{
    [Export]
    public Button MainMenu;
    [Export]
    public OptionButton Language;
    [Export]
    public CheckBox FullScreen;

    public override void _Ready()
    {
        base._Ready();

        MainMenu.ButtonUp += () =>
        {
            UIGlobal.Instance.SaveOptions();
            GetTree().ChangeSceneToPacked(UIGlobal.Instance.GameStartScene);
        };

        if (TranslationServer.GetLocale() == "en")
        {
            Language.Selected = 1;
        }
        Language.ItemSelected += OnLanguageChanged;

        if (DisplayServer.WindowGetMode() == DisplayServer.WindowMode.Fullscreen)
        {
            FullScreen.ButtonPressed = true;
        }
        FullScreen.Toggled += OnFullScreenChanged;
    }

    private void OnLanguageChanged(long index)
    {
        if (index == 0)
        {
            TranslationServer.SetLocale("zh");
            UIGlobal.Instance.Lang = "zh";
        }
        else
        {
            TranslationServer.SetLocale("en");
            UIGlobal.Instance.Lang = "en";
        }
        GetTree().ReloadCurrentScene();
    }

    private void OnFullScreenChanged(bool toggledOn)
    {
        if (toggledOn)
        {
            DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);
            UIGlobal.Instance.Fullscreen = true;
        }
        else
        {
            DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
            UIGlobal.Instance.Fullscreen = false;
        }
    }
}
