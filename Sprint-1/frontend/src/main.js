import { createApp } from 'vue'
import App from './App.vue'
import { createRouter, createWebHistory } from "vue-router";
import Homepage from './components/Homepage.vue';
import LoginRegister from './components/LoginRegister.vue';
import PageEmpresaAdmin from './components/PageEmpresaAdmin.vue'; 
import FormEmpresa from './components/FormEmpresa.vue'; 
import RegisterEmpleado from './components/RegisterEmpleado.vue';
import LogINE from './components/LogInEmpleadores.vue';
import LogE from './components/LoginEmpleado.vue';

const router = createRouter({
  history: createWebHistory(),
  routes: [
    { path: "/", name: "LoginRegister", component: LoginRegister },
    {path: "/PageEmpresaAdmin", name: "PageEmpresaAdmin", component: PageEmpresaAdmin },
    { path: "/FormEmpresa", name: "FormEmpresa", component: FormEmpresa },
    { path: "/LogINEmpleadores", name: "LogInE", component: LogINE },
    { path: "/LogInEmpleado", name: "LogE", component: LogE },
    { path: "/home", name: "Home", component: Homepage },
    { path: "/RegEmpleado", name: "RegEmpleado", component: RegisterEmpleado },
  ],
});

createApp(App).use(router).mount("#app");
