using backend.Models;

namespace backend.Interfaces
{
    public interface IBeneficioService
    {
        string CreateBeneficio(BeneficioModel beneficio);
        string UpdateBeneficio(int id, BeneficioModel beneficio);
        List<BeneficioModel> GetAll();
        List<BeneficioModel> GetByEmpresa(long companyId);
        BeneficioModel GetById(int id);
        string DeleteBeneficio(int benefitId);
    }
}
