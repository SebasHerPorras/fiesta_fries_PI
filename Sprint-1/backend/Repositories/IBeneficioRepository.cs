using backend.Models;

namespace backend.Repositories
{
    public interface IBeneficioRepository
    {
        string CreateBeneficio(BeneficioModel beneficio);
        List<BeneficioModel> GetAll();
        List<BeneficioModel> GetByEmpresa(long cedulaJuridica);
        BeneficioModel GetById(int id);
        bool Update(BeneficioModel beneficio);
        bool ExistsInEmployerBenefitDeductions(int benefitId);
        void PhysicalDeletion(int benefitId);
        void LogicalDeletion(int benefitId, DateTime? lastPeriod);
    }
}
