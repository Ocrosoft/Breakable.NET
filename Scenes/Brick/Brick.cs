using Godot;

public partial class Brick : StaticBody2D
{
    [Export]
    public Sprite2D Sprite2D { get; set; }
    [Export]
    public CollisionShape2D CollisionShape2D { get; set; }
    [Export]
    public BrickType BrickType { get; set; } = BrickType.Normal1;

    [Signal]
    public delegate void BrickHitEventHandler(Brick brick, int newHealth);

    private int _health = 1;

    public override void _Ready()
    {
        base._Ready();

        // 设置对应的纹理
        Sprite2D.Texture = BrickSprites.Instance.GetBrickTexture(BrickType);

        if (BrickType > BrickType.Normal5)
        {
            // 硬砖的初始生命值为2
            _health = 2;
        }
        else
        {
            _health = 1;
        }
    }

    public void TakeDamage()
    {
        --_health;
        EmitSignal(SignalName.BrickHit, this, _health);
        if (_health == 0)
        {
            QueueFree();
        }
    }
}
