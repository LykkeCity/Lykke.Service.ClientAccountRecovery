using System.Collections.Generic;
using System.Linq;

namespace Lykke.Service.ClientAccountRecovery.Core.Domain
{
    public class RecoveryUnit
    {
        public readonly IReadOnlyCollection<RecoveryContext> Log;
        public bool Empty => Log.Count == 0;
        public string RecoveryId => Log.First().RecoveryId;
        public string ClientId => Log.First().ClientId;
        public RecoveryContext ActualStatus => Log.Last();
        public RecoveryUnit(IReadOnlyCollection<RecoveryContext> log)
        {
            Log = log;
        }
    }
}