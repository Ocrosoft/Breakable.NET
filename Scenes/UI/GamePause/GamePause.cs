using Godot;

public partial class GamePause : Node2D
{
    [Export]
    public Button Continue;
    [Export]
    public Button MainMenu;

    public override void _Ready()
    {
        base._Ready();

        Continue.ButtonUp += ContinueGame;

        MainMenu.ButtonUp += () =>
        {
            GetTree().Paused = false;
            GetTree().ChangeSceneToPacked(UIGlobal.Instance.GameStartScene);
        };
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        if (Input.IsActionJustPressed("ui_cancel"))
        {
            ContinueGame();
        }
    }

    private void ContinueGame()
    {
        GetTree().Paused = false;
        QueueFree();
    }
}
