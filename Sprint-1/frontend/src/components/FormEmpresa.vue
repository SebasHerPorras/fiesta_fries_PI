<template>
  <div class="formulario-container">
    <div class="container">
      <div class="header">
        <div class="logo">
          <span class="logo-text">F</span>
        </div>
        <div class="brand-text">
          <h1>Fiesta Fries</h1>
          <p>Crear Nueva Empresa</p>
        </div>
      </div>
      
      <div class="form-card">
        <h2 class="form-title">Crea Nueva Empresa</h2>
        
        <form id="companyForm" @submit.prevent="submitForm">
          <div class="form-group">
            <input 
              type="text" 
              id="ruc" 
              v-model="formData.cedulaJuridica"
              class="form-control" 
              placeholder="Cédula jurídica *"
              required
              maxlength="10" 
              @input="validateCedulaJuridica"
            >
            <div class="error" id="cedulaJuridicaError">{{ errors.cedulaJuridica }}</div>
          </div>
          
          <div class="form-group">
            <input 
              type="text" 
              id="name" 
              v-model="formData.nombre"
              class="form-control" 
              placeholder="Nombre de la empresa *"
              required
              @input="validateNombre"
            >
            <div class="error" id="nombreError">{{ errors.nombre }}</div>
          </div>
          
          <div class="form-group">
            <input 
              type="text" 
              id="direccion" 
              v-model="formData.direccionEspecifica"
              class="form-control" 
              placeholder="Direccion especifica (opcional)"
              @input="validateDireccion"
            >
            <div class="error" id="direccionError">{{ errors.direccionEspecifica }}</div>
          </div>
          
          <div class="form-group">
            <input 
              type="tel" 
              id="phone" 
              v-model="formData.telefono"
              class="form-control" 
              placeholder="Telefono (opcional)"
              @input="validateTelefono"
            >
            <div class="error" id="telefonoError">{{ errors.telefono }}</div>
          </div>
          
          <div class="form-group">
            <input 
              type="number" 
              id="maxBenefits" 
              v-model="formData.noMaxBeneficios"
              class="form-control" 
              placeholder="Numero maximo de beneficios (0 = sin limite) *"
              required
              min="0"
              @input="validateMaxBenefits"
            >
            <div class="error" id="maxBenefitsError">{{ errors.noMaxBeneficios }}</div>
          </div>
          
          <div class="form-group">
            <select 
              id="paymentFrequency" 
              v-model="formData.frecuenciaPago"
              class="form-control" 
              required
              @change="validateFrecuenciaPago"
            >
              <option value="" disabled selected>Frecuencia de Pago *</option>
              <option value="quincenal">Quincenal</option>
              <option value="mensual">Mensual</option>
            </select>
            <div class="error" id="frecuenciaPagoError">{{ errors.frecuenciaPago }}</div>
          </div>
          
          <div class="form-group">
            <input 
              type="number" 
              id="paymentDay" 
              v-model="formData.diaPago"
              class="form-control" 
              placeholder="Dia de pago (1-30) *"
              required
              min="1"
              max="31"
              @input="validateDiaPago"
            >
            <div class="error" id="diaPagoError">{{ errors.diaPago }}</div>
          </div>
          
          <button 
            type="submit" 
            class="btn-primary"
            :disabled="loading"
          >
            <span v-if="loading">⏳ Registrando...</span>
            <span v-else>Registrar Empresa</span>
          </button>
          
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
  name: 'CompanyFormulario',
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
      existingCompanies: [],
      userId: '',
      loading: false 
    }
  },

  mounted() {
  this.obtenerUserId();
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
      const dia = this.formData.diaPago.toString().trim();
      
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
          this.resetForm();
          this.successMessage = '';
        }, 2000);
        
      } catch (error) {
        console.error('Error al guardar empresa:', error);
        if (error.message.includes('cédula jurídica')) {
          this.successMessage =    error.message;
        } else {
          this.successMessage = 'Error: ' + error.message;
        }
        this.messageType = 'error';
      }finally {
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
          "https://localhost:7056/api/Empresa", 
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

.btn-primary {
  background: #1fb9b4;
  color: white;
  border: none;
  padding: 12px 20px;
  border-radius: 6px;
  font-weight: 600;
  cursor: pointer;
  width: 100%;
  font-size: 16px;
  margin-top: 10px;
  transition: background 0.3s, transform 0.1s;
}

.btn-primary:hover {
  background: #1aa8a4;
}

.btn-primary:active {
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
}
</style>