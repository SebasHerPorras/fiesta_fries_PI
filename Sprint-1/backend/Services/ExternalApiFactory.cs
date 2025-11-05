using backend.Interfaces;

namespace backend.Services
{
    public class ExternalApiFactory : IExternalApiFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public ExternalApiFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public ISolidarityAssociationService CreateSolidarityAssociationService()
        {
            return _serviceProvider.GetRequiredService<ISolidarityAssociationService>();
        }

        public IPrivateInsuranceService CreatePrivateInsuranceService()
        {
            return _serviceProvider.GetRequiredService<IPrivateInsuranceService>();
        }

        public IVoluntaryPensionsService CreateVoluntaryPensionsService()
        {
            return _serviceProvider.GetRequiredService<IVoluntaryPensionsService>();
        }
    }
}