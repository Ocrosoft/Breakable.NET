using Godot;

public partial class Paddle : CharacterBody2D
{
    [Export]
    public float Speed { get; set; } = 400;
    [Export]
    // 使用 UpdateWidth 方法调整挡板宽度
    public float PaddleWidth
    {
        get => _paddleWidth;
        set
        {
            _paddleWidth = Mathf.Clamp(value, MinPaddleWidth, MaxPaddleWidth); ;
            ResizePaddle();
        }
    }
    [Export]
    public float MinPaddleWidth { get; set; } = 100;
    [Export]
    public float MaxPaddleWidth { get; set; } = 600;
    [Export]
    public Sprite2D LeftPaddle { get; set; }
    [Export]
    public Sprite2D MiddlePaddle { get; set; }
    [Export]
    public Sprite2D RightPaddle { get; set; }
    [Export]
    public CollisionShape2D CollisionShape2D { get; set; }

    private float _paddleWidth = 100;

    public override void _Ready()
    {
        base._Ready();

        ResizePaddle();
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        HandleInput();
    }

    // 根据用户输入计算移动速度
    private void HandleInput()
    {
        if (Input.IsActionPressed("ui_left"))
        {
            Velocity = new(-Speed, 0);
        }
        else if (Input.IsActionPressed("ui_right"))
        {
            Velocity = new(Speed, 0);
        }
        else
        {
            Velocity = Vector2.Zero;
        }

        MoveAndSlide();
    }

    // 获取球的起始位置（挡板顶部）
    public Vector2 GetBallStartPosition()
    {
        // 挡板高度为 15px
        return Position + new Vector2(0, -7.5f);
    }

    // 调整挡板的大小
    private void ResizePaddle()
    {
        // 左右挡板宽度为 30px
        float middleWidth = PaddleWidth - 30 - 30;
        // 中间挡板宽度为 20px
        float middleScale = middleWidth / 20;
        MiddlePaddle.Scale = new Vector2(middleScale, 1);
        LeftPaddle.Position = new(
            MiddlePaddle.Position.X - middleWidth / 2 - 15, MiddlePaddle.Position.Y);
        RightPaddle.Position = new(
            MiddlePaddle.Position.X + middleWidth / 2 + 15, MiddlePaddle.Position.Y);

        var shape = ((RectangleShape2D)CollisionShape2D.Shape);
        shape.Size = new Vector2(PaddleWidth, shape.Size.Y);
    }
}
