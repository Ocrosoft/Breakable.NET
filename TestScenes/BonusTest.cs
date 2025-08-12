using Godot;
using System;

public partial class BonusTest : Node2D
{
    [Export]
    public int MaxBallCount { get; set; } = 500; // 最大球数

    private Node2D _balls;
    private Node2D _bonuses;
    private Paddle _paddle;

    private int _ballCount
    {
        get => _ballCount_;
        set
        {
            _ballCount_ = value;
            GetNode<Label>("BallCount").Text = $"BallCount: {_ballCount_}";
        }
    }
    private int _ballCount_ = 0;

    public override void _Ready()
    {
        base._Ready();

        _balls = GetNode<Node2D>("Balls");
        _bonuses = GetNode<Node2D>("Bonuses");
        _paddle = GetNode<Paddle>("Paddle");

        GetNode<Button>("BallCountBonus").ButtonUp += () =>
        {
            CreateBonus(BonusType.BallCount);
        };
        GetNode<Button>("PaddleSizeBonus").ButtonUp += () =>
        {
            CreateBonus(BonusType.PaddleSize);
        };

        GetNode<CheckButton>("BottomWallToggle").Toggled += (pressed) =>
        {
            GetNode<StaticBody2D>("Walls/WallBottom").CollisionLayer = pressed ? 1u : 0u;
        };
        GetNode<CheckButton>("PaddleToggle").Toggled += (pressed) =>
        {
            GetNode<Paddle>("Paddle").CollisionLayer = pressed ? 1 << 3 : 0u;
        };

        AddFissionBalls(1);
    }

    private void AddFissionBall(Vector2 position)
    {
        var ball = LevelGlobal.Instance.BallScene.Instantiate<Ball>();
        ball.BallOutOfPlayArea += OnBallOutOfPlayArea;

        ball.Position = position;
        ball.Start(-200, 200, -200, 200);

        _balls.AddChild(ball);
        ++_ballCount;
    }

    private void OnBallOutOfPlayArea(Ball ball)
    {
        --_ballCount;
    }

    private void CreateBonus(BonusType type)
    {
        var bonus = LevelGlobal.Instance.BonusScene.Instantiate<Bonus>();
        bonus.Position = GetViewportRect().Size / 2;
        bonus.Type = type;
        bonus.BonusCollected += OnBonusCollected;
        AddChild(bonus);
    }

    private void OnBonusCollected(Bonus bonus)
    {
        switch (bonus.Type)
        {
            case BonusType.PaddleSize:
                _paddle.PaddleWidth += bonus.PaddleSizeChange;
                break;
            case BonusType.BallCount:
                UpdateBallCount(bonus);
                break;
            default:
                break;
        }
    }

    private void UpdateBallCount(Bonus bonus)
    {
        GD.Print("Collected bonus.");
        switch (bonus.BallCountChangeOp)
        {
            case BallCountOperation.Add:
                CallDeferred(nameof(AddFissionBalls), bonus.BallCountChangeValue);
                break;
            case BallCountOperation.Remove:
                CallDeferred(nameof(RemoveBalls), bonus.BallCountChangeValue);
                break;
            case BallCountOperation.Multiply:
                CallDeferred(nameof(AddFissionBalls), _ballCount * bonus.BallCountChangeValue - _ballCount);
                break;
            case BallCountOperation.Divide:
                CallDeferred(nameof(RemoveBalls), _ballCount - _ballCount / bonus.BallCountChangeValue);
                break;
            case BallCountOperation.Set:
                int setDiff = bonus.BallCountChangeValue - _ballCount;
                if (setDiff > 0)
                {
                    CallDeferred(nameof(AddFissionBalls), setDiff);
                }
                else if (setDiff < 0)
                {
                    CallDeferred(nameof(RemoveBalls), -setDiff);
                }
                break;
            case BallCountOperation.Modulo:
                int modDiff = _ballCount - _ballCount % bonus.BallCountChangeValue;
                if (modDiff > 0)
                {
                    CallDeferred(nameof(RemoveBalls), modDiff);
                }
                else if (modDiff < 0)
                {
                    CallDeferred(nameof(AddFissionBalls), -modDiff);
                }
                break;
            default:
                break;
        }
    }

    private void AddFissionBalls(int count)
    {
        count = Math.Min(MaxBallCount - _ballCount, count);
        GD.Print($"AddFissionBalls {count}");
        for (int i = 0; i < count; i++)
        {
            AddFissionBall(GetViewportRect().Size / 2);
        }
    }

    private void RemoveBalls(int count)
    {
        count = Math.Min(_ballCount - 1, count);
        GD.Print($"RemoveBalls {count}");
        for (int i = 0; i < count; i++)
        {
            var ball = _balls.GetChild<Ball>(i);
            ball.QueueFree();
            OnBallOutOfPlayArea(ball);
        }
    }
}
