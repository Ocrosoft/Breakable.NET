using Godot;
using System;

public partial class Bonus : Area2D
{
    [Export]
    public BonusType Type { get; set; } = BonusType.None;
    [Export]
    public Sprite2D Sprite2D { get; set; }
    [Export]
    public int Speed { get; set; } = 200;
    [Export]
    public Label Label { get; set; }

    [Signal]
    // 道具被收集的信号
    public delegate void BonusCollectedEventHandler(Bonus bonus);

    // 挡板大小变化值
    public int PaddleSizeChange { get; private set; } = 20;

    // 球数量变化操作
    public BallCountOperation BallCountChangeOp { get; private set; } = BallCountOperation.None;
    // 球数量变化值
    public int BallCountChangeValue { get; private set; } = 1;

    public override void _Ready()
    {
        base._Ready();

        InitBonus();

        BodyEntered += OnBodyEntered;
    }

    // 根据类型初始化数值
    private void InitBonus()
    {
        switch (Type)
        {
            case BonusType.PaddleSize:
                RandomizePaddleSizeChange();
                break;
            case BonusType.BallCount:
                RandomizeBallCountChange();
                break;
            default:
                break;
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        MoveBonus(delta);
        CheckOutOfPlayArea();
    }

    // 下落
    private void MoveBonus(double delta)
    {
        Position += new Vector2(0, Speed * (float)delta);
    }

    // 检查是否超出游戏区域
    private void CheckOutOfPlayArea()
    {
        if (Position.Y > GetViewportRect().Size.Y)
        {
            QueueFree();
        }
    }

    // 检查挡板
    private void OnBodyEntered(Node2D body)
    {
        if (body.IsInGroup("Paddle"))
        {
            EmitSignal(SignalName.BonusCollected, this);
            QueueFree();
        }
    }

    // 随机化挡板大小变化
    private void RandomizePaddleSizeChange()
    {
        switch (new Random().Next(0, 2))
        {
            case 0:
                PaddleSizeChange = 20;
                Sprite2D.Texture = BonusSprites.Instance.PaddleSizeIncTexture;
                break;
            case 1:
                PaddleSizeChange = -20;
                Sprite2D.Texture = BonusSprites.Instance.PaddleSizeDecTexture;
                break;
            default:
                break;
        }
    }

    // 随机化球数量变化
    private void RandomizeBallCountChange()
    {
        Sprite2D.Texture = BonusSprites.Instance.BallCountTexture;
        switch (new Random().Next(0, 5))
        {
            case 0:
                BallCountChangeOp = BallCountOperation.Add;
                BallCountChangeValue = new Random().Next(1, 4);
                Label.Text = $"+{BallCountChangeValue}";
                break;
            case 1:
                BallCountChangeOp = BallCountOperation.Remove;
                BallCountChangeValue = new Random().Next(1, 4);
                Label.Text = $"-{BallCountChangeValue}";
                break;
            case 2:
                BallCountChangeOp = BallCountOperation.Multiply;
                BallCountChangeValue = new Random().Next(1, 4);
                Label.Text = $"x{BallCountChangeValue}";
                break;
            case 3:
                BallCountChangeOp = BallCountOperation.Divide;
                BallCountChangeValue = new Random().Next(1, 4);
                Label.Text = $"/{BallCountChangeValue}";
                break;
            case 4:
                BallCountChangeOp = BallCountOperation.Modulo;
                BallCountChangeValue = new Random().Next(1, 100);
                Label.Text = $"%{BallCountChangeValue}";
                break;
            // - 和 = 容易看不清
            /*case 5:
                BallCountChangeOp = BallCountOperation.Set;
                BallCountChangeValue = new Random().Next(1, 10);
                Label.Text = $"={BallCountChangeValue}";
                break;*/
            default:
                break;
        }
    }
}
