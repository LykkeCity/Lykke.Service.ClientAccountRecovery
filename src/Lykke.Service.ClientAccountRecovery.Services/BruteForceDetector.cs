using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Service.ClientAccountRecovery.Core;
using Lykke.Service.ClientAccountRecovery.Core.Domain;
using Lykke.Service.ClientAccountRecovery.Core.Services;

namespace Lykke.Service.ClientAccountRecovery.Services
{
    public class BruteForceDetector : IBruteForceDetector
    {
        private readonly IStateRepository _stateRepository;
        private readonly IRecoveryFlowServiceFactory _factory;
        private readonly RecoveryConditions _recoveryConditions;
        private static readonly State[] UnsuccessfulStates;
        private static readonly State[] InProgressStates;

        static BruteForceDetector()
        {
            UnsuccessfulStates = new[] { State.PasswordChangeForbidden };
            InProgressStates = Enum.GetValues(typeof(State)).Cast<State>().Except
            (
                new[]
                {
                    State.PasswordChangeAllowed,
                    State.PasswordUpdated,
                    State.PasswordChangeSuspended,
                    State.PasswordChangeForbidden
                }
            ).ToArray();
        }

        public BruteForceDetector(IStateRepository stateRepository, IRecoveryFlowServiceFactory factory, RecoveryConditions recoveryConditions)
        {
            _stateRepository = stateRepository;
            _factory = factory;
            _recoveryConditions = recoveryConditions;
        }

        public async Task<bool> IsNewRecoveryAllowedAsync(string clientId)
        {
            var history = await _stateRepository.FindRecoverySummary(clientId);
            if (history == null)
            {
                return true;
            }

            if (NoOfLastUnsuccessfulStates(history) >= _recoveryConditions.MaxUnsuccessfulRecoveryAttempts)
            {
                return false;
            }

            return true;
        }

        private static int NoOfLastUnsuccessfulStates(RecoveriesSummaryForClient history)
        {
            var noOfLastBadStates = history.Log.OrderByDescending(l => l.ActualStatus.Time)
                .TakeWhile(l => UnsuccessfulStates.Contains(l.ActualStatus.State)).Count();
            return noOfLastBadStates;
        }

        public async Task<IReadOnlyCollection<RecoveryUnit>> GetRecoveriesToSeal(string clientId)
        {
            var history = await _stateRepository.FindRecoverySummary(clientId);

            if (history == null)
            {
                return Array.Empty<RecoveryUnit>();
            }

            return history.Log.Where(l => InProgressStates.Contains(l.ActualStatus.State)).ToArray();

        }
    }
}
