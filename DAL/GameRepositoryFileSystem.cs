using System.Text.Json;
using Domain;

namespace DAL;

public class GameRepositoryFileSystem : IGameRepository
{
    private readonly string _newPath =
        Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, @"..\..\..\..\Domain\SavedGames\"));

    public void SaveGame(Guid id, GameState game)
    {
        var jsonOptions = new JsonSerializerOptions()
        {
            IncludeFields = true,
            WriteIndented = true,
            AllowTrailingCommas = true,
        };
        var fileName = id + ".json";
        var jsonContent = JsonSerializer.Serialize(game, jsonOptions);
        File.WriteAllText(_newPath + fileName, jsonContent);
    }

    public GameState LoadGame(Guid id)
    {
        var jsonOptions = new JsonSerializerOptions()
        {
            IncludeFields = true,
            WriteIndented = true,
            AllowTrailingCommas = true,
        };
        var jsonContent = File.ReadAllText(_newPath + id + ".json");
        var ww = JsonSerializer.Deserialize<GameState>(jsonContent, jsonOptions)!;
        return ww;
    }

    public List<(Guid id, DateTime dt)> GetSaveGames()
    {
        var data = Directory.EnumerateFiles(_newPath);
        var res = data
            .Select(
                path => (
                    Guid.Parse(Path.GetFileNameWithoutExtension(path)),
                    File.GetLastWriteTime(path)
                )
            ).ToList();
        
        return res;
    }
}