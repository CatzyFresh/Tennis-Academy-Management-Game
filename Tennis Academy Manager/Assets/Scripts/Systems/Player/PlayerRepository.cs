using System.Collections.Generic;
using System.Linq;

namespace TennisAcademyManager.Systems.Players
{
    public sealed class PlayerRepository
    {
        private readonly Dictionary<string, PlayerInstance> players = new();

        public IReadOnlyDictionary<string, PlayerInstance> All => players;

        public void Add(PlayerInstance p) => players[p.Id] = p;

        public bool TryGet(string id, out PlayerInstance p) => players.TryGetValue(id, out p);

        public IEnumerable<PlayerInstance> ActivePlayers()
            => players.Values.Where(p => p.Status == PlayerStatus.Active);

        public IEnumerable<PlayerInstance> ActiveBySegment(Systems.PlayerSegment segment)
            => players.Values.Where(p => p.Status == PlayerStatus.Active && p.Segment == segment);

        public bool Remove(string id) => players.Remove(id);
    }
}
