using Domain;
using Menus;

namespace ConsoleApp;

public static class ProgramMenus
{
    public static Menu MainMenu(Func<string?> newGame, Func<string?> loadGame) => new Menu("MAIN MENU", new List<MenuItem>()
        {
            new MenuItem()
            {
                Name = "START NEW GAME",
                Shortcut = "N",
                NextMenu = newGame
            },
            new MenuItem()
            {
                Name = "LOAD",
                Shortcut = "L",
                NextMenu = loadGame
            }
        }
    );

    public static Menu SettingsMenu(Settings settings) => new Menu("SETTINGS", new List<MenuItem>()
    {
        new MenuItem()
        {
            Shortcut = "R",
            MenuLabelFuction = () => "Number Of Rounds: " + settings.NumberOfRounds,
            NextMenu = () => ChangeSettings.ChangeNumberOfRounds(settings),
        },
        new MenuItem()
        {
            Shortcut = "P",
            MenuLabelFuction = () => "Points To Win: " + settings.PointsToWin,
            NextMenu = () => ChangeSettings.ChangePointsToWin(settings),
        }, 
        new MenuItem()
        {
            Name = "Hand Size",
            Shortcut = "H",
            MenuLabelFuction = () => "Hand Size: " + settings.HandSize + " cards",
            NextMenu = () => ChangeSettings.ChangeHandSize(settings),
        }, 
        new MenuItem()
        {
            Shortcut = "U",
            MenuLabelFuction = () => "Must Say UNO: " + (settings.MustSayUno ? "True" : "False"),
            NextMenu = () => ChangeSettings.ChangeUno(settings),
        },
        new MenuItem()
        {
            Name = "Start The Game",
            Shortcut = "S",
            NextMenu = () => "continue_game",
        },
    });
}