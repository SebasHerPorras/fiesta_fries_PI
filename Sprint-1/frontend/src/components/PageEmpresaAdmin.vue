<template>
  <div class="wrap">
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
      <button @click="volverAtras" class="btn-primary">
        ← Volver
      </button>
      <button @click="agregarEmpleado" class="btn-secondary" :disabled="loading">
        ➕ Agregar Empleado
      </button>
      <button @click="agregarBeneficios" class="btn-secondary" :disabled="loading">
        ➕ Agregar Beneficios
      </button>
      <button @click="toggleBeneficios" class="btn-info">
        🙏 {{ mostrandoBeneficios ? 'Ver Empresas' : 'Lista de Beneficios' }}
      </button>
      <button @click="toggleEmpleados" class="btn-info">
        👥 {{ mostrandoEmpleados ? 'Ver Empresas' : 'Lista de Empleados' }}
      </button>
      <button @click="togglePayroll" class="btn-info">
        📝 {{ mostrandoPayroll ? 'Ver Empresas' : 'Generar Planilla' }}
      </button>
      <button class="btn-info" @click="editarEmpresaPropia">
        ✏️ Modificar Empresa
      </button>

    </div>

    <div class="content">
      <!-- Estado de carga -->
      <div v-if="loading" class="loading">
    ⏳ {{  mostrandoEmpleados ? 'Cargando empleados...' : 
            mostrandoBeneficios ? 'Cargando beneficios...' : 
            mostrandoPayroll ? 'Cargando planillas...' :
            'Cargando empresas...' 
          }}
        </div>
      <!-- Vista principal con condiciones anidadas -->
      <div v-else>
        <!-- Vista de EMPRESAS -->
        <div v-if="!mostrandoEmpleados && !mostrandoBeneficios && !mostrandoPayroll">
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
        <div v-else-if="mostrandoEmpleados">
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

        <!-- Vista de BENEFICIOS -->
        <div v-else-if="mostrandoBeneficios">
          <div class="beneficios-list">
            <div class="section-header">
              <h3>🙏 Lista de Beneficios</h3>
              <span class="count-badge">{{ beneficios.length }} beneficios</span>
            </div>
            <div class="table-container">
              <table class="beneficios-table">
                <thead>
                  <tr>
                    <th>Nombre</th>
                    <th>Tipo</th>
                    <th>Quien Asume</th>
                    <th>Valor</th>
                    <th>Etiqueta</th>
                    <th>Editar</th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="beneficio in beneficios" :key="beneficio.idBeneficio" class="beneficio-row">
                    <td>{{ beneficio.nombre }}</td>
                    <td>
                      <span class="type-badge" :class="getTypeClass(beneficio.tipo)">
                        {{ beneficio.tipo }}
                      </span>
                    </td>
                    <td>
                      <span class="assume-badge" :class="getAssumeClass(beneficio.quienAsume)">
                        {{ beneficio.quienAsume }}
                      </span>
                    </td>
                    <td>
                      <span :class="getValueClass(beneficio.etiqueta)">
                        {{ formatValor(beneficio) }}
                      </span>
                    </td>
                    <td>
                      <span class="etiqueta-badge" :class="getEtiquetaClass(beneficio.etiqueta)">
                        {{ beneficio.etiqueta }}
                      </span>
                    </td>
                    <td>
                      <button @click="editarBeneficio(beneficio)" class="btn-editar">
                          Editar
                      </button>
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>
        </div>
  

         <!-- Vista de PAYROLL -->
        <div v-else-if="mostrandoPayroll" class="payroll-management">
          <div class="payroll-header">
            <h3>📝 Gestión de Planillas - {{ selectedCompany?.nombre }}</h3>
            <span class="count-badge">{{ payrolls.length }} planillas históricas</span>
          </div>

          <div v-if="payrollLoading" class="loading">
            ⏳ Cargando información de nóminas...
          </div>

          <div v-else class="payroll-content">
            
            <!-- 📅 INFORMACIÓN DEL PERIODO SELECCIONADO -->
            <transition name="fade">
              <div v-if="selectedPeriod" key="period-detail" class="selected-period-detail">
                <h4>📅 Información del Periodo Seleccionado</h4>
                <table class="payroll-table">
                  <thead>
                    <tr>
                      <th>Periodo</th>
                      <th>Salario Bruto</th>
                      <th>Deducciones Empleado</th> 
                      <th>Deducciones Empleador</th> 
                      <th>Beneficios</th>
                      <th>Salario Neto</th>
                      <th>Costo Total</th>
                    </tr>
                  </thead>
                  <tbody>
                    <tr>
                      <td>{{ selectedPeriod.description }}</td>
                      <td>₡{{ formatCurrency(selectedPeriod.totalGrossSalary) }}</td>
                      <td>₡{{ formatCurrency(selectedPeriod.totalEmployeeDeductions) }}</td>
                      <td>₡{{ formatCurrency(selectedPeriod.totalEmployerDeductions) }}</td>
                      <td>₡{{ formatCurrency(selectedPeriod.totalBenefits) }}</td>
                      <td>₡{{ formatCurrency(selectedPeriod.totalNetSalary) }}</td>
                      <td>₡{{ formatCurrency(selectedPeriod.totalEmployerCost) }}</td>
                    </tr>
                  </tbody>
                </table>

                <!-- 🚀 BOTÓN PROCESAR -->
                <div class="process-section">
                  <button 
                    @click="procesarPlanilla" 
                    class="btn-process-main"
                    :disabled="payrollLoading"
                  >
                    {{ payrollLoading ? '⏳ Procesando...' : '🚀 Procesar Nómina' }}
                  </button>
                  <!--  Sección del período seleccionado -->
                  <div class="selected-period-info">
                    Periodo seleccionado: <strong>{{ selectedPeriod.description }} (Día de pago: {{ getDiaPagoEmpresa() }})</strong>
                  </div>
                </div>
              </div>
            </transition>

            <!-- 📋 PERIODOS PENDIENTES -->
            <div class="pending-section">
              <div class="section-title">
                <h4>📋 Periodos Pendientes</h4>
                <span class="count-badge">{{ getProcessablePendingPeriods().length }} procesables</span>
              </div>
              
              <div v-if="getProcessablePendingPeriods().length === 0" class="empty-periods">
                <p>No hay periodos pendientes procesables</p>
                <p class="period-help">Solo se pueden procesar nóminas para períodos pasados o actuales</p>
              </div>

              <div v-else class="periods-list">
                <div 
                  v-for="period in getProcessablePendingPeriods()" 
                  :key="period.startDate"
                  class="period-card" 
                  :class="[getPeriodClass(period), { 'selected': selectedPeriod?.startDate === period.startDate }]"
                  @click="selectPeriod(period)"
                >
                  <div class="period-content">
                    <div class="period-icon">{{ getPeriodIcon(period) }}</div>
                    <div class="period-main-info">
                      <div class="period-title">{{ period.description }}</div>
                    </div>
                    <button 
                      @click.stop="selectPeriod(period)" 
                      class="btn-select"
                      :class="{ 'active': selectedPeriod?.startDate === period.startDate }"
                    >
                      {{ selectedPeriod?.startDate === period.startDate ? 'Seleccionado' : 'Seleccionar' }}
                    </button>
                  </div>
                </div>
              </div>
            </div>

            <!-- 📅 PERIODOS FUTUROS -->
            <div v-if="hasFuturePeriods()" class="future-section">
              <div class="section-title">
                <h4>📅 Periodos Futuros</h4>
                <span class="count-badge">{{ getFuturePeriods().length }} futuros</span>
              </div>
              <div class="periods-list">
                <div 
                  v-for="period in getFuturePeriods()" 
                  :key="period.startDate"
                  class="period-card future" 
                >
                  <div class="period-content">
                    <div class="period-icon">🔵</div>
                    <div class="period-main-info">
                      <div class="period-title">{{ period.description }}</div>
                      <div class="period-help">Disponible a partir de {{ formatDate(period.startDate) }}</div>
                    </div>
                    <button class="btn-select disabled" disabled>
                      Futuro
                    </button>
                  </div>
                </div>
              </div>
            </div>

            <!-- 📊 HISTORIAL -->
            <div class="history-section">
              <div class="section-title">
                <h4>📊 Historial de Planillas</h4>
                <span class="count-badge">{{ payrolls.length }} procesadas</span>
              </div>
              
              <div v-if="payrolls.length === 0" class="empty-history">
                <p>No hay planillas procesadas</p>
              </div>

              <div v-else class="table-container">
                <table class="payroll-table">
                  <thead>
                    <tr>
                      <th>Periodo</th>
                      <th>Salario Bruto</th>
                      <th>Deducciones Empleado</th>
                      <th>Deducciones Empleador</th> 
                      <th>Beneficios</th>
                      <th>Salario Neto</th>
                      <th>Costo Total</th>
                    </tr>
                  </thead>
                  <tbody>
                    <tr v-for="payroll in payrolls" :key="payroll.payrollId" class="payroll-row">
                      <td>{{ formatDate(payroll.periodDate) }}</td>
                      <td>₡{{ formatCurrency(payroll.totalGrossSalary) }}</td>
                      <td>₡{{ formatCurrency(payroll.totalEmployeeDeductions) }}</td>
                      <td>₡{{ formatCurrency(payroll.totalEmployerDeductions) }}</td> 
                      <td>₡{{ formatCurrency(payroll.totalBenefits) }}</td>
                      <td>₡{{ formatCurrency(payroll.totalNetSalary) }}</td>
                      <td>₡{{ formatCurrency(payroll.totalEmployerCost) }}</td>
                    </tr>
                  </tbody>
                </table>
              </div>
            </div>
          </div>
        </div>
   </div> 
      <!-- Mensajes de éxito/error -->
      <div v-if="message" class="message" :class="{ 'error': messageType === 'error', 'success': messageType === 'success' }">
        {{ message }}
      </div>
    </div>

    <!-- Footer de la página con copyright y redes sociales -->
    <footer>
      <div>©2025 Fiesta Fries</div>
      <div class="socials">
        <!-- Enlaces a redes sociales (solo íconos, no funcionales) -->
        <a href="#" aria-label="Facebook">f</a>
        <a href="#" aria-label="LinkedIn">in</a>
        <a href="#" aria-label="YouTube">▶</a>
        <a href="#" aria-label="Instagram">✶</a>
      </div>
    </footer>
  </div> 
</template>


<script>
import axios from "axios";
import { API_ENDPOINTS } from '../config/apiConfig';

export default {
  name: 'EmpresaAdmin',
  data() {
    return {
      empresas: [],
      loading: false,
      message: '',
      messageType: 'success',
      mostrandoEmpleados: false,
      mostrandoBeneficios: false,
      mostrandoPayroll: false,
      empleadosQuemados: [],
      beneficios: [],
      payrolls: [],
      pendingPeriods: [],
      overduePeriods: [], 
      nextPeriod: null,
      selectedPeriod: null,
      payrollLoading: false,
      selectedCompany: null,
      selectedCompanyId: null,
      selectedCompanyCedula: null  
    }
  },
  mounted() {
    this.loadEmpresas();
    this.loadSelectedCompany();
  },
  methods: {
    async toggleEmpleados() {
      this.mostrandoEmpleados = !this.mostrandoEmpleados;
      
      if (this.mostrandoEmpleados) {
        if (this.loadSelectedCompany()) {
          await this.loadEmpleadosReales(); 
        } else {
          this.showMessage('Selecciona una empresa primero desde Datos Personales', 'error');
          this.mostrandoEmpleados = false;
        }
      } else {
        this.showMessage('Mostrando lista de empresas', 'success');
      }
    },

    async loadEmpleadosReales() {
      if (!this.selectedCompanyCedula) {
        this.showMessage('No hay empresa seleccionada', 'error');
        return;
      }

      this.loading = true;
      try {
        console.log('Cargando empleados para empresa:', this.selectedCompanyCedula);
        
        const response = await axios.get(API_ENDPOINTS.EMPLEADOS(this.selectedCompanyCedula));

        console.log('Respuesta empleados:', response.data);
        
        let empleados = [];

        if (response.data && Array.isArray(response.data)) {
          empleados = response.data;
        } else if (response.data.success && Array.isArray(response.data.empleados)) {
          empleados = response.data.empleados;
        }

        this.empleadosQuemados = empleados;
        
        if (this.empresas.length > 0 && this.empresas[0].cedulaJuridica === this.selectedCompanyCedula) {
          this.empresas[0].cantidadEmpleados = empleados.length;
        }

        this.showMessage(`Se cargaron ${empleados.length} empleados de ${this.selectedCompany.nombre}`, 'success');
      } catch (error) {
        console.error('Error cargando empleados:', error);
        this.empleadosQuemados = [];
      
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

  async toggleBeneficios() {
    this.mostrandoEmpleados = false;
    this.mostrandoBeneficios = !this.mostrandoBeneficios;
    
    if (this.mostrandoBeneficios) {
      if (this.loadSelectedCompany()) {
        await this.loadBeneficiosReales();
      } else {
        this.showMessage('Selecciona una empresa primero desde Datos Personales', 'error');
        this.mostrandoBeneficios = false;
      }
    } else {
      this.showMessage('Mostrando lista de empresas', 'success');
    }
  },

    async loadBeneficiosReales() {
      if (!this.selectedCompanyCedula) {
        this.showMessage('No hay empresa seleccionada', 'error');
        return;
      }

      this.loading = true;
      try {
        console.log('Cargando beneficios para empresa:', this.selectedCompanyCedula);

        const response = await axios.get(API_ENDPOINTS.BENEFICIOS_POR_EMPRESA(this.selectedCompanyCedula));

        console.log('Respuesta completa:', response);
        console.log('Datos de beneficios:', response.data);
        
        let beneficiosData = [];

        if (response.data && response.data.success && Array.isArray(response.data.beneficios)) {
          beneficiosData = response.data.beneficios;
          console.log(`Se cargaron ${beneficiosData.length} beneficio/s`);
        } else {
          console.log('Formato inesperado:', response.data);
          beneficiosData = [];
        }

        this.beneficios = beneficiosData;
        this.showMessage(`Se cargaron ${beneficiosData.length} beneficio/s de ${this.selectedCompany.nombre}`, 'success');
        
      } catch (error) {
        console.error('Error cargando beneficios:', error);
        console.error('Detalles del error:', error.response?.data);
        this.beneficios = [];
        
        let errorMessage = 'Error al cargar beneficios';
        if (error.response?.status === 404) {
          errorMessage = 'No se encontraron beneficios para esta empresa';
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

    getDiaPagoEmpresa() {
      if (!this.selectedCompany) return 'No definido';
      
      const diaPago = this.selectedCompany.diaPago;
      const frecuencia = this.selectedCompany.frecuenciaPago;
      
      if (frecuencia === 'quincenal') {
        return `Día ${diaPago}`;
      } else if (frecuencia === 'mensual') {
        return `Día ${diaPago}`;
      }
      
      return `${diaPago}`;
    },

     async hacerPreview(period) {
      if (!period || !this.selectedCompanyCedula) return;
        
      this.payrollLoading = true;
      try {
        const periodDate = new Date(period.startDate);
        const formattedDate = periodDate.toISOString().split('T')[0];

        const request = {
          companyId: parseInt(this.selectedCompanyCedula),
          periodDate: formattedDate
        };

        const response = await axios.post(API_ENDPOINTS.PAYROLL_PREVIEW, request);

        if (response.data.success) {
          this.selectedPeriod = {
            ...period,
            ...response.data.previewData,
            totalEmployeeDeductions: response.data.previewData.totalEmployeeDeductions || 0,
            totalEmployerDeductions: response.data.previewData.totalEmployerDeductions || 0,
            totalBenefits: response.data.previewData.totalBenefits || 0,
            totalNetSalary: response.data.previewData.totalNetSalary || 0,
            totalEmployerCost: response.data.previewData.totalEmployerCost || 0,
            totalAmount: response.data.totalAmount
          };
          
          this.showMessage(`Preview calculado - ${response.data.message}`, 'success');
        } else {
          this.showMessage(`${response.data.message}`, 'error');
        }
      } catch (error) {
        console.error('Error en preview:', error);
        this.showMessage('Error al calcular preview', 'error');
      } finally {
        this.payrollLoading = false;
      }
    },
       
   async generarNuevaPlanilla() {
      if (!this.loadSelectedCompany()) {
        this.showMessage('No hay empresa seleccionada', 'error');
        return;
      }

      this.loading = true;
      try {
        const request = {
          companyId: parseInt(this.selectedCompanyCedula),
          approvedBy: 'Sistema'
        };

        console.log('🔄 Solicitando generación de planilla...');
        
        const response = await axios.post(API_ENDPOINTS.PAYROLL_PROCESS_NEXT, request);

        if (response.data.success) {
          this.showMessage(`${response.data.message}`, 'success');
          await this.mostrarPlanillaReciente();
        } else {
          this.showMessage(response.data.message, 'info');
        }
      } catch (error) {
        console.error('Error:', error);
        this.showMessage('Error al generar planilla', 'error');
      } finally {
        this.loading = false;
      }
    },

    async mostrarPlanillaReciente() {
      const response = await axios.get(API_ENDPOINTS.PAYROLLS(this.selectedCompanyCedula));
      if (response.data && response.data.length > 0) {
        this.payrolls = [response.data[0]];
      }
    },

  // Obtener clase CSS para tipo de beneficio
  getTypeClass(tipo) {
    const classes = {
      'Monto Fijo': 'monto-fijo',
      'Porcentual': 'porcentual',
      'API': 'api'
    };
    return classes[tipo] || 'default';
  },

  // Obtener clase CSS para quien asume el beneficio
  getAssumeClass(quienAsume) {
    const classes = {
      'Empresa': 'empresa',
      'Empleado': 'empleado',
      'Compartido': 'compartido'
    };
    return classes[quienAsume] || 'default';
  },

  // Formatear el valor según el tipo
  formatValor(beneficio) {
    if (beneficio.tipo === 'Porcentual') {
      return `${beneficio.valor}%`;
    } else if (beneficio.tipo === 'Monto Fijo') {
      return `₡${beneficio.valor?.toLocaleString() || '0'}`;
    } else {
      return beneficio.valor ? `₡${beneficio.valor.toLocaleString()}` : 'API';
    }
  },

  editarBeneficio(beneficio) {
    this.$router.push({ name: 'FormBeneficios', params: { id: beneficio.idBeneficio } });
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

    // Obtener clase CSS para etiqueta
    getEtiquetaClass(etiqueta) {
      const classes = {
        'Beneficio': 'beneficio',
        'Deducción': 'deduccion'
      };
      return classes[etiqueta] || 'default';
    },

    // Obtener clase para el valor (positivo/negativo)
    getValueClass(etiqueta) {
      return etiqueta === 'Deducción' ? 'valor-negativo' : 'valor-positivo';
    },

    async loadEmpresas() {
      this.loading = true;
      try {
        const hasSelectedCompany = this.loadSelectedCompany();
        
        if (hasSelectedCompany && this.selectedCompany) {
           this.empresas = [this.selectedCompany];
      
           await this.loadEmpleadosReales();
      
           this.showMessage(`Empresa seleccionada: ${this.selectedCompany.nombre}`, 'success');
        } else {
          this.showMessage('No hay empresa seleccionada. Redirigiendo a Datos Personales...', 'error');
          setTimeout(() => {
            this.$router.push('/Profile');
          }, 2000);
        }
      } catch (error) {
        console.error('Error cargando empresas:', error);
        this.showMessage('Error al cargar la empresa: ' + (error.response?.data?.message || error.message), 'error');
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
      this.$router.push('/FormEmpresa');
    },

    viewDetails(empresa) {
      this.$router.push(`/empresa/${empresa.id}`);
    },

    editEmpresa(empresa) {
      this.$router.push(`/editar-empresa/${empresa.id}`);
    },

     volverAtras() {
      this.$router.go(-1);
    },

     agregarEmpleado() {
      this.$router.push('/RegEmpleado');
    },

    agregarBeneficios() {
      this.$router.push('/FormBeneficios');
    },

    verListaBeneficios() {
    this.toggleBeneficios();
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
    },

      async togglePayroll() {
        this.mostrandoEmpleados = false;
        this.mostrandoBeneficios = false;
        this.mostrandoPayroll = !this.mostrandoPayroll;
        
        if (this.mostrandoPayroll) {
          if (this.loadSelectedCompany()) {
            await this.loadPayrollData(); 
          } else {
            this.showMessage('Selecciona una empresa primero desde Datos Personales', 'error');
            this.mostrandoPayroll = false;
          }
        }
      },

  async loadPayrollData() {
    this.payrollLoading = true;
    try {
      const [payrolls, nextPeriod, pendingPeriods, overduePeriods] = await Promise.all([
        axios.get(API_ENDPOINTS.PAYROLLS(this.selectedCompanyCedula)),
        axios.get(API_ENDPOINTS.PAYROLL_NEXT_PERIOD(this.selectedCompanyCedula)),
        axios.get(API_ENDPOINTS.PAYROLL_PENDING_PERIODS(this.selectedCompanyCedula)),
        axios.get(API_ENDPOINTS.PAYROLL_OVERDUE_PERIODS(this.selectedCompanyCedula))
      ]);

      this.payrolls = payrolls.data || [];
      this.nextPeriod = nextPeriod.data;
      this.pendingPeriods = pendingPeriods.data || [];
      this.overduePeriods = overduePeriods.data || [];
      
      this.filterProcessablePeriods();
      
      this.showMessage(`Sistema listo. ${this.overduePeriods.length} periodos atrasados detectados`, 'success');
    } catch (error) {
      console.error('Error loading payroll data:', error);
      
      let errorMessage = 'Error al cargar datos de nóminas';
      if (error.response?.data?.message) {
        errorMessage = error.response.data.message;
      }
      
      this.showMessage(errorMessage, 'error');
      
      this.payrolls = [];
      this.pendingPeriods = [];
      this.overduePeriods = [];
    } finally {
      this.payrollLoading = false;
    }
  },

  selectOldestPendingPeriod() {
      const processablePeriods = this.getProcessablePendingPeriods();
      
      if (processablePeriods.length > 0) {
        const today = new Date();
        const minAllowedDate = new Date(today);
        minAllowedDate.setFullYear(today.getFullYear() - 1);
        
        const validPeriods = processablePeriods.filter(period => {
          const periodDate = new Date(period.startDate);
          return periodDate >= minAllowedDate;
        });
        
        if (validPeriods.length > 0) {
          const oldestValidPeriod = validPeriods
            .sort((a, b) => new Date(a.startDate) - new Date(b.startDate))[0];
          this.selectedPeriod = oldestValidPeriod;
        } else {
          const mostRecentPeriod = processablePeriods
            .sort((a, b) => new Date(b.startDate) - new Date(a.startDate))[0];
          this.selectedPeriod = mostRecentPeriod;
        }
      } else {
        this.selectedPeriod = null;
      }
    },

    isPeriodProcessable(period) {
      if (!period || !period.startDate) return false;
      const today = new Date();
      const periodStart = new Date(period.startDate);
      return periodStart <= today;
    },

    getPeriodType(period) {
        if (!period || !period.startDate || !period.endDate) return 'future';
        
        const today = new Date();
        const startDate = new Date(period.startDate);
        const endDate = new Date(period.endDate);

        if (endDate < today) {
          return 'overdue';
        } else if (startDate <= today && endDate >= today) {
          return 'current';
        } else {
          return 'future';
        }
      },


    selectPeriod(period) {
      this.selectedPeriod = period;
      this.hacerPreview(period);
    },

     async procesarPlanilla() {
      if (!this.selectedPeriod) {
        this.showMessage('Selecciona un periodo primero', 'error');
        return;
      }

      if (!this.isPeriodProcessable(this.selectedPeriod)) {
        this.showMessage('No se pueden procesar nóminas para periodos futuros', 'error');
        return;
      }

      if (!this.selectedCompanyCedula) {
        this.showMessage('No hay empresa seleccionada', 'error');
        return;
      }

      this.payrollLoading = true;
      try {
        const periodDate = new Date(this.selectedPeriod.startDate);
        const formattedDate = periodDate.toISOString().split('T')[0];

        const request = {
          companyId: parseInt(this.selectedCompanyCedula),
          periodDate: formattedDate,
          approvedBy: 'Usuario'
        };

        const response = await axios.post(API_ENDPOINTS.PAYROLL_PROCESS, request);

        if (response.data.success) {
          this.showMessage(`Planilla procesada exitosamente`, 'success');
          await this.loadPayrollData();
        } else {
          this.showMessage(`Error al procesar planilla`, 'error');
        }
      } catch (error) {
        console.error('Error procesando planilla:', error);
        
        let errorMessage = 'Error al procesar nómina';
        if (error.response?.data) {
          const errorData = error.response.data;
          
          if (typeof errorData === 'string') {
            errorMessage = errorData;
          } else if (errorData.error) {
            errorMessage = errorData.error;
          } else if (errorData.message) {
            errorMessage = errorData.message;
          } else if (errorData.errors) {
            errorMessage = Object.values(errorData.errors).flat().join(', ');
          }
        }
        
        this.showMessage(errorMessage, 'error');
      } finally {
        this.payrollLoading = false;
      }
    },

      getProcessablePendingPeriods() {
        if (!this.pendingPeriods) return [];
        
        const today = new Date();
        const minAllowedDate = new Date(today);
        minAllowedDate.setFullYear(today.getFullYear() - 1);
        
        return this.pendingPeriods.filter(period => {
          const isProcessable = this.isPeriodProcessable(period);
          const isWithinLimit = new Date(period.startDate) >= minAllowedDate;
          return isProcessable && isWithinLimit;
        });
      },

      filterProcessablePeriods() {
        const today = new Date();
        const minAllowedDate = new Date(today);
        minAllowedDate.setFullYear(today.getFullYear() - 1);
        
        if (this.nextPeriod && this.nextPeriod.startDate) {
          const nextPeriodStart = new Date(this.nextPeriod.startDate);
          if (nextPeriodStart > today || nextPeriodStart < minAllowedDate) {
            this.nextPeriod = null;
          }
        }

    
        if (this.pendingPeriods && this.pendingPeriods.length > 0) {
          this.pendingPeriods = this.pendingPeriods.filter(period => {
            if (!period.startDate) return false;
            const periodStart = new Date(period.startDate);
            return periodStart >= minAllowedDate && periodStart <= today;
          });
        }
        
        this.selectOldestPendingPeriod();
      },

    getFuturePeriods() {
    if (!this.pendingPeriods) return [];
    const today = new Date();
    return this.pendingPeriods.filter(period => {
      if (!period.startDate) return false;
      const periodStart = new Date(period.startDate);
      return periodStart > today;
    });
  },

    hasFuturePeriods() {
      return this.getFuturePeriods().length > 0;
    },

    hasProcessablePeriods() {
      return (this.nextPeriod && this.isPeriodProcessable(this.nextPeriod)) || 
            this.getProcessablePendingPeriods().length > 0;
    },


  selectFirstProcessablePeriod() {
    if (this.overduePeriods && this.overduePeriods.length > 0) {
      this.selectedPeriod = this.overduePeriods[0];
    } else if (this.nextPeriod && this.isPeriodProcessable(this.nextPeriod)) {
      this.selectedPeriod = this.nextPeriod;
    } else if (this.getProcessablePendingPeriods().length > 0) {
      this.selectedPeriod = this.getProcessablePendingPeriods()[0];
    } else {
      this.selectedPeriod = null;  
    }
},
      
    editarEmpresaPropia() {
      if (!this.selectedCompanyCedula) {
        this.showMessage('No hay empresa seleccionada para editar', 'error');
        return;
      }
      this.$router.push({ name: 'FormEmpresa', params: { cedula: this.selectedCompanyCedula } });
    },

    getPeriodClass(period) {
        const types = {
          'overdue': 'period-overdue',
          'current': 'period-current', 
          'future': 'period-future'
        };
        return types[this.getPeriodType(period)] || '';
      },

      getPeriodIcon(period) {
        const icons = {
          'overdue': '🔴',
          'current': '🟡',
        };
        return icons[this.getPeriodType(period)] || '⚪';
      },

     async loadPayrollsReales() {
        if (!this.selectedCompanyCedula) {
          this.showMessage('No hay empresa seleccionada', 'error');
          return;
        }

        this.loading = true;
        try {
          const response = await axios.get(API_ENDPOINTS.PAYROLLS(this.selectedCompanyCedula));
          
          this.payrolls = Array.isArray(response.data) ? response.data : [];
          
          this.showMessage(
            `Se cargaron ${this.payrolls.length} planilla(s) de ${this.selectedCompany?.nombre || 'la empresa'}`,
            'success'
          );
        } catch (error) {
          this.payrolls = [];
          
          let errorMessage = 'Error al cargar planillas';
          if (error.response?.status === 404) {
            errorMessage = 'No se encontraron planillas para esta empresa';
          } else if (error.response?.data?.message) {
            errorMessage = error.response.data.message;
          }
          
          this.showMessage(errorMessage, 'error');
        } finally {
          this.loading = false;
        }
      },

    formatDate(dateString) {
      if (!dateString) return 'N/A';
      const date = new Date(dateString);
      return date.toLocaleDateString('es-ES');
    },

    formatCurrency(amount) {
      if (!amount) return '0';
      return parseFloat(amount).toLocaleString('es-CR');
    },
    
  }
}
</script>

<style scoped>
.wrap {
  min-height: 100vh;
  display: flex;
  flex-direction: column;
  background: #1e1e1e;
  color: whitesmoke;
  justify-content: space-between;
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
  background: #6c757d;
  color: white;
}

.btn-secondary {
  background:  #1fb9b4;
  color: white;
}

.btn-info {
  background: #1fb9b4;;
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
  flex: 1;
  margin-bottom: 20px; 
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

/* Estilos para la tabla de beneficios */
.beneficios-table {
  width: 100%;
  border-collapse: collapse;
  background: rgba(0,0,0,0.25);
  border-radius: 8px;
  overflow: hidden;
}

.beneficios-table th,
.beneficios-table td {
  padding: 12px 15px;
  text-align: left;
  border-bottom: 1px solid rgba(255,255,255,0.1);
}

.beneficios-table th {
  background: rgba(31, 185, 180, 0.2);
  font-weight: 600;
  color: #1fb9b4;
}

.beneficio-row:hover {
  background: rgba(255,255,255,0.05);
}

.type-badge {
  padding: 3px 8px;
  border-radius: 12px;
  font-size: 11px;
  font-weight: 600;
  text-transform: uppercase;
}

.type-badge.default {
  background: rgba(108, 117, 125, 0.2);
  color: #6c757d;
}

.status-badge {
  padding: 3px 8px;
  border-radius: 12px;
  font-size: 11px;
  font-weight: 600;
  text-transform: uppercase;
}

.status-badge.default {
  background: rgba(108, 117, 125, 0.2);
  color: #6c757d;
}

/* Badges para tipos de beneficios (actualizados) */
.type-badge.monto-fijo {
  background: rgba(40, 167, 69, 0.2);
  color: #28a745;
}

.type-badge.porcentual {
  background: rgba(23, 162, 184, 0.2);
  color: #17a2b8;
}

.type-badge.api {
  background: rgba(102, 16, 242, 0.2);
  color: #6610f2;
}

.type-badge.default {
  background: rgba(108, 117, 125, 0.2);
  color: #6c757d;
}

.btn-editar {
  background: #28a745;
  color: white;
  border: none;
  padding: 8px 16px;
  border-radius: 6px;
  font-size: 0.9rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s ease;
  min-width: 80px;
}

/* Badges para quien asume */
.assume-badge {
  padding: 3px 8px;
  border-radius: 12px;
  font-size: 11px;
  font-weight: 600;
  text-transform: uppercase;
}

.assume-badge.empresa {
  background: rgba(40, 167, 69, 0.2);
  color: #28a745;
}

.assume-badge.empleado {
  background: rgba(220, 53, 69, 0.2);
  color: #dc3545;
}

.assume-badge.compartido {
  background: rgba(255, 193, 7, 0.2);
  color: #ffc107;
}

.assume-badge.default {
  background: rgba(108, 117, 125, 0.2);
  color: #6c757d;
}

/* Badges para etiquetas */
.etiqueta-badge {
  padding: 3px 8px;
  border-radius: 12px;
  font-size: 11px;
  font-weight: 600;
  text-transform: uppercase;
}

.etiqueta-badge.beneficio {
  background: rgba(40, 167, 69, 0.2);
  color: #28a745;
}

.etiqueta-badge.deduccion {
  background: rgba(220, 53, 69, 0.2);
  color: #dc3545;
}

.etiqueta-badge.default {
  background: rgba(108, 117, 125, 0.2);
  color: #6c757d;
}

/* Estilos para valores */
.valor-positivo {
  color: #28a745;
  font-weight: 600;
}

.valor-negativo {
  color: #dc3545;
  font-weight: 600;
}

/* Footer de la página */
footer {
  background: #fff;
  padding: 28px 64px;
  border-top: 1px solid #eee;
  color: #8b8b8b;
  display: flex;
  align-items: center;
  justify-content: space-between;
}

/* Contenedor de redes sociales */
.socials {
  display: flex;
  gap: 12px;
}

/* Íconos de redes sociales */
.socials a {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 34px;
  height: 34px;
  border-radius: 50%;
  border: 1px solid #e6e6e6;
  text-decoration: none;
  color: #bdbdbd;
  font-size: 14px;
}

/* Estilos para la tabla de payroll */
.payroll-table {
  width: 100%;
  border-collapse: collapse;
  background: rgba(0,0,0,0.25);
  border-radius: 8px;
  overflow: hidden;
}

.payroll-table th,
.payroll-table td {
  padding: 12px 15px;
  text-align: left;
  border-bottom: 1px solid rgba(255,255,255,0.1);
}

.payroll-table th {
  background: rgba(31, 185, 180, 0.2);
  font-weight: 600;
  color: #1fb9b4;
}

.payroll-row:hover {
  background: rgba(255,255,255,0.05);
}

/* Asegurar que los números se alineen a la izquierda */
.payroll-table td:not(:first-child) {
  text-align: left;
  font-family: 'Courier New', monospace;
}

.payroll-management {
  width: 100%;
  max-width: 1400px;
  margin: 0 auto;
}

.payroll-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  flex-wrap: wrap;
  gap: 15px;
  margin-bottom: 25px;
}

.payroll-content {
  display: flex;
  flex-direction: column;
  gap: 25px;
}

/* Contenedor principal para información del periodo seleccionado */
.selected-period-detail {
  background: rgba(0, 0, 0, 0.3);
  border-radius: 10px;
  padding: 20px;
  border: 1px solid rgba(31, 185, 180, 0.3);
}

/* Layout mejorado para period cards */
.periods-list {
  display: grid;
  gap: 12px;
}

.period-card {
  background: rgba(0, 0, 0, 0.25);
  border-radius: 8px;
  padding: 15px;
  border-left: 4px solid transparent;
  transition: all 0.3s ease;
  cursor: pointer;
}

.period-card:hover {
  background: rgba(255, 255, 255, 0.05);
  transform: translateY(-2px);
}

.period-card.selected {
  border-color: #1fb9b4;
  background: rgba(31, 185, 180, 0.1);
}

.period-overdue {
  border-left-color: #ff6b6b;
}

.period-current {
  border-left-color: #ffd93d;
}

.period-future {
  border-left-color: #6c757d;
  opacity: 0.7;
}

.period-content {
  display: flex;
  align-items: center;
  gap: 15px;
}

.period-main-info {
  flex: 1;
}

.period-title {
  font-weight: 600;
  margin-bottom: 5px;
}

.period-help {
  font-size: 12px;
  color: #bdbdbd;
  margin-top: 5px;
  font-style: italic;
}

/* Botones */
.btn-select {
  padding: 6px 12px;
  border: 1px solid #1fb9b4;
  background: transparent;
  color: #1fb9b4;
  border-radius: 6px;
  cursor: pointer;
  font-size: 12px;
  transition: all 0.3s ease;
}

.btn-select.active {
  background: #1fb9b4;
  color: white;
}

.btn-select.disabled {
  background: #6c757d;
  color: white;
  cursor: not-allowed;
  border-color: #6c757d;
}

.btn-process-main {
  background: linear-gradient(135deg, #1fb9b4, #1aa8a4);
  color: white;
  border: none;
  padding: 12px 30px;
  border-radius: 8px;
  font-weight: 600;
  cursor: pointer;
  font-size: 16px;
  transition: all 0.3s ease;
  margin: 15px 0;
}

.btn-process-main:hover:not(:disabled) {
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(31, 185, 180, 0.3);
}

.btn-process-main:disabled {
  background: #6c757d;
  cursor: not-allowed;
  transform: none;
}

.process-section {
  text-align: center;
  padding: 20px;
  background: rgba(31, 185, 180, 0.1);
  border-radius: 10px;
  margin-top: 20px;
}

.selected-period-info {
  margin-top: 10px;
  color: #bdbdbd;
  font-size: 14px;
  text-align: center;
}

.section-title {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 15px;
  padding-bottom: 10px;
  border-bottom: 1px solid rgba(255, 255, 255, 0.1);
}

/* Estados vacíos */
.empty-periods, .empty-history {
  text-align: center;
  padding: 40px 20px;
  color: #bdbdbd;
  background: rgba(255, 255, 255, 0.05);
  border-radius: 8px;
}


/* Pantallas grandes (>1200px) */
@media (min-width: 1200px) {
  .payroll-content {
    display: grid;
    grid-template-columns: 1fr 1fr;
    grid-template-rows: auto auto;
    gap: 25px;
    grid-template-areas: 
      "selected-period selected-period"
      "pending-periods history"
      "future-periods history";
  }
  
  .selected-period-detail {
    grid-area: selected-period;
  }
  
  .pending-section {
    grid-area: pending-periods;
  }
  
  .future-section {
    grid-area: future-periods;
  }
  
  .history-section {
    grid-area: history;
    max-height: 600px;
    overflow-y: auto;
  }
  
  .periods-list {
    grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
  }
}

/* Tablets (768px - 1199px) */
@media (min-width: 768px) and (max-width: 1199px) {
  .periods-list {
    grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
  }
}

/* Pantallas muy grandes (>1600px) */
@media (min-width: 1600px) {
  .payroll-content {
    grid-template-columns: 1fr 1fr 1fr;
    grid-template-areas: 
      "selected-period selected-period selected-period"
      "pending-periods future-periods history";
  }
  
  .history-section {
    max-height: 700px;
  }
}

/* Móviles (<768px) */
@media (max-width: 900px) {
  .hero {
    flex-direction: column;
    align-items: flex-start;
    padding: 36px;
  }

  .brand {
    max-width: 100%;
    margin-bottom: 40px;
  }

  .form-card {
    width: 100%;
    max-width: 420px;
  }

  .buttons-row {
    flex-direction: column;
  }

  .buttons-row .btn {
    width: 100%;
  }

  .period-card {
    flex-direction: column;
    text-align: center;
  }
  
  .period-content {
    flex-direction: column;
    gap: 10px;
  }
  
  .btn-process-main {
    width: 100%;
    margin-bottom: 10px;
  }
  
  .payroll-header {
    flex-direction: column;
    align-items: flex-start;
  }

  footer {
    flex-direction: column;
    gap: 10px;
    text-align: center;
  }
}

/* Móviles pequeños (<480px) */
@media (max-width: 480px) {
  .actions-bar {
    flex-direction: column;
    gap: 10px;
  }
  
  .btn-primary, .btn-secondary, .btn-info {
    width: 100%;
    text-align: center;
  }
  
  .section-header, .section-title {
    flex-direction: column;
    align-items: flex-start;
    gap: 10px;
  }
  
  .payroll-management {
    padding: 15px;
  }
  
  .content {
    padding: 15px;
  }
}
</style>