<template>
  <div class="wrap">
    <!-- HEADER -->
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
              <!-- Siempre visibles -->
              <li><router-link to="/Profile">Datos Personales</router-link></li>

              <!-- Solo Empleador: Registrar Empresa -->
              <li v-if="userRole === 'Empleador'">
                  <router-link to="/FormEmpresa">Registrar Empresa</router-link>
              </li>

              <!--Solo Empleado: Añadir horas-->
              <li v-f="userRole === 'Empleado'">
                  <router-link to="/RegisterHoras">Registrar Horas</router-link>
              </li>

              <!-- Dropdown de empresas SOLO para Empleador -->
              <li v-if="userRole === 'Empleador' && companies.length > 0" class="company-dropdown-item">
                  <select v-model="selectedCompany" @change="onCompanyChange" class="company-select">
                      <option disabled value="">Seleccionar Empresa</option>
                      <option v-for="company in companies" :key="company.cedulaJuridica" :value="company">
                      {{ company.nombre }}
                      </option>
                  </select>
              </li>

              <!-- Solo Administrador: Ver Toda Empresa -->
              <li v-if="isAdmin">
                  <router-link to="/PageEmpresaAdmin">Ver Toda Empresa</router-link>
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
      isAdmin: false
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
    const profileRes = await axios.get(`http://localhost:5081/api/person/profile/${userId}`);
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
        if (!userId)
        return;

      // Si es ADMIN, cargar TODAS las empresas
      if (this.isAdmin || stored.isAdmin) {
        console.log('Usuario es admin, cargando TODAS las empresas');
        const empresasRes = await axios.get(`http://localhost:5081/api/empresa/todas`);
        
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

      // Si NO es admin, verificar si es Empleador
      const isEmpleador = stored.personType === "Empleador";

      if (!isEmpleador) {
        console.log(`Usuario es ${stored.personType || 'empleado'}, no se cargan empresas`);
        this.companies = [];
        return;
      }
 
      // Buscar empresas del Empleador específico
      console.log('Usuario es Empleador, cargando sus empresas');
      const empresasRes = await axios.get(`http://localhost:5081/api/empresa/mis-empresas/${userId}`);
      
      // Verificar la estructura de la respuesta
      if (empresasRes.data && empresasRes.data.success) {
        // Si la respuesta tiene estructura { success: true, empresas: [...] }
        this.companies = empresasRes.data.empresas || [];
      } else if (Array.isArray(empresasRes.data)) {
        // Si la respuesta es directamente un array
        this.companies = empresasRes.data;
      } else {
        this.companies = [];
      }

      if (this.companies.length > 0) {
          const savedCompany = localStorage.getItem("selectedCompany");
          if (savedCompany) {
            try {
              this.selectedCompany = JSON.parse(savedCompany);
              console.log("Empresa recuperada de localStorage:", this.selectedCompany);
            } catch (e) {
              console.error("Error parsing saved company:", e);
              this.selectedCompany = this.companies[0];
              this.saveSelectedCompany();
            }
          } else {
            this.selectedCompany = this.companies[0];
            this.saveSelectedCompany();
          }
        } else {
          console.log("Empleador no tiene empresas registradas");
        }
      } catch (err) {
        console.error("Error cargando empresas:", err);
        this.companies = [];
      }
    },

    saveSelectedCompany() {
      if (this.selectedCompany) {
        // Guardar toda la información de la empresa en localStorage
        localStorage.setItem("selectedCompany", JSON.stringify(this.selectedCompany));
        console.log("Empresa seleccionada guardada:", this.selectedCompany.nombre);
      } else {
        localStorage.removeItem("selectedCompany");
      }
    },

    onCompanyChange() {
      if (this.selectedCompany) {
        this.saveSelectedCompany();
        // Redirigir a la página de administración de empresas
        this.$router.push('/PageEmpresaAdmin');
      }
    },

    logout() {
      localStorage.removeItem("userData");
      localStorage.removeItem("selectedCompany");
      this.$router.push("/");
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

    .brand {
        display: flex;
        align-items: center;
        gap: 18px;
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
</style>
