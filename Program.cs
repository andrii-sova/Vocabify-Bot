using VocabifyBot.Data;
using VocabifyBot.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Telegram.Bot;

var config = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
    .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: false)
    .Build();

var botToken         = config["BotToken"]!;
var openAiKey        = config["OpenAiKey"]!;
var connectionString = config.GetConnectionString("Default")!;

var dbOptions = new DbContextOptionsBuilder<EnglishBotDbContext>()
    .UseSqlServer(connectionString)
    .LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information)
    .EnableSensitiveDataLogging()
    .Options;

var bot     = new TelegramBotClient(botToken);
var db      = new DatabaseService(dbOptions);
var openAi  = new OpenAiService(openAiKey);
var service = new BotService(bot, db, openAi);

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };

await service.StartAsync(cts.Token);
//
try { await Task.Delay(Timeout.Infinite, cts.Token); }
catch (TaskCanceledException) { }

Console.WriteLine("Bot stopped.");
