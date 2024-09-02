using Helper;

namespace Menus;

public class Menu
{
    private string Title { get; set; }
    private readonly Dictionary<string, MenuItem> _menuItems = new();
    private readonly List<string> _reservedShortcuts = new() { "b", "x", "" };
    private const string MenuSeparator = "=================================";

    public Menu(string title, List<MenuItem> menuItems)
    {
        Title = title;
        foreach (var menuItem in menuItems)
        {
            if (_reservedShortcuts.Contains(menuItem.Shortcut.ToLower()))
            {
                throw new ApplicationException("This shortcut is already in use: " + menuItem.Shortcut);
            }
            if (string.IsNullOrWhiteSpace(menuItem.Shortcut))
            {
                throw new ApplicationException("Menu Item does not have shortcut: " + menuItem.Name);
            }
            _menuItems[menuItem.Shortcut] = menuItem;
        }
    }
    
    private void PrintOut(MenuLevel menuLevel)
    {
        Console.WriteLine(Title);
        Console.WriteLine(MenuSeparator);
        
        foreach (var menuItem in _menuItems)
        {
            Console.Write(menuItem.Key);
            Console.Write(") ");
            Console.WriteLine(menuItem.Value.MenuLabelFuction != null
                ? menuItem.Value.MenuLabelFuction()
                : menuItem.Value.Name);
        }
        
        switch (menuLevel)
        {
            case MenuLevel.Main:
            {
                WriteExit();
                break;
            }
            case MenuLevel.Second:
                WriteBack();
                WriteExit();
                break;
            case MenuLevel.None:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        Console.WriteLine(MenuSeparator);
        Console.WriteLine();
    }
    
    public string RunMenu(MenuLevel menuLevel)
    {
        Console.Clear();

        string userInput;
        do
        {
            PrintOut(menuLevel);
            userInput = ReadLine.ReadLineHelper("Your choice?", new string("string"), 1, new List<int>(), new List<string>(){"B","X","R","P","H","U","S","N","L"})[0];
            if (_menuItems.ContainsKey(userInput.ToUpper()))
            {
                if (_menuItems[userInput.ToUpper()].NextMenu == null) continue;
                var previousInput = _menuItems[userInput.ToUpper()].NextMenu!();
                Console.Clear();
                if (previousInput != null)
                    switch (previousInput.ToLower())
                    {
                        case "x":
                            userInput = "x";
                            break;
                        case "continue_game":
                            return previousInput;
                    }
            }

            else if (!_reservedShortcuts.Contains(userInput))
            {
                Console.WriteLine("No such shortcut: " + userInput);
            }
        } while (!_reservedShortcuts.Contains(userInput));

        {
            return userInput;
        }
    }
    private static void WriteExit()
    {
        Console.WriteLine("X) EXIT");
    }

    private static void WriteBack()
    {
        Console.WriteLine("B) BACK");
    }
}