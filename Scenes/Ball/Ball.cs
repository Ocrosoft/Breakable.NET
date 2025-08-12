using Godot;
using System;

public partial class Ball : RigidBody2D
{
    [Export]
    Area2D Area2D { get; set; }
    [Export]
    public float Speed { get; set; } = 500;

    [Signal]
    // 球离开游戏区域的信号
    public delegate void BallOutOfPlayAreaEventHandler(Ball ball);

    private AudioStreamPlayer _wallHitAudio;
    private AudioStreamPlayer _paddleHitAudio;
    private AudioStreamPlayer _brickHitAudio;

    private Paddle _paddle;

    // 球的半径，目前不支持修改
    private float _radius = 8;
    // 是否已经开始运动
    // 未开始移动时，球会跟随当盘移动，可以按键发射
    private bool _started = false;

    public override void _Ready()
    {
        base._Ready();

        _wallHitAudio = GetNode<AudioStreamPlayer>("HitWall");
        _paddleHitAudio = GetNode<AudioStreamPlayer>("HitPaddle");
        _brickHitAudio = GetNode<AudioStreamPlayer>("HitBrick");

        Area2D.BodyExited += OnBodyExited;
    }

    // 设置 Paddle 对象，用于球跟随挡板移动
    public void SetPaddle(Paddle paddle)
    {
        _paddle = paddle;
    }

    // 其他物体离开 Area2D，保持速度（Godot 即使完全反弹也有速度衰减）
    private void OnBodyExited(Node2D body)
    {
        if (!_started)
        {
            return;
        }

        if (body.IsInGroup("Paddle"))
        {
            // 撞到挡板时，不同位置的碰撞会有不同的反弹角度
            Paddle paddle = (Paddle)body;
            Vector2 ballPos = Position;
            Vector2 paddlePos = paddle.Position;
            var coeff = (ballPos.X - paddlePos.X) / paddle.PaddleWidth;
            if (coeff < -0.5 || coeff > 0.5)
            {
                // 撞到侧边，不调整
                coeff = 0;
            }
            var angle = coeff * Mathf.Pi / 2.4;
            var velocity = LinearVelocity.Rotated((float)angle);
            // 以一个比较平的角度撞到侧边时，旋转可能导致角度转为向下，此时不做调整
            if (velocity.Y <= 0)
            {
                LinearVelocity = velocity;
            }

            _paddleHitAudio.Play();
        }
        else
        {
            if (body.IsInGroup("Bricks"))
            {
                var brick = (Brick)body;
                // 攻击砖块
                brick.TakeDamage();

                _brickHitAudio.Play();
            }
            else if (body.IsInGroup("Walls"))
            {
                _wallHitAudio.Play();
            }
        }

        SetVelocity(LinearVelocity.Normalized() * Speed);
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        HandleInput();
        CheckOutOfPlayArea();
    }

    // 处理输入
    private void HandleInput()
    {
        if (_started)
        {
            return;
        }
        if (Input.IsActionJustPressed("ui_accept"))
        {
            Start(-100, 100, -200, -200);
        }
    }

    // 检查是否超出游戏区域
    private void CheckOutOfPlayArea()
    {
        if (IsQueuedForDeletion())
        {
            return;
        }
        var viewport = GetViewportRect().Size;
        if (Position.X < -_radius ||
            Position.X > viewport.X + _radius ||
            Position.Y < -_radius ||
            Position.Y > viewport.Y + _radius)
        {
            QueueFree();
            EmitSignal(SignalName.BallOutOfPlayArea, this);
        }
    }

    // 使用 _Process 等方法设置 Position 可能会导致物理引擎出错，例如发射的时候位置不正确
    // https://docs.godotengine.org/en/stable/classes/class_rigidbody2d.html#description
    public override void _IntegrateForces(PhysicsDirectBodyState2D state)
    {
        base._IntegrateForces(state);

        if (!_started)
        {
            Position = _paddle.GetBallStartPosition() + new Vector2(0, -_radius);
            // 没有这行有的时候不跟着动
            LinearVelocity = Vector2.Zero;
        }
    }

    // 按照指定范围随机发射球
    public void Start(int x1, int x2, int y1, int y2)
    {
        if (_started)
        {
            return;
        }

        _started = true;

        Vector2 velocity = new(new Random().Next(x1, x2), new Random().Next(y1, y2));
        SetVelocity(velocity.Normalized() * Speed);
    }

    // 设置速度方向，并调整角度
    public void SetVelocity(Vector2 velocity)
    {
        LinearVelocity = velocity.Normalized() * Speed;
        AdjustAngle();
    }

    // 防止角度过于水平
    private void AdjustAngle()
    {
        float minAngle = 20;
        Vector2 velocity = LinearVelocity;
        var angle = Mathf.RadToDeg(velocity.AngleTo(new Vector2(1, 0)));

        if (angle > 0 && angle < minAngle)
        {
            angle = angle - minAngle;
        }
        else if (angle < 0 && angle > -minAngle)
        {
            angle = angle + minAngle;
        }
        else if (angle > 180 - minAngle && angle < 180)
        {
            angle = angle - 180 + minAngle;
        }
        else if (angle < -180 + minAngle && angle > -180)
        {
            angle = angle + 180 - minAngle;
        }
        else
        {
            angle = 0;
        }

        if (angle != 0)
        {
            velocity = velocity.Rotated(Mathf.DegToRad(angle));
            LinearVelocity = velocity;
        }
    }
}
