using NUnit.Framework;
using Moq;
using System.Threading.Tasks;
using backend.Services;
using backend.Models;
using backend.Interfaces;
using System.Collections.Generic;

namespace backend_test
{
    public class EmployeeBenefitServiceTests
    {
        [Test]
        public async Task SaveSelectionAsyncCorrectlySavesAndSetsFields()
        {
            var mockRepo = new Mock<IEmployeeBenefitRepository>();
            var mockEmpleadoRepo = new Mock<IEmpleadoRepository>();

            var entity = new EmployeeBenefit { EmployeeId = 1, BenefitId = 2 };

            mockRepo.Setup(r => r.CanEmployeeSelectBenefitAsync(1, 2)).ReturnsAsync(true);
            mockRepo.Setup(r => r.GetBeneficioByIdAsync(2)).ReturnsAsync(new BeneficioModel { Nombre = "Ejemplo", Valor = 9000, Tipo = "Monto Fijo" });
            mockRepo.Setup(r => r.SaveSelectionAsync(It.IsAny<EmployeeBenefit>())).ReturnsAsync(true);

            var svc = new EmployeeBenefitService(mockRepo.Object, mockEmpleadoRepo.Object);

            var result = await svc.SaveSelectionAsync(entity);

            Assert.IsTrue(result);
            Assert.AreEqual("Ejemplo", entity.ApiName);
            Assert.AreEqual(9000, entity.BenefitValue);
        }

        [Test]
        public void SaveSelectionAsyncNullEmployeeThrowsNullException()
        {
            var mockRepo = new Mock<IEmployeeBenefitRepository>();
            var mockEmpleadoRepo = new Mock<IEmpleadoRepository>();

            var svc = new EmployeeBenefitService(mockRepo.Object, mockEmpleadoRepo.Object);

            Assert.ThrowsAsync<ArgumentNullException>(async () => await svc.SaveSelectionAsync((EmployeeBenefit?)null));
        }

        [Test]
        public void SaveSelectionAsyncInvalidIdsThrowsArgumentException()
        {
            var mockRepo = new Mock<IEmployeeBenefitRepository>();
            var mockEmpleadoRepo = new Mock<IEmpleadoRepository>();

            var svc = new EmployeeBenefitService(mockRepo.Object, mockEmpleadoRepo.Object);
            var entity = new EmployeeBenefit { EmployeeId = 0, BenefitId = 0 };

            Assert.ThrowsAsync<ArgumentException>(async () => await svc.SaveSelectionAsync(entity));
        }

        [Test]
        public async Task SaveSelectionAsyncCannotSelectReturnsFalse()
        {
            var mockRepo = new Mock<IEmployeeBenefitRepository>();
            var mockEmpleadoRepo = new Mock<IEmpleadoRepository>();

            var entity = new EmployeeBenefit { EmployeeId = 1, BenefitId = 2 };

            mockRepo.Setup(r => r.CanEmployeeSelectBenefitAsync(1, 2)).ReturnsAsync(false);

            var svc = new EmployeeBenefitService(mockRepo.Object, mockEmpleadoRepo.Object);

            var result = await svc.SaveSelectionAsync(entity);

            Assert.IsFalse(result);
            mockRepo.Verify(r => r.SaveSelectionAsync(It.IsAny<EmployeeBenefit>()), Times.Never);
        }

        [Test]
        public async Task SaveSelectionAsyncBenefitNotFoundReturnsFalse()
        {
            var mockRepo = new Mock<IEmployeeBenefitRepository>();
            var mockEmpleadoRepo = new Mock<IEmpleadoRepository>();

            var entity = new EmployeeBenefit { EmployeeId = 1, BenefitId = 99 };

            mockRepo.Setup(r => r.CanEmployeeSelectBenefitAsync(1, 99)).ReturnsAsync(true);
            mockRepo.Setup(r => r.GetBeneficioByIdAsync(99)).ReturnsAsync((BeneficioModel?)null);

            var svc = new EmployeeBenefitService(mockRepo.Object, mockEmpleadoRepo.Object);

            var result = await svc.SaveSelectionAsync(entity);

            Assert.IsFalse(result);
            mockRepo.Verify(r => r.SaveSelectionAsync(It.IsAny<EmployeeBenefit>()), Times.Never);
        }

        [Test]
        public void GetEmpleadosConBeneficiosExcedidosReturnsExpectedIds()
        {
            var mockRepo = new Mock<IEmployeeBenefitRepository>();
            var mockEmpleadoRepo = new Mock<IEmpleadoRepository>();

            var empleados = new List<EmpleadoModel>
            {
                new EmpleadoModel { id = 1 },
                new EmpleadoModel { id = 2 },
                new EmpleadoModel { id = 3 }
            };

            mockEmpleadoRepo.Setup(r => r.GetEmpleadosPorEmpresa(123)).Returns(empleados);

            mockRepo.Setup(r => r.CountBeneficiosPorEmpleado(1)).Returns(2);
            mockRepo.Setup(r => r.CountBeneficiosPorEmpleado(2)).Returns(5);
            mockRepo.Setup(r => r.CountBeneficiosPorEmpleado(3)).Returns(4);

            var svc = new EmployeeBenefitService(mockRepo.Object, mockEmpleadoRepo.Object);

            var result = svc.GetEmpleadosConBeneficiosExcedidos(123, 3);

            Assert.That(result, Is.EquivalentTo(new List<int> { 2, 3 }));
        }
    }
}