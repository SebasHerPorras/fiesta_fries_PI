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
      <!-- NUEVO BOTÓN DE REPORTES -->
      <button @click="toggleReportes" class="btn-info">
        📊 {{ mostrandoReportes ? 'Ver Empresas' : 'Reportes' }}
      </button>
      <button class="btn-info" @click="editarEmpresaPropia">
        ✏️ Modificar Empresa
      </button>
    </div>

    <div class="content">
      <!-- Estado de carga -->
      <div v-if="loading" class="loading">
        ⏳ {{ mostrandoEmpleados ? 'Cargando empleados...' : 
               mostrandoBeneficios ? 'Cargando beneficios...' : 
               mostrandoPayroll ? 'Cargando planillas...' :
               mostrandoReportes ? 'Cargando reportes...' :
               'Cargando empresas...' 
            }}
      </div>

      <!-- Vista principal -->
      <div v-else>
        <!-- Vista de EMPRESAS -->
        <div v-if="!mostrandoEmpleados && !mostrandoBeneficios && !mostrandoPayroll && !mostrandoReportes">
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
                    <th>Eliminar</th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="empresa in empresas" :key="empresa.id" class="empresa-row">
                    <td>{{ empresa.cedulaJuridica }}</td>
                    <td>{{ empresa.nombre }}</td>
                    <td>{{ empresa.cantidadEmpleados || 0 }}</td>
                    <td>{{ formatFrecuenciaPago(empresa.frecuenciaPago) }}</td>
                    <td>
                      <button @click="abrirModalEliminarEmpresa(empresa)" class="btn-eliminar">
                        Borrar
                      </button>
                    </td>
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
                    <th>Editar</th>
                    <th>Eliminar</th>
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
                    <td>
                      <button @click="editarEmpleado(empleado)" class="btn-editar">
                        Editar
                      </button>
                    </td>
                    <td>
                      <button @click="abrirModalEliminarEmpleado(empleado)" class="btn-eliminar">
                        Eliminar
                      </button>
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
                    <th>Eliminar</th>
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
                    <td>
                      <button @click="abrirModalEliminarBeneficio(beneficio)" class="btn-eliminar">
                          Eliminar
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
              <div v-if="selectedPeriod" :key="selectedPeriod.startDate" class="selected-period-detail">
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

        <!-- 📊 NUEVA VISTA DE REPORTES -->
        <div v-else-if="mostrandoReportes" class="reportes-management">
          <div class="reportes-header">
            <h3>📊 Reportes de Planillas - {{ selectedCompany?.nombre }}</h3>
            <span class="count-badge">{{ last12Payrolls.length }} reportes disponibles</span>
          </div>

          <div v-if="reportLoading" class="loading">
            ⏳ Cargando reportes...
          </div>

          <div v-else class="reportes-content">
            <!-- LISTA DE ÚLTIMAS 12 PLANILLAS -->
            <div class="reportes-list-section">
              <h4>📋 Seleccione una planilla para generar reporte</h4>
              
              <div v-if="last12Payrolls.length === 0" class="empty-state">
                <p>No hay planillas procesadas disponibles</p>
              </div>

              <div v-else class="table-container">
                <table class="payroll-table">
                  <thead>
                    <tr>
                      <th>Periodo</th>
                      <th>Costo Total</th>
                      <th>Salario Neto</th>
                      <th>Formato</th>
                      <th>Acción</th>
                    </tr>
                  </thead>
                  <tbody>
                    <tr v-for="payroll in last12Payrolls" :key="payroll.payrollId" 
                        class="payroll-row"
                        :class="{ 'selected-row': selectedReportPayrollId === payroll.payrollId }">
                      <td>{{ formatDate(payroll.periodDate) }}</td>
                      <td>₡{{ formatCurrency(payroll.totalEmployerCost) }}</td>
                      <td>₡{{ formatCurrency(payroll.totalNetSalary) }}</td>
                      <td>
                        <select 
                          v-model="reportFormats[payroll.payrollId]"
                          class="format-selector"
                          @change="clearReport"
                        >
                          <option value="pdf">📄 PDF</option>
                          <option value="csv">📊 CSV</option>
                        </select>
                      </td>
                      <td>
                        <button 
                          @click="generateReport(payroll.payrollId)"
                          class="btn-generate"
                          :disabled="generatingReport"
                        >
                          {{ generatingReport && selectedReportPayrollId === payroll.payrollId 
                             ? '⏳ Generando...' 
                             : '🚀 Generar' }}
                        </button>
                      </td>
                    </tr>
                  </tbody>
                </table>
              </div>
            </div>

            <!-- VISOR DE REPORTES -->
            <transition name="fade">
              <div v-if="reportUrl || currentReportText" class="report-viewer">
                <div class="viewer-header">
                  <h4>📄 Vista Previa del Reporte</h4>
                  <div class="viewer-actions">
                    <button @click="downloadReport" class="btn-download">
                      ⬇️ Descargar {{ currentReportFormat.toUpperCase() }}
                    </button>
                    <button @click="clearReport" class="btn-close-viewer">
                      ✕ Cerrar
                    </button>
                  </div>
                </div>

                <!-- IFRAME PARA PDF -->
                <div v-if="currentReportFormat === 'pdf' && reportUrl" class="pdf-viewer">
                  <iframe 
                    :src="reportUrl" 
                    width="100%" 
                    height="700px"
                    frameborder="0"
                  ></iframe>
                </div>

                <!-- PREVIEW PARA CSV -->
                <div v-else-if="currentReportFormat === 'csv' && currentReportText" class="csv-preview-wrapper">
                  <div class="csv-message">
                    <div class="csv-icon">📊</div>
                    <h3>CSV - Vista Previa</h3>
                    <p>Contenido de la planilla (solo lectura) — use "Descargar" para obtener el archivo.</p>
                  </div>

                  <div class="csv-preview">
                    <pre class="csv-preview-text">{{ currentReportText }}</pre>
                  </div>
                </div>

                <!-- MENSAJE SI NO HAY PREVIEW -->
                <div v-else class="csv-message">
                  <div class="csv-icon">📄</div>
                  <h3>Previsualización no disponible</h3>
                  <p>Use "Descargar" para obtener el archivo.</p>
                </div>
              </div>
            </transition>
          </div>
        </div>
      </div>

      <!-- Mensajes de éxito/error -->
      <div v-if="message" class="message" :class="{ 'error': messageType === 'error', 'success': messageType === 'success' }">
        {{ message }}
      </div>
    </div>

    <!-- Confirmar eliminación para beneficios -->
    <ModalWarning
      v-if="selectedBeneficio"
      :visible="showDeleteModal"
      :itemName="selectedBeneficio.nombre"
      :submitting="isDeleting"
      @volver="showDeleteModal = false"
      @confirm="confirmarEliminarBeneficio"
    />
    <!-- Confirmar eliminación para empleados -->
    <ModalWarning
      v-if="selectedEmpleado"
      :visible="showDeleteModal"
      :itemName="selectedEmpleado.nombre"
      :submitting="isDeleting"
      @volver="showDeleteModal = false"
      @confirm="confirmarEliminarEmpleado"
    />

    <!-- Confirmar eliminación para empresa (placeholder) -->
    <ModalWarning
      v-if="selectedEmpresa"
      :visible="showDeleteModal"
      :itemName="selectedEmpresa.nombre"
      :submitting="isDeleting"
      @volver="showDeleteModal = false"
      @confirm="confirmarEliminarEmpresa"
    />

    <!-- Confirmar eliminación para beneficios -->
    <ModalWarning
      v-if="selectedBeneficio"
      :visible="showDeleteModal"
      :itemName="selectedBeneficio.nombre"
      :submitting="isDeleting"
      @volver="showDeleteModal = false"
      @confirm="confirmarEliminarBeneficio"
    />
    <!-- Confirmar eliminación para empleados -->
    <ModalWarning
      v-if="selectedEmpleado"
      :visible="showDeleteModal"
      :itemName="selectedEmpleado.nombre"
      :submitting="isDeleting"
      @volver="showDeleteModal = false"
      @confirm="confirmarEliminarEmpleado"
    />

    <!-- Footer -->
    
    <footer>
      <div>©2025 Fiesta Fries</div>
      <div class="socials">
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
import ModalWarning from "./ModalWarning.vue"; 

export default {
  name: 'EmpresaAdmin',
  components: {
    ModalWarning
  },
  data() {
    return {
      empresas: [],
      loading: false,
      message: '',
      messageType: 'success',
      mostrandoEmpleados: false,
      mostrandoBeneficios: false,
      mostrandoPayroll: false,
      mostrandoReportes: false, // NUEVO
      empleadosQuemados: [],
      beneficios: [],
      payrolls: [],
      
      // NUEVOS DATOS PARA REPORTES
      last12Payrolls: [],
      reportFormats: {}, // { payrollId: 'pdf' | 'csv' }
      reportUrl: null,
      currentReportFormat: 'pdf',
      selectedReportPayrollId: null,
      reportLoading: false,
      generatingReport: false,
      
      // Guardar blob para preview / descarga segura
      currentReportBlob: null,
      currentReportText: null,

      // PROPIEDADES REACTIVAS PARA PAYROLL (CRÍTICO para Vue)
      pendingPeriods: [],
      overduePeriods: [],
      nextPeriod: null,
      selectedPeriod: null,
      payrollLoading: false,

      selectedCompany: null,
      selectedCompanyId: null,
      selectedCompanyCedula: null,
      selectedBeneficio: null,
      selectedEmpleado: null,
      selectedEmpresa: null,
      selectedEmpresaCedula: null,
      showDeleteModal: false,
      isDeleting: false
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

    editarEmpleado(empleado) {
      this.$router.push({ name: 'EditEmpleado', params: { id: empleado.cedula } });
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

        console.log('📡 Solicitando preview para:', formattedDate);
        const response = await axios.post(API_ENDPOINTS.PAYROLL_PREVIEW, request);
        console.log('📥 Respuesta del backend:', response.data);

        if (response.data.success) {
          // ✅ Asignación simple - Vue ahora rastrea selectedPeriod reactivamente
          this.selectedPeriod = {
            description: period.description,
            startDate: period.startDate,
            endDate: period.endDate,
            isProcessed: period.isProcessed,
            periodType: period.periodType,
            totalGrossSalary: response.data.previewData.totalGrossSalary || 0,
            totalEmployeeDeductions: response.data.previewData.totalEmployeeDeductions || 0,
            totalEmployerDeductions: response.data.previewData.totalEmployerDeductions || 0,
            totalBenefits: response.data.previewData.totalBenefits || 0,
            totalNetSalary: response.data.previewData.totalNetSalary || 0,
            totalEmployerCost: response.data.previewData.totalEmployerCost || 0,
            totalAmount: response.data.totalAmount || 0
          };
          
          console.log('✅ selectedPeriod actualizado:', this.selectedPeriod);
          this.showMessage(`Preview calculado - ${response.data.message}`, 'success');
        } else {
          this.showMessage(`${response.data.message}`, 'error');
        }
      } catch (error) {
        console.error('❌ Error en preview:', error);
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


    async selectPeriod(period) {
      // 🔒 Evitar cambiar periodo si hay un procesamiento en curso
      if (this.payrollLoading) {
        console.warn('⚠️ No se puede cambiar de periodo mientras se procesa una planilla');
        this.showMessage('Espera a que termine el procesamiento actual', 'warning');
        return;
      }
      
      console.log('🎯 Periodo seleccionado:', period.description);
      // NO asignar period vacío - esperar a que hacerPreview cargue datos reales
      await this.hacerPreview(period);
    },

     async procesarPlanilla() {
      // 🔒 Evitar doble procesamiento
      if (this.payrollLoading) {
        console.warn('⚠️ Ya hay un procesamiento en curso, ignorando...');
        return;
      }

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

      // 🔒 Crear copia inmutable del periodo para evitar cambios durante el procesamiento
      const periodToProcess = {
        startDate: this.selectedPeriod.startDate,
        description: this.selectedPeriod.description
      };

      this.payrollLoading = true;
      try {
        const periodDate = new Date(periodToProcess.startDate);
        const formattedDate = periodDate.toISOString().split('T')[0];

        const request = {
          companyId: parseInt(this.selectedCompanyCedula),
          periodDate: formattedDate,
          approvedBy: 'Usuario'
        };

        console.log('🚀 PROCESANDO PLANILLA DEFINITIVA');
        console.log('📋 Periodo a procesar:', periodToProcess.description);
        console.log('📅 Fecha enviada:', formattedDate);
        console.log('🏢 CompanyId:', request.companyId);
        console.log('📦 Request completo:', JSON.stringify(request, null, 2));

        const response = await axios.post(API_ENDPOINTS.PAYROLL_PROCESS, request);
        
        console.log('✅ Respuesta del servidor:', response.data);

        if (response.data.success) {
          this.showMessage(`Planilla procesada exitosamente: ${periodToProcess.description}`, 'success');
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
      if (amount === null || amount === undefined) return '0';
      return Number(amount).toLocaleString('es-CR');
    },

    abrirModalEliminarBeneficio(beneficio) {
      if (!beneficio) {
        console.error("No se recibió beneficio válido");
        return;
      }
      this.selectedBeneficio = beneficio;
      this.showDeleteModal = true;
    },

    async confirmarEliminarBeneficio() {
      if (!this.selectedBeneficio) return;

      try {
        const response = await fetch(API_ENDPOINTS.DELETE_BENEFICIO(this.selectedBeneficio.idBeneficio), {
          method: "DELETE",
          headers: {
            "Content-Type": "application/json",
            "Authorization": `Bearer ${localStorage.getItem("token")}`
          }
        });

        if (!response.ok) {
          const errorData = await response.json();
          alert(`Error: ${errorData.message || "No se pudo eliminar el beneficio"}`);
        } else {
            this.beneficios = this.beneficios.filter(
              b => b.idBeneficio !== this.selectedBeneficio.idBeneficio
          );
          this.$emit("beneficioEliminado", this.selectedBeneficio.idBeneficio);
        }
      } catch (error) {
        alert("Error interno al eliminar beneficio");
      } finally {
        this.showDeleteModal = false;
        this.selectedBeneficio = null;
      }
    },

    abrirModalEliminarEmpleado(empleado) {
      this.selectedEmpleado = empleado;
      this.showDeleteModal = true;

      console.log("Empleado a eliminar:", empleado.nombre);
    },

    async confirmarEliminarEmpleado() {
      if (!this.selectedEmpleado) return;

      this.isDeleting = true;

      try {
        // Obtener id del empleado (intenta varios campos comunes)
        const empId = this.selectedEmpleado.cedula || this.selectedEmpleado.id || this.selectedEmpleado.personaId || this.selectedEmpleado.id_employee;
        const companyId = this.selectedCompanyCedula || this.selectedCompanyId;

        if (!empId || !companyId) {
          this.showMessage('ID de empleado o cédula de empresa faltante', 'error');
          return;
        }

        const url = API_ENDPOINTS.DELETE_EMPLEADO(empId, companyId);

        const resp = await axios.delete(url, {
          headers: {
            Authorization: `Bearer ${localStorage.getItem('token')}`
          }
        });

        // Considerar distintos formatos de respuesta
        const success = resp.status === 200 || resp.data?.success === true;

        if (success) {
          // Remover empleado de la lista local
          this.empleadosQuemados = this.empleadosQuemados.filter(e => {
            const idCandidate = e.cedula || e.id || e.personaId || e.id_employee;
            return String(idCandidate) !== String(empId);
          });

          this.showMessage('Empleado eliminado correctamente', 'success');
        } else {
          const msg = resp.data?.message || 'No se pudo eliminar el empleado';
          this.showMessage(msg, 'error');
        }
      } catch (error) {
        console.error('Error eliminando empleado:', error);
        const msg = error.response?.data?.message || error.message || 'Error al eliminar empleado';
        this.showMessage(msg, 'error');
      } finally {
        this.isDeleting = false;
        this.showDeleteModal = false;
        this.selectedEmpleado = null;
      }
    },

    abrirModalEliminarEmpresa(empresa) {
      this.selectedEmpresa = empresa;
      this.selectedEmpresaCedula = empresa?.cedulaJuridica || empresa?.cedula || null;
      this.showDeleteModal = true;

      console.log("Empresa a eliminar:", empresa?.nombre);
      console.log("Cédula de empresa seleccionada:", this.selectedEmpresaCedula);
    },

    async confirmarEliminarEmpresa() {
      if (!this.selectedEmpresa || !this.selectedEmpresaCedula) {
        this.showMessage('No se ha seleccionado una empresa válida', 'error');
        return;
      }

      this.isDeleting = true;

      try {
        const resultado = await this.BorrarEmpresa(this.selectedEmpresaCedula);
        
        if (resultado.success) {
          this.showMessage(
            `Empresa eliminada exitosamente. ` +
            `Empleados: ${resultado.successfulDeletions}/${resultado.employeesProcessed}, ` +
            `Beneficios: ${resultado.successfulBenefitDeletions}/${resultado.benefitsProcessed}. `, 
            'success'
          );
          setTimeout(() => {
            this.$router.push('/Profile');
          }, 3500);
        } else {
          this.showMessage(`Error al eliminar empresa: ${resultado.message}`, 'error');
        }
      } catch (err) {
        console.error('Error en confirmarEliminarEmpresa:', err);
        
        if (err.response?.data?.message) {
          this.showMessage(`Error: ${err.response.data.message}`, 'error');
        } else if (err.message?.includes('Network Error')) {
          this.showMessage('Error de conexión. Verifique su internet e intente nuevamente.', 'error');
        } else {
          this.showMessage('Error interno al intentar borrar la empresa', 'error');
        }
      } finally {
        this.isDeleting = false;
        this.showDeleteModal = false;
        this.selectedEmpresa = null;
        this.selectedEmpresaCedula = null;
      }
    },

    async BorrarEmpresa(cedulaJuridica) {
      try {
        const response = await fetch(API_ENDPOINTS.COMPANY_DELETION(cedulaJuridica), {
          method: "DELETE",
          headers: {
            "Content-Type": "application/json",
            "Authorization": `Bearer ${localStorage.getItem("token")}`
          }
        });

        if (!response.ok) {
          const errorText = await response.text();
          let errorMessage = `Error ${response.status}: ${response.statusText}`;
          
          try {
            const errorData = JSON.parse(errorText);
            errorMessage = errorData.message || errorMessage;
          } catch {
            errorMessage = errorText || errorMessage;
          }
          
          throw new Error(errorMessage);
        }

        return await response.json();
      } catch (error) {
        console.error('Error en BorrarEmpresa:', error);
        throw error;
      }
    },
        
    // ============================================
    // NUEVOS MÉTODOS PARA REPORTES
    // ============================================
    async toggleReportes() {
      this.mostrandoEmpleados = false;
      this.mostrandoBeneficios = false;
      this.mostrandoPayroll = false;
      this.mostrandoReportes = !this.mostrandoReportes;
      
      if (this.mostrandoReportes) {
        if (this.loadSelectedCompany()) {
          await this.loadLast12Payrolls();
        } else {
          this.showMessage('Selecciona una empresa primero desde Datos Personales', 'error');
          this.mostrandoReportes = false;
        }
      } else {
        this.clearReport();
        this.showMessage('Mostrando lista de empresas', 'success');
      }
    },

    async loadLast12Payrolls() {
  console.log('🔍 INICIANDO loadLast12Payrolls');
  console.log('Cedula empresa:', this.selectedCompanyCedula);
  
  if (!this.selectedCompanyCedula) {
    console.error('❌ No hay cedula de empresa');
    this.showMessage('No hay empresa seleccionada', 'error');
    return;
  }

  this.reportLoading = true;
  try {
    const url = API_ENDPOINTS.PAYROLL_REPORT_LAST_12(this.selectedCompanyCedula);
    console.log('📡 Llamando a:', url);
    
    const response = await axios.get(url);
    console.log('📥 Respuesta completa:', response);
    console.log('📦 Datos recibidos:', response.data);

    let payrolls = [];
    
    if (response.data && response.data.success) {
      payrolls = response.data.payrolls || [];
      console.log('✅ Formato con success:', payrolls);
    } else if (Array.isArray(response.data)) {
      payrolls = response.data;
      console.log('✅ Formato array directo:', payrolls);
    } else {
      console.warn('⚠️ Formato inesperado:', response.data);
      payrolls = [];
    }

    this.last12Payrolls = payrolls;
    console.log('📋 Planillas asignadas:', this.last12Payrolls.length);

    // Inicializar formatos
    const formats = {};
    payrolls.forEach(payroll => {
      formats[payroll.payrollId] = 'pdf';
    });
    this.reportFormats = formats;
    console.log('🎨 Formatos inicializados:', this.reportFormats);

    this.showMessage(
      `${payrolls.length} reporte(s) disponible(s)`, 
      payrolls.length > 0 ? 'success' : 'error'
    );
  } catch (error) {
    console.error('❌ ERROR COMPLETO:', error);
    console.error('Respuesta de error:', error.response?.data);
    console.error('Status:', error.response?.status);
    
    this.last12Payrolls = [];
    this.showMessage('Error al cargar reportes: ' + (error.response?.data?.message || error.message), 'error');
  } finally {
    this.reportLoading = false;
    console.log('✅ Carga finalizada');
  }
},

    async generateReport(payrollId) {
      const format = this.reportFormats[payrollId] || 'pdf';
      this.generatingReport = true;
      this.selectedReportPayrollId = payrollId;
      this.currentReportFormat = format;
      
      // limpiar vista previa previa (revoca URL si existe)
      this.clearReport();
       this.generatingReport = true;
      this.selectedReportPayrollId = payrollId;
      this.currentReportFormat = format;
      

      try {
        const urlPdf = API_ENDPOINTS.PAYROLL_REPORT_PDF(payrollId);
        const urlCsv = API_ENDPOINTS.PAYROLL_REPORT_CSV(payrollId);

        if (format === 'pdf') {
          // Traer PDF como blob para evitar descarga forzada por headers y permitir preview en iframe
          const resp = await axios.get(urlPdf, { responseType: 'blob' });
          const pdfBlob = new Blob([resp.data], { type: 'application/pdf' });
          this.currentReportBlob = pdfBlob;
          this.reportUrl = window.URL.createObjectURL(pdfBlob); // usado por iframe
          this.showMessage('PDF listo para vista previa', 'success');
        } else {
          // Traer CSV como blob y extraer texto para mostrar en vista previa
          const resp = await axios.get(urlCsv, { responseType: 'blob' });
          const csvBlob = new Blob([resp.data], { type: 'text/csv' });
          this.currentReportBlob = csvBlob;
          this.currentReportText = await csvBlob.text(); // texto para preview
          // opcional: también crear blob url por si se quiere abrir en nueva pestaña
          this.reportUrl = window.URL.createObjectURL(csvBlob);
          this.showMessage('CSV listo para vista previa', 'success');
        }
      } catch (error) {
        console.error('Error generando reporte:', error);
        this.showMessage('Error al generar reporte', 'error');
        // limpieza por si algo quedó
        this.clearReport();
      } finally {
        this.generatingReport = false;
      }
    },

    async downloadReport() {
      // No hacer nada si no hay nada para descargar
      if (!this.currentReportBlob && !this.currentReportText && !this.reportUrl) return;

      try {
        // construir nombre: Reporte_Planilla_Nombre_Empresa_idPayrrol
        const rawName = this.selectedCompany?.nombre || 'Empresa';
        const safeName = rawName.replace(/\s+/g, '_').replace(/[^\w-]/g, '');
        const payrollIdPart = this.selectedReportPayrollId || 'unknown';
        const fileBase = `Reporte_Planilla_${safeName}_${payrollIdPart}`; // sigue patrón solicitado

        if (this.currentReportFormat === 'pdf') {
          // descargar blob PDF
          if (this.currentReportBlob) {
            const blobUrl = window.URL.createObjectURL(this.currentReportBlob);
            const link = document.createElement('a');
            link.href = blobUrl;
            link.download = `${fileBase}.pdf`; // nombre final
            document.body.appendChild(link);
            link.click();
            link.remove();
            window.URL.revokeObjectURL(blobUrl);
            this.showMessage('PDF descargado exitosamente', 'success');
          } else if (this.reportUrl) {
            // fallback
            window.open(this.reportUrl, '_blank');
          }
        } else {
          // CSV: si tenemos texto, creamos blob y forzamos descarga
          if (this.currentReportText != null) {
            const blob = new Blob([this.currentReportText], { type: 'text/csv' });
            const blobUrl = window.URL.createObjectURL(blob);
            const link = document.createElement('a');
            link.href = blobUrl;
            link.download = `${fileBase}.csv`; // nombre final
            document.body.appendChild(link);
            link.click();
            link.remove();
            window.URL.revokeObjectURL(blobUrl);
            this.showMessage('CSV descargado exitosamente', 'success');
          } else if (this.currentReportBlob) {
            const blobUrl = window.URL.createObjectURL(this.currentReportBlob);
            const link = document.createElement('a');
            link.href = blobUrl;
            link.download = `${fileBase}.csv`;
            document.body.appendChild(link);
            link.click();
            link.remove();
            window.URL.revokeObjectURL(blobUrl);
            this.showMessage('CSV descargado exitosamente', 'success');
          } else if (this.reportUrl) {
            const resp = await axios.get(this.reportUrl, { responseType: 'blob' });
            const blob = new Blob([resp.data], { type: 'text/csv' });
            const blobUrl = window.URL.createObjectURL(blob);
            const link = document.createElement('a');
            link.href = blobUrl;
            link.download = `${fileBase}.csv`;
            document.body.appendChild(link);
            link.click();
            link.remove();
            window.URL.revokeObjectURL(blobUrl);
            this.showMessage('CSV descargado exitosamente', 'success');
          }
        }
      } catch (error) {
        console.error('Error descargando reporte:', error);
        this.showMessage('Error al descargar reporte', 'error');
      }
    },

    clearReport() {
      // revocar object URL si existe
      try {
        if (this.reportUrl && this.reportUrl.startsWith('blob:')) {
          window.URL.revokeObjectURL(this.reportUrl);
        }
      } catch (e) {
        // noop
      }

      this.reportUrl = null;
      this.selectedReportPayrollId = null;
      this.currentReportBlob = null;
    },

    // ... RESTO DE TUS MÉTODOS EXISTENTES ...
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
  background: rgba(195, 70, 245, 0.2);
  color: #d321ff;
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

.btn-eliminar {
  background: #a00101;
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

/* ============================================ */
/* NUEVOS ESTILOS PARA REPORTES */
/* ============================================ */

.reportes-management {
  width: 100%;
}

.reportes-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 25px;
}

.reportes-content {
  display: flex;
  flex-direction: column;
  gap: 25px;
}

.reportes-list-section h4 {
  color: #1fb9b4;
  margin-bottom: 15px;
}

.format-selector {
  background: rgba(0, 0, 0, 0.25);
  border: 1px solid rgba(255, 255, 255, 0.1);
  color: whitesmoke;
  padding: 6px 10px;
  border-radius: 4px;
  cursor: pointer;
  font-size: 12px;
}

.format-selector:focus {
  outline: none;
  border-color: #1fb9b4;
}

.btn-generate {
  background: #1fb9b4;
  color: white;
  border: none;
  padding: 8px 16px;
  border-radius: 6px;
  cursor: pointer;
  font-weight: 600;
  transition: all 0.3s ease;
  font-size: 12px;
}

.btn-generate:hover:not(:disabled) {
  background: #1aa8a4;
  transform: translateY(-1px);
}

.btn-generate:disabled {
  background: #6c757d;
  cursor: not-allowed;
}

.selected-row {
  background: rgba(31, 185, 180, 0.1);
  border-left: 3px solid #1fb9b4;
}

/* VISOR DE REPORTES */
.report-viewer {
  background: rgba(0, 0, 0, 0.3);
  border-radius: 10px;
  padding: 20px;
  border: 1px solid rgba(31, 185, 180, 0.3);
  animation: slideIn 0.3s ease;
}

@keyframes slideIn {
  from {
    opacity: 0;
    transform: translateY(20px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

.viewer-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 15px;
  padding-bottom: 15px;
  border-bottom: 1px solid rgba(255, 255, 255, 0.1);
}

.viewer-header h4 {
  color: #1fb9b4;
  margin: 0;
}

.viewer-actions {
  display: flex;
  gap: 10px;
}

.btn-download {
  background: #1fb9b4;
  color: white;
  border: none;
  padding: 8px 16px;
  border-radius: 6px;
  cursor: pointer;
  font-weight: 600;
  transition: all 0.3s ease;
}

.btn-download:hover {
  background: #1aa8a4;
  transform: translateY(-1px);
}

.btn-close-viewer {
  background: #6c757d;
  color: white;
  border: none;
  padding: 8px 16px;
  border-radius: 6px;
  cursor: pointer;
  font-weight: 600;
}

.pdf-viewer {
  border-radius: 8px;
  overflow: hidden;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.3);
}

.pdf-viewer iframe {
  border-radius: 8px;
}

/* MENSAJE CSV */
.csv-message {
  text-align: center;
  padding: 60px 20px;
}

.csv-icon {
  font-size: 64px;
  margin-bottom: 20px;
}

.csv-message h3 {
  color: #1fb9b4;
  margin-bottom: 10px;
}

.csv-message p {
  color: #bdbdbd;
  margin-bottom: 20px;
}

.btn-download-big {
  background: #1fb9b4;
  color: white;
  border: none;
  padding: 15px 40px;
  border-radius: 8px;
  cursor: pointer;
  font-weight: 600;
  font-size: 16px;
  transition: all 0.3s ease;
}

.btn-download-big:hover {
  background: #1aa8a4;
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(31, 185, 180, 0.3);
}

.csv-hint {
  margin-top: 15px;
  font-size: 12px;
  color: #888;
}

/* TRANSICIONES */
.fade-enter-active, .fade-leave-active {
  transition: opacity 0.3s;
}

.fade-enter, .fade-leave-to {
  opacity: 0;
}

/* RESPONSIVE */
@media (max-width: 900px) {
  .reportes-header {
    flex-direction: column;
    align-items: flex-start;
    gap: 10px;
  }

  .viewer-header {
    flex-direction: column;
    gap: 10px;
  }

  .viewer-actions {
    width: 100%;
    flex-direction: column;
  }

  .btn-download,
  .btn-close-viewer {
    width: 100%;
  }
}
</style>