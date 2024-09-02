namespace Menus;

public class MenuItem
{
    public string Name { get; set; } = default!;
    public Func<string>? MenuLabelFuction { get; set; }
    public string Shortcut { get; set; } = default!;
    public Func<string?>? NextMenu { get; set; }
}