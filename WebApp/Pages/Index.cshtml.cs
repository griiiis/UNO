using DAL;
using Domain.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace WepApp.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly AppDbContext _ctx;

    public IndexModel(ILogger<IndexModel> logger, AppDbContext ctx)
    {
        _logger = logger;
        _ctx = ctx;
    }

    public int Count { get; set; }
    
    public IList<Game> Game { get;set; } = default!;
    
    
    public async Task OnGetAsync()
    {
        Count = _ctx.Games.Count();
        
        Game = await _ctx.Games
            .Include(g => g.Players)
            .OrderByDescending(g => g.UpdatedAtDt)
            .ToListAsync();
    }
}