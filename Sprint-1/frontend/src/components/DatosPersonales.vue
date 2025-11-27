<template>
  <div class="wrap">
    <!-- HEADER -->
    <header class="header">
      <nav class="nav">
          <div class="display">
              <div class="logo-box">
                  <span class="f">F</span>
              </div>
              <div class="texts">
                  <h1>{{ userName }}</h1>
                  <p>{{ userRole }}</p>
              </div>
          </div>

          <ul class="nav-list">
              <!-- Siempre visibles -->
              <li><router-link to="/Profile">Datos Personales</router-link></li>
              <li><router-link to="/Reportes">Reportes</router-link></li>
              <!-- Solo Empleador: Registrar Empresa -->
              <li v-if="userRole === 'Empleador'">
                  <router-link to="/FormEmpresa">Registrar Empresa</router-link>
              </li>

              <!--Solo Empleado: Añadir horas-->
              <li v-if="userRole === 'Empleado'">
                  <router-link to="/RegisterHoras">Registrar Horas</router-link>
              </li>


              <!-- Dropdown de empresas SOLO para Empleador -->
              <li v-if="userRole === 'Empleador'" class="company-dropdown-item">
                  <select v-model="selectedCompany" @change="onCompanyChange" class="company-select">
                      <option :value="null">Seleccionar Empresa</option>
                      <option v-for="company in companies" :key="company.cedulaJuridica" :value="company">
                          {{ company.nombre }}
                      </option>
                  </select>
              </li>

              <!-- Solo Administrador: Ver Toda Empresa -->
              <li v-if="isAdmin">
                  <router-link to="/PageEmpresaAdmin">Ver Toda Empresa</router-link>
              </li>

              <li v-if="userRole === 'Empleado'">
                  <router-link to="/DashboardEmpleado">Dashboard de Pago</router-link>
              </li>

              <li v-if="userRole === 'Empleador'">
                  <router-link to="/DashboardEmpleador">Dashboard de pago</router-link>
              </li>

              <!-- Solo Empleado: Seleccionar Beneficios -->
              <li v-if="userRole === 'Empleado'">
                  <router-link to="/SelectBeneficios">Seleccionar Beneficios</router-link>
              </li>

              <!-- Nombre de Empresa: Solo Empleados -->
              <li v-if="userRole === 'Empleado' && selectedCompany" class="company-info">
                  <a href="#" @click.prevent>
                      Empresa: {{ selectedCompany.nombre }}
                  </a>
              </li>

              <!-- Siempre visible -->
              <li><a href="#" @click.prevent="logout">Cerrar Sesión</a></li>
          </ul>
      </nav>
  </header>

<main class="hero">
    <div class="profile-card">
      <h2>Perfil del Usuario</h2>
      <div class="table-container">
        <table class="profile-table">
          <tbody>
            <tr>
              <th>Nombre</th>
              <td>{{ userName }}</td>
            </tr>
            <tr>
              <th>Correo</th>
              <td>{{ email }}</td>
            </tr>
            <tr>
              <th>Teléfono</th>
              <td>{{ personalPhone }}</td>
            </tr>
            <tr>
              <th>Dirección</th>
              <td>{{ direction }}</td>
            </tr>
            <tr>
              <th>Fecha de nacimiento</th>
              <td>{{ birthdate }}</td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </main>    <!-- FOOTER -->
    <footer>
      <div>©2025 Fiesta Fries</div>
    </footer>
  </div>
</template>

<script>
import axios from "axios";
import { API_ENDPOINTS } from '../config/apiConfig';

export default {
  name: "PerfilUsuario",
  data() {
    return {
      userName: "Cargando...",
      email: "",
      personalPhone: "",
      direction: "",
      birthdate: "",
      userRole: "",
      companies: [],
      selectedCompany: null,
      userData: {},
      isAdmin: false,
      // Reportes
      mostrandoReportes: false,
      reportLoading: false,
      last12Payrolls: [],
      reportFormats: {},
      generatingReport: false,
      selectedReportPayrollId: null,
      currentReportUrl: null,
      currentReportFormat: null,
      currentReportBlob: null
    };
  },
  mounted() {
    this.loadCurrentUserProfile();
    this.loadCompanies();
  },
  methods: {
    async loadCurrentUserProfile() {
    const stored = localStorage.getItem("userData");
    if (!stored) {
      this.$router.push("/");
      return;
  }

  // parseamos todo el objeto para usarlo luego
  let userData;
  try {
    userData = JSON.parse(stored);
  } catch (e) {
    console.error("Error parseando userData desde localStorage:", e);
    this.$router.push("/");
    return;
  }
  this.userData = userData;

  // Normalizar distintas representaciones del flag admin
  this.isAdmin = userData?.isAdmin;
 

  // Si es Admin, asignar valores y salir antes de cualquier llamada al backend
  if (this.isAdmin) {
    this.userName = "AdminAccount";
    this.userRole = "Admin";
    this.email = userData?.email || "admin@local";
    this.personalPhone = "N/A";
    this.direction = "N/A";
    this.birthdate = "N/A";
    console.log("Usuario es admin, seteado AdminAccount y saliendo")
    return;
  } else {

    console.log("\n\n\nUsuario NO es admin, cargando perfil Persona\n\n\n");
    
  }
  console.log("userData cargado:", userData.isAdmin ? "(Admin)" : "(No Admin)", userData);

  // Si no es admin, proceder a cargar perfil Persona
  const userId = userData?.id || userData?.PK_User || userData?.userId;
  if (!userId) {
    console.warn("No se encontró userId en userData:", userData);
    // asignar algo para no quedar en "Cargando..."
    this.userName = `${userData?.firstName || "Usuario"} ${userData?.secondName || ""}`.trim();
    return;
  }

  try {
    const profileRes = await axios.get(API_ENDPOINTS.PERSON_PROFILE(userId));
    const p = profileRes.data || {};

    this.userName = `${p.firstName || ""} ${p.secondName || ""}`.trim() || (userData?.email || "Usuario");
    this.email = p.email || userData?.email || "";
    this.personalPhone = p.personalPhone || "N/A";
    this.direction = p.direction || "N/A";
    this.birthdate = p.birthdate ? new Date(p.birthdate).toLocaleDateString() : "N/A";
    this.userRole = p.personType || userData?.personType || "";
  } catch (err) {
    console.error("Error obteniendo perfil:", err);
    // En caso de fallo, evitar dejar "Cargando..."
    this.userName = userData?.email || "Usuario";
    this.email = userData?.email || "";
  }
},
async loadCompanies() {
  try {
    const stored = JSON.parse(localStorage.getItem("userData"));
    const userId = stored?.id;
    if (!userId) return;

    // Si es ADMIN, cargar TODAS las empresas
    if (this.isAdmin || stored.isAdmin) {
      console.log('Usuario es admin, cargando TODAS las empresas');
      const empresasRes = await axios.get(API_ENDPOINTS.EMPRESAS_TODAS);
      
      // Verificar estructura de respuesta
      if (empresasRes.data && empresasRes.data.success) {
        this.companies = empresasRes.data.empresas || [];
      } else if (Array.isArray(empresasRes.data)) {
        this.companies = empresasRes.data;
      } else {
        this.companies = [];
      }
      
      console.log(`Admin - ${this.companies.length} empresas cargadas`);
      return;
    }

    // Determinar tipo de usuario
    const userType = stored.personType;
    console.log(`Usuario es ${userType}, cargando empresas...`);

    if (userType === "Empleador") {
      // Buscar empresas del Empleador específico
      console.log('Usuario es Empleador, cargando sus empresas');
      const empresasRes = await axios.get(API_ENDPOINTS.MIS_EMPRESAS_ID(userId));
      
      // Verificar la estructura de la respuesta
      if (empresasRes.data && empresasRes.data.success) {
        this.companies = empresasRes.data.empresas || [];
      } else if (Array.isArray(empresasRes.data)) {
        this.companies = empresasRes.data;
      } else {
        this.companies = [];
      }
    } 
    else if (userType === "Empleado") {
        console.log('=== DEBUG EMPLEADO ===');
        console.log('User ID:', userId);
        console.log('User Data completo:', stored);

    try {
        const endpointURL = API_ENDPOINTS.EMPRESA_BY_EMPLOYEE(userId);
        console.log('🔗 Endpoint URL:', endpointURL);
        
        const empresaRes = await axios.get(endpointURL);
        console.log('Response status:', empresaRes.status);
        console.log('Response headers:', empresaRes.headers);
        console.log('Response data:', empresaRes.data);
        console.log('Response data type:', typeof empresaRes.data);
        
        if (empresaRes.data) {
          console.log('Data recibida válida');
          if (empresaRes.data.success && empresaRes.data.empresa) {
            console.log('Empresa encontrada');
            this.companies = [empresaRes.data.empresa];
          } else if (empresaRes.data.nombre) {
            console.log('Estructura alternativa - objeto directo');
            this.companies = [empresaRes.data];
          } else {
            console.log('Estructura inesperada');
            console.log('Keys en response.data:', Object.keys(empresaRes.data));
            this.companies = [];
          }
        } else {
          console.log('Response.data es null/undefined');
          this.companies = [];
        }
        
      } catch (error) {
        console.error('Error message:', error.message);
        console.error('Error code:', error.code);
        console.error('Error response:', error.response);
        console.error('Error response data:', error.response?.data);
        console.error('Error response status:', error.response?.status);
        console.error('Error response headers:', error.response?.headers);
        
        this.companies = [];
      }

    }
    else {
      console.log(`Usuario es ${userType}, no se cargan empresas`);
      this.companies = [];
      return;
    }

    // Para empleados: auto-seleccionar si solo hay una empresa
    // Para empleadores: SIEMPRE reiniciar el dropdown
    if (userType === "Empleado" && this.companies.length === 1) {
      this.selectedCompany = this.companies[0];
      this.saveSelectedCompany();
      console.log(`Empresa auto-seleccionada para empleado: ${this.selectedCompany.nombre}`);
    } else {
      // Empleadores o múltiples empresas: SIEMPRE mostrar "Seleccionar Empresa"
      this.selectedCompany = null;
      if (this.companies.length > 0) {
        console.log(`${this.companies.length} empresa(s) cargada(s) - Usuario debe seleccionar`);
      } else {
        console.log("No se encontraron empresas asociadas");
      }
    }
  } catch (err) {
    console.error("Error cargando empresas:", err);
    this.companies = [];
  }
},

    saveSelectedCompany() {
      if (this.selectedCompany && this.selectedCompany.cedulaJuridica) {
        // Guardar toda la información de la empresa en localStorage
        localStorage.setItem("selectedCompany", JSON.stringify(this.selectedCompany));
        console.log("Empresa seleccionada guardada:", this.selectedCompany.nombre);
      } else {
        localStorage.removeItem("selectedCompany");
      }
    },

    onCompanyChange() {
      if (this.selectedCompany && this.selectedCompany.cedulaJuridica) {
        this.saveSelectedCompany();
        // Redirigir a la página de administración de empresas
        this.$router.push('/PageEmpresaAdmin');
      }
    },

    logout() {
      localStorage.removeItem("userData");
      localStorage.removeItem("selectedCompany");
      this.$router.push("/");
    },

    // ============================================
    // MÉTODOS PARA REPORTES
    // ============================================
    async toggleReportes() {
      this.mostrandoReportes = !this.mostrandoReportes;
      
      if (this.mostrandoReportes && this.last12Payrolls.length === 0) {
        await this.loadLast12Payrolls();
      }
    },

    async loadLast12Payrolls() {
      // Obtener cédula del empleado (ID numérico)
      const stored = JSON.parse(localStorage.getItem("userData"));
      
      console.log('Datos en localStorage:', stored);
      console.log('PersonaId del empleado:', stored?.personaId);
      
      // Usar el personaId del empleado (ID numérico)
      const employeeId = stored?.personaId;
      
      if (!employeeId) {
        console.error('No hay personaId en localStorage');
        alert('No se pudo obtener el ID del empleado');
        return;
      }

      this.reportLoading = true;
      try {
        const url = API_ENDPOINTS.PAYROLL_EMPLOYEE_LAST_12_PAYMENTS(employeeId);
        console.log('Cargando reportes desde:', url);
        console.log('   PersonaId empleado:', employeeId);
        
        const response = await axios.get(url);
        console.log('📦 Respuesta completa:', response.data);

        let reports = [];
        
        // El backend retorna un array directo con reportId, periodo, salarioBruto, salarioNeto
        if (Array.isArray(response.data)) {
          reports = response.data;
        } else if (response.data && response.data.success) {
          reports = response.data.reports || response.data.payrolls || [];
        }

        this.last12Payrolls = reports;
        console.log('Reportes cargados:', this.last12Payrolls.length);
        console.log('Datos del primer reporte:', this.last12Payrolls[0]);

        // Inicializar formatos usando reportId
        const formats = {};
        reports.forEach(report => {
          formats[report.reportId] = 'pdf';
        });
        this.reportFormats = formats;

      } catch (error) {
        console.error('Error cargando reportes:', error);
        console.error('Response:', error.response?.data);
        console.error('Status:', error.response?.status);
        alert('Error al cargar reportes: ' + (error.response?.data?.message || error.message));
        this.last12Payrolls = [];
      } finally {
        this.reportLoading = false;
      }
    },

    async generateReport(payrollId) {
      const format = this.reportFormats[payrollId] || 'pdf';
      this.generatingReport = true;
      this.selectedReportPayrollId = payrollId;

      try {
        // Obtener personaId del empleado
        const stored = JSON.parse(localStorage.getItem("userData"));
        const employeeId = stored?.personaId;
        
        console.log('Generando reporte - PersonaId empleado:', employeeId);
        
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

        console.log('🔗 Generando reporte:', url);

        // Hacer petición para obtener el archivo
        const response = await axios.get(url, {
          responseType: 'blob'
        });

        console.log('Reporte generado');

        // Crear URL del blob
        const blob = new Blob([response.data], {
          type: format === 'pdf' ? 'application/pdf' : 'text/csv'
        });
        const blobUrl = window.URL.createObjectURL(blob);

        this.currentReportUrl = blobUrl;
        this.currentReportFormat = format;
        this.currentReportBlob = blob;

      } catch (error) {
        console.error('Error generando reporte:', error);
        alert('Error al generar el reporte');
      } finally {
        this.generatingReport = false;
      }
    },

    async downloadReport() {
      if (!this.currentReportBlob || !this.selectedReportPayrollId) return;

      const format = this.currentReportFormat;
      const extension = format === 'pdf' ? 'pdf' : 'csv';
      const fileName = `Reporte_Planilla_${this.selectedReportPayrollId}.${extension}`;

      const link = document.createElement('a');
      link.href = this.currentReportUrl;
      link.download = fileName;
      link.click();

      console.log('Descargando:', fileName);
    },

    clearReport() {
      if (this.currentReportUrl) {
        window.URL.revokeObjectURL(this.currentReportUrl);
      }
      this.currentReportUrl = null;
      this.currentReportFormat = null;
      this.selectedReportPayrollId = null;
      this.currentReportBlob = null;
    },

    formatDate(dateString) {
      if (!dateString) return 'N/A';
      const date = new Date(dateString);
      return date.toLocaleDateString('es-ES');
    },

    formatCurrency(amount) {
      if (!amount) return '0';
      return parseFloat(amount).toLocaleString('es-CR');
    }
  }
};
</script>

<style scoped>
    /* wrap es el contenedor principal (el fondo) */
    .wrap {
        min-height: 100vh;
        display: flex;
        flex-direction: column;
        background: #1e1e1e;
        color: whitesmoke;
    }

    .header {
        background: rgba(0, 0, 0, 0.25);
        border-bottom: 1px solid rgba(255, 255, 255, 0.15);
        padding: 16px 64px;
    }

    .nav {
        display: flex;
        justify-content: space-between;
        align-items: center;
    }

    .display {
        display: flex;
        align-items: center;
        gap: 18px;
        margin-bottom :20px;
    }

    .logo-box {
        width: 50px;
        height: 50px;
        background: linear-gradient(180deg, #51a3a0, hsl(178, 77%, 86%));
        /* Degradado azul */
        border-radius: 16px;
        display: flex;
        align-items: center;
        justify-content: center;
        flex-shrink: 0;
    }

        .logo-box .f {
            font-weight: 800;
            font-size: 24px;
            color: white;
            line-height: 1;
        }

    /* Estilo Nombre Usuario */
    .texts h1 {
        margin: 0;
        font-size: 24px;
    }

    /* Estilo Rol Usuario */
    .texts p {
        margin: 6px 0 0;
        color: #bdbdbd;
        font-size: 14px;
    }

    /* Lista de botones */
    .nav-list {
        list-style: none;
        display: flex;
        gap: 2rem;
        margin: 0;
        padding: 0;
        flex-wrap: wrap; 
        align-items: center; 
    }

        .nav-list a {
            color: #bdbdbd;
            text-decoration: none;
            padding: 0.5rem 1rem;
            border-radius: 4px;
            transition: all 0.3s;
            font-size: 14px;
        }

            .nav-list a:hover,
            .nav-list a.router-link-active {
                background-color: rgba(255, 255, 255, 0.1);
                color: white;
            }

    /* Dropdown en header */
    .company-select {
      background: rgba(255, 255, 255, 0.1);
      border: 1px solid rgba(255, 255, 255, 0.2);
      border-radius: 4px;
      color: #bdbdbd;
      padding: 0.5rem 1rem;
      font-size: 14px;
      cursor: pointer;
      transition: all 0.3s;
    }

    .company-select:hover {
        background-color: rgba(255, 255, 255, 0.15);
        color: rgb(0, 0, 0);
    }

    .company-select:focus {
        outline: none;
        background-color: rgba(255, 255, 255, 0.2);
        color: rgb(0, 0, 0);
    }

    /* Asegurar que el select tenga el mismo alto que los otros enlaces */
    .company-select {
        height: 100%;
    }

    /* Sección principal con flex para centrar contenido */
    .hero {
        display: flex;
        align-items: center;
        justify-content: center;
        color: whitesmoke;
        padding: 48px 64px;
        gap: 40px;
        flex: 1 0 auto;
    }

    .features-container {
        display: flex;
        gap: 20px;
        flex-wrap: wrap;
        justify-content: center;
    }

    .login-card {
        width: 280px;
        min-height: 180px;
        background: rgb(71, 69, 69);
        border: 1px solid rgba(255, 255, 255, 0.15);
        padding: 22px;
        border-radius: 8px;
        box-shadow: 0 6px 18px rgba(0, 0, 0, 0.35);
        display: flex;
        flex-direction: column;
        justify-content: space-between;
    }

        /* Título del card */
        .login-card h2 {
            color: #eee;
            margin: 0 0 12px;
            font-weight: 600;
            font-size: 16px;
        }

        /* Texto del card */
        .login-card p {
            color: #bdbdbd;
            font-size: 14px;
            margin-bottom: 20px;
            flex-grow: 1;
        }

    /* Botón */
    .btn {
        width: 100%;
        padding: 10px 12px;
        border-radius: 6px;
        border: 0;
        font-weight: 600;
        cursor: pointer;
        background: #1fb9b4;
        color: white;
        text-align: center;
        text-decoration: none;
        font-size: 14px;
        display: block;
        transition: background-color 0.3s;
    }

        .btn:hover {
            background: #1aa19c;
        }
    

    /* Estilo tabla Datos */
    .profile-card {
        width: 100%;
        max-width: 600px;
        background: rgb(71, 69, 69);
        border: 1px solid rgba(255, 255, 255, 0.15);
        padding: 24px;
        border-radius: 10px;
        box-shadow: 0 6px 18px rgba(0, 0, 0, 0.35);
    }

    .profile-card h2 {
        color: #eee;
        margin: 0 0 16px;
        font-weight: 600;
        font-size: 18px;
        text-align: center;
    }

    .table-container {
        overflow-x: auto;
    }

    .profile-table {
        width: 100%;
        border-collapse: collapse;
        font-size: 14px;
    }

    .profile-table th {
        text-align: left;
        padding: 10px;
        background: rgba(255, 255, 255, 0.08);
        color: #1fb9b4;
        width: 40%;
        border-bottom: 1px solid rgba(255, 255, 255, 0.1);
    }

    .profile-table td {
        padding: 10px;
        color: #eee;
        border-bottom: 1px solid rgba(255, 255, 255, 0.1);
    }

    .profile-table tr:last-child th,
    .profile-table tr:last-child td {
        border-bottom: none;
    }


    /* Estilo footer *Compartido entre rutas* */
    footer {
        background: #fff;
        padding: 28px 64px;
        border-top: 1px solid #eee;
        color: #8b8b8b;
        display: flex;
        align-items: center;
        justify-content: space-between;
    }

    .footer-text p {
        margin: 0;
        font-size: 14px;
    }

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

    /* Adaptar según resolución */
    @media (max-width: 900px) {
        .hero {
            flex-direction: column;
            align-items: flex-start;
            padding: 36px;
            gap: 30px;
        }

        .header {
            padding: 16px 36px;
        }

        .nav {
            flex-direction: column;
            gap: 20px;
        }

        .nav-list {
            flex-wrap: wrap;
            justify-content: center;
        }

        .features-container {
            flex-direction: column;
            width: 100%;
        }

        .login-card {
            width: 100%;
            max-width: 100%;
        }

        footer {
            flex-direction: column;
            gap: 10px;
            text-align: center;
            padding: 20px 36px;
        }
    }

    @media (max-width: 600px) {
        .hero {
            padding: 24px;
        }

        .brand {
            flex-direction: column;
            text-align: center;
            gap: 10px;
        }

        .texts h1 {
            font-size: 20px;
        }
    }

    /* ============================================ */
    /* ESTILOS PARA REPORTES */
    /* ============================================ */

    .content {
      background: rgb(71,69,69);
      border-radius: 10px;
      padding: 25px;
      border: 1px solid rgba(255,255,255,0.12);
      margin: 20px 64px;
    }

    .reportes-management {
      width: 100%;
    }

    .reportes-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 25px;
    }

    .reportes-header h3 {
      margin: 0;
      color: #eee;
    }

    .btn-secondary {
      background: #6c757d;
      color: white;
      border: none;
      padding: 10px 15px;
      border-radius: 6px;
      cursor: pointer;
      font-weight: 600;
      transition: all 0.3s;
    }

    .btn-secondary:hover {
      background: #5a6268;
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

    .loading {
      text-align: center;
      padding: 40px;
      color: #1fb9b4;
      font-size: 18px;
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

    .table-container {
      overflow-x: auto;
    }

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

    .selected-row {
      background: rgba(31, 185, 180, 0.1);
      border-left: 3px solid #1fb9b4;
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
        transform: translateY(-10px);
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

    /* RESPONSIVE REPORTES */
    @media (max-width: 900px) {
      .content {
        margin: 20px;
        padding: 15px;
      }

      .reportes-header {
        flex-direction: column;
        gap: 15px;
      }

      .viewer-header {
        flex-direction: column;
        gap: 15px;
      }

      .viewer-actions {
        width: 100%;
        justify-content: center;
      }

      .btn-download,
      .btn-close-viewer {
        flex: 1;
      }
    }
</style>
