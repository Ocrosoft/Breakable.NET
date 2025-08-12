using Godot;

public partial class GameStart : Node2D
{
    [Export]
    public Button NewGame;
    [Export]
    public Button Options;
    [Export]
    public Button Quit;

    public override void _Ready()
    {
        base._Ready();

        NewGame.ButtonUp += () =>
        {
            GetTree().ChangeSceneToPacked(LevelGlobal.Instance.LevelScene);
        };

        Options.ButtonUp += () =>
        {
            GetTree().ChangeSceneToPacked(UIGlobal.Instance.GameOptionsScene);
        };

        Quit.ButtonUp += () =>
        {
            GetTree().Quit();
        };
    }
}
