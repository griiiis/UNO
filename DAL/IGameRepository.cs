using Domain;

namespace DAL;

public interface IGameRepository
{
    void SaveGame(Guid id, GameState game);
    GameState LoadGame(Guid id);
    
    List<(Guid id, DateTime dt)> GetSaveGames();
}