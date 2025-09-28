import { createApp } from 'vue'
import App from './App.vue'
import { createRouter, createWebHistory } from "vue-router";
import CountriesList from './components/CountriesList.vue';
import CountryForm from "./components/CountryForm.vue";
import LoginRegister from './components/LoginRegister.vue';
import FormEmpresa from './components/FormEmpresa.vue'; 
import LogINE from './components/LogInEmpleadores.vue';

const router = createRouter({
  history: createWebHistory(),
  routes: [
    { path: "/", name: "LoginRegister", component: LoginRegister },
    { path: "/country", name: "Country", component: CountryForm },
    { path: "/countries", name: "Countries", component: CountriesList },
    { path: "/FormEmpresa", name: "FormEmpresa", component: FormEmpresa },
    { path: "/LogINEmpleadores", name: "LogInE", component: LogINE },
  ],
});

createApp(App).use(router).mount("#app");
