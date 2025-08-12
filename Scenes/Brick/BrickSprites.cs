using Godot;
using System.Collections.Generic;

public partial class BrickSprites : Sprite2D
{
    public static BrickSprites Instance { get; private set; }

    private readonly Dictionary<BrickType, string> BRICK_SPRITES = new() {
        { BrickType.Normal1, "res://Assets/Brick01.png" },
        { BrickType.Normal2, "res://Assets/Brick02.png" },
        { BrickType.Normal3, "res://Assets/Brick03.png" },
        { BrickType.Normal4, "res://Assets/Brick04.png" },
        { BrickType.Normal5, "res://Assets/Brick05.png" },

        { BrickType.Hard1, "res://Assets/Brick11.png" },
        { BrickType.Hard2, "res://Assets/Brick12.png" }
    };

    private Dictionary<BrickType, Texture2D> _brickTextures = [];

    public override void _Ready()
    {
        base._Ready();

        Instance = this;

        foreach (var brickSprite in BRICK_SPRITES)
        {
            _brickTextures.Add(brickSprite.Key, GD.Load<Texture2D>(brickSprite.Value));
        }
    }

    public Texture2D GetBrickTexture(BrickType brickType)
    {
        if (_brickTextures.TryGetValue(brickType, out var texture))
        {
            return texture;
        }
        return null;
    }
}
