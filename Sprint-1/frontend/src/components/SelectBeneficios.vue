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

        
        <div v-if="loading" class="loading">
          Cargando beneficios
        </div>

        <!-- Tabla de beneficios -->
        <div v-else class="beneficios-table-container">
          <div v-if="beneficios.length > 0" class="table-wrapper">
            <table class="beneficios-table">
              <thead>
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
                <tr v-for="beneficio in beneficios" :key="beneficio.idBeneficio" 
                    :class="{ 'selected': beneficio.elegido }">
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
                      v-if="!beneficio.elegido"
                      @click="elegirBeneficio(beneficio)"
                      class="btn-elegir"
                      :disabled="elegiendo"
                    >
                      Elegir
                    </button>
                    <button 
                      v-else
                      disabled
                      class="btn-elegido"
                    >
                      Seleccionado
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

        <!-- Mensajes -->
        <div v-if="message" class="message" :class="{ 'error': messageType === 'error', 'success': messageType === 'success' }">
          {{ message }}
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
      messageType: "success"
    };
  },
  mounted() {
    this.loadUserData();
    this.loadSelectedCompany();
    this.loadBeneficios();
  },
  methods: {
    loadUserData() {
      const stored = localStorage.getItem("userData");
      if (stored) {
        const userData = JSON.parse(stored);
        this.userName = `${userData.firstName || ""} ${userData.secondName || ""}`.trim() || "Usuario";
        this.userRole = userData.personType || "";
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

    async loadBeneficios() {
      if (!this.selectedCompany) return;

      this.loading = true;
      try {
        console.log("Cargando beneficios para empresa:", this.selectedCompany.cedulaJuridica);
        
        const response = await axios.get(
          `http://localhost:5081/api/Beneficio/por-empresa/${this.selectedCompany.cedulaJuridica}`
        );

        console.log("Respuesta beneficios:", response.data);

        let beneficiosData = [];
        if (response.data && response.data.success && Array.isArray(response.data.beneficios)) {
          // Agregar propiedad 'elegido' a cada beneficio
          beneficiosData = response.data.beneficios.map(beneficio => ({
            ...beneficio,
            elegido: false // Por ahora siempre false, luego se cargará desde BD
          }));
        }

        this.beneficios = beneficiosData;
        
        if (beneficiosData.length === 0) {
          this.showMessage("No se encontraron beneficios para tu empresa", "info");
        } else {
          this.showMessage(`Se cargaron ${beneficiosData.length} beneficio(s) disponibles`, "success");
        }

      } catch (error) {
        console.error("Error cargando beneficios:", error);
        let errorMessage = "Error al cargar beneficios";
        if (error.response?.status === 404) {
          errorMessage = "No se encontraron beneficios para esta empresa";
        } else if (error.response?.data?.message) {
          errorMessage = error.response.data.message;
        }
        this.showMessage(errorMessage, "error");
        this.beneficios = [];
      } finally {
        this.loading = false;
      }
    },

    async elegirBeneficio(beneficio) {
      this.elegiendo = true;
      try {
        console.log("Eligiendo beneficio:", beneficio);
        
        // TODO: Implementar llamada al backend para guardar en BeneficioDeEmpleado
        // const response = await axios.post(`http://localhost:5081/api/BeneficioEmpleado/elegir`, {
        //   empleadoId: this.getEmpleadoId(),
        //   beneficioId: beneficio.idBeneficio,
        //   empresaId: this.selectedCompany.cedulaJuridica
        // });
        
        // Simulación de éxito (eliminar al conectar con BD)
        await new Promise(resolve => setTimeout(resolve, 500));
        
        // Marcar como elegido localmente
        beneficio.elegido = true;
        
        this.showMessage(`✅ Has elegido el beneficio: ${beneficio.nombre}`, "success");
        
      } catch (error) {
        console.error("Error eligiendo beneficio:", error);
        this.showMessage("Error al seleccionar el beneficio", "error");
      } finally {
        this.elegiendo = false;
      }
    },

    // Método auxiliar para obtener ID del empleado (necesitarás implementarlo)
    getEmpleadoId() {
      const userData = JSON.parse(localStorage.getItem("userData") || "{}");
      return userData.id || userData.PK_User;
    },

    // Formato Lista
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

    getValueClass(etiqueta) {
      return etiqueta === "Deducción" ? "valor-negativo" : "valor-positivo";
    },

    formatValor(beneficio) {
      if (beneficio.tipo === "Porcentual") {
        return `${beneficio.valor}%`;
      } else if (beneficio.tipo === "Monto Fijo") {
        return `₡${beneficio.valor?.toLocaleString() || "0"}`;
      } else {
        return beneficio.valor ? `₡${beneficio.valor.toLocaleString()}` : "API";
      }
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