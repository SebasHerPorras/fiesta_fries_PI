<template>
  <div class="wrap">
    <!-- HEADER -->
    <header class="header">
      <nav class="nav">
        <div class="brand">
          <div class="logo-box">
            <span class="f">F</span>
          </div>
          <div class="texts">
            <h1>{{ userName }}</h1>
            <p>{{ userRole }}</p>
          </div>
        </div>

        <ul class="nav-list">
          <li><router-link to="/Profile">Datos Personales</router-link></li>
          <li><router-link to="/Reportes">Reportes</router-link></li>
          <li v-if="userRole === 'Empleador'">
            <router-link to="/FormEmpresa">Registrar Empresa</router-link>
          </li>
          <li v-if="userRole === 'Empleado'">
            <router-link to="/RegisterHoras">Registrar Horas</router-link>
          </li>
          <li v-if="isAdmin">
            <router-link to="/PageEmpresaAdmin">Ver Toda Empresa</router-link>
          </li>
          <li><a href="#" @click.prevent="logout">Cerrar Sesión</a></li>
        </ul>
      </nav>
    </header>

    <!-- BOTONES DE VISTA -->
    <div class="report-buttons">
      <!-- Empleador: ver las tres vistas históricas/completo/porEmpleado -->
      <template v-if="userRole === 'Empleador' || isAdmin">
        <button :class="{ active: currentView === 'historico' }" @click="setView('historico')">Histórico</button>
        <button :class="{ active: currentView === 'completo' }" @click="setView('completo')">Completo</button>
        <button :class="{ active: currentView === 'porEmpleado' }" @click="setView('porEmpleado')">Por Empleado</button>
      </template>

      <!-- Empleado: ver dos botones de función PARA REPORTES PERSONALES -->
      <template v-else-if="userRole === 'Empleado'">
        <button 
          class="btn-function" 
          :class="{ active: mostrarReportesEmpleado }"
          @click="activarReporteCompleto"
        >
        Reporte Completo
        </button>
        <button 
          class="btn-function" 
          :class="{ active: mostrarReporteHistoricoEmpleado }"
          @click="activarReporteHistorico"
        >
          Reporte Histórico
        </button>
      </template>

      <!-- Otros roles: mostrar las tres por defecto -->
      <template v-else>
        <button :class="{ active: currentView === 'historico' }" @click="setView('historico')">Histórico</button>
        <button :class="{ active: currentView === 'completo' }" @click="setView('completo')">Completo</button>
        <button :class="{ active: currentView === 'porEmpleado' }" @click="setView('porEmpleado')">Por Empleado</button>
      </template>
    </div>

    <!-- MAIN -->
    <main class="hero">
      <div class="profile-card">
        
        <!-- VISTAS PARA EMPLEADOR/ADMIN -->
        <template v-if="userRole === 'Empleador' || isAdmin">
          <!-- HISTÓRICO -->
        <div v-if="currentView === 'historico'">
          <h2>Reportes Históricos</h2>

          <!-- Filtros -->
          <div class="filters">
            <label>
              Empresa:
              <select v-model="selectedCompanyId" @change="loadHistoricalReport">
                <option value="">Todas</option>
                <option v-for="company in companies" :key="company.cedulaJuridica" :value="company.cedulaJuridica">
                  {{ company.nombre }}
                </option>
              </select>
            </label>

            <label>
              Fecha Inicio:
              <input type="date" v-model="employerHistoricalStartDate" />
            </label>

            <label>
              Fecha Fin:
              <input type="date" v-model="employerHistoricalEndDate" />
            </label>

            <div class="filter-group">
              <button @click="loadHistoricalReport" class="btn-apply-filters">
                Aplicar Filtros
              </button>
            </div>
          </div>

          <!-- Vista previa -->
          <transition name="fade">
            <div v-if="employerHistoricalData.length > 0" class="report-viewer">
              <div class="viewer-header">
                <h4>📄 Vista Previa del Reporte</h4>
                <div class="viewer-actions">
                  <button @click="downloadEmployerHistoricalCsv" class="btn-download">⬇️ Descargar CSV</button>
                </div>
              </div>

              <div class="csv-preview">
                <table class="profile-table">
                  <thead>
                    <tr>
                      <th>Empresa</th>
                      <th>Frecuencia Pago</th>
                      <th>Periodo</th>
                      <th>Fecha Pago</th>
                      <th>Salario Bruto</th>
                      <th>Cargas Sociales</th>
                      <th>Deducciones Vol.</th>
                      <th>Costo Empleador</th>
                    </tr>
                  </thead>
                  <tbody>
                    <tr v-for="(row, index) in employerHistoricalData" :key="index">
                      <td>{{ row.nombre }}</td>
                      <td>{{ row.frecuenciaPago }}</td>
                      <td>{{ formatDate(row.periodoPago) }}</td>
                      <td>{{ formatDate(row.fechaPago) }}</td>
                      <td>{{ row.salarioBrutoText ?? formatMoney(row.salarioBruto) }}</td>
                      <td>{{ row.cargasSocialesEmpleadorText ?? formatMoney(row.cargasSocialesEmpleador) }}</td>
                      <td>{{ row.deduccionesVoluntariasText ?? formatMoney(row.deduccionesVoluntarias) }}</td>
                      <td>{{ row.costoEmpleadorText ?? formatMoney(row.costoEmpleador) }}</td>
                    </tr>
                  </tbody>
                </table>
              </div>
            </div>
          </transition>
        </div>

        <!-- COMPLETO -->
        <div v-if="currentView === 'completo'">
          <h2>Reportes Completos por Empresa</h2>

          <div class="filters">
            <div class="filters-column">
              <label>
                Empresa:
                <select v-model="selectedCompanyId" @change="onCompanyChange">
                  <option value="">Selecciona una empresa</option>
                  <option v-for="company in companies" :key="company.cedulaJuridica" :value="company.cedulaJuridica">
                    {{ company.nombre }}
                  </option>
                </select>
              </label>

              <div class="load-row">
                <button class="btn-small" @click="loadLast12EmployerPayrolls" :disabled="!selectedCompany || employerReportLoading">Cargar últimos 12</button>
              </div>
            </div>
          </div>

          <div v-if="employerLast12Payrolls.length" class="report-viewer" style="margin-top:12px;">
            <div class="viewer-header">
              <h4>📊 Últimas 12 planillas</h4>
            </div>

            <div class="csv-preview">
              <table class="payroll-table" style="width:100%">
                <thead>
                  <tr>
                    <th>Periodo</th>
                    <th>Salario Neto</th>
                    <th>Costo Total</th>
                    <th>Formato</th>
                    <th>Acciones</th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="payroll in employerLast12Payrolls" :key="payrollKey(payroll) || JSON.stringify(payroll)" class="payroll-row">
                    <td>{{ payrollPeriod(payroll) }}</td>
                    <td>{{ payrollNetSalary(payroll) }}</td>
                    <td>{{ payrollCost(payroll) }}</td>
                    <td>
                      <select v-model="employerReportFormats[payrollKey(payroll) || payroll.payrollId]">
                        <option value="pdf">PDF</option>
                        <option value="csv">CSV</option>
                      </select>
                    </td>
                    <td style="display:flex;gap:8px;align-items:center;">
                      <button class="btn-generate" @click="generateEmployerReport(payrollKey(payroll) || payroll.payrollId)" :disabled="generatingReport">Generar</button>
                    </td>
                  </tr>
                </tbody>
              </table>

              <!-- Preview viewer similar a PageEmpresaAdmin -->
              <div v-if="employerReportUrl || employerCurrentReportText || employerCurrentReportBlob" class="report-viewer" style="margin-top:12px;">
                <div class="viewer-header">
                  <h4>Visor de Reporte</h4>
                  <div class="viewer-actions">
                    <button class="btn-download" @click="downloadEmployerCompleteReport(employerSelectedReportPayrollId)">⬇️ Descargar</button>
                    <button class="btn-close-viewer" @click="clearAllReports">Cerrar</button>
                  </div>
                </div>

                <div class="viewer-content" style="padding:12px;">
                  <div v-if="employerCurrentReportText">
                    <h5>CSV Preview</h5>
                    <pre style="white-space:pre-wrap;max-height:400px;overflow:auto;background:#111;padding:12px;border-radius:6px;color:#dcdcdc">{{ employerCurrentReportText }}</pre>
                  </div>
                  <div v-else-if="employerReportUrl">
                    <h5>PDF Preview</h5>
                    <div class="pdf-viewer" style="height:500px;">
                      <iframe :src="employerReportUrl" style="width:100%;height:100%;border:0"></iframe>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
        <!-- POR EMPLEADO -->
        <div v-if="currentView === 'porEmpleado'">
        <h2>Reporte por Empleado</h2>

        <!-- Filtros -->
        <div class="filters" style="display: grid; grid-template-columns: 1fr 1fr; gap: 16px; margin-bottom: 20px;">
          <!-- Fila 1 -->
          <div class="filter-group">
            <label>
              Fecha Inicio:
              <input type="date" v-model="reportePorEmpleadoStartDate" />
            </label>
          </div>
          
          <div class="filter-group">
            <label>
              Fecha Fin:
              <input type="date" v-model="reportePorEmpleadoEndDate" />
            </label>
          </div>

          <!-- Fila 2 -->
          <div class="filter-group">
            <label>
              Tipo de Empleado:
              <select v-model="reportePorEmpleadoTipoEmpleado">
                <option value="">Todos</option>
                <option value="Tiempo Completo">Tiempo Completo</option>
                <option value="Medio Tiempo">Medio Tiempo</option>
                <option value="Por horas">Por horas</option>
              </select>
            </label>
          </div>

          <div class="filter-group">
            <label>
              Empresa:
              <select v-model="reportePorEmpleadoCompanyId">
                <option value="">Todas</option>
                <option v-for="company in companies" :key="company.cedulaJuridica" :value="company.cedulaJuridica">
                  {{ company.nombre }}
                </option>
              </select>
            </label>
          </div>

          <!-- Fila 3 - Cédula ocupa toda la fila -->
          <div class="filter-group" style="grid-column: span 2;">
            <label>
              Cédula de Empleado:
              <input 
                type="text" 
                v-model="reportePorEmpleadoCedula" 
                placeholder="Ingrese la cédula específica"
                style="width: 100%; max-width: 300px;"
              />
            </label>
          </div>

          <!-- Fila 4 - Botón aplicar -->
          <div class="filter-group" style="grid-column: span 2;">
            <button 
              @click="aplicarFiltrosPorEmpleado" 
              :disabled="reportePorEmpleadoLoading"
              class="btn-apply-filters"
            >
              {{ reportePorEmpleadoLoading ? 'Aplicando...' : 'Aplicar Filtros' }}
            </button>
          </div>
        </div>

        <!-- Resultados -->
        <div v-if="reportePorEmpleadoLoading" class="loading">
          Cargando reporte por empleado...
        </div>

        <div v-else-if="reportePorEmpleadoData.length > 0" class="table-container">
          <h4>Resultados del Reporte ({{ reportePorEmpleadoData.length }} registros)</h4>

          <!-- TABLA CON COLUMNAS ESPECÍFICAS -->
          <table class="profile-table">
            <thead>
              <tr>
                <th>Empleado</th>
                <th>Cédula</th>
                <th>Periodo Pago</th>
                <th>Fecha Pago</th>
                <th>Salario Bruto</th>
                <th>Cargas Sociales</th>
                <th>Deducciones Voluntarias</th>
                <th>Costo Empleador</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="(row, index) in reportePorEmpleadoData" :key="index">
                <td>{{ row.nombre || 'N/A' }}</td>
                <td>{{ row.cedula || 'N/A' }}</td>
                <td>{{ formatDate(row.periodoPago) }}</td>
                <td>{{ formatDate(row.fechaPago) }}</td>
                <td>{{ formatMoney(row.salarioBruto) }}</td>
                <td>{{ formatMoney(row.cargasSocialesEmpleador) }}</td>
                <td>{{ formatMoney(row.deduccionesVoluntarias) }}</td>
                <td>{{ formatMoney(row.costoEmpleador) }}</td>
              </tr>
            </tbody>
          </table>

          <!-- Botón descarga CSV -->
          <div style="margin-top: 16px;">
            <button @click="descargarReportePorEmpleadoCSV" class="btn-download-csv">
              📥 Descargar CSV - Reporte por Empleado
            </button>
          </div>
        </div>

        <div v-else-if="!reportePorEmpleadoLoading" class="empty-state">
          No hay datos para los filtros seleccionados. Aplica los filtros para ver los resultados.
        </div>
      </div>
        </template>

        <!-- VISTA PARA EMPLEADO - REPORTES PERSONALES -->
        <template v-else-if="userRole === 'Empleado' && mostrarReportesEmpleado">
          <div class="employee-reports-section">
            <h2>📊 Mis Reportes de Planilla</h2>

            <!-- Lista de reportes -->
            <div class="reportes-list-section">
              <h4>Últimos 12 Reportes</h4>
              
              <div v-if="employeeReportLoading" class="loading">
                Cargando reportes...
              </div>

              <div v-else-if="employeeLast12Payrolls.length === 0" class="empty-state">
                <div class="empty-icon">📋</div>
                <h3>No hay reportes disponibles</h3>
                <p>Aún no se han generado reportes de planilla</p>
              </div>

              <div v-else class="table-container">
                <table class="payroll-table">
                  <thead>
                    <tr>
                      <th>Periodo</th>
                      <th>Salario Bruto</th>
                      <th>Salario Neto</th>
                      <th>Formato</th>
                      <th>Acciones</th>
                    </tr>
                  </thead>
                  <tbody>
                    <tr 
                      v-for="report in employeeLast12Payrolls" 
                      :key="report.reportId"
                      :class="{ 'selected-row': employeeSelectedReportId === report.reportId }"
                      class="payroll-row"
                    >
                      <td>{{ formatDate(report.periodo) }}</td>
                      <td>{{ formatMoney(report.salarioBruto) }}</td>
                      <td>{{ formatMoney(report.salarioNeto) }}</td>
                      <td>
                        <select 
                          v-model="employeeReportFormats[report.reportId]" 
                          class="format-selector"
                        >
                          <option value="pdf">PDF</option>
                          <option value="csv">CSV</option>
                        </select>
                      </td>
                      <td>
                        <button 
                          @click="generateEmployeeReport(report.reportId)"
                          class="btn-generate"
                          :disabled="employeeGeneratingReport && employeeSelectedReportId === report.reportId"
                        >
                          {{ employeeGeneratingReport && employeeSelectedReportId === report.reportId ? 'Generando...' : 'Ver' }}
                        </button>
                      </td>
                    </tr>
                  </tbody>
                </table>
              </div>
            </div>

            <!-- Visor de reporte -->
            <div v-if="employeeCurrentReportUrl" class="report-viewer">
              <div class="viewer-header">
                <h4>Vista Previa del Reporte</h4>
                <div class="viewer-actions">
                  <button @click="downloadEmployeeReport" class="btn-download">
                    📥 Descargar
                  </button>
                  <button @click="clearEmployeeReport" class="btn-close-viewer">
                    ✖ Cerrar
                  </button>
                </div>
              </div>

              <!-- Visor PDF -->
              <div v-if="employeeCurrentReportFormat === 'pdf'" class="pdf-viewer">
                <iframe 
                  :src="employeeCurrentReportUrl" 
                  width="100%" 
                  height="600px"
                  style="border: none;"
                ></iframe>
              </div>

              <!-- Mensaje para CSV -->
              <div v-else-if="employeeCurrentReportFormat === 'csv'" class="csv-message">
                <div class="csv-icon">📊</div>
                <h3>Archivo CSV Listo</h3>
                <p>El archivo CSV se ha generado correctamente</p>
                <button @click="downloadEmployeeReport" class="btn-download-big">
                  📥 Descargar CSV
                </button>
                <p class="csv-hint">El archivo se abrirá en Excel o tu programa de hojas de cálculo predeterminado</p>
              </div>
            </div>
          </div>
        </template>

      <div v-if="userRole === 'Empleado' && mostrarReporteHistoricoEmpleado" class="historical-report-section">
        <h2>📈 Reporte Histórico</h2>
        
        <!-- FILTROS DE FECHA (NUEVO) -->
        <div class="filters" style="display: grid; grid-template-columns: 1fr 1fr auto; gap: 16px; margin-bottom: 20px; align-items: end;">
          <div class="filter-group">
            <label>
              Fecha Inicio:
              <input type="date" v-model="employeeHistoricalStartDate" />
            </label>
          </div>
          
          <div class="filter-group">
            <label>
              Fecha Fin:
              <input type="date" v-model="employeeHistoricalEndDate" />
            </label>
          </div>

          <div class="filter-group">
            <button 
              @click="loadEmployeeHistoricalData" 
              :disabled="employeeHistoricalLoading"
              class="btn-apply-filters"
            >
              {{ employeeHistoricalLoading ? 'Aplicando...' : 'Aplicar Filtros' }}
            </button>
          </div>
        </div>

        <!-- TABLA DE PREVIEW (EXISTENTE) -->
        <div v-if="employeeHistoricalLoading" class="loading">Cargando datos históricos...</div>
        
        <div v-else-if="employeeHistoricalData.length > 0" class="table-container">
          <table class="historical-table">
            <thead>
              <tr>
                <th>Periodo</th>
                <th>Salario Bruto</th>
                <th>Salario Neto</th>
                <th>Deducciones</th>
                <th>Beneficios</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="(row, index) in employeeHistoricalData" :key="index">
                <td>{{ formatDate(row.periodo) }}</td>
                <td>{{ formatMoney(row.salarioBruto) }}</td>
                <td>{{ formatMoney(row.salarioNeto) }}</td>
                <td>{{ formatMoney(row.deducciones) }}</td>
                <td>{{ formatMoney(row.beneficios) }}</td>
              </tr>
            </tbody>
          </table>
          
          <div style="margin-top: 16px;">
            <button @click="downloadEmployeeHistoricalCSV" class="btn-download-csv">
              📥 Descargar CSV Histórico
            </button>
          </div>
        </div>
        
        <div v-else-if="!employeeHistoricalLoading" class="empty-state">
          No hay datos históricos en el rango seleccionado. Aplica los filtros para ver los resultados.
        </div>
      </div>

        <!-- MENSAJE PARA OTROS ROLES SIN VISTA SELECCIONADA -->
        <div v-else-if="!(userRole === 'Empleador') && !mostrarReportesEmpleado" class="welcome-message">
          <h2>Módulo de Reportes</h2>
          <p>Selecciona una vista para comenzar</p>
        </div>

      </div>
    </main>

    <!-- FOOTER -->
    <footer>
      <div>©2025 Fiesta Fries</div>
    </footer>
  </div>
</template>


<script>
import axios from "axios";
import { API_ENDPOINTS } from "../config/apiConfig";

export default {
  name: "EmployerHistoricalReport",
  data() {
    return {
      // Perfil
      userName: "Cargando...",
      userRole: "",
      isAdmin: false,
      userData: {},

      // Empresas y filtros (Histórico)
      companies: [],
      selectedCompanyId: "",

      // Vistas
      currentView: "historico",
      selectedCompany: null,
      message: '',
      messageType: 'success',

      // Reporte histórico Empleador
      employerHistoricalData: [],
      employerHistoricalStartDate: "",
      employerHistoricalEndDate: "",

      // Reporte Completo Empleador
      employerLast12Payrolls: [],
      employerReportFormats: {},
      employerReportLoading: false,
      generatingReport: false,
      employerCurrentReportBlob: null,
      employerCurrentReportText: null,
      employerReportUrl: null,
      employerSelectedReportPayrollId: null,

      //Reporte Por Empleado Empleador
      reportePorEmpleadoStartDate: '',
      reportePorEmpleadoEndDate: '',
      reportePorEmpleadoTipoEmpleado: '', // 'Tiempo Completo', 'Medio Tiempo', 'Por horas'
      reportePorEmpleadoCompanyId: '',
      reportePorEmpleadoCedula: '', // Cédula específica de empleado

      // Reporte Completo Empleado
      employeeReportLoading: false,
      employeeLast12Payrolls: [],
      employeeReportFormats: {},
      employeeGeneratingReport: false,
      employeeSelectedReportId: null,
      employeeCurrentReportUrl: null,
      employeeCurrentReportFormat: null,
      employeeCurrentReportBlob: null,
      mostrarReportesEmpleado: false,

      // Reporte Historico Empleado
      employeeHistoricalData: [],
      employeeHistoricalStartDate: '',
      employeeHistoricalEndDate: '',  
      employeeHistoricalLoading: false,
      mostrarReporteHistoricoEmpleado: false,
    };
  },

  watch: {
    currentView() {
      this.clearAllReports();
    },
    selectedCompanyId(newVal) {
      if (!newVal) {
        this.selectedCompany = null;
        localStorage.removeItem('selectedCompany');
        return;
      }
      const found = this.companies.find(c => String(c.cedulaJuridica) === String(newVal));
      if (found) {
        this.selectedCompany = found;
        try { 
          localStorage.setItem('selectedCompany', JSON.stringify(found)); 
        } catch (e) { 
          // noop 
        }
      }
    }
  },
  async mounted() {
    this.loadCurrentUserProfile();
    if (this.userRole === 'Empleador' || this.isAdmin) {
      await this.loadCompanies();
      this.loadSelectedCompany();
    } else if (this.userRole === 'Empleado') {
      try {
        this.employeeAction('Funcion 1 ejecutada (mounted)');
      } catch (e) {
        // noop
      }
    } else {
      await this.loadCompanies();
    }
  },

  methods: {

    loadCurrentUserProfile() {
      const stored = localStorage.getItem("userData");
      if (!stored) {
        this.$router.push("/");
        return;
      }
      try {
        this.userData = JSON.parse(stored);
        this.userName = this.userData.firstName + " " + (this.userData.secondName || "");
        this.userRole = this.userData.personType;
        this.isAdmin = this.userData?.isAdmin;
        try {
          this.clearAllReportState();
        } catch (e) {
          // noop
        }
      } catch (e) {
        console.error("Error parseando userData:", e);
        this.$router.push("/");
      }
    },

    clearAllReportState() {
      try {
        localStorage.removeItem('selectedCompany'); } catch (e) { /* noop */ }
      this.selectedCompany = null;
      this.selectedCompanyId = "";

      try { this.clearAllReports(); } catch (e) { /* noop */ }
      this.employerLast12Payrolls = [];
      this.employerReportFormats = {};
      this.employerReportLoading = false;
      this.generatingReport = false;
      this.employerCurrentReportBlob = null;
      this.employerCurrentReportText = null;
      this.employerReportUrl = null;
      this.employerSelectedReportPayrollId = null;
    },

    async loadCompanies() {
      try {
        const stored = localStorage.getItem("userData");
        if (!stored) return;

        const userData = JSON.parse(stored);

        if (userData.personType === "Empleador") {
          const res = await axios.get(API_ENDPOINTS.MIS_EMPRESAS_ID(userData.id));

          if (res.data && res.data.success) {
            this.companies = res.data.empresas || [];
          } else if (Array.isArray(res.data)) {
            this.companies = res.data;
          } else {
            this.companies = [];
          }
        } else {
          this.companies = [];
        }

        this.selectedCompanyId = "";
      } catch (err) {
        console.error("Error cargando empresas:", err);
        this.companies = [];
        this.selectedCompanyId = "";
      }
    },

    onCompanyChange() {
      // keep selectedCompany in sync and persist
      if (!this.selectedCompanyId) {
        this.selectedCompany = null;
        localStorage.removeItem('selectedCompany');
        return;
      }
      const found = this.companies.find(c => String(c.cedulaJuridica) === String(this.selectedCompanyId));
        if (found) {
        this.selectedCompany = found;
        try { localStorage.setItem('selectedCompany', JSON.stringify(found)); } catch (e) { /* noop */ }
      }
    },

    // Restaurar empresa seleccionada desde localStorage (si existe)
    loadSelectedCompany() {
      try {
        const raw = localStorage.getItem('selectedCompany');
        if (!raw) return;
        const obj = JSON.parse(raw);
        if (obj && obj.cedulaJuridica) {
          this.selectedCompany = obj;
          this.selectedCompanyId = String(obj.cedulaJuridica);
          // Disparar filtros automáticamente para mostrar datos de la empresa
          this.loadHistoricalReport();
        }
      } catch (e) {
        // noop
      }
    },

    // ---------------------------
    // Histórico Empleador
    // ---------------------------
    async loadHistoricalReport() {
      try {
        // Construir params sólo con valores definidos (el endpoint no debe recibir params nulos)
        const params = {};

        const employerId = this.userData?.personaId ?? this.userData?.id ?? null;
        if (employerId !== null && employerId !== undefined && employerId !== "") {
          // Mantener como número si puede convertirse
          const n = Number(employerId);
          params.employerId = Number.isNaN(n) ? employerId : n;
        }

        const companyId = this.selectedCompanyId || this.selectedCompany?.cedulaJuridica || this.selectedCompany?.cedula || this.selectedCompany?.cedulaFiscal || null;
        if (companyId !== null && companyId !== undefined && String(companyId) !== "") {
          const n = Number(companyId);
          params.companyId = Number.isNaN(n) ? companyId : n;
        }

        if (this.employerHistoricalStartDate) params.startDate = this.employerHistoricalStartDate;
        if (this.employerHistoricalEndDate) params.endDate = this.employerHistoricalEndDate;

        try {
          const q = new URLSearchParams();
          Object.entries(params).forEach(([k, v]) => q.append(k, String(v)));
          const fullUrl = `${API_ENDPOINTS.EMPLOYER_HISTORICAL_REPORT}${q.toString() ? '?' + q.toString() : ''}`;
          if (fullUrl){
            console.log(fullUrl);
          }
        } catch (e) {
          console.log('loadHistoricalReport - could not build fullUrl', e);
        }

        const res = await axios.get(API_ENDPOINTS.EMPLOYER_HISTORICAL_REPORT, { params });
        if (Array.isArray(res.data)) {
          this.employerHistoricalData = res.data;
        } else if (res.data?.success) {
          this.employerHistoricalData = res.data.data;
        } else {
          this.employerHistoricalData = [];
        }
      } catch (err) {
        console.error("Error cargando reporte:", err?.message, err?.response?.data);
        this.employerHistoricalData = [];
      }
    },

    async downloadEmployerHistoricalCsv() {
      try {
        const employerId = this.userData?.personaId;
        if (!Number.isInteger(employerId)) {
          console.error("employerId inválido:", employerId);
          return;
        }

        const params = { employerId };

        if (this.selectedCompanyId) {
          params.companyId = Number(this.selectedCompanyId);
        }
        if (this.employerHistoricalStartDate) {
          params.startDate = this.employerHistoricalStartDate;
        }
        if (this.employerHistoricalEndDate) {
          params.endDate = this.employerHistoricalEndDate;
        }

        const res = await axios.get(API_ENDPOINTS.EMPLOYER_HISTORICAL_REPORT_CSV, {
          params,
          responseType: "blob"
        });

        const url = window.URL.createObjectURL(new Blob([res.data]));
        const link = document.createElement("a");
        link.href = url;
        link.setAttribute("download", "EmployerHistoricalReport.csv");
        document.body.appendChild(link);
        link.click();
        link.remove();
        window.URL.revokeObjectURL(url);
      } catch (err) {
        console.error("Error descargando CSV:", err?.message, err?.response?.data);
      }
    },

    // ---------------------------
    // Completo Empleador
    // ---------------------------
    async loadLast12EmployerPayrolls() {

      const companyCedula = this.selectedCompany?.cedulaJuridica || this.selectedCompanyId || this.selectedCompany?.cedula || this.selectedCompany?.cedulaFiscal;
      if (!companyCedula) {
        console.warn('No hay empresa seleccionada para cargar planillas (companyCedula vacío)');
        return;
      }

      this.employerReportLoading = true;
      try {
        const url = API_ENDPOINTS.PAYROLL_REPORT_LAST_12(companyCedula);

        const res = await axios.get(url);

        let payrolls = [];
        // admitir distintos formatos de respuesta
        if (res.data && res.data.success) payrolls = res.data.payrolls || res.data.data || [];
        else if (Array.isArray(res.data)) payrolls = res.data;
        else {
          console.warn('Formato inesperado en la respuesta de last12:', res.data);
          payrolls = [];
        }

        this.employerLast12Payrolls = payrolls;

        // Inicializar formatos defensivamente
        const formats = {};

        payrolls.forEach(p => {
          const id = p?.payrollId || p?.id || p?.planillaId || JSON.stringify(p);
          formats[id] = 'pdf';
        });

        this.employerReportFormats = formats;
      } catch (err) {
        console.error('Error cargando últimas 12 planillas', err?.response || err);
        this.employerLast12Payrolls = [];
      } finally {
        this.employerReportLoading = false;
      }
    },

    // Helper para obtener un id consistente de una planilla
    payrollKey(payroll) {
      return payroll?.payrollId || payroll?.id || payroll?.planillaId || payroll?.uuid || payroll?.key || null;
    },

    payrollPeriod(payroll) {
      if (payroll?.periodDate) return this.formatDate(payroll.periodDate);
      return '—';
    },

    payrollCost(payroll) {
      const v = payroll?.totalEmployerCost ?? null;
      return v == null ? '—' : this.formatMoney(v);
    },

    payrollNetSalary(payroll) {
      const v = payroll?.totalNetSalary ?? null;
      return v == null ? '—' : this.formatMoney(v);
    },

    // Mostrar mensaje temporal (copiado de PageEmpresaAdmin)
    showMessage(msg, type) {
      this.message = msg;
      this.messageType = type;
      setTimeout(() => {
        this.message = '';
      }, 5000);
    },

    // Generar reporte por planilla (muestra preview para PDF/CSV)
    async generateEmployerReport(payrollId) {
      this.generatingReport = true;
      // limpiar previa antes de setear el id, para que no se borre inmediatamente
      try { this.clearAllReports(); } catch (e) { /* noop */ }

      try {
        const urlPdf = API_ENDPOINTS.PAYROLL_REPORT_PDF(payrollId);
        const resp = await axios.get(urlPdf, { responseType: 'blob' });
        const pdfBlob = new Blob([resp.data], { type: 'application/pdf' });
        this.employerCurrentReportBlob = pdfBlob;
        this.employerReportUrl = window.URL.createObjectURL(pdfBlob);
        // asignar el id seleccionado PARA EL VISOR una vez que el preview está listo
        this.employerSelectedReportPayrollId = payrollId;
        this.employerCurrentReportText = null;
        this.showMessage('PDF listo para vista previa', 'success');

      } catch (err) {
        console.error('Error generando reporte', err?.response || err);
        this.clearAllReports();
        this.showMessage('Error al generar reporte', 'error');
      } finally {
        this.generatingReport = false;
      }
    },

    // Descargar el blob o solicitar CSV/PDF según formato seleccionado
    async downloadEmployerCompleteReport(payrollId) {
      try {
        const pid = payrollId || this.employerSelectedReportPayrollId;
        if (!pid) {
          console.warn('downloadEmployerCompleteReport: payrollId faltante');
          return;
        }

        const desiredFormat = this.employerReportFormats[pid] || 'pdf';

        // intentar localizar el objeto planilla para construir el nombre de archivo
        const payrollObj = this.getPayrollById(pid);
        const fileBaseName = this.buildReportFileName(payrollObj);

        if (desiredFormat === 'csv') {
          // Descargar CSV directamente desde endpoint
          const res = await axios.get(API_ENDPOINTS.PAYROLL_REPORT_CSV(pid), { responseType: 'blob' });
          const blob = new Blob([res.data], { type: res.headers['content-type'] || 'text/csv' });
          const url = window.URL.createObjectURL(blob);
          const a = document.createElement('a');
          a.href = url;
          a.download = `${fileBaseName}.csv`;
          document.body.appendChild(a);
          a.click();
          a.remove();
          window.URL.revokeObjectURL(url);
          return;
        }

        // FORMATO PDF: si ya generamos preview para este payroll, reutilizar blob
        if (this.employerCurrentReportBlob && String(this.employerSelectedReportPayrollId) === String(pid)) {
          const url = window.URL.createObjectURL(this.employerCurrentReportBlob);
          const a = document.createElement('a');
          a.href = url;
          a.download = `${fileBaseName}.pdf`;
          document.body.appendChild(a);
          a.click();
          a.remove();
          window.URL.revokeObjectURL(url);
          return;
        }

        // Si no existe preview, solicitar PDF y forzar descarga
        const resPdf = await axios.get(API_ENDPOINTS.PAYROLL_REPORT_PDF(pid), { responseType: 'blob' });
        const pdfBlob = new Blob([resPdf.data], { type: resPdf.headers['content-type'] || 'application/pdf' });
        const pdfUrl = window.URL.createObjectURL(pdfBlob);
        const link = document.createElement('a');
        link.href = pdfUrl;
        link.download = `${fileBaseName}.pdf`;
        document.body.appendChild(link);
        link.click();
        link.remove();
        window.URL.revokeObjectURL(pdfUrl);
      } catch (err) {
        console.error('Error descargando reporte', err?.response || err);
      }
    },

    // Buscar objeto planilla en la lista local por id/clave
    getPayrollById(pid) {
      try {
        if (!this.employerLast12Payrolls || !this.employerLast12Payrolls.length) return null;
        return this.employerLast12Payrolls.find(p => String(this.payrollKey(p) || p.payrollId || p.id || p.planillaId) === String(pid)) || null;
      } catch (e) {
        return null;
      }
    },

    // Construir nombre de archivo seguro: Reporte_Completo_Nombre de Empresa-Periodo
    buildReportFileName(payroll) {
      const prefix = 'Reporte_Completo';
      let companyName = this.selectedCompany?.nombre || payroll?.companyName || payroll?.nombre || payroll?.empresa || 'Empresa';
      let period = this.payrollPeriod(payroll) || this.formatDate(payroll?.periodDate) || payroll?.periodo || payroll?.period || 'Periodo';

      // sanitize: reemplazar espacios y caracteres problemáticos
      const sanitize = (s) => String(s || '').trim().replace(/\s+/g, '_').replace(/[^a-zA-Z0-9_.-]/g, '');
      const name = `${prefix}_${sanitize(companyName)}-${sanitize(period)}`;
      return name;
    },

    formatDate(value) {
      if (!value) return "";
      try {
        if (typeof value === "string" && value.length >= 10) {
          return value.substring(0, 10);
        }
        const d = new Date(value);
        return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, "0")}-${String(d.getDate()).padStart(2, "0")}`;
      } catch {
        return "";
      }
    },

    formatMoney(value) {
      if (value == null) return "";
      try {
        return new Intl.NumberFormat("es-CR", { style: "currency", currency: "CRC", maximumFractionDigits: 0 }).format(value);
      } catch {
        return value;
      }
    },
  
    setView(view) {
      // Limpia todo antes de cambiar vista para evitar datos cruzados
      this.clearAllReports();
      this.mostrarReportesEmpleado = false;
      this.currentView = view;
    },

    async activarReporteCompleto() {
      this.mostrarReporteHistoricoEmpleado = false;
      this.mostrarReportesEmpleado = !this.mostrarReportesEmpleado;
      
      if (this.mostrarReportesEmpleado && this.employeeLast12Payrolls.length === 0) {
        await this.loadEmployeeLast12Payrolls();
      }
    },
    // ---------------------------
    // Por Empleado - Empleador
    // ---------------------------
      async aplicarFiltrosPorEmpleado() {
      this.reportePorEmpleadoLoading = true;
      
      try {
        // Construir objeto con todos los filtros
        const params = {
          employerId: this.userData?.personaId || this.userData?.id
        };

        if (this.reportePorEmpleadoStartDate) {
          params.startDate = this.reportePorEmpleadoStartDate;
        }
        if (this.reportePorEmpleadoEndDate) {
          params.endDate = this.reportePorEmpleadoEndDate;
        }
        if (this.reportePorEmpleadoTipoEmpleado) {
          params.employmentType = this.reportePorEmpleadoTipoEmpleado;
        }
        if (this.reportePorEmpleadoCompanyId) {
          params.companyId = this.reportePorEmpleadoCompanyId;
        }
        if (this.reportePorEmpleadoCedula) {
          params.cedula = this.reportePorEmpleadoCedula;
        }

        const queryString = new URLSearchParams(params).toString();
        const urlCompleta = `${API_ENDPOINTS.EMPLOYER_BY_PERSON_REPORT}?${queryString}`;

        const response = await axios.get(API_ENDPOINTS.EMPLOYER_BY_PERSON_REPORT, { params });

        if (response.data && response.data.success) {
          this.reportePorEmpleadoData = response.data.data || [];
        } else {
          this.reportePorEmpleadoData = [];
        }

        this.$forceUpdate();
        

      } catch (error) {
        console.error('Error cargando reporte por empleado:', error);
        this.reportePorEmpleadoData = [];
        alert('Error al cargar el reporte: ' + (error.response?.data?.message || error.message));
      } finally {
        this.reportePorEmpleadoLoading = false;
      }
    },
        async descargarReportePorEmpleadoCSV() {
          try {
            const params = {
              employerId: this.userData?.personaId || this.userData?.id
            };

            if (this.reportePorEmpleadoStartDate) {
              params.startDate = this.reportePorEmpleadoStartDate;
            }
            if (this.reportePorEmpleadoEndDate) {
              params.endDate = this.reportePorEmpleadoEndDate;
            }
            if (this.reportePorEmpleadoTipoEmpleado) {
              params.employmentType = this.reportePorEmpleadoTipoEmpleado; 
            }
            if (this.reportePorEmpleadoCompanyId) {
              params.companyId = this.reportePorEmpleadoCompanyId;
            }
            if (this.reportePorEmpleadoCedula) {
              params.cedula = this.reportePorEmpleadoCedula; 
            }

            const response = await axios.get(API_ENDPOINTS.EMPLOYER_BY_PERSON_REPORT_CSV, {
              params,
              responseType: "blob"
    });

        let fileName = 'Reporte_Por_Empleado';
        
        const filtros = [];
        if (this.reportePorEmpleadoStartDate) filtros.push(`desde-${this.reportePorEmpleadoStartDate}`);
        if (this.reportePorEmpleadoEndDate) filtros.push(`hasta-${this.reportePorEmpleadoEndDate}`);
        if (this.reportePorEmpleadoTipoEmpleado) filtros.push(this.reportePorEmpleadoTipoEmpleado.replace(/\s+/g, '-'));
        if (this.reportePorEmpleadoCedula) filtros.push(`cedula-${this.reportePorEmpleadoCedula}`);
        
        if (filtros.length > 0) {
          fileName += '_' + filtros.join('_');
        }
        fileName += '.csv';

        const blob = new Blob([response.data], { 
          type: response.headers['content-type'] || 'text/csv' 
        });
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = fileName;
        document.body.appendChild(link);
        link.click();
        link.remove();
        window.URL.revokeObjectURL(url);


      } catch (error) {
        console.error('Error descargando CSV por empleado:', error);
        console.error('Response:', error.response?.data);
        console.error('Status:', error.response?.status);
        
        alert('Error al descargar el CSV: ' + (error.response?.data?.message || error.message));
      }
    },

    // ---------------------------
    // Completo Empleado
    // ---------------------------
    async loadEmployeeLast12Payrolls() {
      const stored = JSON.parse(localStorage.getItem("userData"));
      const employeeId = stored?.personaId;
      
      if (!employeeId) {
        console.error('❌ No hay personaId en localStorage');
        alert('No se pudo obtener el ID del empleado');
        return;
      }

      this.employeeReportLoading = true;
      try {
        const url = API_ENDPOINTS.PAYROLL_EMPLOYEE_LAST_12_PAYMENTS(employeeId);        
        const response = await axios.get(url);

        let reports = [];
        
        if (Array.isArray(response.data)) {
          reports = response.data;
        } else if (response.data && response.data.success) {
          reports = response.data.reports || response.data.payrolls || [];
        }

        this.employeeLast12Payrolls = reports;

        // Inicializar formatos usando reportId
        const formats = {};
        reports.forEach(report => {
          formats[report.reportId] = 'pdf';
        });
        this.employeeReportFormats = formats;

      } catch (error) {
        console.error('❌ Error cargando reportes de empleado:', error);
        alert('Error al cargar reportes: ' + (error.response?.data?.message || error.message));
        this.employeeLast12Payrolls = [];
      } finally {
        this.employeeReportLoading = false;
      }
    },

    async generateEmployeeReport(payrollId) {
      const format = this.employeeReportFormats[payrollId] || 'pdf';
      this.employeeGeneratingReport = true;
      this.employeeSelectedReportId = payrollId;

      try {
        const stored = JSON.parse(localStorage.getItem("userData"));
        const employeeId = stored?.personaId;
        
        if (!employeeId) {
          alert('No se pudo obtener el ID del empleado');
          return;
        }

        let url;
        if (format === 'pdf') {
          url = API_ENDPOINTS.PAYROLL_EMPLOYEE_REPORT_PDF(payrollId, employeeId);
        } else if (format === 'csv') {
          url = API_ENDPOINTS.PAYROLL_EMPLOYEE_REPORT_CSV(payrollId, employeeId);
        }

        const response = await axios.get(url, {
          responseType: 'blob'
        });

        const blob = new Blob([response.data], {
          type: format === 'pdf' ? 'application/pdf' : 'text/csv'
        });
        const blobUrl = window.URL.createObjectURL(blob);

        this.employeeCurrentReportUrl = blobUrl;
        this.employeeCurrentReportFormat = format;
        this.employeeCurrentReportBlob = blob;

      } catch (error) {
        console.error('❌ Error generando reporte de empleado:', error);
        alert('Error al generar el reporte');
      } finally {
        this.employeeGeneratingReport = false;
      }
    },

    downloadEmployeeReport() {
      if (!this.employeeCurrentReportBlob || !this.employeeSelectedReportId) return;

      const format = this.employeeCurrentReportFormat;
      const extension = format === 'pdf' ? 'pdf' : 'csv';

      const selectedReport = this.employeeLast12Payrolls.find(
        report => report.reportId === this.employeeSelectedReportId
      );
      
      let fileName;
      if (selectedReport && selectedReport.periodo) {
        const periodoFormateado = selectedReport.periodo.replace(/\//g, '_');
        fileName = `Reporte_Completo_${periodoFormateado}.${extension}`;
      } else {
        fileName = `Reporte_Completo_${this.employeeSelectedReportId}.${extension}`;
      }

      const link = document.createElement('a');
      link.href = this.employeeCurrentReportUrl;
      link.download = fileName;
      link.click();
    },

    activarReporteHistorico() {
      this.mostrarReportesEmpleado = false;
      // Activar histórico
      this.mostrarReporteHistoricoEmpleado = !this.mostrarReporteHistoricoEmpleado;
    },

    // ---------------------------
    // Histórico Empleado (SIN ENDPOINT AÚN)
    // ---------------------------
    async loadEmployeeHistoricalData() {
      this.employeeHistoricalLoading = true;
      try {
        const employeeId = this.userData?.personaId;
        const params = {};
        
        // Solo agregar fechas si tienen valor
        if (this.employeeHistoricalStartDate) {
          params.startDate = this.employeeHistoricalStartDate;
        }
        if (this.employeeHistoricalEndDate) {
          params.endDate = this.employeeHistoricalEndDate;
        }
        
        // Implementar endpoint real cuando esté disponible
        console.log('🔍 Cargando histórico empleado - employeeId:', employeeId, 'params:', params);
        // console.log('📡 Endpoint:', API_ENDPOINTS.EMPLOYEE_HISTORICAL_REPORT(employeeId));
        
        // Simular carga de datos mientras tanto
        await new Promise(resolve => setTimeout(resolve, 1000));
        
        // Datos de ejemplo para preview
        this.employeeHistoricalData = [
          {
            periodo: '2024-01-01',
            salarioBruto: 500000,
            salarioNeto: 450000,
            deducciones: 50000,
            beneficios: 20000
          }
        ];
        
      } catch (error) {
        console.error('Error cargando histórico de empleado:', error);
        this.employeeHistoricalData = [];
      } finally {
        this.employeeHistoricalLoading = false;
      }
    },


    async downloadEmployeeHistoricalCSV() {
      try {
        const employeeId = this.userData?.personaId;
        const params = {};
        
        // Solo agregar fechas si tienen valor
        if (this.employeeHistoricalStartDate) {
          params.startDate = this.employeeHistoricalStartDate;
        }
        if (this.employeeHistoricalEndDate) {
          params.endDate = this.employeeHistoricalEndDate;
        }
        
        // a implementar
        console.log('📥 Descargando CSV histórico - employeeId:', employeeId, 'params:', params);
        // console.log('📡 Endpoint CSV:', API_ENDPOINTS.EMPLOYEE_HISTORICAL_REPORT_CSV(employeeId));
        
        // Simular descarga
        alert('Funcionalidad de descarga CSV lista para conectar con endpoint real');
        
      } catch (error) {
        console.error('Error descargando CSV histórico:', error);
      }
    },


    clearAllReports() {
      // Limpiar reportes de EMPLEADOR
      try {
        if (this.employerReportUrl && this.employerReportUrl.startsWith('blob:')) {
          window.URL.revokeObjectURL(this.employerReportUrl);
        }
      } catch (e) {
        // noop
      }
      this.employerReportUrl = null;
      this.employerCurrentReportBlob = null;
      this.employerSelectedReportPayrollId = null;
      this.employerCurrentReportText = null;
      this.employerHistoricalData = [];

      this.reportePorEmpleadoData = [];
      this.reportePorEmpleadoStartDate = '';
      this.reportePorEmpleadoEndDate = '';
      this.reportePorEmpleadoTipoEmpleado = '';
      this.reportePorEmpleadoCompanyId = '';
      this.reportePorEmpleadoCedula = '';

      // Info Historico EMPLEADO
      this.employeeHistoricalData = [];
      this.employeeHistoricalStartDate = '';
      this.employeeHistoricalEndDate = '';
      this.mostrarReporteHistoricoEmpleado = false;

      // Limpiar reportes de EMPLEADO
      try {
        if (this.employeeCurrentReportUrl && this.employeeCurrentReportUrl.startsWith('blob:')) {
          window.URL.revokeObjectURL(this.employeeCurrentReportUrl);
        }
      } catch (e) {
        // noop
      }
      this.employeeCurrentReportUrl = null;
      this.employeeCurrentReportFormat = null;
      this.employeeSelectedReportId = null;
      this.employeeCurrentReportBlob = null;
    },

    logout() {
      this.clearAllReports();
      localStorage.removeItem("userData");
      localStorage.removeItem("selectedCompany");
      this.$router.push("/");
    }
  }
};
</script>

<style scoped src="@/assets/style/Reportes.css"></style>