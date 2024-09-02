using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using DAL;
using Domain;
using Domain.Database;
using Microsoft.EntityFrameworkCore;
using Engine;
using Player = Domain.Player;
using Settings = Domain.Settings;

namespace WepApp.Pages_Games
{
    public class CreateModel : PageModel
    {
        private readonly DAL.AppDbContext _context;

        public CreateModel(DAL.AppDbContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
            return Page();
        }
        [BindProperty(SupportsGet = true)] public string? PlayerCount { get; set; }
        [BindProperty(SupportsGet = true)] public Settings? Settings { get; set; }
        [BindProperty(SupportsGet = true)] public List<Player>? Players { get; set; }
        [BindProperty]
        public Game Game { get; set; } = default!;

        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public RedirectToPageResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                if (Request.Form["SelectedPlayerCount"].Count != 0)
                {
                   return RedirectToPage("/Games/Create",new { playerCount = int.Parse(Request.Form["SelectedPlayerCount"]!)});
                }if (Request.Form["PlayerType"].Count == Request.Form["FirstName"].Count)
                {
                    var players = new List<Player>();
                    var settings = new Settings();
                    for (int i = 0; i < Request.Form["PlayerType"].Count; i++)
                    {
                        players.Add(new Player(Request.Form["FirstName"][i]!, Request.Form["PlayerType"][i] == "1" ? PlayerType.Human : PlayerType.Ai));
                    } //players
                    settings.NumberOfRounds = int.Parse(Request.Form["NumberOfRounds"]!);
                    settings.PointsToWin = int.Parse(Request.Form["PointsToWin"]!);
                    settings.HandSize = int.Parse(Request.Form["HandSize"]!);
                    settings.MustSayUno = Request.Form["MustSayUno"] == "1";
                    //settings
                    var unoEngine = new UnoEngine(settings, players);
                    //Engine
                    var connectionString = "DataSource=<%temp%>app1.db;Cache=Shared";

                    connectionString = connectionString.Replace("<%temp%>", Path.GetTempPath());

                    var contextOptions = new DbContextOptionsBuilder<AppDbContext>()
                        .UseSqlite(connectionString)
                        .EnableDetailedErrors()
                        .EnableSensitiveDataLogging()
                        .Options;

                    var db = new AppDbContext(contextOptions);
                    db.Database.Migrate();
                    IGameRepository gameRepository = new GameRepositoryDb(db);
                    //Gamerepository
                    
                    unoEngine.MakeNewDeck();
                    unoEngine.GivePlayersTheirCards();
                    unoEngine.GetFirstCard();
                    gameRepository.SaveGame(unoEngine.State.Id, unoEngine.State);
                    return RedirectToPage("/Games/Index");
                }
                
            }
            return RedirectToPage("./Index");
        }
        
    }
}
