import { createApp } from "vue";
import App from "./App.vue";
import { createRouter, createWebHistory } from "vue-router";
import LoginRegister from "./components/LoginRegister.vue";
import PageEmpresaAdmin from "./components/PageEmpresaAdmin.vue";
import FormEmpresa from "./components/FormEmpresa.vue";
import RegisterEmpleado from "./components/RegisterEmpleado.vue";
import SignInEmpleador from "./components/SignInEmpleadores.vue";
import PersonalProfile from "./components/DatosPersonales.vue";
import FormBeneficios from "./components/FormBeneficios.vue";

const DEVELOPMENT_MODE = false;

const router = createRouter({
  history: createWebHistory(),
  routes: [
    {
      path: "/",
      name: "LoginRegister",
      component: LoginRegister,
      meta: {
        requiresAuth: false,
        public: true,
        allowedRoles: ["any"],
        blockIfLoggedIn: true,
      },
    },
    {
      path: "/Profile",
      name: "Profile",
      component: PersonalProfile,
      meta: {
        requiresAuth: true,
        public: false,
        allowedRoles: ["admin", "empleado", "empleador"],
      },
    },
    {
      path: "/PageEmpresaAdmin",
      name: "PageEmpresaAdmin",
      component: PageEmpresaAdmin,
      meta: {
        requiresAuth: true,
        public: false,
        allowedRoles: ["admin", "empleador"],
      },
    },
    {
      path: "/FormEmpresa",
      name: "FormEmpresa",
      component: FormEmpresa,
      meta: {
        requiresAuth: true,
        public: false,
        allowedRoles: ["admin", "empleador"],
      },
    },
    {
      path: "/SignInEmpleadores",
        name: "SignInEmpleador",
       component: SignInEmpleador,
      meta: {
        requiresAuth: false,
        public: true,
        allowedRoles: ["any"],
        blockIfLoggedIn: true,
      },
    },
    {
      path: "/RegEmpleado",
      name: "RegEmpleado",
      component: RegisterEmpleado,
      meta: {
        requiresAuth: true,
        public: false,
        allowedRoles: ["empleador"],
      },
    },
    {
      path: "/FormBeneficios",
      name: "FormBeneficios",
      component: FormBeneficios,
      meta: {
        requiresAuth: true,
        public: false,
        allowedRoles: ["empleador"],
      },
    },
  ],
});

// validacion de development mode, si estamos en develop no hay restricciones
// entrada libre a todas las rutas
router.beforeEach((to, from, next) => {
  if (DEVELOPMENT_MODE) {
    next();
    return;
  }

  const authResult = checkUserAuthentication();
  const isLoggedIn = authResult.isAuthenticated;
  const userRole = authResult.userRole;

  const requiresAuth = to.meta.requiresAuth;
  const isPublic = to.meta.public;
  const allowedRoles = to.meta.allowedRoles || ["any"];
  const blockIfLoggedIn = to.meta.blockIfLoggedIn || false;

  if (blockIfLoggedIn && isLoggedIn) {
    redirectLoggedInUser(userRole, next);
    return;
  }

  if (isPublic && !requiresAuth) {
    next();
    return;
  }

  if (requiresAuth) {
    if (!isLoggedIn) {
      next("/");
      return;
    }

    if (allowedRoles.includes(userRole)) {
      next();
    } else {
      redirectByRole(userRole, next);
    }
    return;
  }

  next("/");
});

function checkUserAuthentication() {
  try {
    const userData = localStorage.getItem("userData");
    if (!userData) {
      return { isAuthenticated: false, userRole: null };
    }

    const user = JSON.parse(userData);
    if (!user || !user.id) {
      return { isAuthenticated: false, userRole: null };
    }

    let userRole = "empleado";

    if (user.isAdmin === true || user.admin === true) {
      userRole = "admin";
    } else if (user.personType === "Empleador" || user.tipo === "empleador") {
      userRole = "empleador";
    } else if (user.personType === "Empleado" || user.tipo === "empleado") {
      userRole = "empleado";
    }

    return { isAuthenticated: true, userRole: userRole };
  } catch (error) {
    return { isAuthenticated: false, userRole: null };
  }
}

function redirectLoggedInUser(userRole, next) {
  switch (userRole) {
    case "admin":
      next("/PageEmpresaAdmin");
      break;
    case "empleador":
      next("/Profile");
      break;
    case "empleado":
      next("/Profile");
      break;
    default:
      next("/Profile");
      break;
  }
}

function redirectByRole(userRole, next) {
  switch (userRole) {
    case "admin":
      next("/PageEmpresaAdmin");
      break;
    case "empleador":
      next("/Profile");
      break;
    case "empleado":
      next("/Profile");
      break;
    default:
      next("/");
      break;
  }
}

createApp(App).use(router).mount("#app");
