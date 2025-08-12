using Godot;
using System;
using System.Text.Json.Nodes;

public partial class UIGlobal : Node
{
    public static UIGlobal Instance { get; private set; }

    public PackedScene GameStartScene { get; private set; }
    public PackedScene GameOptionsScene { get; private set; }
    public PackedScene GamePauseScene { get; private set; }
    public PackedScene GameEndScene { get; private set; }

    public string Lang { get; set; } = "zh";
    public bool Fullscreen { get; set; } = false;

    private static readonly string OptionsFilePath = "user://options.json";

    public override void _Ready()
    {
        base._Ready();

        Instance = this;

        GameStartScene = GD.Load<PackedScene>("res://Scenes/UI/GameStart/GameStart.tscn");
        GameOptionsScene = GD.Load<PackedScene>("res://Scenes/UI/GameOptions/GameOptions.tscn");
        GamePauseScene = GD.Load<PackedScene>("res://Scenes/UI/GamePause/GamePause.tscn");
        GameEndScene = GD.Load<PackedScene>("res://Scenes/UI/GameEnd/GameEnd.tscn");

        LoadOptions();
    }

    public void SaveOptions()
    {
        var options = new JsonObject
        {
            ["lang"] = Lang,
            ["fullscreen"] = Fullscreen
        };
        FileAccess file = FileAccess.Open(OptionsFilePath, FileAccess.ModeFlags.Write);
        if (file != null)
        {
            file.StoreString(options.ToJsonString());
            file.Close();
        }
    }

    private void LoadOptions()
    {
        FileAccess file = FileAccess.Open(OptionsFilePath, FileAccess.ModeFlags.Read);
        if (file != null)
        {
            string json = file.GetAsText();
            var root = JsonNode.Parse(json);

            var lang = (string?)root?["lang"] ?? "zh";
            TranslationServer.SetLocale(lang);

            var fullscreen = (bool?)root?["fullscreen"] ?? false;
            if (fullscreen)
            {
                DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);
            }
        }
    }
}
