using Godot;
using System;
using System.Collections.Generic;

public partial class Level : Node2D
{
    [Export]
    public int BrickMargin { get; set; } = 0; // 砖块区域左右两侧预留的距离（px）
    [Export]
    public int BonusChance { get; set; } = 50; // 砖块被销毁时生成道具的概率（百分比）
    [Export]
    public int MaxBallCount { get; set; } = 500; // 最大球数

    private Node2D _balls;
    private Node2D _bricks;
    private Node2D _bonuses;
    private Paddle _paddle;

    private readonly int _brickWidth = 86;
    private readonly int _brickHeight = 37;

    private HashSet<Rid> _ballRids = [];
    private int _ballCount = 0;

    public override void _Ready()
    {
        base._Ready();

        _balls = GetNode<Node2D>("Balls");
        _bricks = GetNode<Node2D>("Bricks");
        _bonuses = GetNode<Node2D>("Bonuses");
        _paddle = GetNode<Paddle>("Paddle");

        InitLevel();
        AddBall();
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

#if DEBUG
        if (Input.IsActionJustPressed("ui_page_up"))
        {
            LevelGlobal.Instance.CurrentLevel = Math.Max(LevelGlobal.Instance.CurrentLevel - 1, 1);
            RestartLevel();
        }
        else if (Input.IsActionJustPressed("ui_page_down"))
        {
            LevelGlobal.Instance.CurrentLevel = Math.Min(LevelGlobal.Instance.CurrentLevel + 1, LevelData.Levels.Count);
            RestartLevel();
        }
#endif

        HandlePauseInput();
    }

    private void HandlePauseInput()
    {
        if (Input.IsActionJustPressed("ui_cancel"))
        {
            if (GetTree().Paused)
            {
                GetTree().Paused = false;
                return;
            }
            GetTree().Paused = true;
            var pauseMenu = UIGlobal.Instance.GamePauseScene.Instantiate<GamePause>();
            AddChild(pauseMenu);
        }
    }

    // 创建一个球（初始状态）
    private void AddBall()
    {
        var ball = LevelGlobal.Instance.BallScene.Instantiate<Ball>();
        ball.SetPaddle(_paddle);
        ball.BallOutOfPlayArea += OnBallOutOfPlayArea;

        _balls.AddChild(ball);
        ++_ballCount;
    }

    // 创建一个分裂球（从指定位置发射）
    private void AddFissionBall(Vector2 position)
    {
        var ball = LevelGlobal.Instance.BallScene.Instantiate<Ball>();
        ball.SetPaddle(_paddle);
        ball.BallOutOfPlayArea += OnBallOutOfPlayArea;

        ball.Position = position;
        ball.Start(-200, 200, -200, 200);

        _balls.AddChild(ball);
        ++_ballCount;
    }

    // 球出界处理
    private void OnBallOutOfPlayArea(Ball ball)
    {
        if (_ballRids.Contains(ball.GetRid()))
        {
            return;
        }
        _ballRids.Add(ball.GetRid());

        --_ballCount;
        if (_ballCount == 0)
        {
            RestartLevel();
            return;
        }
    }

    // 初始化当前关卡
    private void InitLevel()
    {
        if (LevelData.Levels.Count < LevelGlobal.Instance.CurrentLevel)
        {
            Console.WriteLine("GameComplete");
            return;
        }

        foreach (var child in _bricks.GetChildren())
        {
            child.Free();
        }

        var level = LevelData.Levels[LevelGlobal.Instance.CurrentLevel - 1];
        var bricksInRow = level[0].Length;
        var viewportWidth = GetViewportRect().Size.X;
        float realBrickWidth = (viewportWidth - 2 * BrickMargin) / bricksInRow;
        float brickScale = realBrickWidth / _brickWidth;
        var top = 100;

        for (int i = 0; i < level.Length; ++i)
        {
            var brickLine = level[i];
            for (int j = 0; j < bricksInRow; ++j)
            {
                if (brickLine[j] == 0) continue;

                var brickInstance = LevelGlobal.Instance.BrickScene.Instantiate<Brick>();
                brickInstance.Name = $"Brick_{i}_{j}";
                brickInstance.AddToGroup("Bricks");
                brickInstance.Scale = new(brickScale, 1);
                brickInstance.BrickType = (BrickType)brickLine[j];
                brickInstance.Position = new(BrickMargin + j * realBrickWidth + realBrickWidth / 2, top + i * (_brickHeight));
                brickInstance.BrickHit += OnBrickHit;
                _bricks.AddChild(brickInstance);
            }
        }
    }

    // 重启当前关卡
    private void RestartLevel()
    {
        GetTree().ReloadCurrentScene();
    }

    // 砖块被击中
    private void OnBrickHit(Brick brick, int newHealth)
    {
        if (newHealth > 0)
        {
            return;
        }

        CreateBonus(brick.Position);

        if (_bricks.GetChildCount() <= 1)
        {
            if (LevelGlobal.Instance.CurrentLevel < LevelData.Levels.Count)
            {
                ++LevelGlobal.Instance.CurrentLevel;
                RestartLevel();
            }
            else
            {
                Console.WriteLine("GameComplete");
                return;
            }
        }
    }

    // 创建道具
    private void CreateBonus(Vector2 position)
    {
        if (GD.RandRange(0, 100) > BonusChance)
        {
            return;
        }

        var bonus = LevelGlobal.Instance.BonusScene.Instantiate<Bonus>();
        bonus.Position = position;
        bonus.Type = GetRandomBonusType();
        bonus.BonusCollected += OnBonusCollected;
        AddChild(bonus);
    }

    // 随机获取一个道具类型
    private static BonusType GetRandomBonusType()
    {
        var bonusTypes = Enum.GetValues(typeof(BonusType));
        return (BonusType)bonusTypes.GetValue(GD.RandRange(1, bonusTypes.Length - 1));
    }

    // 道具被收集
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

    // 更新球的数量
    private void UpdateBallCount(Bonus bonus)
    {
        switch (bonus.BallCountChangeOp)
        {
            case BallCountOperation.Add:
                AddFissionBalls(bonus.BallCountChangeValue);
                break;
            case BallCountOperation.Remove:
                RemoveBalls(bonus.BallCountChangeValue);
                break;
            case BallCountOperation.Multiply:
                AddFissionBalls(_ballCount * bonus.BallCountChangeValue - _ballCount);
                break;
            case BallCountOperation.Divide:
                RemoveBalls(_ballCount - _ballCount / bonus.BallCountChangeValue);
                break;
            case BallCountOperation.Set:
                int setDiff = bonus.BallCountChangeValue - _ballCount;
                if (setDiff > 0)
                {
                    AddFissionBalls(setDiff);
                }
                else if (setDiff < 0)
                {
                    RemoveBalls(-setDiff);
                }
                break;
            case BallCountOperation.Modulo:
                int modDiff = _ballCount - _ballCount % bonus.BallCountChangeValue;
                if (modDiff > 0)
                {
                    RemoveBalls(modDiff);
                }
                else if (modDiff < 0)
                {
                    AddFissionBalls(-modDiff);
                }
                break;
            default:
                break;
        }
    }

    // 添加分裂球
    private void AddFissionBalls(int count)
    {
        count = Math.Min(MaxBallCount - _ballCount, count);
        for (int i = 0; i < count; i++)
        {
            var index = new Random().Next(0, _ballCount);
            var pos = _balls.GetChild<Ball>(index).Position;
            AddFissionBall(pos);
        }
    }

    // 移除球
    private void RemoveBalls(int count)
    {
        count = Math.Min(_ballCount - 1, count);
        for (int i = 0; i < count; i++)
        {
            var ball = _balls.GetChild<Ball>(0);
            ball.QueueFree();
            OnBallOutOfPlayArea(ball);
        }
    }
}