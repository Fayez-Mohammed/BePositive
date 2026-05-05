// Base.Services/Implementations/PresenceTracker.cs
//
// Singleton — tracks which userIds are currently connected to the SignalR hub.
// Register as: services.AddSingleton<PresenceTracker>();

namespace Base.Services.Implementations
{
    public class PresenceTracker
    {
        // userId → set of connectionIds (a user can have multiple browser tabs)
        private readonly Dictionary<string, HashSet<string>> _connections
            = new(StringComparer.OrdinalIgnoreCase);

        private readonly object _lock = new();

        public void UserConnected(string userId, string connectionId)
        {
            lock (_lock)
            {
                if (!_connections.TryGetValue(userId, out var conns))
                {
                    conns = new HashSet<string>();
                    _connections[userId] = conns;
                }
                conns.Add(connectionId);
            }
        }

        public bool UserDisconnected(string userId, string connectionId)
        {
            lock (_lock)
            {
                if (!_connections.TryGetValue(userId, out var conns))
                    return false;

                conns.Remove(connectionId);

                if (conns.Count == 0)
                {
                    _connections.Remove(userId);
                    return true; // fully offline
                }
                return false;
            }
        }

        public bool IsOnline(string userId)
        {
            lock (_lock)
                return _connections.ContainsKey(userId) &&
                       _connections[userId].Count > 0;
        }

        public List<string> GetOnlineUserIds()
        {
            lock (_lock)
                return _connections.Keys.ToList();
        }
    }
}
