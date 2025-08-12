using Godot;

public partial class BonusSprites : Node2D
{
    public static BonusSprites Instance { get; private set; }

    public Texture2D BallCountTexture { get; private set; }
    public Texture2D PaddleSizeIncTexture { get; private set; }
    public Texture2D PaddleSizeDecTexture { get; private set; }

    public override void _Ready()
    {
        base._Ready();

        Instance = this;

        BallCountTexture = GD.Load<Texture2D>("res://Assets/Bonus_BallCount.png");
        PaddleSizeIncTexture = GD.Load<Texture2D>("res://Assets/Bonus_PaddleSizeIncrease.png");
        PaddleSizeDecTexture = GD.Load<Texture2D>("res://Assets/Bonus_PaddleSizeDecrease.png");
    }
}
