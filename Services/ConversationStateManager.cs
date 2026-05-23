using System.Collections.Concurrent;
using VocabifyBot.Models;

namespace VocabifyBot.Services;

public sealed class ConversationStateManager
{
    private readonly ConcurrentDictionary<long, ConversationState> _states = new();

    public ConversationState Get(long userId) => _states.TryGetValue(userId, out var s) ? s : new ConversationState();

    public void Set(long userId, ConversationState state) => _states[userId] = state;

    public void Reset(long userId) => _states[userId] = new ConversationState();

    public void Mutate(long userId, Action<ConversationState> mutate)
    {
        var s = Get(userId);
        mutate(s);
        Set(userId, s);
    }
}
