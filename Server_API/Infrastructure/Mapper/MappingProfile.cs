using AutoMapper;
using Server_API.Domain.Model.BB;

namespace Server_API.Infrastructure.Mapper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<ProcessedData, RecoveredData>();
            CreateMap<RecoveredData, ProcessedData>();
            // Adicione outros mapeamentos conforme necessário
        }
    }
}