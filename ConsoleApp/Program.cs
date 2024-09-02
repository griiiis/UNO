using ConsoleApp;
using ConsoleUI;
using DAL;
using Domain;
using Menus;
using Microsoft.EntityFrameworkCore;
using Engine;

//var gameRepository = new GameRepositoryFileSystem();

var connectionString = "DataSource=<%temp%>app.db;Cache=Shared";

connectionString = connectionString.Replace("<%temp%>", Path.GetTempPath());

var contextOptions = new DbContextOptionsBuilder<AppDbContext>()
    .UseSqlite(connectionString)
    .EnableDetailedErrors()
    .EnableSensitiveDataLogging()
    .Options;

var db = new AppDbContext(contextOptions);
db.Database.Migrate();
IGameRepository gameRepository = new GameRepositoryDb(db);

var mainMenu = ProgramMenus.MainMenu(NewGame, LoadGame);

//============MAIN============================
mainMenu.RunMenu(MenuLevel.Main);
return;


//=============NEW GAME=======================
string? NewGame()
{
    //PLAYERS
    var players = PlayerSetup.PlayerQuestions();

    //SETTINGS
    var settings = new Settings();
    ProgramMenus.SettingsMenu(settings).RunMenu(MenuLevel.None);

    //GAME LOGIC
    var unoEngine = new UnoEngine(settings, players);

    //CONSOLE CONTROLLER
    var gameController = new GameController(unoEngine, gameRepository);
    
    unoEngine.MakeNewDeck();
    unoEngine.GivePlayersTheirCards();
    unoEngine.GetFirstCard();
    
    gameController.RunGame();
    return null;
}

//============LOAD GAME=======================
string? LoadGame()
{
    Console.Clear();
    Console.WriteLine("Saved games");
    var saveGameList = gameRepository.GetSaveGames();
    var saveGameListDisplay = saveGameList.Select((s, i) => i + 1 + " - " + s).ToList();

    if (saveGameListDisplay.Count == 0)
    {
        Console.WriteLine("There are no saved games yet!");
        Console.Write("Press any button to continue");
        Console.ReadLine();
        return null;
    }

    Guid gameId;
    while (true)
    {
        Console.WriteLine(string.Join("\n", saveGameListDisplay));
        Console.Write($"Select game to load (1..{saveGameListDisplay.Count}):");
        Console.Write("\n\nB) BACK");
        var userChoiceStr = Console.ReadLine();
        if (int.TryParse(userChoiceStr, out var userChoice))
        {
            if (userChoice < 1 || userChoice > saveGameListDisplay.Count)
            {
                Console.WriteLine("Not in range...");
                continue;
            }

            gameId = saveGameList[userChoice - 1].id;
            Console.WriteLine($"Loading file: {gameId}");
            break;
        }

        if (string.IsNullOrWhiteSpace(userChoiceStr))
        {
            Console.WriteLine("Parse error...");
        }
        else if (userChoiceStr.ToLower() == "b")
        {
            return null;
        }
    }
    
    var gameState = gameRepository.LoadGame(gameId);
    var players = gameState.PlayerList;

    var gameEngine = new UnoEngine(gameState.GameSettings, players)
    {
        State = gameState
    };
    
    var gameController = new GameController(gameEngine, gameRepository);

    gameController.RunGame();

    return null;
}