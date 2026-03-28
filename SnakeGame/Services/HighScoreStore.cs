using System.IO;
using System.Text.Json;

namespace SnakeGame.Services;

public sealed class HighScoreStore
{
    private readonly string _filePath;

    public HighScoreStore()
    {
        var dir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "SnakeGame");
        Directory.CreateDirectory(dir);
        _filePath = Path.Combine(dir, "highscore.json");
    }

    public int Load()
    {
        try
        {
            if (!File.Exists(_filePath))
                return 0;
            var json = File.ReadAllText(_filePath);
            var data = JsonSerializer.Deserialize<HighScoreData>(json);
            return data?.BestScore ?? 0;
        }
        catch
        {
            return 0;
        }
    }

    public void SaveIfBetter(int score)
    {
        var current = Load();
        if (score <= current)
            return;
        var data = new HighScoreData { BestScore = score };
        var json = JsonSerializer.Serialize(data);
        File.WriteAllText(_filePath, json);
    }

    private sealed class HighScoreData
    {
        public int BestScore { get; set; }
    }
}
