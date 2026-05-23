using VocabifyBot.Interfaces;
using VocabifyBot.Models;
using VocabifyBot.UI;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using TelegramUser = Telegram.Bot.Types.User;

namespace VocabifyBot.Services.Handlers;

public sealed class RegistrationHandler : HandlerBase
{
    public RegistrationHandler(ITelegramBotClient bot, IDatabaseService db, ConversationStateManager states)
        : base(bot, db, states)
    {
    }

    public Task ShowRoleSelectionAsync(long chatId, CancellationToken ct) =>
        Bot.SendMessage(
            chatId,
            "👋 Welcome to *EnglishBot*!\n\nAre you a teacher or a student?",
            parseMode: ParseMode.Markdown,
            replyMarkup: Keyboards.RoleSelection(),
            cancellationToken: ct);

    public Task ShowMainMenuAsync(long chatId, string role, CancellationToken ct) => SendMenuAsync(chatId, role, ct);

    public Task GoToMenuAsync(long userId, long chatId, CancellationToken ct) => GoMenuAsync(userId, chatId, ct);

    public async Task HandleCallbackAsync(string data, TelegramUser from, long chatId, CancellationToken ct)
    {
        switch (data)
        {
            case "role_teacher":
                await RegisterUserAsync(from.Id, from, "Teacher", chatId, ct);
                break;
            case "role_student":
                await RegisterStudentAsync(from.Id, from, chatId, ct);
                break;
            case "menu_set_name":
                await HandleSetNameCallbackAsync(from.Id, chatId, ct);
                break;
            case "back_to_menu":
                ResetState(from.Id);
                await GoMenuAsync(from.Id, chatId, ct);
                break;
        }
    }

    public async Task HandleDisplayNameInputAsync(long userId, long chatId, string name, CancellationToken ct)
    {
        if (name.Length > 60)
        {
            await Bot.SendMessage(chatId, "❌ Max 60 characters. Please try again:", cancellationToken: ct);
            return;
        }

        await Db.UpdateDisplayNameAsync(userId, name);
        ResetState(userId);
        await Bot.SendMessage(
            chatId,
            $"✅ Display name set to *{WordFormatter.EscapeMarkdown(name)}*!",
            parseMode: ParseMode.Markdown,
            cancellationToken: ct);

        await GoMenuAsync(userId, chatId, ct);
    }

    public async Task HandleStudentUsernameInputAsync(long teacherId, long chatId, string input, CancellationToken ct)
    {
        var username = input.TrimStart('@').Trim();
        var student  = await Db.GetUserByUsernameAsync(username);

        ResetState(teacherId);

        if (student is not null && student.Role == "Student")
        {
            // Student already activated — link directly
            await Db.LinkTeacherStudentAsync(teacherId, student.TelegramId);
            await Bot.SendMessage(chatId,
                $"✅ *{WordFormatter.EscapeMarkdown(student.DisplayName)}* added to your list!",
                parseMode: ParseMode.Markdown, cancellationToken: ct);
        }
        else
        {
            // Student hasn't started the bot yet — save a pending invitation
            await Db.AddPendingInvitationAsync(teacherId, username);
            await Bot.SendMessage(chatId,
                $"⏳ @{WordFormatter.EscapeMarkdown(username)} hasn't started the bot yet.\n" +
                "They are added as *awaiting activation* and will be linked automatically when they join.",
                parseMode: ParseMode.Markdown, cancellationToken: ct);
        }

        await SendMenuAsync(chatId, "Teacher", ct);
    }

    private async Task RegisterUserAsync(long userId, TelegramUser from, string role, long chatId, CancellationToken ct)
    {
        await Db.UpsertUserAsync(new User
        {
            TelegramId = userId,
            Username = from.Username ?? string.Empty,
            FirstName = from.FirstName,
            Role = role
        });

        ResetState(userId);
        await SendMenuAsync(chatId, role, ct);
    }

    private async Task RegisterStudentAsync(long userId, TelegramUser from, long chatId, CancellationToken ct)
    {
        await Db.UpsertUserAsync(new User
        {
            TelegramId = userId,
            Username   = from.Username ?? string.Empty,
            FirstName  = from.FirstName,
            Role       = "Student"
        });

        // Auto-link to any teacher who added this student before they joined
        if (!string.IsNullOrEmpty(from.Username))
            await Db.ClaimPendingInvitationsAsync(userId, from.Username);

        ResetState(userId);
        await SendMenuAsync(chatId, "Student", ct);
    }

    private async Task HandleSetNameCallbackAsync(long userId, long chatId, CancellationToken ct)
    {
        var user = await Db.GetUserAsync(userId);
        SetState(userId, new ConversationState { State = UserState.AwaitingDisplayName });

        await Bot.SendMessage(
            chatId,
            $"✏️ Enter your display name (current: *{WordFormatter.EscapeMarkdown(user?.DisplayName ?? "-")}*):",
            parseMode: ParseMode.Markdown,
            replyMarkup: Keyboards.BackButton("back_to_menu"),
            cancellationToken: ct);
    }
}
