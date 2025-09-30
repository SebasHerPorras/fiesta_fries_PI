<template>
    <div class="wrap">
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

                <div class="company-dropdown">
                    <label for="companySelect">Empresa :</label>
                    <select id="companySelect" v-model="selectedCompany" @change="saveSelectedCompany">
                        <option disabled value="">No tienes empresas registradas</option>
                        <option v-for="company in companies" :key="company.cedulaJuridica" :value="company">
                            {{ company.nombre }}
                        </option>
                    </select>
                </div>

                <ul class="nav-list">
                    <li><router-link to="/home">Home</router-link></li>
                    <li><router-link to="/FormEmpresa">Registrar Empresa</router-link></li>
                    <li><router-link to="/RegEmpleado">Registrar Empleado</router-link></li>
                    <li><router-link to="/PageEmpresaAdmin">Ver Toda Empresa</router-link></li>
                    <li><a href="#" @click.prevent="logout">Cerrar Sesión</a></li>
                </ul>
            </nav>
        </header>
        <main class="hero">
            <aside class="register-card">
                <h2>Registrar Empleado</h2>
                <form id="EmployeeRegister" @submit.prevent="handleSubmit" @reset="handleReset">

                <label class="input">
                    <input type="email"
                        v-model="form.email"
                        placeholder="Correo del empleado"
                        @blur="validateEmail"
                        required />
                </label>
                
                <div v-if="emailError" class="error-msg">{{ emailError }}</div>

                    <label class="input">
                        <input type="text"
                               v-model="form.position"
                               placeholder="Puesto"
                               required />
                    </label>

                    <label class="input">
                        <select v-model="form.employmentType" required>
                            <option disabled value="">Tipo de Contrato</option>
                            <option value="Tiempo Completo">Tiempo Completo</option>
                            <option value="Medio Tiempo">Medio Tiempo</option>
                            <option value="Por Horas">Por Horas</option>
                        </select>
                    </label>

                    <div class="buttons">
                        <button class="btn" type="submit">Registrar</button>
                    </div>
                </form>
            </aside>
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
export default {
    name: "RegistrarEmpleado",
    data() {
        return {
            userName: "Cargando...",
            userRole: "Cargando...",
            companies: [],
            selectedCompany: null,
            form: {
                email: "",
                position: "",
                employmentType: ""
        },
        emailError: ""
        };
    },
    mounted() {
        this.loadUserFromLocalStorage();
    },
    methods: {
        loadUserFromLocalStorage() {
            const stored = localStorage.getItem("userData");
            if (!stored) {
                this.$router.push("/"); // si no hay sesión, redirige
                return;
            }
            const userData = JSON.parse(stored);

            // Aquí ajusta según cómo guardaste los datos en login
            this.userName = `${userData.firstName} ${userData.secondName}`;
            this.userRole = userData.personType; // Ej: "Empleador" o "Empleado"
        },
        validateEmail() {
            const regex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
            if (!regex.test(this.form.email)) {
                this.emailError = "Formato de correo incorrecto";
            } else {
                this.emailError = "";
            }
        },
        handleSubmit() {
            this.validateEmail();
            if (this.emailError) return;

            console.log("Datos del empleado a registrar:", this.form);

            // Simulación de guardado
            let empleados = JSON.parse(localStorage.getItem("empleados")) || [];
            empleados.push(this.form);
            localStorage.setItem("empleados", JSON.stringify(empleados));

            alert("Empleado guardado en LocalStorage (modo prueba)");
            this.handleReset();
        },
        handleReset() {
            this.form = { email: "", position: "", employmentType: "" };
            this.emailError = "";
        },
        saveSelectedCompany() {
            localStorage.setItem("selectedCompany", JSON.stringify(this.selectedCompany));
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

    .wrap {
        min-height: 100vh;
        display: flex;
        flex-direction: column;
        background: #1e1e1e;
        color: whitesmoke;
    }

    .hero {
        display: flex;
        align-items: flex-start;
        justify-content: center;
        gap: 40px;
        padding: 48px 64px;
        flex: 1 0 auto;
    }   

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

        .input input {
            background: transparent;
            border: 0;
            outline: 0;
            color: whitesmoke;
            width: 100%;
            font-size: 14px;
        }

    .error-msg {
        color: #ff6b6b;
        font-size: 13px;
        margin: -6px 0 10px 4px;
        text-align: left;
    }

    .input select {
        background: transparent;
        border: 0;
        outline: 0;
        color: whitesmoke;
        width: 100%;
        font-size: 14px;
        appearance: none;
        cursor: pointer;
    }

        .input select option {
            background: #1e1e1e;
            color: whitesmoke;
        }

    @media (max-width: 900px) {
        .hero {
            flex-direction: column;
            align-items: center;
            padding: 36px;
        }

        .brand {
            margin-bottom: 20px;
            max-width: 100%;
        }

        .register-card {
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

