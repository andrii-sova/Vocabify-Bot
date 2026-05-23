using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using VocabifyBot.Data;
using VocabifyBot.Interfaces;
using VocabifyBot.Services;
using VocabifyBot.Services.Handlers;

var config = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
    .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: false)
    .Build();

var botToken = config["BotToken"]!;
var openAiKey = config["OpenAiKey"]!;
var connectionString = config.GetConnectionString("Default")!;

var services = new ServiceCollection();
services.AddSingleton<ITelegramBotClient>(_ => new TelegramBotClient(botToken));
services.AddSingleton(_ => new DbContextOptionsBuilder<EnglishBotDbContext>().UseSqlServer(connectionString).Options);
services.AddSingleton<IDatabaseService, DatabaseService>();
services.AddSingleton<IOpenAiService>(_ => new OpenAiService(openAiKey));
services.AddSingleton<ConversationStateManager>();
services.AddSingleton<RegistrationHandler>();
services.AddSingleton<TeacherHandler>();
services.AddSingleton<StudentHandler>();
services.AddSingleton<WordEntryHandler>();
services.AddSingleton<QuizHandler>();
services.AddSingleton<BotService>();

using var provider = services.BuildServiceProvider();
var bot = provider.GetRequiredService<BotService>();

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    cts.Cancel();
};

await bot.StartAsync(cts.Token);
try
{
    await Task.Delay(Timeout.Infinite, cts.Token);
}
catch (TaskCanceledException)
{
}

Console.WriteLine("Bot stopped.");
