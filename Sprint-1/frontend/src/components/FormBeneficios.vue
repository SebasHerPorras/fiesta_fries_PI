<template>
  <div class="wrap">
    <main class="hero">
      <div class="brand">
        <div class="logo-box">
          <span class="f">F</span>
        </div>
        <div class="texts">
          <h1>Fiesta Fries</h1>
          <p>Gestor de Planillas</p>
        </div>
      </div>

      <form id="EmployerLogIn" @submit.prevent="submitForm" novalidate>
        <h2 style="color: #eee; margin: 0 0 20px; font-weight: 600; font-size: 18px; text-align: center;">
          {{ modoEdicion ? 'Editar Beneficio' : 'Registrar Beneficio' }}
        </h2>

        <div class="field-group">
          <label class="input">
            <input
              type="text"
              v-model="formData.nombre"
              placeholder=" Nombre del beneficio*"
              required
              maxlength="100"
              @input="validateNombre"
            />
          </label>
          <div class="error-msg">{{ errors.nombre }}</div>
        </div>

        <div class="field-group">
          <label class="input">
            <select v-model="formData.tipo"
              :disabled = "modoEdicion"
              :required="!modoEdicion"
              @change="validateTipo">
              <option value="" disabled selected>Tipo de Beneficio</option>
              <option value="Monto Fijo">Monto Fijo</option>
              <option value="Porcentual">Porcentual</option>
              <option value="API">API</option>
            </select>
          </label>
          <div class="error-msg">{{ errors.tipo }}</div>
        </div>

        <div class="field-group">
          <label class="input">
            <select v-model="formData.quienAsume"
              :disabled = "modoEdicion"
              :required="!modoEdicion"
              @change="validateQuienAsume">
              <option value="" disabled selected>¿Quién asume el costo? *</option>
              <option value="Empresa">Empresa</option>
              <option value="Empleado">Empleado</option>
              <option value="Ambos">Ambos</option>
            </select>
          </label>
          <div class="error-msg">{{ errors.quienAsume }}</div>
        </div>

        <div class="field-group">
          <label class="input">
            <input
              type="number"
              v-model="formData.valor"
              :readonly="modoEdicion"
              placeholder=" Valor*"
              required
              min="0"
              step="0.01"
              @input="validateValor"
            />
          </label>
          <div class="error-msg">{{ errors.valor }}</div>
        </div>

        <div class="field-group">
          <label class="input">
            <select v-model="formData.etiqueta"
              :disabled = "modoEdicion"
              :required="!modoEdicion"
              @change="validateEtiqueta">
              <option value="" disabled selected>Etiqueta *</option>
              <option value="Beneficio">Beneficio</option>
              <option value="Deducción">Deducción</option>
            </select>
          </label>
          <div class="error-msg">{{ errors.etiqueta }}</div>
        </div>

        <div class="buttons-row">
          <button class="btn btn-secondary" type="button" @click="volverAtras">← Volver</button>
          <button class="btn btn-primary" type="submit" :disabled="loading">
            <span v-if="loading"> Guardando...</span>
            <span v-else>{{ modoEdicion ? 'Actualizar' : 'Registrar' }}</span>
          </button>
        </div>

        <div class="field-group">
          <div class="message" v-if="successMessage" :class="{ error: messageType === 'error', success: messageType === 'success' }">
            {{ successMessage }}
          </div>
        </div>
      </form>
    </main>

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

export default {
  name: 'FormBeneficios',
  data() {
    return {
      formData: {
        nombre: '',
        tipo: '',
        quienAsume: '',
        valor: '',
        etiqueta: ''
      },
      errors: {
        nombre: '',
        tipo: '',
        quienAsume: '',
        valor: '',
        etiqueta: ''
      },
      successMessage: '',
      messageType: 'success',
      selectedCompany: null,
      userId: '',
      loading: false,
      modoEdicion: false,
      beneficioId: null
    }
  },

  mounted() {
    this.obtenerUserId();
    this.obtenerEmpresaSeleccionada();

    const id = this.$route.params.id;
    if (id) {
      this.modoEdicion = true;
      this.beneficioId = id;
      this.cargarBeneficioExistente(id);
    }
  },

  methods: {
    obtenerUserId() {
      const userData = localStorage.getItem('userData');
      if (userData) {
        try {
          const user = JSON.parse(userData);
          this.userId = user.id;
          console.log('UserId obtenido:', this.userId);
        } catch (error) {
          console.error('Error al parsear userData:', error);
        }
      }
      
      if (!this.userId) {
        const sessionUser = sessionStorage.getItem('userData');
        if (sessionUser) {
          try {
            const user = JSON.parse(sessionUser);
            this.userId = user.id;
            console.log('UserId obtenido de sessionStorage:', this.userId);
          } catch (error) {
            console.error('Error al parsear session userData:', error);
          }
        }
      }
      
      if (!this.userId) {
        console.error('No se pudo obtener el userId. El usuario debe estar logueado.');
        this.successMessage = 'Error: Usuario no autenticado. Por favor, inicie sesión.';
        this.messageType = 'error';
      }
    },

    obtenerEmpresaSeleccionada() {
      const empresaData = localStorage.getItem('selectedCompany');
      if (empresaData) {
        try {
          this.selectedCompany = JSON.parse(empresaData);
          console.log('Empresa seleccionada:', this.selectedCompany);
        } catch (error) {
          console.error('Error al parsear empresa seleccionada:', error);
          this.mostrarError('Error al obtener empresa seleccionada');
        }
      } else {
        this.mostrarError('No hay empresa seleccionada. Volviendo a Inicio...');
        setTimeout(() => {
          this.$router.go(-1);
        }, 2000);
      }
    },

    async cargarBeneficioExistente(id) {
      try {
        const response = await axios.get(API_ENDPOINTS.GET_BENEFICIO(id));
        const beneficio = response.data?.beneficio;

        this.formData = {
          nombre: beneficio.nombre,
          tipo: beneficio.tipo,
          quienAsume: beneficio.quienAsume,
          valor: beneficio.valor,
          etiqueta: beneficio.etiqueta
        };
        console.log('Formulario cargado con datos existentes:', this.formData);
      } catch (error) {
        console.error('Error al cargar beneficio existente:', error);
        this.mostrarError('No se pudo cargar el beneficio para editar.');
      }
    },

    
    validateNombre() {
      const nombre = this.formData.nombre.trim();
      
      if (!nombre) {
        this.errors.nombre = 'El nombre del beneficio es obligatorio.';
      } else if (nombre.length > 100) {
        this.errors.nombre = 'El nombre no debe exceder 100 caracteres.';
      } else {
        this.errors.nombre = '';
      }
    },
    
    validateTipo() {
      this.errors.tipo = this.formData.tipo ? '' : 'Seleccione un tipo de cálculo.';
    },
    
    validateQuienAsume() {
      this.errors.quienAsume = this.formData.quienAsume ? '' : 'Seleccione quién asume el costo.';
    },
    
    validateValor() {
      const valor = this.formData.valor;
      
      if (valor === '' || valor === null || valor === undefined) {
        this.errors.valor = 'El valor es obligatorio.';
      } else {
        const numValor = Number(valor);
        
        if (isNaN(numValor)) {
          this.errors.valor = 'Debe ser un número válido.';
        } else if (numValor < 0) {
          this.errors.valor = 'No puede ser negativo.';
        } else {
          this.errors.valor = '';
        }
      }
    },
    
    validateEtiqueta() {
      this.errors.etiqueta = this.formData.etiqueta ? '' : 'Seleccione una etiqueta.';
    },
    
    async submitForm() {
      this.errors = {};

      console.log('Datos del formulario capturados:');
      console.log('Nombre:', this.formData.nombre);
      console.log('Tipo:', this.formData.tipo);
      console.log('Quien Asume:', this.formData.quienAsume);
      console.log('Valor:', this.formData.valor);
      console.log('Etiqueta:', this.formData.etiqueta);
      console.log('Empresa Seleccionada:', this.selectedCompany);
      console.log('UserId:', this.userId);
      
      this.validateNombre();
      this.validateTipo();
      this.validateQuienAsume();
      this.validateValor();
      this.validateEtiqueta();
      
      const hasErrors = Object.values(this.errors).some(error => error !== '');
      
      const camposObligatorios = [
        this.formData.nombre,
        this.formData.tipo,
        this.formData.quienAsume,
        this.formData.valor,
        this.formData.etiqueta
      ];

      const faltanCamposObligatorios = camposObligatorios.some(campo => 
        campo !== 0 && !campo  
      );

      if (hasErrors || faltanCamposObligatorios) {
        console.log('SE DETIENE POR VALIDACIONES');
        this.successMessage = 'Corrija los errores antes de continuar.';
        this.messageType = 'error';
        return;
      }
      
      if (!this.selectedCompany || !this.selectedCompany.cedulaJuridica) {
        this.successMessage = 'Error: No hay empresa seleccionada.';
        this.messageType = 'error';
        return;
      }

      if (!this.userId) {
        this.successMessage = 'Error: Usuario no autenticado.';
        this.messageType = 'error';
        return;
      }
      
      const beneficioData = {
        cedulaJuridica: this.selectedCompany.cedulaJuridica,
        nombre: this.formData.nombre.trim(),
        tipo: this.formData.tipo,
        quienAsume: this.formData.quienAsume,
        valor: Number(this.formData.valor),
        etiqueta: this.formData.etiqueta
      };

      console.log('=== VERIFICACION DE TIPOS ===');
      console.log('Tipo de cedulaJuridica:', typeof beneficioData.cedulaJuridica);
      console.log('Valor de cedulaJuridica:', beneficioData.cedulaJuridica);
      console.log('Tipo de valor:', typeof beneficioData.valor);
      console.log('Valor de valor:', beneficioData.valor);
      console.log('JSON completo beneficio:', JSON.stringify(beneficioData));
      console.log('================================');
      
      if (this.loading) return; 
      this.loading = true;

      try {
        await this.guardarBeneficioEnBackend(beneficioData);
        
        this.successMessage = 'Beneficio registrado correctamente.';
        this.messageType = 'success';
        
        // Redirigir después de 2s
        setTimeout(() => {
          this.$router.push({ name: "PageEmpresaAdmin" });
        }, 2000);
        
      } catch (error) {
        console.error('Error al guardar beneficio:', error);
        this.successMessage = 'Error: ' + error.message;
        this.messageType = 'error';
      } finally {
        this.loading = false; 
      }
    },
        
    async guardarBeneficioEnBackend(beneficioData) {
      try {
        const requestData = {
          UserId: this.userId,
          Beneficio: beneficioData
        };

        let response;
        if (this.modoEdicion && this.beneficioId) {
          response = await axios.put(API_ENDPOINTS.UPDATE_BENEFICIO(this.beneficioId), requestData, {
            headers: { "Content-Type": "application/json" }
          });
        } else {
          response = await axios.post(API_ENDPOINTS.CREATE_BENEFICIO, requestData, {
            headers: { "Content-Type": "application/json" }
          });
        }

        return response.data;
      } catch (error) {
        console.error('Error en guardarBeneficioEnBackend:', error);
        const serverMessage = error.response?.data?.message || error.message || 'Error de conexión';
        throw new Error(serverMessage);
      }
    },
    
    volverAtras() {
      this.$router.go(-1);
    },
    
    mostrarError(mensaje) {
      this.successMessage = mensaje;
      this.messageType = 'error';
    },
        
    resetForm() {
      this.formData = {
        nombre: '',
        tipo: '',
        quienAsume: '',
        valor: '',
        etiqueta: ''
      };
      
      for (const key in this.errors) {
        this.errors[key] = '';
      }
    }
  }
}
</script>

<style

  src="@/assets/style/RegisterEmpleado.css">

.register-card h2 {
  font-size: 22px;
  font-weight: 600;
  margin-bottom: 24px;
  text-align: center;
}

.field-group label {
  display: block;
  margin-bottom: 6px;
  font-size: 14px;
  color: #ccc;
}

.readonly-field input,
.readonly-field select {
  background-color: rgba(255, 255, 255, 0.05);
  color: #aaa;
  cursor: not-allowed;
  border: 1px solid #555;
}


</style>