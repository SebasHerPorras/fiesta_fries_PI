<template>
  <div class="wrap">
    <!-- Sección principal -->
    <main class="hero">
      <!-- Logo + título -->
      <div class="brand">
        <!-- Logo de la aplicación -->
        <div class="logo-box">
          <span class="f">F</span>
        </div>
        <!-- Título y subtítulo -->
        <div class="texts">
          <h1>Fiesta Fries</h1>
          <p>Crear Nueva Empresa</p>
        </div>
      </div>

      <!-- Card del formulario -->
      <aside class="form-card">
        <h2>Crea Nueva Empresa</h2>
        
        <form @submit.prevent="submitForm">
          <div class="form-group">
            <label class="input">
              <input 
                type="text" 
                v-model="formData.cedulaJuridica"
                placeholder="🪪Cédula jurídica *"
                required
                maxlength="10" 
                @input="validateCedulaJuridica"
              >
            </label>
            <div class="error">{{ errors.cedulaJuridica }}</div>
          </div>
          
          <div class="form-group">
            <label class="input">
              <input 
                type="text" 
                v-model="formData.nombre"
                placeholder="🏢Nombre de la empresa *"
                required
                @input="validateNombre"
              >
            </label>
            <div class="error">{{ errors.nombre }}</div>
          </div>
          
          <div class="form-group">
            <label class="input">
              <input 
                type="text" 
                v-model="formData.direccionEspecifica"
                placeholder="🌍Dirección específica (opcional)"
                @input="validateDireccion"
              >
            </label>
            <div class="error">{{ errors.direccionEspecifica }}</div>
          </div>
          
          <div class="form-group">
            <label class="input">
              <input 
                type="tel" 
                v-model="formData.telefono"
                placeholder="📱Teléfono (opcional)"
                @input="validateTelefono"
              >
            </label>
            <div class="error">{{ errors.telefono }}</div>
          </div>
          
          <div class="form-group">
            <label class="input">
              <input 
                type="number" 
                v-model="formData.noMaxBeneficios"
                placeholder="🙏Número máximo de beneficios (mínimo 1) *"
                required
                min="0"
                @input="validateMaxBenefits"
              >
            </label>
            <div class="error">{{ errors.noMaxBeneficios }}</div>
          </div>
          
          <div class="form-group">
            <label class="input">
              <select 
                v-model="formData.frecuenciaPago"
                required
                @change="validateFrecuenciaPago"
              >
                <option value="" disabled selected>💵Frecuencia de Pago *</option>
                <option value="quincenal">Quincenal</option>
                <option value="mensual">Mensual</option>
              </select>
            </label>
            <div class="error">{{ errors.frecuenciaPago }}</div>
          </div>
          
          <div class="form-group">
            <label class="input">
              <input 
                type="number" 
                v-model="formData.diaPago"
                placeholder="📅Día de pago (1-30) *"
                required
                min="1"
                max="31"
                @input="validateDiaPago"
              >
            </label>
            <div class="error">{{ errors.diaPago }}</div>
          </div>
          
          <!-- BOTONES -->
          <div class="buttons-row">
            <button 
              type="button" 
              class="btn btn-secondary"
              @click="volverAlHome"
              :disabled="loading"
            >
              ← Volver
            </button>
            <button 
              class="btn btn-primary" 
              type="submit"
              :disabled="loading"
            >
              <span v-if="loading">⏳ Registrando...</span>
              <span v-else>Agregar Empresa</span>
            </button>
          </div>
          
          <div class="message" :class="{ error: messageType === 'error', success: messageType === 'success' }">
            {{ successMessage }}
          </div>
        </form>
      </aside>
    </main>

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
import { API_ENDPOINTS } from "../config/apiConfig";

export default {
  name: 'FormEmpresa',
  data() {
    return {
      formData: {
        cedulaJuridica: '',
        nombre: '',
        direccionEspecifica: '',
        telefono: '',
        noMaxBeneficios: '',
        frecuenciaPago: '',
        diaPago: '',
      },
      errors: {
        cedulaJuridica: '',
        nombre: '',
        direccionEspecifica: '',
        telefono: '',
        noMaxBeneficios: '',
        frecuenciaPago: '',
        diaPago: ''
      },
      successMessage: '',
      messageType: 'success',
      userId: '',
      loading: false 
    }
  },

  mounted() {
    this.obtenerUserId();
  },

  methods: {
    volverAlHome() {
      this.$router.go(-1);
    },

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
        console.error('No se pudo obtener el userId. El usuario debe estar logueado.');
        this.successMessage = 'Error: Usuario no autenticado. Por favor, inicie sesión.';
        this.messageType = 'error';
      }
    },

    validateCedulaJuridica() {
      const cedula = this.formData.cedulaJuridica.trim();
  
      if (!cedula) {
        this.errors.cedulaJuridica = 'La cédula jurídica es obligatoria.';
      } else if (!/^\d+$/.test(cedula)) {
        this.errors.cedulaJuridica = 'La cédula jurídica debe contener solo números.';
      } else if (cedula.length !== 10) {
        this.errors.cedulaJuridica = 'La cédula jurídica debe tener exactamente 10 dígitos.';
      } else {
        this.errors.cedulaJuridica = '';
      }
    },
    
    validateNombre() {
      const nombre = this.formData.nombre.trim();
      
      if (!nombre) {
        this.errors.nombre = 'El nombre de la empresa es obligatorio.';
      } else if (nombre.length > 100) {
        this.errors.nombre = 'El nombre no debe exceder 100 caracteres.';
      } else {
        this.errors.nombre = '';
      }
    },
    
    validateDireccion() {
      const direccion = this.formData.direccionEspecifica;
      
      if (direccion.length > 200) {
        this.errors.direccionEspecifica = 'La direccion no debe exceder 200 caracteres.';
      } else {
        this.errors.direccionEspecifica = '';
      }
    },
    
    validateTelefono() {
      const telefono = this.formData.telefono ? this.formData.telefono.toString().trim() : '';
      
      if (telefono && !/^\d+$/.test(telefono)) {
        this.errors.telefono = 'El telefono debe contener solo numeros.';
      } else if (telefono && telefono.length > 15) {
        this.errors.telefono = 'El telefono no debe exceder 15 digitos.';
      } else {
        this.errors.telefono = '';
      }
    },
    
    validateMaxBenefits() {
      const beneficios = this.formData.noMaxBeneficios;
      if (beneficios === '' || beneficios === null || beneficios === undefined) {
        this.errors.noMaxBeneficios = 'El numero maximo de beneficios es obligatorio.';
      } else {
        const numBeneficios = Number(beneficios);
        
        if (isNaN(numBeneficios)) {
          this.errors.noMaxBeneficios = 'Debe ser un numero valido.';
        } else if (numBeneficios < 0) {
          this.errors.noMaxBeneficios = 'No puede ser negativo.';
        } else {
          this.errors.noMaxBeneficios = '';
        }
      }
    },
    
    validateFrecuenciaPago() {
      this.errors.frecuenciaPago = this.formData.frecuenciaPago ? '' : 'Seleccione una frecuencia de pago.';
    },
    
    validateDiaPago() {
      const dia = this.formData.diaPago ? this.formData.diaPago.toString().trim() : '';
      
      if (!dia) {
        this.errors.diaPago = 'El dia de pago es obligatorio.';
      } else if (!/^\d+$/.test(dia)) {
        this.errors.diaPago = 'Debe ser un numero valido.';
      } else {
        const diaNum = parseInt(dia);
        if (diaNum < 1 || diaNum > 30) {
          this.errors.diaPago = 'El dia debe estar entre 1 y 30.';
        } else {
          this.errors.diaPago = '';
        }
      }
    },
    
    async submitForm() {
      console.log('Datos del formulario capturados:');
      console.log('Cedula Juridica:', this.formData.cedulaJuridica);
      console.log('Nombre Empresa:', this.formData.nombre);
      console.log('Direccion:', this.formData.direccionEspecifica);
      console.log('Telefono:', this.formData.telefono);
      console.log('Max Beneficios:', this.formData.noMaxBeneficios);
      console.log('Frecuencia Pago:', this.formData.frecuenciaPago);
      console.log('Dia Pago:', this.formData.diaPago);
      
      console.log('Objeto completo formData:', JSON.parse(JSON.stringify(this.formData)));
      
      this.validateCedulaJuridica();
      this.validateNombre();
      this.validateDireccion();
      this.validateTelefono();
      this.validateMaxBenefits();
      this.validateFrecuenciaPago();
      this.validateDiaPago();
      
      const hasErrors = Object.values(this.errors).some(error => error !== '');
      
      const camposObligatorios = [
      this.formData.cedulaJuridica,
      this.formData.nombre, 
      this.formData.noMaxBeneficios,
      this.formData.frecuenciaPago,
      this.formData.diaPago
      ];

      const faltanCamposObligatorios = camposObligatorios.some(campo => 
        campo !== 0 && !campo  
      );

      console.log('=== DEBUG DETALLADO DE CAMPOS OBLIGATORIOS ===');
      console.log('cedulaJuridica:', this.formData.cedulaJuridica, '¿Vacío?:', !this.formData.cedulaJuridica);
      console.log('nombre:', this.formData.nombre, '¿Vacío?:', !this.formData.nombre);
      console.log('noMaxBeneficios:', this.formData.noMaxBeneficios, '¿Vacío?:', !this.formData.noMaxBeneficios);
      console.log('frecuenciaPago:', this.formData.frecuenciaPago, '¿Vacío?:', !this.formData.frecuenciaPago);
      console.log('diaPago:', this.formData.diaPago, '¿Vacío?:', !this.formData.diaPago);
      console.log('faltanCamposObligatorios:', faltanCamposObligatorios);
      console.log('===============================================');

      if (hasErrors || faltanCamposObligatorios) {
        console.log('SE DETIENE POR VALIDACIONES');
        this.successMessage = 'Corrija los errores antes de continuar.';
        this.messageType = 'error';
        return;
      }
      
      const empresaData = {
        cedulaJuridica: Number(this.formData.cedulaJuridica),
        nombre: this.formData.nombre.trim(),
        direccionEspecifica: this.formData.direccionEspecifica.trim() || null,
        telefono: this.formData.telefono ? Number(this.formData.telefono) : null,
        noMaxBeneficios: Number(this.formData.noMaxBeneficios),
        frecuenciaPago: this.formData.frecuenciaPago,
        diaPago: Number(this.formData.diaPago)
      };

      console.log('=== VERIFICACION DE TIPOS ===');
      console.log('Tipo de cedula:', typeof empresaData.cedulaJuridica);
      console.log('Valor de cedula:', empresaData.cedulaJuridica);
      console.log('Tipo de telefono:', typeof empresaData.telefono);
      console.log('Valor de telefono:', empresaData.telefono);
      console.log('JSON completo:', JSON.stringify(empresaData));
      console.log('================================');
      
      if (this.loading) return; 
      this.loading = true;

      try {
        await this.guardarEmpresaEnBackend(empresaData);
        
        this.successMessage = 'Empresa registrada correctamente.';
        this.messageType = 'success';
        
        setTimeout(() => {
          this.volverAlHome();
        }, 1000);
        
      } catch (error) {
        console.error('Error al guardar empresa:', error);
        if (error.message.includes('cédula jurídica')) {
          this.successMessage = error.message;
        } else {
          this.successMessage = 'Error: ' + error.message;
        }
        this.messageType = 'error';
      } finally {
        this.loading = false; 
      }
    },
    
    async guardarEmpresaEnBackend(empresaData) {
      try {
        console.log('INICIANDO ENVIO AL BACKEND');
        console.log('Datos recibidos en guardarEmpresaEnBackend:', empresaData);
        console.log('UserId a enviar:', this.userId);
        
        if (!this.userId) {
          throw new Error('Usuario no autenticado. Por favor, inicie sesión.');
        }
        
        const requestData = {
          userId: this.userId,
          empresa: empresaData
        };
        
        console.log('Datos completos a enviar:', requestData);
        

        const response = await axios.post(
          API_ENDPOINTS.CREATE_EMPRESA,
          requestData,
          {
            headers: { "Content-Type": "application/json" }
          }
        );

        console.log('Status de respuesta:', response.status);
        console.log('Respuesta del servidor:', response.data);
        
        return response.data;
        
      } catch (error) {
        console.error('Error en guardarEmpresaEnBackend:', error);
        if (error.response && error.response.data) {
          const serverMessage = error.response.data.message || error.response.data;
          throw new Error(serverMessage);
        } else {
          throw new Error('Error de conexión con el servidor');
        }
      }
    },
        
    resetForm() {
      this.formData = {
        cedulaJuridica: '',
        nombre: '',
        direccionEspecifica: '',
        telefono: '',
        noMaxBeneficios: '',
        frecuenciaPago: '',
        diaPago: ''
      };
      
      for (const key in this.errors) {
        this.errors[key] = '';
      }
    }
  }
}
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

/* Contenedor de logo y textos */
.brand {
  display: flex;
  align-items: center;
  gap: 18px;
  max-width: 55%;
  margin-bottom: 150px;
}

/* Caja del logo */
.logo-box {
  width: 84px;
  height: 84px;
  background: linear-gradient(180deg, #51a3a0, hsl(178, 77%, 86%));
  border-radius: 16px;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

/* Letra F del logo */
.logo-box .f {
  font-weight: 800;
  font-size: 44px;
  color: white;
  line-height: 1;
}

/* Título principal */
.texts h1 {
  margin: 0;
  font-size: 34px;
}

/* Subtítulo */
.texts p {
  margin: 6px 0 0;
  color: #bdbdbd;
}

/* Card del formulario */
.form-card {
  width: 400px;
  min-height: 220px;
  background: rgb(71, 69, 69);
  border: 1px solid rgba(255, 255, 255, 0.15);
  padding: 25px;
  border-radius: 8px;
  box-shadow: 0 6px 18px rgba(0, 0, 0, 0.35);
}

/* Título del card */
.form-card h2 {
  color: #eee;
  margin: 0 0 20px;
  font-weight: 600;
  font-size: 18px;
  text-align: center;
}

/* Estilo de los campos de entrada */
.input {
  display: flex;
  align-items: center;
  padding: 10px 12px;
  border-radius: 6px;
  background: rgba(0, 0, 0, 0.25);
  border: 1px solid rgba(255, 255, 255, 0.06);
  margin-bottom: 12px;
  color: #ece6e6ff;
}

/* Input dentro del formulario */
.input input, .input select {
  background: transparent;
  border: 0;
  outline: 0;
  color: rgb(255, 255, 255);
  width: 100%;
  font-size: 14px;
}

.input select {
  color: rgb(116, 116, 116);
}

.input select:valid,
.input select:focus {
  color: whitesmoke;
}

/* Estilos para el dropdown desplegado */
.input select option {
  background: rgb(71, 69, 69);
  color: whitesmoke;
  padding: 10px 12px;
  border: none;
}

.input select option:hover {
  background: #1fb9b4;
  color: white;
}

.input select option:checked {
  background: #1fb9b4;
  color: white;
}

/* Estilos para los botones */
.buttons-row {
  display: flex;
  gap: 12px;
  margin-top: 10px;
}

.buttons-row .btn {
  width: 50%; 
  padding: 10px 12px; 
  font-size: 14px;
}

/* Estilos para botón primario */
.btn-primary {
  background: #1fb9b4;
  color: white;
}

.btn-primary:hover:not(:disabled) {
  background: #1aa8a4;
}

/* Estilos para botón secundario (Volver) */
.btn-secondary {
  background: #6c757d;
  color: white;
}

.btn-secondary:hover:not(:disabled) {
  background: #5a6268;
}

/* Estilos comunes para botones */
.btn {
  border-radius: 6px;
  border: 0;
  font-weight: 600;
  cursor: pointer;
  transition: background 0.3s;
  text-align: center;
}

.btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

/* Mensajes de error y éxito */
.error {
  color: #ff6b6b;
  font-size: 13px;
  margin-top: 5px;
  min-height: 18px;
}

.message {
  margin-top: 15px;
  text-align: center;
  font-size: 14px;
  padding: 0;
}

.message.success {
  color: #9fe6cf;
}

.message.error {
  color: #ff6b6b;
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

/* Responsivo para pantallas pequeñas */
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

  footer {
    flex-direction: column;
    gap: 10px;
    text-align: center;
  }
}
</style>