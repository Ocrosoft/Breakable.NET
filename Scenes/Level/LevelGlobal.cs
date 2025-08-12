using Godot;

public partial class LevelGlobal : Node2D
{
    public static LevelGlobal Instance { get; private set; }

    public int CurrentLevel { get; set; } = 1;

    public PackedScene LevelScene { get; private set; }
    public PackedScene BallScene { get; private set; }
    public PackedScene BonusScene { get; private set; }
    public PackedScene BrickScene { get; private set; }

    public override void _Ready()
    {
        base._Ready();

        Instance = this;

        LevelScene = GD.Load<PackedScene>("res://Scenes/Level/Level.tscn");
        BallScene = GD.Load<PackedScene>("res://Scenes/Ball/Ball.tscn");
        BonusScene = GD.Load<PackedScene>("res://Scenes/Bonus/Bonus.tscn");
        BrickScene = GD.Load<PackedScene>("res://Scenes/Brick/Brick.tscn");
    }
}