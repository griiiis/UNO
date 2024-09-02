using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Domain;
using Domain.Database;

namespace DAL;

public class GameRepositoryDb : IGameRepository
{

    private readonly AppDbContext _ctx;

    public GameRepositoryDb(AppDbContext ctx)
    {
        _ctx = ctx;
    }
    public void SaveGame(Guid id, GameState state)
    {
        var jsonOptions = new JsonSerializerOptions()
        {
            IncludeFields = true,
            WriteIndented = true,
            AllowTrailingCommas = true,
        };
        
         //is it already in db?
        
         var game = _ctx.Games.FirstOrDefault(game1 => game1.Id == state.Id);
         if (game == null)
         {
             game = new Game()
             {
                 Id = state.Id,
                 State = JsonSerializer.Serialize(state, jsonOptions),
                 Players = state.PlayerList.Select(p => new Domain.Database.Player()
                 {
                     Id = p.Id,
                     NickName = p.Nickname,
                     PlayerType = p.PlayerType
                 }).ToList()

             };
             _ctx.Games.Add(game);
         }
         else
         {
             game.UpdatedAtDt = DateTime.Now;
             game.State = JsonSerializer.Serialize(state, jsonOptions);
         }

         var changeCount = _ctx.SaveChanges();
         //Console.WriteLine("SaveChanges: " + changeCount);
    }
    
    public GameState LoadGame(Guid id)
    {
        var jsonOptions = new JsonSerializerOptions()
        {
            IncludeFields = true,
            WriteIndented = true,
            AllowTrailingCommas = true,
        };
        var game = _ctx.Games.First(g => g.Id == id);
        return JsonSerializer.Deserialize<GameState>(game.State, jsonOptions)!;
    }

    public List<(Guid id, DateTime dt)> GetSaveGames()
    {
        return _ctx.Games.OrderByDescending(g => g.UpdatedAtDt).ToList().Select(g => (g.Id, g.UpdatedAtDt)).ToList();
    }
}