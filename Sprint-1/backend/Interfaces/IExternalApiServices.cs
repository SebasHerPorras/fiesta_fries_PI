using backend.Models;

namespace backend.Interfaces
{
    public interface ISolidarityAssociationService
    {
        Task<ExternalApiResponse> CalculateContributionAsync(SolidarityAssociationRequest request);
    }

    public interface IPrivateInsuranceService
    {
        Task<ExternalApiResponse> CalculatePremiumAsync(PrivateInsuranceRequest request);
    }

    public interface IVoluntaryPensionsService
    {
        Task<ExternalApiResponse> CalculatePremiumAsync(VoluntaryPensionsRequest request);
    }

    public interface IExternalApiFactory
    {
        ISolidarityAssociationService CreateSolidarityAssociationService();
        IPrivateInsuranceService CreatePrivateInsuranceService();
        IVoluntaryPensionsService CreateVoluntaryPensionsService();
    }
}