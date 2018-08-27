﻿using System.Threading.Tasks;

namespace Lykke.Service.ClientAccountRecovery.Core
{
    public interface IWalletCredentialsRepository
    {
        Task<IWalletCredentials> GetAsync(string clientId);
    }
}
