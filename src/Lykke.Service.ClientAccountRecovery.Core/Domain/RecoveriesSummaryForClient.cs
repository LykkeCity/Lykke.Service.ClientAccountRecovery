using System;
using System.Collections.Generic;
using System.Linq;

namespace Lykke.Service.ClientAccountRecovery.Core.Domain
{
    public class RecoveriesSummaryForClient
    {
        private readonly Dictionary<string, RecoveryUnit> _recoveryUnits = new Dictionary<string, RecoveryUnit>();
        public string ClientId { get; private set; }

        public RecoveriesSummaryForClient(string clientId)
        {
            ClientId = clientId;
        }

        public IReadOnlyList<RecoveryUnit> Log
        {
            get
            {
                if (_recoveryUnits.Count == 0)
                {
                    throw new InvalidOperationException("Client must contain at least one recovery record");
                }
                return _recoveryUnits.Values.ToArray();
            }
        }

        public void AddItem(RecoveryUnit unit)
        {
            _recoveryUnits[unit.RecoveryId] = unit;
        }

    }
}
