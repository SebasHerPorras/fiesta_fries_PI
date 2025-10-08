<template>
  <div class="formulario-container">
    <div class="container">
      <div class="header">
        <div class="logo">
          <span class="logo-text">F</span>
        </div>
        <div class="brand-text">
          <h1>Fiesta Fries</h1>
          <p>Crear Nuevo Beneficio</p>
        </div>
      </div>
      
      <div class="form-card">
        <h2 class="form-title">Crea Nuevo Beneficio</h2>
        
        <form id="benefitForm" @submit.prevent="submitForm">
          <div class="form-group">
            <input 
              type="text" 
              id="name" 
              v-model="formData.nombre"
              class="form-control" 
              placeholder="Nombre del beneficio *"
              required
              maxlength="100"
              @input="validateNombre"
            >
            <div class="error" id="nombreError">{{ errors.nombre }}</div>
          </div>
          
          <div class="form-group">
            <select 
              id="tipo" 
              v-model="formData.tipo"
              class="form-control" 
              required
              @change="validateTipo"
            >
              <option value="" disabled selected>Tipo de cálculo *</option>
              <option value="Monto Fijo">Monto Fijo</option>
              <option value="Porcentual">Porcentual</option>
              <option value="API">API</option>
            </select>
            <div class="error" id="tipoError">{{ errors.tipo }}</div>
          </div>
          
          <div class="form-group">
            <select 
              id="quienAsume" 
              v-model="formData.quienAsume"
              class="form-control" 
              required
              @change="validateQuienAsume"
            >
              <option value="" disabled selected>¿Quién asume el costo? *</option>
              <option value="Empresa">Empresa</option>
              <option value="Empleado">Empleado</option>
              <option value="Ambos">Ambos</option>
            </select>
            <div class="error" id="quienAsumeError">{{ errors.quienAsume }}</div>
          </div>
          
          <div class="form-group">
            <input 
              type="number" 
              id="valor" 
              v-model="formData.valor"
              class="form-control" 
              placeholder="Valor *"
              required
              min="0"
              step="0.01"
              @input="validateValor"
            >
            <div class="error" id="valorError">{{ errors.valor }}</div>
          </div>
          
          <div class="form-group">
            <select 
              id="etiqueta" 
              v-model="formData.etiqueta"
              class="form-control" 
              required
              @change="validateEtiqueta"
            >
              <option value="" disabled selected>Etiqueta *</option>
              <option value="Beneficio">Beneficio</option>
              <option value="Deducción">Deducción</option>
            </select>
            <div class="error" id="etiquetaError">{{ errors.etiqueta }}</div>
          </div>
          
          <div class="form-actions">
            <button 
              type="button" 
              class="btn-secondary"
              @click="volverAHome"
            >
              ← Volver al Inicio
            </button>
            <button 
              type="submit" 
              class="btn-primary"
              :disabled="loading"
            >
              <span v-if="loading">⏳ Registrando...</span>
              <span v-else>Registrar Beneficio</span>
            </button>
          </div>
          
          <div class="success" id="successMessage" :class="{ error: messageType === 'error' }">{{ successMessage }}</div>
        </form>
      </div>
    </div>
    
    <div class="footer">
      <div>©2025 Fiesta Fries</div>
    </div>
  </div>
</template>

<script>
import axios from "axios";

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
      loading: false 
    }
  },

  mounted() {
    this.obtenerUserId();
    this.obtenerEmpresaSeleccionada();
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

      console.log('=== DEBUG DETALLADO DE CAMPOS OBLIGATORIOS ===');
      console.log('nombre:', this.formData.nombre, '¿Vacío?:', !this.formData.nombre);
      console.log('tipo:', this.formData.tipo, '¿Vacío?:', !this.formData.tipo);
      console.log('quienAsume:', this.formData.quienAsume, '¿Vacío?:', !this.formData.quienAsume);
      console.log('valor:', this.formData.valor, '¿Vacío?:', !this.formData.valor);
      console.log('etiqueta:', this.formData.etiqueta, '¿Vacío?:', !this.formData.etiqueta);
      console.log('faltanCamposObligatorios:', faltanCamposObligatorios);
      console.log('===============================================');

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
        
        // Solo limpiamos el formulario después del éxito, sin redirigir
        this.resetForm();
        
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
        console.log('INICIANDO ENVIO AL BACKEND - BENEFICIO');
        console.log('Datos recibidos en guardarBeneficioEnBackend:', beneficioData);
        console.log('UserId a enviar:', this.userId);
        
        if (!this.userId) {
          throw new Error('Usuario no autenticado. Por favor, inicie sesión.');
        }
        
        // Crear el objeto request según la estructura del backend
        const requestData = {
          UserId: this.userId,
          Beneficio: beneficioData
        };
        
        console.log('Datos completos a enviar:', requestData);
        console.log('JSON completo request:', JSON.stringify(requestData));
        
        const response = await axios.post(
          "https://localhost:7056/api/Beneficio", 
          requestData,  
          {
            headers: { "Content-Type": "application/json" }
          }
        );

        console.log('Status de respuesta:', response.status);
        console.log('Respuesta del servidor:', response.data);
        
        return response.data;
        
      } catch (error) {
        console.error('Error en guardarBeneficioEnBackend:', error);
        if (error.response && error.response.data) {
          const serverMessage = error.response.data.message || error.response.data;
          throw new Error(serverMessage);
        } else {
          throw new Error('Error de conexión con el servidor');
        }
      }
    },
    
    volverAHome() {
      this.$router.push('/Profile');
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

<style scoped>
/* Los estilos se mantienen igual */
* {
  margin: 0;
  padding: 0;
  box-sizing: border-box;
  font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
}

.formulario-container {
  background-color: #1e1e1e;
  color: whitesmoke;
  min-height: 100vh;
  display: flex;
  flex-direction: column;
  padding: 20px;
}

.container {
  max-width: 500px;
  margin: 0 auto;
  padding: 20px;
  width: 100%;
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

.form-card {
  background: rgb(71,69,69);
  border: 1px solid rgba(255,255,255,0.12);
  padding: 25px;
  border-radius: 10px;
  box-shadow: 0 6px 18px rgba(0,0,0,0.35);
}

.form-title {
  font-size: 22px;
  margin-bottom: 20px;
  color: #eee;
  text-align: center;
}

.form-group {
  margin-bottom: 20px;
}

.form-group label {
  display: block;
  margin-bottom: 8px;
  font-size: 14px;
  color: #ddd;
}

.form-control {
  width: 100%;
  padding: 12px 15px;
  border-radius: 6px;
  background: rgba(0,0,0,0.25);
  border: 1px solid rgba(255,255,255,0.06);
  color: whitesmoke;
  font-size: 14px;
  transition: border-color 0.3s;
}

select.form-control {
  color: #bdbdbd;
}

select.form-control:valid,
select.form-control:focus {
  color: whitesmoke;
}

select option {
  color: whitesmoke;
  background: rgb(71,69,69);
}

select option[disabled] {
  color: #bdbdbd;
}

.form-control:focus {
  outline: none;
  border-color: #1fb9b4;
  box-shadow: 0 0 0 2px rgba(31, 185, 180, 0.2);
}

.form-control::placeholder {
  color: #bdbdbd;
}

.form-actions {
  display: flex;
  gap: 10px;
  margin-top: 10px;
}

.btn-primary, .btn-secondary {
  flex: 1;
  padding: 12px 20px;
  border-radius: 6px;
  font-weight: 600;
  cursor: pointer;
  font-size: 16px;
  transition: background 0.3s, transform 0.1s;
  border: none;
}

.btn-primary {
  background: #1fb9b4;
  color: white;
}

.btn-primary:hover {
  background: #1aa8a4;
}

.btn-primary:active {
  transform: scale(0.98);
}

.btn-primary:disabled {
  background: #6c757d;
  cursor: not-allowed;
  transform: none;
}

.btn-secondary {
  background: #6c757d;
  color: white;
}

.btn-secondary:hover {
  background: #5a6268;
}

.btn-secondary:active {
  transform: scale(0.98);
}

.error {
  color: #ff6b6b;
  font-size: 13px;
  margin-top: 5px;
  min-height: 18px;
}

.success {
  color: #9fe6cf;
  font-size: 13px;
  margin-top: 10px;
  text-align: center;
  min-height: 18px;
}

.success.error {
  color: #ff6b6b;
}

.footer {
  background: #2c2c2c;
  padding: 20px;
  text-align: center;
  margin-top: 40px;
  color: #8b8b8b;
  border-radius: 8px;
}

@media (max-width: 600px) {
  .container {
    padding: 15px;
  }
  
  .form-card {
    padding: 20px;
  }
  
  .header {
    flex-direction: column;
    text-align: center;
  }
  
  .logo {
    margin-right: 0;
    margin-bottom: 10px;
  }

  .form-actions {
    flex-direction: column;
  }
}
</style>