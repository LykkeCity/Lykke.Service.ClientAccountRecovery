using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.ClientAccountRecovery.Client.AutoRestClient.Models;

namespace Lykke.Service.ClientAccountRecovery.Client
{
    /// <summary>
    /// An interface of the client recovery service
    /// </summary>
    [PublicAPI]
    public interface IAccountRecoveryService
    {
        /// <summary>
        /// Checks service is alive
        /// </summary>
        /// <param name='cancellationToken'>
        /// The cancellation token.
        /// </param>
        Task<object> IsAliveAsync(CancellationToken cancellationToken = default(CancellationToken));


        /// <summary>
        /// Starts password recovering process
        /// </summary>
        /// <param name='request'>
        /// </param>
        /// <param name='cancellationToken'>
        /// The cancellation token.
        /// </param>
        /// <exception cref="BadRequestException"></exception>
        /// <exception cref="ConflictException"></exception>
        /// <exception cref="ServerErrorException"></exception>
        /// <exception cref="ForbiddenException"></exception>
        Task<NewRecoveryResponse> StartNewRecoveryAsync(NewRecoveryRequest request, CancellationToken cancellationToken = default(CancellationToken));


        /// <summary>
        /// Returns the current recovery state
        /// </summary>
        /// <param name='recoveryId'>
        /// Recovery Id
        /// </param>
        /// <param name='cancellationToken'>
        /// The cancellation token.
        /// </param>
        /// <exception cref="BadRequestException"></exception>
        /// <exception cref="NotFoundException"></exception>
        /// <exception cref="ServerErrorException"></exception>
        Task<RecoveryStatusResponse> GetRecoveryStatusAsync(string recoveryId, CancellationToken cancellationToken = default(CancellationToken));


        /// <summary>
        /// Accepts challenge values
        /// </summary>
        /// <param name='request'>
        /// </param>
        /// <param name='cancellationToken'>
        /// The cancellation token.
        /// </param>
        /// <exception cref="BadRequestException"></exception>
        /// <exception cref="ConflictException"></exception>
        /// <exception cref="ServerErrorException"></exception>
        /// <exception cref="NotFoundException"></exception>
        Task<OperationStatus> SubmitChallengeAsync(ChallengeRequest request, CancellationToken cancellationToken = default(CancellationToken));


        /// <summary>
        /// Updates the user password
        /// </summary>
        /// <param name='request'>
        /// </param>
        /// <param name='cancellationToken'>
        /// The cancellation token.
        /// </param>
        /// <exception cref="BadRequestException"></exception>
        /// <exception cref="ConflictException"></exception>
        /// <exception cref="ServerErrorException"></exception>
        /// <exception cref="NotFoundException"></exception>
        Task UpdatePasswordAsync(PasswordRequest request, CancellationToken cancellationToken = default(CancellationToken));


        /// <summary>
        /// Approves user challenges. Only for support.
        /// </summary>
        /// <param name='request'>
        /// </param>
        /// <param name='cancellationToken'>
        /// The cancellation token.
        /// </param>
        /// <exception cref="BadRequestException"></exception>
        /// <exception cref="ConflictException"></exception>
        /// <exception cref="ServerErrorException"></exception>
        /// <exception cref="NotFoundException"></exception>
        /// <exception cref="UnauthorizedException"></exception>
        Task ApproveChallengeAsync(ApproveChallengeRequest request, CancellationToken cancellationToken = default(CancellationToken));


        /// <summary>
        /// Updates current state of the recovery process. Only for support.
        /// </summary>
        /// <param name='request'>
        /// </param>
        /// <param name='cancellationToken'>
        /// The cancellation token.
        /// </param>
        /// <exception cref="BadRequestException"></exception>
        /// <exception cref="ConflictException"></exception>
        /// <exception cref="ServerErrorException"></exception>
        /// <exception cref="NotFoundException"></exception>
        /// <exception cref="UnauthorizedException"></exception>
        Task SubmitResolutionAsync(ResolutionRequest request , CancellationToken cancellationToken = default(CancellationToken));


        /// <summary>
        /// Returns brief information about all client's recoveries
        /// </summary>
        /// <param name='clientId'>
        /// The client id
        /// </param>
        /// <param name='cancellationToken'>
        /// The cancellation token.
        /// </param>
        /// <exception cref="BadRequestException"></exception>
        /// <exception cref="ServerErrorException"></exception>
        /// <exception cref="NotFoundException"></exception>
        Task<IList<ClientRecoveryHistoryResponse>> GetClientRecoveriesAsync(string clientId, CancellationToken cancellationToken = default(CancellationToken));


        /// <summary>
        /// Returns detailed information about the recovery
        /// </summary>
        /// <param name='recoveryId'>
        /// The recovery id
        /// </param>
        /// <param name='cancellationToken'>
        /// The cancellation token.
        /// </param>
        /// <exception cref="BadRequestException"></exception>
        /// <exception cref="ServerErrorException"></exception>
        /// <exception cref="NotFoundException"></exception>
        Task<IList<RecoveryTraceResponse>> GetRecoveryTraceAsync(string recoveryId, CancellationToken cancellationToken = default(CancellationToken));

    }
}
