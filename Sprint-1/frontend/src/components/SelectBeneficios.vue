<template>
  <div class="wrap">
    <!-- HEADER (igual al de DatosPersonales.vue) -->
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
          <li v-if="userRole === 'Empleado'">
            <router-link to="/RegisterHoras">Registrar Horas</router-link>
          </li>
          <li v-if="userRole === 'Empleado' && selectedCompany" class="company-info">
            <a href="#" @click.prevent>Empresa: {{ selectedCompany.nombre }}</a>
          </li>
          <li v-if="userRole === 'Empleado'">
            <router-link to="/SelectBeneficios">Seleccionar Beneficios</router-link>
          </li>
          <li><a href="#" @click.prevent="logout">Cerrar Sesión</a></li>
        </ul>
      </nav>
    </header>

    <main class="hero">
      <div class="beneficios-container">
        <div class="page-header">
          <div class="header-actions">
            <button @click="volverAtras" class="btn-volver">
              ← Volver
            </button>
            <h2>Lista de Beneficios</h2>
          </div>
          <div class="company-badge">
            Empresa: <strong>{{ selectedCompany?.nombre }}</strong>
          </div>
        </div>

        <!-- Mensaje global -->
        <div v-if="message" class="message" :class="{ 'error': messageType === 'error', 'success': messageType === 'success' }">
          {{ message }}
        </div>

        <!-- Beneficios Seleccionados -->
        <div v-if="empleadoId" class="beneficios-table-container" style="margin-bottom: 16px;">
          <div class="table-wrapper">
            <table class="beneficios-table">
              <thead>
                <tr class="title-row">
                  <th colspan="6" class="table-title">Beneficios Seleccionados</th>
                </tr>
                <tr>
                  <th>Nombre</th>
                  <th>Tipo</th>
                  <th>Quien Asume</th>
                  <th>Valor</th>
                </tr>
              </thead>

              <tbody>
                <tr v-if="loadingSelectedBenefits">
                  <td colspan="6" style="padding: 20px; color: #bdbdbd;">Cargando beneficios seleccionados…</td>
                </tr>

                <tr v-else-if="!selectedBenefits.length">
                  <td colspan="6" style="padding: 20px; color: #bdbdbd;">No hay beneficios seleccionados</td>
                </tr>

                <tr v-else v-for="item in selectedBenefits" :key="item.id || `${item.employeeId}-${item.benefitId}`" class="selected-row">
                  <td class="nombre-cell">
                    <strong>
                      {{ item.apiName || (beneficios.find(b => b.idBeneficio === item.benefitId) || {}).nombre || '—' }}
                    </strong>
                    <div v-if="item.descripcion || (beneficios.find(b => b.idBeneficio === item.benefitId) || {}).descripcion" class="descripcion">
                      {{ item.descripcion || (beneficios.find(b => b.idBeneficio === item.benefitId) || {}).descripcion }}
                    </div>
                  </td>

                  <!-- Tipo -->
                  <td>
                    <span class="type-badge" :class="getTypeClass(item.benefitType || (beneficios.find(b => b.idBeneficio === item.benefitId) || {}).tipo)">
                      {{ item.benefitType || (beneficios.find(b => b.idBeneficio === item.benefitId) || {}).tipo || '—' }}
                    </span>
                  </td>

                  <!-- Quien Asume -->
                  <td>
                    <span class="assume-badge" :class="getAssumeClass((beneficios.find(b => b.idBeneficio === item.benefitId) || {}).quienAsume)">
                      {{ (beneficios.find(b => b.idBeneficio === item.benefitId) || {}).quienAsume || '—' }}
                    </span>
                  </td>

                  <!-- Valor -->
                  <td>
                    <span :class="getValueClass((beneficios.find(b => b.idBeneficio === item.benefitId) || {}).etiqueta)" class="valor">
                      {{ formatValor(item) }}
                    </span>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>

        <!-- Estado de carga general -->
        <div v-if="loading" class="loading">
          Cargando beneficios
        </div>

        <!-- Tabla de beneficios disponibles -->
        <div v-else class="beneficios-table-container">
          <div v-if="beneficios.length > 0" class="table-wrapper">
            <table class="beneficios-table">
              <thead>
                <tr class="title-row">
                  <th colspan="6" class="table-title">Beneficios Disponibles</th>
                </tr>
                <tr>
                  <th>Nombre</th>
                  <th>Tipo</th>
                  <th>Quien Asume</th>
                  <th>Valor</th>
                  <th>Etiqueta</th>
                  <th>Acción</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="beneficio in beneficios" :key="beneficio.idBeneficio" :class="{ 'selected': beneficio.elegido }">
                  <td class="nombre-cell">
                    <strong>{{ beneficio.nombre }}</strong>
                    <div v-if="beneficio.descripcion" class="descripcion">
                      {{ beneficio.descripcion }}
                    </div>
                  </td>
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
                    <span :class="getValueClass(beneficio.etiqueta)" class="valor">
                      {{ formatValor(beneficio) }}
                    </span>
                  </td>
                  <td>
                    <span class="etiqueta-badge" :class="getEtiquetaClass(beneficio.etiqueta)">
                      {{ beneficio.etiqueta }}
                    </span>
                  </td>
                  <td class="accion-cell">
                    <button
                      :disabled="!beneficio.canSelect || beneficio.isProcessing"
                      :class="beneficio.canSelect ? 'btn-primary' : 'btn-disabled'"
                      @click="onSelectBenefit(beneficio)"
                    >
                      <span v-if="beneficio.isProcessing">Procesando...</span>
                      <span v-else>{{ beneficio.canSelect ? 'Elegir' : 'No disponible' }}</span>
                    </button>
                  </td>
                </tr>
              </tbody>
            </table>
            <div class="table-footer">
              <span class="beneficios-count">{{ beneficios.length }} beneficio(s)</span>
            </div>
          </div>

          <!-- Estado vacío -->
          <div v-else class="empty-state">
            <div class="empty-icon">📋</div>
            <h3>No hay beneficios disponibles</h3>
            <p>Actualmente no hay beneficios configurados para tu empresa</p>
            <button @click="volverAtras" class="btn-primary">
              ← Volver
            </button>
          </div>
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
import { API_ENDPOINTS } from '../config/apiConfig';

export default {
  name: "SelectBeneficios",
  data() {
    return {
      userName: "Cargando...",
      userRole: "",
      selectedCompany: null,
      beneficios: [],
      loading: false,
      elegiendo: false,
      message: "",
      messageType: "success",
      empleadoId: null, // Se obtiene desde backend

      // Nuevos estados para la tabla de "Beneficios Seleccionados"
      selectedBenefits: [],
      loadingSelectedBenefits: false
    };
  },
  mounted() {
    console.log("mounted: SelectBeneficios mounted");
    this.loadUserData();
    this.loadSelectedCompany();
    this.resolveEmpleadoId().then(() => {
      console.log("mounted: resolveEmpleadoId finished, empleadoId =", this.empleadoId);
      if (this.empleadoId) {
        this.loadBeneficios();
        this.loadSelectedBenefits(); // cargar la lista de seleccionados al inicio
      } else {
        console.warn("mounted: empleadoId no resuelto, no se cargan beneficios");
      }
    });
  },
  methods: {
    loadUserData() {
      console.log("loadUserData: entrando");
      const guidFromKey = localStorage.getItem("userID");

      const stored = localStorage.getItem("userData");
      let parsed = null;
      if (stored) {
        try {
          parsed = JSON.parse(stored);
          console.log("loadUserData: parsed userData =", parsed);
        } catch (e) {
          console.error("loadUserData: error parsing userData", e);
        }
      }

      this.userUniqueId = guidFromKey || (parsed && (parsed.id || parsed.PK_User || parsed.userID)) || null;

      if (parsed && parsed.personaId) {
        this.empleadoId = parsed.personaId;
        console.log("loadUserData: empleadoId tomado de userData.personaId =", this.empleadoId);
      }

      if (parsed) {
        this.userName = `${parsed.firstName || ""} ${parsed.secondName || ""}`.trim() || this.userName;
        this.userRole = parsed.personType || parsed.role || this.userRole;
      }
    },

    loadSelectedCompany() {
      try {
        const savedCompany = localStorage.getItem("selectedCompany");
        if (savedCompany) {
          this.selectedCompany = JSON.parse(savedCompany);
          return true;
        } else {
          this.showMessage("No hay empresa seleccionada", "error");
          this.$router.push("/Profile");
          return false;
        }
      } catch (error) {
        console.error("Error cargando empresa:", error);
        this.showMessage("Error al cargar empresa", "error");
        return false;
      }
    },

    async resolveEmpleadoId() {
      if (this.empleadoId) {
        return;
      }

      if (!this.userUniqueId) {
        console.warn("resolveEmpleadoId: no userUniqueId, abortando");
        return;
      }

      try {
        const url = API_ENDPOINTS.PERSON_BY_USER(this.userUniqueId);
        console.log("resolveEmpleadoId: GET ->", url);
        const resp = await axios.get(url);
        const persona = resp?.data?.persona ?? resp?.data ?? null;
        this.empleadoId = persona?.id ?? persona?.Id ?? null;
      } catch (err) {
        console.error("resolveEmpleadoId: error en petición", err);
      }
    },

    async loadBeneficios() {
      if (!this.empleadoId) {
        this.showMessage("No se pudo determinar el id del empleado. Imposible cargar beneficios.", "error");
        return;
      }

      this.loading = true;
      try {
        const beneficiosResponse = await axios.get(
          API_ENDPOINTS.BENEFICIOS_POR_EMPRESA(this.selectedCompany.cedulaJuridica)
        );

        let beneficiosData = [];
        if (beneficiosResponse.data?.success && Array.isArray(beneficiosResponse.data.beneficios)) {
          beneficiosData = beneficiosResponse.data.beneficios.map(b => ({
            ...b,
            elegido: false,
            canSelect: null,
            isProcessing: false
          }));
        }

        // Obtener beneficios seleccionados (IDs) usando el endpoint que ya tienes
        const seleccionadosResponse = await axios.get(
          API_ENDPOINTS.BENEFICIOS_SELECCIONADOS(this.empleadoId)
        );

        const seleccionados = seleccionadosResponse.data?.selectedBenefitIds || [];
        // Marcar los seleccionados
        beneficiosData = beneficiosData.map(b => ({
          ...b,
          elegido: seleccionados.includes(b.idBeneficio)
        }));

        this.beneficios = beneficiosData;

        // canSelect por cada beneficio 1 x 1
        for (const b of this.beneficios) {
          // eslint-disable-next-line no-await-in-loop
          await this.checkCanSelectForBenefit(b);
        }

        if (this.beneficios.length === 0) {
          this.showMessage("No se encontraron beneficios para tu empresa", "info");
        } else {
          this.showMessage(`Se cargaron ${this.beneficios.length} beneficio(s) disponibles`, "success");
        }
      } catch (error) {
        console.error("Error cargando beneficios:", error);
        this.showMessage("Error al cargar beneficios", "error");
        this.beneficios = [];
      } finally {
        this.loading = false;
      }
    },

    async checkCanSelectForBenefit(benefit) {
      benefit.canSelect = null;
      benefit.isProcessing = false;

      if (benefit.elegido) {
        benefit.canSelect = false;
        return;
      }

      if (!this.empleadoId) {
        benefit.canSelect = false;
        return;
      }

      const rawTipo = (benefit.tipo || "").toString();
      const tipo = rawTipo
        .trim()
        .toLowerCase()
        .normalize("NFD").replace(/[\u0300-\u036f]/g, ""); // quita tildes

      const isMontoFijo = tipo === "monto-fijo" || tipo === "monto fijo";
      const isPorcentual = tipo === "porcentual" || tipo === "porcentaje";

      if (!isMontoFijo && !isPorcentual) {
        benefit.canSelect = false;
        return;
      }

      try {
        const url = API_ENDPOINTS.CAN_SELECT_BENEFIT(this.empleadoId, benefit.idBeneficio);
        const resp = await axios.get(url);
        benefit.canSelect = !!resp?.data?.canSelect;
      } catch (err) {
        console.error("checkCanSelectForBenefit err", err);
        benefit.canSelect = false;
      }
    },

    async onSelectBenefit(benefit) {
      if (!benefit.canSelect || benefit.isProcessing) return;
      if (benefit.tipo === "API") return;

      benefit.isProcessing = true;
      try {
        const payload = {
          EmployeeId: this.empleadoId,      // recuerda PascalCase
          BenefitId: benefit.idBeneficio,
          PensionType: null,
          DependentsCount: null
        };
        console.log("POST EmployeeBenefit payload:", payload);
        const resp = await axios.post(API_ENDPOINTS.ELEGIR_BENEFICIO, payload);
        if (resp?.data?.success) {
          benefit.canSelect = false;
          benefit.elegido = true;
          this.showMessage("Beneficio seleccionado correctamente", "success");

          // REFRESCAR la tabla de Beneficios Seleccionados
          // Si el endpoint POST devuelve la entidad creada en resp.data.data puedes hacer:
          if (resp.data?.data) {
            // insertar al inicio para mostrar inmediatamente sin recargar todo
            this.selectedBenefits.unshift(resp.data.data);
          } else {
            // si no devuelve entidad, recargamos la lista completa desde el backend
            await this.loadSelectedBenefits();
          }
        } else {
          this.showMessage(resp?.data?.message || "No se pudo seleccionar el beneficio", "error");
        }
      } catch (err) {
        console.error("onSelectBenefit error", err);
        console.error("axios response data:", err.response?.data);
        console.error("axios response status:", err.response?.status);
        console.error("axios response headers:", err.response?.headers);
        if (err.response && err.response.status === 400) {
          this.showMessage(err.response.data?.message || "Selección no permitida", "error");
        } else {
          this.showMessage("Error al seleccionar beneficio", "error");
        }
      } finally {
        benefit.isProcessing = false;
      }
    },

    async elegirBeneficio(beneficio) {
      this.elegiendo = true;
      try {
        const payload = {
          employeeId: this.empleadoId,
          benefitId: beneficio.idBeneficio,
          pensionType: null,
          dependentsCount: null
        };

        const response = await axios.post(API_ENDPOINTS.ELEGIR_BENEFICIO, payload);

        if (response.data?.success) {
          beneficio.elegido = true;
          this.showMessage(`✅ Has elegido el beneficio: ${beneficio.nombre}`, "success");

          // REFRESCAR la tabla de Beneficios Seleccionados
          if (response.data?.data) {
            this.selectedBenefits.unshift(response.data.data);
          } else {
            await this.loadSelectedBenefits();
          }
        } else {
          this.showMessage("No se pudo registrar tu selección", "error");
        }

      } catch (error) {
        console.error("Error eligiendo beneficio:", error);
        this.showMessage("Error al seleccionar el beneficio", "error");
      } finally {
        this.elegiendo = false;
      }
    },

    // Nuevos métodos: carga y manejo de "Beneficios Seleccionados"
    async loadSelectedBenefits() {
      if (!this.empleadoId) {
        this.selectedBenefits = [];
        return;
      }

      this.loadingSelectedBenefits = true;
      try {
        // Asegúrate de tener configurado en API_ENDPOINTS el endpoint:
        // EMPLOYEE_BENEFIT_SELECTED(employeeId) => /api/EmployeeBenefit/{employeeId}/selected
        const resp = await axios.get(API_ENDPOINTS.EMPLOYEE_BENEFIT_SELECTED(this.empleadoId));
        this.selectedBenefits = resp.data?.success ? resp.data.data : [];
      } catch (err) {
        console.error("loadSelectedBenefits error", err);
        this.selectedBenefits = [];
      } finally {
        this.loadingSelectedBenefits = false;
      }
    },

    // Método público para que un padre o ref lo llame cuando quiera forzar refresco
    async refreshSelectedBenefits() {
      await this.loadSelectedBenefits();
    },

    // Estilos y formato
    getTypeClass(tipo) {
      const classes = {
        "Monto Fijo": "monto-fijo",
        "Porcentual": "porcentual",
        "API": "api"
      };
      return classes[tipo] || "default";
    },

    getAssumeClass(quienAsume) {
      const classes = {
        "Empresa": "empresa",
        "Empleado": "empleado",
        "Compartido": "compartido"
      };
      return classes[quienAsume] || "default";
    },

    getEtiquetaClass(etiqueta) {
      const classes = {
        "Beneficio": "beneficio",
        "Deducción": "deduccion"
      };
      return classes[etiqueta] || "default";
    },

    getEtiquetaForSelected(item) {
      if (item.tipo) return item.tipo;

      // Intentar tomarla del listado general de beneficios por beneficioId
      const id = item.benefitId ?? item.idBeneficio ?? null;
      if (!id || !Array.isArray(this.beneficios)) return null;

      const found = this.beneficios.find(b => b.idBeneficio === id || b.benefitId === id);
      return found ? (found.etiqueta || null) : null;
    },

    getValueClass(etiqueta) {
      return etiqueta === "Deducción" ? "valor-negativo" : "valor-positivo";
    },

    formatValor(item) {
      // Normalizar tipo y valor desde ambos formatos posibles
      const tipo = (item.tipo || item.benefitType || "").toString();
      const rawValor = item.valor ?? item.benefitValue ?? null;

      // Detectar si es porcentual (comparación tolerante)
      const tipoNorm = tipo.trim().toLowerCase();
      if (tipoNorm === "porcentual" || tipoNorm === "porcentaje") {
        // Aceptar tanto number como string convertible a number
        const n = Number(rawValor);
        return Number.isFinite(n) ? `${n}%` : (rawValor ? `${rawValor}%` : "—");
      }

      // Si el tipo indica API y no hay valor numérico, retornar indicador API
      const isApiType = tipoNorm === "api" || tipoNorm.includes("api");
      if (isApiType && (rawValor === null || rawValor === undefined)) {
        return "API";
      }

      // Determinar moneda: preferir campos explícitos
      const currencyFromItem = (item.currency || item.moneda || item.currencyCode || "").toString().trim();
      const currencyCode = currencyFromItem || "CRC"; // fallback a CRC

      // Formatear número si existe
      const valorNum = rawValor == null ? null : Number(rawValor);
      if (!Number.isFinite(valorNum)) {
        // Si no es numérico pero existe, mostrar tal cual
        return rawValor != null ? String(rawValor) : (isApiType ? "API" : "—");
      }

      // Usar Intl.NumberFormat para formateo por moneda
      try {
        return new Intl.NumberFormat("es-CR", {
          style: "currency",
          currency: currencyCode,
          maximumFractionDigits: 2
        }).format(valorNum);
      } catch (e) {
        // Si currencyCode no es válido para Intl, formatear con separador de miles y prefijo
        return `${currencyCode} ${valorNum.toLocaleString()}`;
      }
    },

    // formateo reutilizable para moneda (usado por la tabla de seleccionados)
    formatCurrency(v) {
      if (v == null) return "—";
      return new Intl.NumberFormat("es-CR", { style: "currency", currency: "CRC" }).format(v);
    },

    volverAtras() {
      this.$router.go(-1);
    },

    logout() {
      localStorage.removeItem("userData");
      localStorage.removeItem("selectedCompany");
      this.$router.push("/");
    },

    showMessage(msg, type) {
      this.message = msg;
      this.messageType = type;
      setTimeout(() => {
        this.message = "";
      }, 5000);
    }
  }
};
</script>


<style scoped>
/* Estilos del header (style de DatosPersonales.vue) */
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

.brand {
  display: flex;
  align-items: center;
  gap: 18px;
}

.logo-box {
  width: 50px;
  height: 50px;
  background: linear-gradient(180deg, #51a3a0, hsl(178, 77%, 86%));
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

.texts h1 {
  margin: 0;
  font-size: 24px;
}

.texts p {
  margin: 6px 0 0;
  color: #bdbdbd;
  font-size: 14px;
}

.nav-list {
  list-style: none;
  display: flex;
  gap: 2rem;
  margin: 0;
  padding: 0;
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

.company-info a {
  color: #bdbdbd;
  text-decoration: none;
  padding: 0.5rem 1rem;
  border-radius: 4px;
  transition: all 0.3s;
  font-size: 14px;
}

/* Estilos específicos de la página de beneficios */
.hero {
  display: flex;
  align-items: flex-start;
  justify-content: center;
  color: whitesmoke;
  padding: 48px 64px;
  flex: 1 0 auto;
}

.beneficios-container {
  width: 100%;
  max-width: 1400px;
}

.page-header {
  margin-bottom: 30px;
}

.header-actions {
  display: flex;
  align-items: center;
  gap: 20px;
  margin-bottom: 15px;
}

.btn-volver {
  background: #6c757d;
  color: white;
  border: none;
  padding: 10px 20px;
  border-radius: 6px;
  cursor: pointer;
  font-weight: 600;
  transition: background 0.3s;
}

.btn-volver:hover {
  background: #5a6268;
}

.page-header h2 {
  font-size: 2rem;
  margin: 0;
  color: #eee;
}

.subtitle {
  color: #bdbdbd;
  margin-bottom: 15px;
  font-size: 1.1rem;
}

.company-badge {
  background: rgba(31, 185, 180, 0.2);
  color: #1fb9b4;
  padding: 8px 16px;
  border-radius: 20px;
  display: inline-block;
  font-size: 14px;
}

/* TABLA de beneficios */
.beneficios-table-container {
  background: rgb(71, 69, 69);
  border-radius: 10px;
  border: 1px solid rgba(255, 255, 255, 0.15);
  overflow: hidden;
}

.table-wrapper {
  overflow-x: auto;
}

/* fila de título que ocupa todas las columnas */
.title-row .table-title {
  text-align: left;
  font-weight: 700;
  font-size: 1rem;
  padding: 10px 12px;
  background: #6b7280;
  border-bottom: 1px solid #e6e9ef;
}

/* cuando la tabla está vacía, centrar el mensaje */
.sb-table-empty .empty-cell {
  text-align: center;
  padding: 16px;
  color: #FFFFFF;
}

/* pequeñas mejoras visuales por si las quieres */
.sb-table thead tr + tr th,
.beneficios-table thead tr + tr th {
  background: rgba(0, 0, 0, 0.25);
  font-weight: 600;
  padding: 8px 10px;
}


.beneficios-table {
  width: 100%;
  border-collapse: collapse;
  background: rgba(0, 0, 0, 0.25);
}

.beneficios-table th {
  background: rgba(31, 185, 180, 0.2);
  color: #1fb9b4;
  font-weight: 600;
  padding: 15px 12px;
  text-align: left;
  border-bottom: 2px solid rgba(255, 255, 255, 0.1);
  white-space: nowrap;
}

.beneficios-table td {
  padding: 15px 12px;
  border-bottom: 1px solid rgba(255, 255, 255, 0.1);
  vertical-align: middle;
}

.beneficios-table tbody tr:hover {
  background: rgba(255, 255, 255, 0.05);
}

.beneficios-table tbody tr.selected {
  background: rgba(40, 167, 69, 0.1);
  border-left: 3px solid #28a745;
}

/* Celdas específicas */
.nombre-cell {
  min-width: 200px;
}

.nombre-cell strong {
  color: #eee;
  font-size: 1rem;
  display: block;
  margin-bottom: 5px;
}

.descripcion {
  color: #bdbdbd;
  font-size: 0.85rem;
  line-height: 1.3;
}

.accion-cell {
  text-align: center;
  min-width: 120px;
}

/* Badges */
.type-badge,
.assume-badge,
.etiqueta-badge {
  padding: 6px 12px;
  border-radius: 15px;
  font-size: 0.8rem;
  font-weight: 600;
  text-transform: uppercase;
  display: inline-block;
  text-align: center;
  min-width: 80px;
}

.type-badge.monto-fijo {
  background: rgba(40, 167, 69, 0.2);
  color: #28a745;
  border: 1px solid rgba(40, 167, 69, 0.3);
}

.type-badge.porcentual {
  background: rgba(23, 162, 184, 0.2);
  color: #17a2b8;
  border: 1px solid rgba(23, 162, 184, 0.3);
}

.type-badge.api {
  background: rgba(102, 16, 242, 0.2);
  color: #6610f2;
  border: 1px solid rgba(102, 16, 242, 0.3);
}

.assume-badge.empresa {
  background: rgba(40, 167, 69, 0.2);
  color: #28a745;
  border: 1px solid rgba(40, 167, 69, 0.3);
}

.assume-badge.empleado {
  background: rgba(220, 53, 69, 0.2);
  color: #dc3545;
  border: 1px solid rgba(220, 53, 69, 0.3);
}

.assume-badge.compartido {
  background: rgba(255, 193, 7, 0.2);
  color: #ffc107;
  border: 1px solid rgba(255, 193, 7, 0.3);
}

.etiqueta-badge.beneficio {
  background: rgba(40, 167, 69, 0.2);
  color: #28a745;
  border: 1px solid rgba(40, 167, 69, 0.3);
}

.etiqueta-badge.deduccion {
  background: rgba(220, 53, 69, 0.2);
  color: #dc3545;
  border: 1px solid rgba(220, 53, 69, 0.3);
}

/* Valores */
.valor {
  font-weight: 600;
  font-size: 1rem;
}

.valor-positivo {
  color: #28a745;
}

.valor-negativo {
  color: #dc3545;
}

/* Botones de acción */
.btn-elegir {
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

.btn-elegir:hover:not(:disabled) {
  background: #218838;
  transform: translateY(-1px);
}

.btn-elegir:disabled {
  background: #6c757d;
  cursor: not-allowed;
  transform: none;
}

.btn-elegido {
  background: #6c757d;
  color: #bdbdbd;
  border: none;
  padding: 8px 16px;
  border-radius: 6px;
  font-size: 0.9rem;
  font-weight: 600;
  cursor: not-allowed;
  min-width: 80px;
}

/* Footer de la tabla */
.table-footer {
  padding: 15px 20px;
  background: rgba(0, 0, 0, 0.3);
  border-top: 1px solid rgba(255, 255, 255, 0.1);
  text-align: right;
}

.beneficios-count {
  color: #bdbdbd;
  font-size: 0.9rem;
  font-weight: 600;
}

/* Estados */
.loading {
  text-align: center;
  padding: 40px;
  color: #1fb9b4;
  font-size: 1.1rem;
}

.empty-state {
  text-align: center;
  padding: 60px 20px;
}

.empty-icon {
  font-size: 4rem;
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

.btn-primary {
  background: #1fb9b4;
  color: white;
  border: none;
  padding: 10px 20px;
  border-radius: 6px;
  cursor: pointer;
  font-weight: 600;
  transition: background 0.3s;
}

.btn-primary:hover {
  background: #1aa19c;
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

.message.info {
  background: rgba(23, 162, 184, 0.2);
  color: #17a2b8;
  border: 1px solid #17a2b8;
}

/* Footer */
footer {
  background: #fff;
  padding: 28px 64px;
  border-top: 1px solid #eee;
  color: #8b8b8b;
  display: flex;
  align-items: center;
  justify-content: space-between;
}

/* Responsive */
@media (max-width: 768px) {
  .header {
    padding: 16px 20px;
  }

  .nav {
    flex-direction: column;
    gap: 20px;
  }

  .nav-list {
    flex-wrap: wrap;
    justify-content: center;
    gap: 1rem;
  }

  .hero {
    padding: 30px 20px;
  }

  .header-actions {
    flex-direction: column;
    align-items: flex-start;
    gap: 10px;
  }

  .beneficios-table {
    font-size: 0.9rem;
  }

  .beneficios-table th,
  .beneficios-table td {
    padding: 10px 8px;
  }

  .type-badge,
  .assume-badge,
  .etiqueta-badge {
    font-size: 0.7rem;
    padding: 4px 8px;
    min-width: 60px;
  }

  footer {
    flex-direction: column;
    gap: 10px;
    text-align: center;
    padding: 20px;
  }
}
</style>