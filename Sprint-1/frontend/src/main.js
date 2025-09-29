import { createApp } from 'vue'
import App from './App.vue'
import { createRouter, createWebHistory } from "vue-router";
import CountriesList from './components/CountriesList.vue';
import CountryForm from "./components/CountryForm.vue";
import LoginRegister from './components/LoginRegister.vue';
import PageEmpresaAdmin from './components/PageEmpresaAdmin.vue'; 
import FormEmpresa from './components/FormEmpresa.vue'; 
import LogINE from './components/LogInEmpleadores.vue';
import LogE from './components/LoginEmpleado.vue';

const router = createRouter({
  history: createWebHistory(),
  routes: [
    { path: "/", name: "LoginRegister", component: LoginRegister },
    { path: "/country", name: "Country", component: CountryForm },
    { path: "/countries", name: "Countries", component: CountriesList },
    {path: "/PageEmpresaAdmin", name: "PageEmpresaAdmin", component: PageEmpresaAdmin },
    { path: "/FormEmpresa", name: "FormEmpresa", component: FormEmpresa },
    { path: "/LogINEmpleadores", name: "LogInE", component: LogINE },
    { path: "/LogInEmpleado", name: "LogE", component: LogE },
  ],
});

createApp(App).use(router).mount("#app");
