﻿namespace Lykke.Service.ClientAccountRecovery.Models
{
    public class NewRecoveryResponse
    {
        /// <summary>
        ///     JWE token containing current state of recovery process.
        /// </summary>
        public string StateToken { get; set; }
    }
}
