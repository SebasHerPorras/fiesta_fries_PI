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
    </div>

    <div class="content">
      <!-- Estado de carga -->
      <div v-if="loading" class="loading">
        ⏳ Cargando empresas...
      </div>

      <!-- Lista de empresas -->
      <div v-else-if="empresas.length > 0" class="empresas-list">
        <div class="table-container">
          <table class="empresas-table">
            <thead>
              <tr>
                <th>Cédula</th>
                <th>Nombre</th>
                <th>Empleados</th>
                <th>Frecuencia Pago</th>
                <!--<th>Acciones</th> -->
              </tr>
            </thead>
            <tbody>
              <tr v-for="empresa in empresas" :key="empresa.id" class="empresa-row">
                <td>{{ empresa.cedulaJuridica }}</td>
                <td>{{ empresa.nombre }}</td>
                <td>{{ empresa.cantidadEmpleados || 0 }}</td>
                <td>{{ formatFrecuenciaPago(empresa.frecuenciaPago) }}</td>
                <!--
                <td class="actions">
                  <button @click="viewDetails(empresa)" class="btn-info">
                    👁️ Ver
                  </button>
                  <button @click="editEmpresa(empresa)" class="btn-warning">
                    ✏️ Editar
                  </button>
                  
                </td>
                -->
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <!-- Estado vacío -->
      <div v-else class="empty-state">
        <div class="empty-icon">🏢</div>
        <h3>No tienes empresas registradas</h3>
        <p>Comienza agregando tu primera empresa</p>
        <button @click="navigateToCreate" class="btn-primary">
          ＋ Crear Primera Empresa
        </button>
      </div>
    </div>

    <!-- Mensajes de éxito/error -->
    <div class="message" :class="{ 'error': messageType === 'error', 'success': messageType === 'success' }">
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
      messageType: 'success'
    }
  },
  mounted() {
    this.loadEmpresas();
  },
  methods: {
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

    refreshList() {
      this.loadEmpresas();
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
  padding: 5px 10px;
  font-size: 12px;
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

.loading {
  text-align: center;
  padding: 40px;
  color: #1fb9b4;
  font-size: 18px;
}

.table-container {
  overflow-x: auto;
}

.empresas-table {
  width: 100%;
  border-collapse: collapse;
  background: rgba(0,0,0,0.25);
  border-radius: 8px;
  overflow: hidden;
}

.empresas-table th,
.empresas-table td {
  padding: 12px 15px;
  text-align: left;
  border-bottom: 1px solid rgba(255,255,255,0.1);
}

.empresas-table th {
  background: rgba(31, 185, 180, 0.2);
  font-weight: 600;
  color: #1fb9b4;
}

.empresa-row:hover {
  background: rgba(255,255,255,0.05);
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
  
  .empresas-table {
    font-size: 14px;
  }
  
  .actions {
    flex-direction: column;
  }
}
</style>