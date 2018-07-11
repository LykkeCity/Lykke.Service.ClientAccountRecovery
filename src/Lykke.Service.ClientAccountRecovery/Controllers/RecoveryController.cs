﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Service.ClientAccount.Client;
using Lykke.Service.ClientAccountRecovery.Core;
using Lykke.Service.ClientAccountRecovery.Core.Domain;
using Lykke.Service.ClientAccountRecovery.Core.Services;
using Lykke.Service.ClientAccountRecovery.Middleware;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;
using Lykke.Service.ClientAccountRecovery.Models;

namespace Lykke.Service.ClientAccountRecovery.Controllers
{

    [Route("api/recovery")]
    [Produces("application/json")]
    public class RecoveryController : Controller
    {
        private readonly IStateRepository _stateRepository;
        private readonly IRecoveryLogRepository _logRepository;
        private readonly IRecoveryFlowServiceFactory _factory;
        private readonly IClientAccountClient _clientAccountClient;
        private readonly IChallengeManager _challengeManager;
        private readonly IBrutForceDetector _brutForceDetector;
        private readonly ILog _log;

        public RecoveryController(IStateRepository stateRepository,
            IRecoveryLogRepository logRepository,
            IRecoveryFlowServiceFactory factory,
            IClientAccountClient clientAccountClient,
            IChallengeManager challengeManager,
            IBrutForceDetector brutForceDetector,
            ILog log)
        {
            _stateRepository = stateRepository;
            _logRepository = logRepository;
            _factory = factory;
            _clientAccountClient = clientAccountClient;
            _challengeManager = challengeManager;
            _brutForceDetector = brutForceDetector;
            _log = log.CreateComponentScope(nameof(RecoveryController));
        }

        /// <summary>
        /// Starts password recovering process
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [SwaggerOperation("StartNewRecovery")]
        [ProducesResponseType(typeof(NewRecoveryResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Conflict)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        public async Task<IActionResult> Index([FromBody]NewRecoveryRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _brutForceDetector.BlockPreviousRecoveries(request.ClientId, request.Ip, request.UserAgent);

            if (!await _brutForceDetector.IsNewRecoveryAllowedAsync(request.ClientId))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, "Recovery attempts limits reached");
            }

            var flow = await _factory.InitiateNew(request.ClientId);
            flow.Context.Initiator = Consts.InitiatorUser;
            flow.Context.Ip = request.Ip;
            flow.Context.UserAgent = request.UserAgent;

            try
            {
                await flow.StartRecoveryAsync();
            }
            catch (InvalidActionException ex)
            {
                _log.WriteWarning("StartNewRecovery", request, "Unable to start new account recovery process", ex);
                return Conflict(ex.Message);
            }

            return Ok(new NewRecoveryResponse { RecoveryId = flow.Context.RecoveryId });
        }

        /// <summary>
        /// Returns the current recovery state
        /// </summary>
        /// <param name="recoveryId">Recovery Id</param>
        /// <returns></returns>
        [HttpGet("{recoveryId}")]
        [SwaggerOperation("GetRecoveryStatus")]
        [ProducesResponseType(typeof(RecoveryStatusResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetRecoveryStatus([FromRoute]string recoveryId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var flow = await _factory.FindExisted(recoveryId);
            if (flow == null)
            {
                return NotFound();
            }
            await flow.TryUnfreezeAsync();
            var challenge = flow.Context.State.MapToChallenge();
            var progress = flow.Context.State.MapToProgress();
            var message = flow.Context.SignChallengeMessage;
            return Ok(new RecoveryStatusResponse { Challenge = challenge, OverallProgress = progress, ChallengeInfo = message });
        }


        /// <summary>
        /// Accepts challenge values
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("challenge")]
        [SwaggerOperation("SubmitChallenge")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType((int)HttpStatusCode.Conflict)]
        public async Task<IActionResult> PostChallenge([FromBody]ChallengeRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var flow = await _factory.FindExisted(request.RecoveryId);
            if (flow == null)
            {
                return NotFound();
            }

            flow.Context.Initiator = Consts.InitiatorUser;
            flow.Context.Ip = request.Ip;
            flow.Context.UserAgent = request.UserAgent;
            try
            {
                await _challengeManager.ExecuteAction(request.Challenge, request.Action, request.Value, flow);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidActionException ex)
            {
                return Conflict(ex.Message);
            }
            return Ok();
        }

        /// <summary>
        /// Updates the user password
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("password")]
        [SwaggerOperation("UpdatePassword")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType((int)HttpStatusCode.Conflict)]
        public async Task<IActionResult> PostPassword([FromBody]PasswordRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var flow = await _factory.FindExisted(request.RecoveryId);
            if (flow == null)
            {
                return NotFound();
            }

            if (!flow.IsPasswordUpdateAllowed)
            {
                return Conflict($"Unable to update the password from this state: {flow.Context.State}");
            }

            try
            {

                flow.Context.Initiator = Consts.InitiatorUser;
                flow.Context.Comment = null;
                flow.Context.Ip = request.Ip;
                flow.Context.UserAgent = request.UserAgent;
                await _clientAccountClient.ChangeClientPasswordAsync(flow.Context.ClientId, request.PasswordHash);
                await flow.UpdatePasswordCompleteAsync();
            }
            catch (InvalidActionException ex)
            {
                return Conflict(ex.Message);
            }

            return Ok();
        }

        /// <summary>
        /// Approves user challenges. Only for support.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut("challenge/challenge/checkResult")]
        [SwaggerOperation("ApproveChallenge")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ApiKeyAuth]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType((int)HttpStatusCode.Conflict)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        public async Task<IActionResult> ApproveChallenge([FromBody]ApproveChallengeRequest request)
        {
            if (!ModelState.IsValid || request.CheckResult == CheckResult.Unknown || request.Challenge != Core.Domain.Challenge.Selfie)
            {
                return BadRequest(ModelState);
            }

            var flow = await _factory.FindExisted(request.RecoveryId);
            if (flow == null)
            {
                return NotFound();
            }

            try
            {
                flow.Context.Initiator = request.AgentId;
                flow.Context.Ip = null;
                flow.Context.UserAgent = null;
                if (request.CheckResult == CheckResult.Approved)
                {
                    await flow.SelfieVerificationCompleteAsync(); // In a moment supporting only selfie
                }
                else
                {
                    await flow.SelfieVerificationFailAsync();
                }
            }
            catch (InvalidActionException ex)
            {
                return Conflict(ex.Message);
            }

            return Ok();
        }

        //[Authorize]
        /// <summary>
        /// Updates current state of the recovery process. Only for support.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("challenge/resolution")]
        [SwaggerOperation("SubmitResolution")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ApiKeyAuth]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType((int)HttpStatusCode.Conflict)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        public async Task<IActionResult> SubmitResolution([FromBody]ResolutionRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var flow = await _factory.FindExisted(request.RecoveryId);
            if (flow == null)
            {
                return NotFound();
            }

            try
            {
                flow.Context.Initiator = request.AgentId;
                flow.Context.Comment = request.Comment;
                flow.Context.Ip = null;
                flow.Context.UserAgent = null;
                switch (request.Resolution)
                {
                    case Resolution.Suspend:
                        await flow.JumpToSuspendAsync();
                        break;
                    case Resolution.Interview:
                        await flow.JumpToSupportAsync();
                        break;
                    case Resolution.Allow:
                        await flow.JumpToAllowAsync();
                        break;
                    default:
                        return BadRequest();
                }
            }
            catch (InvalidActionException ex)
            {
                return Conflict(ex.Message);
            }
            return Ok();
        }

        //[Authorize]
        /// <summary>
        /// Returns brief information about all client's recoveries
        /// </summary>
        /// <param name="clientId">The client id</param>
        /// <returns></returns>
        [HttpGet("client/{clientId}")]
        [SwaggerOperation("GetClientRecoveries")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(IEnumerable<ClientRecoveryHistoryResponse>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetClientRecoveries([FromRoute]string clientId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var summary = await _stateRepository.FindRecoverySummary(clientId);

            if (summary == null)
            {
                return NotFound();
            }

            var result = from unit in summary.Log
                         let actual = unit.ActualStatus
                         select new ClientRecoveryHistoryResponse
                         {
                             Initiator = actual.Initiator,
                             RecoveryId = actual.RecoveryId,
                             State = actual.State,
                             Time = actual.Time
                         };
            return Ok(result);
        }

        /// <summary>
        /// Returns detailed information about the recovery
        /// </summary>
        /// <param name="recoveryId">The recovery id</param>
        /// <returns></returns>
        [HttpGet("client/trace/{recoveryId}")]
        [SwaggerOperation("GetRecoveryTrace")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(IEnumerable<RecoveryTraceResponse>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetRecoveryTrace([FromRoute]string recoveryId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var unit = await _logRepository.GetAsync(recoveryId);
            if (unit.Empty)
            {
                return NotFound();
            }

            var response = RecoveryTraceResponse.Convert(unit);

            return Ok(response);
        }
    }
}
