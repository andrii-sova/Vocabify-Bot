using VocabifyBot.Interfaces;
using VocabifyBot.Models;
using VocabifyBot.UI;
using Telegram.Bot;

namespace VocabifyBot.Services.Handlers;

public abstract class HandlerBase
{
    protected readonly ITelegramBotClient Bot;
    protected readonly IDatabaseService Db;
    protected readonly ConversationStateManager States;

    protected HandlerBase(ITelegramBotClient bot, IDatabaseService db, ConversationStateManager states)
    {
        Bot = bot;
        Db = db;
        States = states;
    }

    protected ConversationState GetState(long userId) => States.Get(userId);

    protected void SetState(long userId, ConversationState state) => States.Set(userId, state);

    protected void ResetState(long userId) => States.Reset(userId);

    protected void MutateState(long userId, Action<ConversationState> mutate) => States.Mutate(userId, mutate);

    protected async Task SendMenuAsync(long chatId, long userId, string role, CancellationToken ct)
    {
        if (role == "Student" && !await Db.IsStudentLinkedToAnyTeacherAsync(userId))
        {
            await Bot.SendMessage(chatId,
                "🔒 You haven't been added to a teacher's group yet.\n\n" +
                "Please ask your teacher to add you by your Telegram username.",
                cancellationToken: ct);
            return;
        }

        await Bot.SendMessage(
            chatId,
            role == "Teacher" ? "👨‍🏫 Teacher Menu" : "📚 Student Menu",
            replyMarkup: role == "Teacher" ? Keyboards.TeacherMenu() : Keyboards.StudentMenu(),
            cancellationToken: ct);
    }

    protected async Task SendMenuAsync(long chatId, string role, CancellationToken ct) =>
        await Bot.SendMessage(
            chatId,
            role == "Teacher" ? "👨‍🏫 Teacher Menu" : "📚 Student Menu",
            replyMarkup: role == "Teacher" ? Keyboards.TeacherMenu() : Keyboards.StudentMenu(),
            cancellationToken: ct);

    protected async Task GoMenuAsync(long userId, long chatId, CancellationToken ct)
    {
        var user = await Db.GetUserAsync(userId);
        await SendMenuAsync(chatId, userId, user?.Role ?? "Student", ct);
    }
}
