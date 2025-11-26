using backend.Models;

namespace backend.Interfaces
{
    public interface IEmpresaRepository
    {
        string CreateEmpresa(EmpresaModel empresa);
        List<EmpresaModel> GetEmpresas();
        List<EmpresaModel> GetByOwner(int ownerId);
        EmpresaModel GetById(int id);
        EmpresaModel GetByEmployeeUserId(string userId);
        EmpresaModel GetByCedulaJuridica(long cedulaJuridica);
        int CheckCompanyPayroll(EmpresaModel empresa);
        int DeleteEmpresa(long cedulaJuridica, bool physicalDelete);
    }
}