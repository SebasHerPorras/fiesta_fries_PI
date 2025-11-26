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
          <li><router-link to="/ReporteHistorico">Reporte Historico</router-link></li>
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

      <!-- Empleado: ver dos botones de función (por ahora imprimen en consola) -->
      <template v-else-if="userRole === 'Empleado'">
        <button class="btn-function" @click="employeeAction('Histórico de Empleados')">Histórico</button>
        <button class="btn-function" @click="employeeAction('Reporte Completo Empleados')">Completo</button>
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
              <input type="date" v-model="startDate" />
            </label>

            <label>
              Fecha Fin:
              <input type="date" v-model="endDate" />
            </label>

            <button @click="loadHistoricalReport">Aplicar</button>
          </div>

          <!-- Vista previa -->
          <transition name="fade">
            <div v-if="reportData.length > 0" class="report-viewer">
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
                    <tr v-for="(row, index) in reportData" :key="index">
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
                <button class="btn-small" @click="loadLast12EmployerPayrolls" :disabled="!selectedCompany || reportLoading">Cargar últimos 12</button>
              </div>
            </div>
          </div>

          <div v-if="last12Payrolls.length" class="report-viewer" style="margin-top:12px;">
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
                  <tr v-for="payroll in last12Payrolls" :key="payrollKey(payroll) || JSON.stringify(payroll)" class="payroll-row">
                    <td>{{ payrollPeriod(payroll) }}</td>
                    <td>{{ payrollNetSalary(payroll) }}</td>
                    <td>{{ payrollCost(payroll) }}</td>
                    <td>
                      <select v-model="reportFormats[payrollKey(payroll) || payroll.payrollId]">
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
              <div v-if="reportUrl || currentReportText || currentReportBlob" class="report-viewer" style="margin-top:12px;">
                <div class="viewer-header">
                  <h4>Visor de Reporte</h4>
                  <div class="viewer-actions">
                    <button class="btn-download" @click="downloadEmployerCompleteReport(selectedReportPayrollId)">⬇️ Descargar</button>
                    <button class="btn-close-viewer" @click="clearReport">Cerrar</button>
                  </div>
                </div>

                <div class="viewer-content" style="padding:12px;">
                  <div v-if="currentReportText">
                    <h5>CSV Preview</h5>
                    <pre style="white-space:pre-wrap;max-height:400px;overflow:auto;background:#111;padding:12px;border-radius:6px;color:#dcdcdc">{{ currentReportText }}</pre>
                  </div>
                  <div v-else-if="reportUrl">
                    <h5>PDF Preview</h5>
                    <div class="pdf-viewer" style="height:500px;">
                      <iframe :src="reportUrl" style="width:100%;height:100%;border:0"></iframe>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
        <!-- POR EMPLEADO -->
        <div v-if="currentView === 'porEmpleado'">
          <p>Vista de reporte por empleado (pendiente de implementar).</p>
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
      startDate: "",
      endDate: "",
      // timer para debounce al aplicar filtros automáticamente
      filterDebounceTimer: null,

      // Datos del histórico
      reportData: [],

      // Vistas
      currentView: "historico",
      // Empresa seleccionada (objeto)
      selectedCompany: null,
      // Reportes por planilla
      last12Payrolls: [],
      reportFormats: {},
      reportLoading: false,
      generatingReport: false,
      currentReportBlob: null,
      currentReportText: null,
      message: '',
      messageType: 'success',
      reportUrl: null,
      selectedReportPayrollId: null,
    };
  },

  watch: {
    currentView() {
      this.clearReport();
    },
    // Cuando cambie el id seleccionado, actualizar objeto seleccionado y persistir
    selectedCompanyId(newVal) {
      if (!newVal) {
        this.selectedCompany = null;
        localStorage.removeItem('selectedCompany');
        // schedule reload with no company
        this.scheduleLoadHistoricalReport();
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
        // aplicar filtros automáticamente al cambiar empresa
        this.scheduleLoadHistoricalReport();
      }
    }
    ,
    startDate() {
      this.scheduleLoadHistoricalReport();
    },
    endDate() {
      this.scheduleLoadHistoricalReport();
    }
  },
  async mounted() {
    this.loadCurrentUserProfile();
    // Para Empleadores (y admin) mantener comportamiento previo: cargar empresas y restaurar selección
    if (this.userRole === 'Empleador' || this.isAdmin) {
      // Esperar a que carguen las empresas antes de restaurar selección
      await this.loadCompanies();
      this.loadSelectedCompany();
    } else if (this.userRole === 'Empleado') {
      // No cargar reportes históricos para empleados. Ejecutar función 1 (placeholder)
      try {
        this.employeeAction('Funcion 1 ejecutada (mounted)');
      } catch (e) {
        // noop
      }
    } else {
      // Otros roles: cargar empresas pero no disparar historial automáticamente
      await this.loadCompanies();
    }
  },

  methods: {
    // Programa una llamada a `loadHistoricalReport` con debounce corto
    scheduleLoadHistoricalReport(delay = 400) {
      try {
        if (this.filterDebounceTimer) clearTimeout(this.filterDebounceTimer);
      } catch (e) {
        // noop
      }
      this.filterDebounceTimer = setTimeout(() => {
        this.loadHistoricalReport();
        this.filterDebounceTimer = null;
      }, delay);
    },

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

    // Limpia todo lo relacionado a reportes
    clearAllReportState() {
      try {
        localStorage.removeItem('selectedCompany'); } catch (e) { /* noop */ }
      this.selectedCompany = null;
      this.selectedCompanyId = "";

      // limpiar listas y formatos
      try { this.clearReport(); } catch (e) { /* noop */ }
      this.last12Payrolls = [];
      this.reportFormats = {};
      this.reportLoading = false;
      this.generatingReport = false;
      this.currentReportBlob = null;
      this.currentReportText = null;
      this.reportUrl = null;
      this.selectedReportPayrollId = null;
    },

    async loadCompanies() {
      try {
        const stored = localStorage.getItem("userData");
        if (!stored) return;

        const userData = JSON.parse(stored);

        if (userData.personType === "Empleador") {
          // Endpoint para obtener empresas del empleador
          const res = await axios.get(API_ENDPOINTS.MIS_EMPRESAS_ID(userData.id));

          if (res.data && res.data.success) {
            this.companies = res.data.empresas || [];
          } else if (Array.isArray(res.data)) {
            this.companies = res.data;
          } else {
            this.companies = [];
          }
        } else {
          // Si es empleado o admin, puedes adaptar aquí
          this.companies = [];
        }

        // Por defecto: opción "Todas" en histórico
        this.selectedCompanyId = "";
        // si existe una empresa seleccionada en localStorage, restaurarla
        // (no await: esta función puede ser llamada desde mounted que sí espera)
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

    // Cuando cambian fechas, aplicar filtros automáticamente (con debounce)
    // watchers para fechas se manejan aquí (se usan desde template v-model)
    // Agregamos watchers manualmente mediante métodos para mantener claridad.

    // ---------------------------
    // Histórico
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

        if (this.startDate) params.startDate = this.startDate;
        if (this.endDate) params.endDate = this.endDate;

        // Log corto para debugging en caso de parámetros vacíos
        console.log('loadHistoricalReport - params:', params);
        try {
          const q = new URLSearchParams();
          Object.entries(params).forEach(([k, v]) => q.append(k, String(v)));
          const fullUrl = `${API_ENDPOINTS.EMPLOYER_HISTORICAL_REPORT}${q.toString() ? '?' + q.toString() : ''}`;
          console.log('loadHistoricalReport - fullUrl:', fullUrl);
        } catch (e) {
          console.log('loadHistoricalReport - could not build fullUrl', e);
        }

        const res = await axios.get(API_ENDPOINTS.EMPLOYER_HISTORICAL_REPORT, { params });
        if (Array.isArray(res.data)) {
          this.reportData = res.data;
        } else if (res.data?.success) {
          this.reportData = res.data.data;
        } else {
          this.reportData = [];
        }
      } catch (err) {
        console.error("Error cargando reporte:", err?.message, err?.response?.data);
        this.reportData = [];
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
        if (this.startDate) {
          params.startDate = this.startDate;
        }
        if (this.endDate) {
          params.endDate = this.endDate;
        }

        // Log params and full URL for debugging 400 cases
        try {
          const q = new URLSearchParams();
          Object.entries(params).forEach(([k, v]) => q.append(k, String(v)));
          const fullUrl = `${API_ENDPOINTS.EMPLOYER_HISTORICAL_REPORT_CSV}${q.toString() ? '?' + q.toString() : ''}`;
          console.log('downloadEmployerHistoricalCsv - params:', params, 'fullUrl:', fullUrl);
        } catch (e) {
          console.log('downloadEmployerHistoricalCsv - could not build fullUrl', e);
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

    // Cargar últimas 12 planillas para la empresa seleccionada (más robusto y con logs)
    async loadLast12EmployerPayrolls() {
      console.log('loadLast12EmployerPayrolls - selectedCompanyId:', this.selectedCompanyId, 'selectedCompany:', this.selectedCompany);

      // intentar varias rutas para obtener el identificador de la empresa
      const companyCedula = this.selectedCompany?.cedulaJuridica || this.selectedCompanyId || this.selectedCompany?.cedula || this.selectedCompany?.cedulaFiscal;
      if (!companyCedula) {
        console.warn('No hay empresa seleccionada para cargar planillas (companyCedula vacío)');
        return;
      }

      this.reportLoading = true;
      try {
        const url = API_ENDPOINTS.PAYROLL_REPORT_LAST_12(companyCedula);
        console.log('Llamando a endpoint de últimas 12 planillas:', url);

        const res = await axios.get(url);
        console.log('Respuesta de últimas 12 planillas:', res);

        let payrolls = [];
        // admitir distintos formatos de respuesta
        if (res.data && res.data.success) payrolls = res.data.payrolls || res.data.data || [];
        else if (Array.isArray(res.data)) payrolls = res.data;
        else {
          console.warn('Formato inesperado en la respuesta de last12:', res.data);
          payrolls = [];
        }

        this.last12Payrolls = payrolls;

        // DEBUG: mostrar ejemplo de los objetos recibidos para ayudar a mapear campos
        if (payrolls && payrolls.length > 0) {
          console.log('Ejemplo planilla[0]:', payrolls[0]);
          try { console.log('Claves planilla[0]:', Object.keys(payrolls[0])); } catch (e) { /* noop */ }
        }

        // Inicializar formatos defensivamente
        const formats = {};
        payrolls.forEach(p => {
          const id = p?.payrollId || p?.id || p?.planillaId || JSON.stringify(p);
          formats[id] = 'pdf';
        });
        this.reportFormats = formats;

        console.log('Planillas cargadas:', this.last12Payrolls.length, 'formatos inicializados:', this.reportFormats);
      } catch (err) {
        console.error('Error cargando últimas 12 planillas', err?.response || err);
        this.last12Payrolls = [];
      } finally {
        this.reportLoading = false;
      }
    },

    // Helper para obtener un id consistente de una planilla
    payrollKey(payroll) {
      return payroll?.payrollId || payroll?.id || payroll?.planillaId || payroll?.uuid || payroll?.key || null;
    },

    // Helpers para mostrar campos de planilla de forma defensiva
    payrollPeriod(payroll) {
      // Si backend devuelve una fecha de periodo, mostrarla en formato YYYY-MM-DD
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
      try { this.clearReport(); } catch (e) { /* noop */ }

      try {
        const urlPdf = API_ENDPOINTS.PAYROLL_REPORT_PDF(payrollId);
        const resp = await axios.get(urlPdf, { responseType: 'blob' });
        const pdfBlob = new Blob([resp.data], { type: 'application/pdf' });
        this.currentReportBlob = pdfBlob;
        this.reportUrl = window.URL.createObjectURL(pdfBlob);
        // asignar el id seleccionado PARA EL VISOR una vez que el preview está listo
        this.selectedReportPayrollId = payrollId;
        this.currentReportText = null;
        this.showMessage('PDF listo para vista previa', 'success');

      } catch (err) {
        console.error('Error generando reporte', err?.response || err);
        this.clearReport();
        this.showMessage('Error al generar reporte', 'error');
      } finally {
        this.generatingReport = false;
      }
    },

    // Descargar el blob o solicitar CSV/PDF según formato seleccionado
    async downloadEmployerCompleteReport(payrollId) {
      try {
        const pid = payrollId || this.selectedReportPayrollId;
        if (!pid) {
          console.warn('downloadEmployerCompleteReport: payrollId faltante');
          return;
        }

        const desiredFormat = this.reportFormats[pid] || 'pdf';

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
        if (this.currentReportBlob && String(this.selectedReportPayrollId) === String(pid)) {
          const url = window.URL.createObjectURL(this.currentReportBlob);
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
        if (!this.last12Payrolls || !this.last12Payrolls.length) return null;
        return this.last12Payrolls.find(p => String(this.payrollKey(p) || p.payrollId || p.id || p.planillaId) === String(pid)) || null;
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
      this.clearReport();
      this.currentView = view;
    },

    clearReport() {
      // revocar URL previa si existe
      try {
        if (this.reportUrl && this.reportUrl.startsWith('blob:')) {
          window.URL.revokeObjectURL(this.reportUrl);
        }
      } catch (e) {
        // noop
      }
      this.reportUrl = null;
      this.currentReportBlob = null;
      this.selectedReportPayrollId = null;
      this.currentReportText = null;
      this.reportData = [];
    },
    // Acción placeholder para botones de empleado
    employeeAction(msg) {
      try {
        console.log('employeeAction:', msg);
      } catch (e) {
        // noop
      }
    },

    logout() {
      localStorage.removeItem("userData");
      localStorage.removeItem("selectedCompany");
      this.$router.push("/");
    }
  }
};
</script>

<style scoped src="@/assets/style/Reportes.css"></style>