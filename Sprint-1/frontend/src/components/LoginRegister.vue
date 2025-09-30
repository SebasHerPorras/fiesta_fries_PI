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
          <p>Gestor de Planillas</p>
        </div>
      </div>

      <!-- Card de login -->
      <aside class="login-card">
        <h2>Iniciar Sesión</h2>
        <!-- Formulario de login -->
        <form @submit.prevent="login">
          <!-- Campo de email -->
          <label class="input">
            <input v-model="email" type="email" placeholder="👤Email" required />
          </label>

          <!-- Campo de contraseña -->
          <label class="input">
            <input v-model="password" type="password" placeholder="🔒Contraseña" required />
          </label>
          <!-- v if por si hay un error en la contraseña -->
          <div v-if="passwordError" style="color: #ff6b6b; font-size: 13px; margin-bottom: 8px">
            {{ passwordError }}
          </div>

          <!-- Botón para enviar el login -->
          <button class="btn" type="submit">Iniciar Sesión</button>
        </form>

        <!-- Pie del card de login con enlace a registro -->
        <div class="login-footer">
          <p>
            ¿No tienes una cuenta de Empleador?
            <a href="/LogINEmpleadores">Regístrate</a>
          </p>
        </div>
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

// imports necesarios para el login
import axios from 'axios';

export default {
  name: "LoginRegister", // Nombre del componente principal
  data() {
    return {
      email: "", // Estado para el email del login
      password: "", // Estado para la contraseña del login
      passwordError: "", // Estado para el mensaje de error de la contraseña
      loading: false
    };
  },
  methods: {
    showPasswordError(msg) {
      this.passwordError = msg;
    },
    async login() {
      this.passwordError = "";
      this.loading = true;
      try {
        const url = "https://localhost:7056/api/user/login";
        const res = await axios.post(url, {
          email: this.email.trim(),
          password: this.password
        });

        console.log("Email:", this.email);
        console.log("Password length:", this.password?.length);

        // Acepta cualquier 200 OK como login correcto y/o valida la respuesta
        if (res.status === 200 && res.data && (res.data.id || res.data.email)) {
          //Guardar datos en LocalStorage
          const userData = {
            id: res.data.id,           
            email: res.data.email,
            personaId: res.data.personaId,
            personType: res.data.personType,
            firstName: res.data.firstName,
            secondName: res.data.secondName
          };
      
          // Guardar en LocalStorage
          localStorage.setItem('userData', JSON.stringify(userData));
          
          console.log('Datos de usuario guardados:', userData);
          console.log('UserId guardado:', userData.id);

          console.log('Datos guardados en localStorage:');
          console.log('- UserId:', userData.id);
          console.log('- Email:', userData.email);
          console.log('- PersonType:', userData.personType);
          console.log('- PersonaId:', userData.personaId);
          console.log('- Nombre completo:', userData.firstName, userData.secondName);

          // Verificá que se guardó correctmente
          const storedData = localStorage.getItem('userData');
          console.log('✅ Verificación - Datos en localStorage:', storedData);
          
          // login exitoso
          alert("Login exitoso!");
          this.$router.push({ path: "/Home" });

        } else {
          this.showPasswordError("Credenciales inválidas.");
        }
      } catch (err) {
        if (err.response && err.response.status === 401) {
          this.showPasswordError("Credenciales inválidas.");
        } else {
          this.showPasswordError("Error al conectar con el servidor.");
          console.error(err);
        }
      } finally {
        this.loading = false;
      }
    },
  },
};
</script>

<style scoped>
/* wrap es el contenedor principal (el fondo) */
.wrap {
  min-height: 100vh;
  display: flex;
  flex-direction: column;
  background: #1e1e1e;
  /* var --color-background */
  /* Fondo azul oscuro */
  color: whitesmoke;
}

/* Sección principal con flex para centrar contenido */
.hero {
  display: flex;
  align-items: center;
  justify-content: center;
  /* Centra horizontalmente */
  color: whitesmoke;
  padding: 48px 64px;
  gap: 40px;
  /* Menos espacio entre elementos */
  flex: 1 0 auto;
  /* Permite que el main crezca y ocupe espacio */
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
  background: linear-gradient(180deg,
      #51a3a0,
      hsl(178, 77%, 86%));
  /* Degradado azul */
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
  /* Altura de línea para la letra F */
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

/* Card de login */
.login-card {
  width: 360px;
  min-height: 220px;
  background: rgb(71, 69, 69);
  border: 1px solid rgba(255, 255, 255, 0.15);
  padding: 22px;
  border-radius: 8px;
  box-shadow: 0 6px 18px rgba(0, 0, 0, 0.35);
}

/* Título del card de login */
.login-card h2 {
  color: #eee;
  /* Color de texto claro */
  margin: 0 0 12px;
  font-weight: 600;
  font-size: 16px;
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
.input input {
  background: transparent;
  border: 0;
  outline: 0;
  color: whitesmoke;
  width: 100%;
  font-size: 14px;
}

/* Botón de login */
.btn {
  width: 100%;
  padding: 10px 12px;
  border-radius: 6px;
  border: 0;
  font-weight: 600;
  cursor: pointer;
  background: #1fb9b4;
  color: white;
}

/* Pie del card de login */
.login-footer {
  margin-top: 10px;
  font-size: 13px;
  color: #ece6e6ff;
}

/* Estilos para el enlace de "Registrarse" */
.login-footer a {
  color: #1fb1a2ff;
  text-decoration: underline;
}

/* Footer de la página -> seccion de contacto */
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
  }

  .login-card {
    width: 100%;
    max-width: 420px;
  }

  footer {
    flex-direction: column;
    gap: 10px;
    text-align: center;
  }
}
</style>
