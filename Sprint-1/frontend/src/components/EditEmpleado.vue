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

      <form id="EmployerLogIn" @submit.prevent="guardarCambios" novalidate>
        <h2 style="color: #eee; margin: 0 0 20px; font-weight: 600; font-size: 18px; text-align: center;">
          Editar Empleado
        </h2>

        <div class="field-group">
          <div class="error-msg">{{ errors.firstName }}</div>
          <label class="input">
            <input v-model="form.firstName" type="text" placeholder="Primer nombre*" required />
          </label>
        </div>

        <div class="field-group">
          <div class="error-msg">{{ errors.secondName }}</div>
          <label class="input">
            <input v-model="form.secondName" type="text" placeholder="Segundo nombre*" required />
          </label>
        </div>

        <div class="field-group">
          <div class="error-msg">{{ errors.direction }}</div>
          <label class="input">
            <input v-model="form.direction" type="text" placeholder="Dirección*" required />
          </label>
        </div>

        <div class="field-group">
          <div class="error-msg">{{ errors.personalPhone }}</div>
          <label class="input">
            <input v-model="form.personalPhone" type="tel" placeholder="Teléfono personal*" required />
          </label>
        </div>

        <div class="field-group">
          <label class="input">
            <input v-model="form.homePhone" type="tel" placeholder="Teléfono de casa (opcional)" />
          </label>
        </div>

        <div class="field-group">
          <div class="error-msg">{{ errors.position }}</div>
          <label class="input">
            <input v-model="form.position" type="text" placeholder="Cargo*" required />
          </label>
        </div>

        <div class="field-group">
          <div class="error-msg">{{ errors.department }}</div>
          <label class="input">
            <input v-model="form.department" type="text" placeholder="Departamento*" required />
          </label>
        </div>

        <div class="field-group">
          <div class="error-msg">{{ errors.salary }}</div>
          <label class="input">
            <input v-model.number="form.salary" type="number" min="0" placeholder="Salario*" required />
          </label>
        </div>

        <div class="buttons-row">
          <button class="btn btn-secondary" type="button" @click="cancelar">← Volver</button>
          <button class="btn btn-primary" type="submit" :disabled="loading">
            <span v-if="loading">Guardando...</span>
            <span v-else>Actualizar</span>
          </button>
        </div>

        <div class="field-group">
          <div class="message" v-if="mensaje" :class="{ error: mensaje.tipo === 'error', success: mensaje.tipo === 'success' }">
            {{ mensaje.texto }}
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
import apiConfig from '../config/apiConfig';

export default {
  data() {
    return {
      form: {
        id: null,
        firstName: '',
        secondName: '',
        direction: '',
        personalPhone: '',
        homePhone: '',
        position: '',
        department: '',
        salary: 0,
      },
      errors: {
        firstName: '',
        secondName: '',
        direction: '',
        personalPhone: '',
        homePhone: '',
        position: '',
        department: '',
        salary: '',
      },
      mensaje: null,
      loading: false,
    };
  },
  async mounted() {
    try {
      const id = this.$route.params.id;
      const response = await axios.get(apiConfig.API_ENDPOINTS.EMPLEADO_GET_BY_ID(id));
      const data = response.data;

      this.form = { ...data, id: this.$route.params.id };
    } catch (error) {
      console.error("Error al cargar los datos del empleado:", error);
      this.mensaje = { tipo: 'error', texto: 'Error al cargar los datos del empleado.' };
    }
  },
  methods: {
    async guardarCambios() {
      if (!this.validarFormulario()) return;

      this.loading = true;
      try {
        
        const url = apiConfig.API_ENDPOINTS.EMPLEADO_UPDATE_BY_ID(this.form.id);
        await axios.put(url, this.form, {
          headers: { "Content-Type": "application/json" }
        });

        this.mensaje = { tipo: "success", texto: "Empleado actualizado correctamente." };
        setTimeout(() => this.$router.push({ name: "PageEmpresaAdmin" }), 1500);
      } catch (error) {
        console.error("Error al actualizar empleado:", error);
        this.mensaje = { tipo: "error", texto: "No se pudo guardar los cambios." };
      } finally {
        this.loading = false;
      }
    },
    validarFormulario() {
      this.errors = {
        firstName: '',
        secondName: '',
        direction: '',
        personalPhone: '',
        homePhone: '',
        position: '',
        department: '',
        salary: '',
      };

      let valid = true;

      if (!this.form.firstName) {
        this.errors.firstName = 'Este campo es obligatorio.';
        valid = false;
      }

      if (!this.form.secondName) {
        this.errors.secondName = 'Este campo es obligatorio.';
        valid = false;
      }

      if (!this.form.direction) {
        this.errors.direction = 'Este campo es obligatorio.';
        valid = false;
      }

      if (!this.form.personalPhone || String(this.form.personalPhone).length < 8) {
        this.errors.personalPhone = 'Teléfono inválido.';
        valid = false;
      }

      if (!this.form.position) {
        this.errors.position = 'Este campo es obligatorio.';
        valid = false;
      }

      if (!this.form.department) {
        this.errors.department = 'Este campo es obligatorio.';
        valid = false;
      }

      if (this.form.salary <= 0) {
        this.errors.salary = 'El salario debe ser mayor a cero.';
        valid = false;
      }

      return valid;
    },
    cancelar() {
      this.$router.push({ name: 'PageEmpresaAdmin' });
    },
  },
};
</script>

<style src="@/assets/style/RegisterEmpleado.css" scoped> </style>
