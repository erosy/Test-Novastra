

using System.Collections.Generic;
using System.Linq;

namespace NovastraTest
{
    public class BattleTeam
    {
        public UnitFactionType Faction {get;}
        public List<Unit> Units {get;} = new List<Unit>();

        public bool HasLivingUnits => Units.Any(u => u.IsAlive);
        public IEnumerable<Unit> LivingUnits => Units.Where(u => u.IsAlive);
    }
}