using System.Collections.Generic;

namespace DDD.Scripts.Game.rock_paper_scissors.Network
{
    public class RockPeperBetRequest
    {
        public int betAmount { get; set; }
        public List<string> playerInitialHands { get; set; }
        public string sessionId { get; set; }
    }
}