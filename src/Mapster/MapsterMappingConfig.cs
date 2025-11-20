using GateEntryExit.Domain;
using GateEntryExit.Dtos.Gate;
using Mapster;

namespace GateEntryExit.Mapster
{
    public static class MapsterMappingConfig
    {
        public static void ConfigureMappings()
        {
            var config = TypeAdapterConfig.GlobalSettings;
            config.NewConfig<Gate, GateDto>()
                .Map(dest => dest.MapsterName, src => src.Name);
        }
    }
}
