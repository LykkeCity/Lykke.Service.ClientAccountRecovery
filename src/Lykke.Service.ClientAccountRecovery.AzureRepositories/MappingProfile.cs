using AutoMapper;
using JetBrains.Annotations;
using Lykke.Service.ClientAccountRecovery.Core.Domain;

namespace Lykke.Service.ClientAccountRecovery.AzureRepositories
{
    [UsedImplicitly]
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<LogTableEntity, RecoveryContext>()
                .ForMember(m => m.KycPassed, o => o.Ignore())
                .ForMember(m => m.HasPhoneNumber, o => o.Ignore())
                .ForMember(m => m.PinKnown, o => o.Ignore())
                .ForMember(m => m.PublicKeyKnown, o => o.Ignore());
            CreateMap<RecoveryContext, LogTableEntity>()
                .ForMember(m => m.RowKey, o => o.MapFrom(s => LogTableEntity.GetRowKey(s.SeqNo)))
                .ForMember(m => m.PartitionKey, o => o.MapFrom(s => LogTableEntity.GetPartitionKey(s.RecoveryId)))
                .ForMember(m => m.Timestamp, o => o.Ignore())
                .ForMember(m => m.ETag, o => o.Ignore());
        }
    }
}
