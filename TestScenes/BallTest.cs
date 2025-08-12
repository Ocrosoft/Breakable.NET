using Godot;
using System;
using System.Collections.Generic;

public partial class BallTest : Node2D
{
    [Export]
    public int MaxBallCount { get; set; } = 500; // 最大球数

    private Node2D _balls;

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

        GetNode<Button>("Inc1").ButtonUp += () => AddFissionBalls(1);
        GetNode<Button>("Inc10").ButtonUp += () => AddFissionBalls(10);
        GetNode<Button>("Dec10").ButtonUp += () => RemoveBalls(10);
        GetNode<Button>("Mul2").ButtonUp += () => AddFissionBalls(_ballCount);
        GetNode<Button>("Div2").ButtonUp += () => RemoveBalls(_ballCount / 2);

        GetNode<CheckButton>("BottomWallToggle").Toggled += (pressed) =>
        {
            GetNode<StaticBody2D>("Walls/WallBottom").CollisionLayer = pressed ? 1u : 0u;
        };
        GetNode<CheckButton>("PaddleToggle").Toggled += (pressed) =>
        {
            GetNode<Paddle>("Paddle").CollisionLayer = pressed ? 1 << 3 : 0u;
        };
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

    private void AddFissionBalls(int count)
    {
        count = Math.Min(MaxBallCount - _ballCount, count);
        for (int i = 0; i < count; i++)
        {
            AddFissionBall(GetViewportRect().Size / 2);
        }
    }

    private void RemoveBalls(int count)
    {
        count = Math.Min(_ballCount - 1, count);
        for (int i = 0; i < count; i++)
        {
            var ball = _balls.GetChild<Ball>(i);
            ball.QueueFree();
            OnBallOutOfPlayArea(ball);
        }
    }

    private void OnBallOutOfPlayArea(Ball ball)
    {
        --_ballCount;
    }
}
