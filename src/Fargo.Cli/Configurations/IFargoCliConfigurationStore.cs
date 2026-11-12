namespace Fargo.Cli.Configurations;

public interface IFargoCliConfigurationStore
{
    FargoCliConfiguration Load();

    void Save(FargoCliConfiguration configuration);
}
