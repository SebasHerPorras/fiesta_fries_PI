<template>
  <div class="admin-container">
    <div class="header">
      <div class="logo">
        <span class="logo-text">F</span>
      </div>
      <div class="brand-text">
        <h1>Fiesta Fries</h1>
        <p>Administración de Empresas</p>
      </div>
    </div>

    <div class="actions-bar">
      <button @click="navigateToCreate" class="btn-primary">
        ＋ Agregar Nueva Empresa
      </button>
      <button @click="refreshList" class="btn-secondary" :disabled="loading">
        🔄 Actualizar
      </button>
      <button @click="toggleEmpleados" class="btn-info">
        👥 {{ mostrandoEmpleados ? 'Ver Empresas' : 'Lista de Empleados' }}
      </button>
    </div>

    <div class="content">
      <!-- Estado de carga -->
      <div v-if="loading" class="loading">
        ⏳ {{ mostrandoEmpleados ? 'Cargando empleados...' : 'Cargando empresas...' }}
      </div>

      <!-- Vista de EMPRESAS -->
      <div v-else-if="!mostrandoEmpleados">
        <!-- Lista de empresas -->
        <div v-if="empresas.length > 0" class="empresas-list">
          <div class="table-container">
            <table class="empresas-table">
              <thead>
                <tr>
                  <th>Cédula</th>
                  <th>Nombre</th>
                  <th>Empleados</th>
                  <th>Frecuencia Pago</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="empresa in empresas" :key="empresa.id" class="empresa-row">
                  <td>{{ empresa.cedulaJuridica }}</td>
                  <td>{{ empresa.nombre }}</td>
                  <td>{{ empresa.cantidadEmpleados || 0 }}</td>
                  <td>{{ formatFrecuenciaPago(empresa.frecuenciaPago) }}</td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>

        <!-- Estado vacío empresas -->
        <div v-else class="empty-state">
          <div class="empty-icon">🏢</div>
          <h3>No tienes empresas registradas</h3>
          <p>Comienza agregando tu primera empresa</p>
          <button @click="navigateToCreate" class="btn-primary">
            ＋ Crear Primera Empresa
          </button>
        </div>
      </div>

      <!-- Vista de EMPLEADOS -->
      <div v-else>
        <div class="empleados-list">
          <div class="section-header">
            <h3>👥 Lista de Empleados</h3>
            <span class="count-badge">{{ empleadosQuemados.length }} empleados</span>
          </div>
          <div class="table-container">
            <table class="empleados-table">
              <thead>
                <tr>
                  <th>Cédula</th>
                  <th>Nombre Completo</th>
                  <th>Edad</th>
                  <th>Correo</th>
                  <th>Departamento</th>
                  <th>Tipo Contrato</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="empleado in empleadosQuemados" :key="empleado.cedula" class="empleado-row">
                  <td>{{ empleado.cedula }}</td>
                  <td>{{ empleado.nombre }}</td>
                  <td>{{ empleado.edad }}</td>
                  <td>{{ empleado.correo }}</td>
                  <td>
                    <span class="department-badge" :class="getDepartmentClass(empleado.departamento)">
                      {{ empleado.departamento }}
                    </span>
                  </td>
                  <td>
                    <span class="contract-badge" :class="getContractClass(empleado.tipoContrato)">
                      {{ empleado.tipoContrato }}
                    </span>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
      </div>
    </div>

    <!-- Mensajes de éxito/error -->
    <div v-if="message" class="message" :class="{ 'error': messageType === 'error', 'success': messageType === 'success' }">
      {{ message }}
    </div>

    <div class="footer">
      <div>©2025 Fiesta Fries</div>
    </div>
  </div>
</template>

<script>
import axios from 'axios';

export default {
  name: 'EmpresaAdmin',
  data() {
    return {
      empresas: [],
      loading: false,
      message: '',
      messageType: 'success',
      mostrandoEmpleados: false,
      // Empleados - serán reemplazados por datos reales del backend
      empleadosQuemados: [],
      selectedCompany: null,
      selectedCompanyId: null,
      selectedCompanyCedula: null
    }
  },
  mounted() {
    this.loadEmpresas();
    this.loadSelectedCompany(); // Cargar empresa seleccionada al iniciar
  },
  methods: {
    // Alternar entre mostrar empresas y empleados
    async toggleEmpleados() {
      this.mostrandoEmpleados = !this.mostrandoEmpleados;
      
      if (this.mostrandoEmpleados) {
        // Verificar si hay empresa seleccionada
        if (this.loadSelectedCompany()) {
          await this.loadEmpleadosReales(); // Cargar empleados reales
        } else {
          this.showMessage('Selecciona una empresa primero desde Datos Personales', 'error');
          this.mostrandoEmpleados = false; // Volver a empresas
        }
      } else {
        this.showMessage('Mostrando lista de empresas', 'success');
      }
    },

    // Cargar empleados reales desde el backend
    async loadEmpleadosReales() {
      if (!this.selectedCompanyCedula) {
        this.showMessage('No hay empresa seleccionada', 'error');
        return;
      }

      this.loading = true;
      try {
        console.log('Cargando empleados para empresa:', this.selectedCompanyCedula);
        
        // Usar la cédula de la empresa para obtener empleados
        const response = await axios.get(`http://localhost:5081/api/Empleado/empresa/${this.selectedCompanyCedula}`);
        // Taco Bell
        console.log('Respuesta empleados:', response.data);
        
        if (response.data && Array.isArray(response.data)) {
          this.empleadosQuemados = response.data;
          this.showMessage(`Se cargaron ${response.data.length} empleados de ${this.selectedCompany.nombre}`, 'success');
        } else if (response.data.success && Array.isArray(response.data.empleados)) {
          this.empleadosQuemados = response.data.empleados;
          this.showMessage(`Se cargaron ${response.data.empleados.length} empleados de ${this.selectedCompany.nombre}`, 'success');
        } else {
          this.empleadosQuemados = [];
          this.showMessage('No se encontraron empleados para esta empresa', 'info');
        }
      } catch (error) {
        console.error('Error cargando empleados:', error);
        this.empleadosQuemados = [];
        
        // Mensaje de error más específico
        let errorMessage = 'Error al cargar empleados';
        if (error.response?.status === 404) {
          errorMessage = 'No se encontraron empleados para esta empresa';
        } else if (error.response?.data?.message) {
          errorMessage = error.response.data.message;
        } else if (error.message) {
          errorMessage = error.message;
        }
        
        this.showMessage(errorMessage, 'error');
      } finally {
        this.loading = false;
      }
    },

    // Obtener clase CSS para departamento
    getDepartmentClass(departamento) {
      const classes = {
        'Recursos Humanos': 'rrhh',
        'Finanzas': 'finanzas',
        'Operaciones': 'operaciones',
        'Marketing': 'marketing',
        'Ventas': 'ventas'
      };
      return classes[departamento] || 'default';
    },

    // Obtener clase CSS para tipo de contrato
    getContractClass(tipoContrato) {
      const classes = {
        'Tiempo Completo': 'completo',
        'Medio Tiempo': 'medio',
        'Temporal': 'temporal'
      };
      return classes[tipoContrato] || 'default';
    },

    async loadEmpresas() {
      this.loading = true;
      try {
        const userData = localStorage.getItem('userData');
        if (!userData) {
          throw new Error('Usuario no autenticado');
        }
        
        const user = JSON.parse(userData);
        const userId = user.id;
        
        console.log('UserId obtenido para empresas:', userId);
        
        const response = await axios.get(`https://localhost:7056/api/Empresa/mis-empresas/${userId}`);
        
        if (response.data.success) {
          this.empresas = response.data.empresas;
          this.showMessage(`Se cargaron ${response.data.count} empresas`, 'success');
        } else {
          throw new Error(response.data.message);
        }
      } catch (error) {
        console.error('Error cargando empresas:', error);
        this.showMessage('Error al cargar las empresas: ' + (error.response?.data?.message || error.message), 'error');
      } finally {
        this.loading = false;
      }
    },

    async refreshList() {
      if (this.mostrandoEmpleados) {
        // Actualizar empleados
        await this.loadEmpleadosReales();
      } else {
        // Actualizar empresas
        this.loadEmpresas();
      }
    },

    navigateToCreate() {
      this.$router.push('/crear-empresa');
    },

    viewDetails(empresa) {
      this.$router.push(`/empresa/${empresa.id}`);
    },

    editEmpresa(empresa) {
      this.$router.push(`/editar-empresa/${empresa.id}`);
    },

    formatFrecuenciaPago(frecuencia) {
      const frecuencias = {
        'quincenal': 'Quincenal',
        'mensual': 'Mensual'
      };
      return frecuencias[frecuencia] || frecuencia;
    },

    showMessage(msg, type) {
      this.message = msg;
      this.messageType = type;
      setTimeout(() => {
        this.message = '';
      }, 5000);
    },

    // Método para obtener empresa del localStorage
    loadSelectedCompany() {
      try {
        const savedCompany = localStorage.getItem('selectedCompany');
        if (savedCompany) {
          this.selectedCompany = JSON.parse(savedCompany);
          this.selectedCompanyId = this.selectedCompany.id;
          this.selectedCompanyCedula = this.selectedCompany.cedulaJuridica;
          
          console.log('Empresa seleccionada:', this.selectedCompany.nombre);
          console.log('ID de empresa:', this.selectedCompanyId);
          console.log('Cédula de empresa:', this.selectedCompanyCedula);
          
          return true; // Indica que hay empresa seleccionada
        } else {
          console.log('No hay empresa seleccionada en localStorage');
          return false; // No hay empresa seleccionada
        }
      } catch (error) {
        console.error('Error cargando empresa seleccionada:', error);
        return false;
      }
    }
  }
}
</script>

<style scoped>
.admin-container {
  background-color: #1e1e1e;
  color: whitesmoke;
  min-height: 100vh;
  padding: 20px;
}

.header {
  display: flex;
  align-items: center;
  margin-bottom: 30px;
}

.logo {
  width: 60px;
  height: 60px;
  background: linear-gradient(180deg, #51a3a0, hsl(178, 77%, 86%));
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  margin-right: 15px;
}

.logo-text {
  font-weight: 800;
  font-size: 30px;
  color: white;
}

.brand-text h1 {
  font-size: 24px;
  margin-bottom: 5px;
}

.brand-text p {
  color: #bdbdbd;
  font-size: 14px;
}

.actions-bar {
  display: flex;
  gap: 15px;
  margin-bottom: 30px;
}

.btn-primary, .btn-secondary, .btn-info, .btn-warning {
  padding: 10px 15px;
  border: none;
  border-radius: 6px;
  cursor: pointer;
  font-weight: 600;
  transition: all 0.3s;
}

.btn-primary {
  background: #1fb9b4;
  color: white;
}

.btn-secondary {
  background: #6c757d;
  color: white;
}

.btn-info {
  background: #17a2b8;
  color: white;
}

.btn-warning {
  background: #ffc107;
  color: #212529;
  padding: 5px 10px;
  font-size: 12px;
}

.btn-primary:hover { background: #1aa8a4; }
.btn-secondary:hover { background: #5a6268; }
.btn-info:hover { background: #138496; }
.btn-warning:hover { background: #e0a800; }

.content {
  background: rgb(71,69,69);
  border-radius: 10px;
  padding: 25px;
  border: 1px solid rgba(255,255,255,0.12);
}

.section-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
}

.count-badge {
  background: #1fb9b4;
  color: white;
  padding: 4px 12px;
  border-radius: 20px;
  font-size: 12px;
  font-weight: 600;
}

.loading {
  text-align: center;
  padding: 40px;
  color: #1fb9b4;
  font-size: 18px;
}

.table-container {
  overflow-x: auto;
}

.empresas-table, .empleados-table {
  width: 100%;
  border-collapse: collapse;
  background: rgba(0,0,0,0.25);
  border-radius: 8px;
  overflow: hidden;
}

.empresas-table th,
.empresas-table td,
.empleados-table th,
.empleados-table td {
  padding: 12px 15px;
  text-align: left;
  border-bottom: 1px solid rgba(255,255,255,0.1);
}

.empresas-table th,
.empleados-table th {
  background: rgba(31, 185, 180, 0.2);
  font-weight: 600;
  color: #1fb9b4;
}

.empresa-row:hover,
.empleado-row:hover {
  background: rgba(255,255,255,0.05);
}

/* Badges para departamentos */
.department-badge {
  padding: 3px 8px;
  border-radius: 12px;
  font-size: 11px;
  font-weight: 600;
  text-transform: uppercase;
}

.department-badge.rrhh {
  background: rgba(255, 193, 7, 0.2);
  color: #ffc107;
}

.department-badge.finanzas {
  background: rgba(40, 167, 69, 0.2);
  color: #28a745;
}

.department-badge.operaciones {
  background: rgba(23, 162, 184, 0.2);
  color: #17a2b8;
}

.department-badge.marketing {
  background: rgba(220, 53, 69, 0.2);
  color: #dc3545;
}

.department-badge.ventas {
  background: rgba(102, 16, 242, 0.2);
  color: #6610f2;
}

.department-badge.default {
  background: rgba(108, 117, 125, 0.2);
  color: #6c757d;
}

/* Badges para tipos de contrato */
.contract-badge {
  padding: 3px 8px;
  border-radius: 12px;
  font-size: 11px;
  font-weight: 600;
  text-transform: uppercase;
}

.contract-badge.completo {
  background: rgba(40, 167, 69, 0.2);
  color: #28a745;
}

.contract-badge.medio {
  background: rgba(255, 193, 7, 0.2);
  color: #ffc107;
}

.contract-badge.temporal {
  background: rgba(220, 53, 69, 0.2);
  color: #dc3545;
}

.contract-badge.default {
  background: rgba(108, 117, 125, 0.2);
  color: #6c757d;
}

.actions {
  display: flex;
  gap: 8px;
}

.empty-state {
  text-align: center;
  padding: 60px 20px;
}

.empty-icon {
  font-size: 64px;
  margin-bottom: 20px;
}

.empty-state h3 {
  margin-bottom: 10px;
  color: #eee;
}

.empty-state p {
  color: #bdbdbd;
  margin-bottom: 20px;
}

.message {
  padding: 15px;
  border-radius: 6px;
  margin: 20px 0;
  text-align: center;
}

.message.success {
  background: rgba(159, 230, 207, 0.2);
  color: #9fe6cf;
  border: 1px solid #9fe6cf;
}

.message.error {
  background: rgba(255, 107, 107, 0.2);
  color: #ff6b6b;
  border: 1px solid #ff6b6b;
}

.footer {
  background: #2c2c2c;
  padding: 20px;
  text-align: center;
  margin-top: 40px;
  color: #8b8b8b;
  border-radius: 8px;
}

@media (max-width: 768px) {
  .actions-bar {
    flex-direction: column;
  }
  
  .empresas-table, .empleados-table {
    font-size: 14px;
  }
  
  .actions {
    flex-direction: column;
  }
}
</style>