<template>
    <div class="wrap">
        <header class="header">
            <div class="header-content">
                <div class="display">
                    <div class="logo-box">
                        <span class="f">F</span>
                    </div>
                    <div class="texts">
                        <h1 v-if="companyName">{{companyName}}</h1>
                        <p>{{employeeName}}</p>
                    </div>
                </div>

                <button @click="volverAtras" class="btn-volver">
                    ← Volver
                </button>

                <!-- Texto alineado a la derecha -->
                <div class="dash-right">
                    <h2>Dashboard Empleador</h2>
                </div>
            </div>
        </header>

        <section class="hero">
           
            <div class="big-container">
                <div class="img-grid">
                    
                    <div class="img-box">
                        <img v-bind:src="imageDataEmployeesRoleCount" alt="Gráfico de empleados por tipo" />
                    </div>

                    
                    <div class="img-box">
                        <div class="table-container">
                            <h3 class="table-title">&Uacute;ltimos pagos</h3>
                            <table class="empresas-table">
                                <thead>
                                    <tr>
                                        <th>Planillas</th>
                                        <th>Fecha</th>
                                        <th>Costo total</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    <tr>
                                        <td>{{empresas.primeraEmpresa}}</td>
                                        <td>{{planillas.primeraPlanilla}}</td>
                                        <td>{{costo.primerCosto}}</td>
                                    </tr>
                                    <tr>
                                        <td>{{empresas.segundaEmpresa}}</td>
                                        <td>{{planillas.segundaPlanilla}}</td>
                                        <td>{{costo.segundoCosto}}</td>
                                    </tr>
                                    <tr>
                                        <td>{{empresas.terceraEmpresa}}</td>
                                        <td>{{planillas.terceraPlanilla}}</td>
                                        <td>{{costo.tercerCosto}}</td>
                                    </tr>
                                </tbody>
                            </table>
                        </div>
                    </div>
                </div>
            </div>

            
            <div class="big-container">
                <div class="single-image-container">
                    <img v-bind:src="imageDataCakeChart" alt="Gráfico principal" class="main-image" />
                </div>
            </div>
        </section>
    </div>
</template>

<script>
    import axios from "axios";
    import { API_ENDPOINTS } from "../config/apiConfig";

    export default {
        name: "DashboardEmpleadoImagenes",
        data() {
            return {
                companyId: "",
                imageDataEmployeesRoleCount: "",
                employeeName: "",
                companyName: "",
                hoursList: "",
                imageDataCakeChart: "",
                planillas: {
                    primeraPlanilla: "",
                    segundaPlanilla: "",
                    terceraPlanilla: "",
                },
                empresas: {
                    primeraEmpresa: "",
                    segundaEmpresa: "",
                    terceraEmpresa: "",
                },
                costo: {
                    primerCosto: "",
                    segundoCosto: "",
                    tererCosto: "",
                },
                fecha: "",
            };
        },
        async mounted() {
            this.getCompanyData();
            this.setLocalData();
            await this.serveImages();
            await this.getHoursList();
            await this.displayTableData();
            await this.serveImageCake();
        },
        methods: {
            volverAtras() {
                this.$router.push('/Profile');
            },
            getCompanyData() {
                let data = JSON.parse(localStorage.getItem("selectedCompany"));
                this.companyId = data.cedulaJuridica;
                this.companyName = data.nombre;
            },

            async getHoursList() {
                const fecha = new Date();

                const yy = fecha.getFullYear(); 
                const mm = String(fecha.getMonth() + 1).padStart(2, '0');
                const dd = String(fecha.getDate()).padStart(2, '0');

                const formato = `${yy}-${mm}-${dd}`;

                this.fecha = formato;

                const getHoursUrl = API_ENDPOINTS.EMPRESA_PAY_DATES(this.companyId, formato);
                const result = await axios.get(getHoursUrl);

                this.hoursList = result.data;
                
            },
            setLocalData() {
                let data = JSON.parse(localStorage.getItem("userData"));
                let completeName = data.firstName + " " + data.secondName;
                this.employeeName = completeName;
            },
            async serveImageCake() {
                if (this.hoursList[0] == null) {
                    this.imageDataCakeChart = new URL('@/assets/NotFound.png', import.meta.url).href
                    return;
                }

                const fechaSimple = this.hoursList[0].split("T")[0];
                try {
                    const imageUrl = API_ENDPOINTS.CAKE_GRAPH(this.companyId,fechaSimple)
                    const response = await axios.get(imageUrl, {
                        responseType: 'arraybuffer'
                    });

                    console.log("Datos recibidos:", response.data);
                    console.log("Tipo de datos:", typeof response.data);

                    const bytes = new Uint8Array(response.data);
                    const base64String = btoa(
                        bytes.reduce((acc, byte) => acc + String.fromCharCode(byte), "")
                    );

                    this.imageDataCakeChart = `data:image/png;base64,${base64String}`;
                    console.log("Imagen convertida correctamente");
                } catch (error) {
                    console.error("Error al cargar la imagen:", error);
                }
            }, 
            async displayTableData() {
                if (this.hoursList[0] != null) {

                    this.planillas.primeraPlanilla = this.hoursList[0].split("T")[0];
                    this.empresas.primeraEmpresa = this.companyName;
                    const fechaSimple = this.hoursList[0].split("T")[0];

                    const URL = API_ENDPOINTS.SPREADSHEET_COST(this.companyId, fechaSimple);

                    const response = await axios.get(URL);

                    this.costo.primerCosto = response.data + "₡";
                }
                if (this.hoursList[1] != null) {
                    this.planillas.segundaPlanilla = this.hoursList[1].split("T")[0];
                    this.empresas.segundaEmpresa = this.companyName;

                    const fechaSimple = this.hoursList[1].split("T")[0];

                    const URL = API_ENDPOINTS.SPREADSHEET_COST(this.companyId, fechaSimple);

                    const response = await axios.get(URL);

                    this.costo.segundoCosto = response.data + "₡";
                } 
  
                if (this.hoursList[2] != null) {
                    this.planillas.terceraPlanilla = this.hoursList[2].split("T")[0];
                    this.empresas.terceraEmpresa = this.companyName;
                    const fechaSimple = this.hoursList[1].split("T")[0];

                    const URL = API_ENDPOINTS.SPREADSHEET_COST(this.companyId, fechaSimple);

                    const response = await axios.get(URL);

                    this.costo.tercerCosto = response.data + "₡";
                }  
            },


            async serveImages() {
                await this.serveRoleCountImage();
            },

            async serveRoleCountImage() {
                console.log(this.companyId);
                try {
                    const imageUrl = API_ENDPOINTS.EMPRESA_COUNT_ROLES(this.companyId,);
                    const response = await axios.get(imageUrl, {
                        responseType: 'arraybuffer'
                    });

                    console.log("Datos recibidos:", response.data);
                    console.log("Tipo de datos:", typeof response.data);

                    const bytes = new Uint8Array(response.data);
                    const base64String = btoa(
                        bytes.reduce((acc, byte) => acc + String.fromCharCode(byte), "")
                    );

                    this.imageDataEmployeesRoleCount = `data:image/png;base64,${base64String}`;
                    console.log("Imagen convertida correctamente");
                } catch (error) {
                    console.error("Error al cargar la imagen:", error);
                }
            },
        }

    };
</script>

<style src="@/assets/style/DashboardEmpleador.css" scoped></style>
<style scoped>
.btn-volver {
  background: linear-gradient(135deg, #1fb9b4, #51a3a0);
  color: white;
  border: none;
  padding: 10px 20px;
  border-radius: 8px;
  font-size: 16px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s ease;
  box-shadow: 0 2px 8px rgba(31, 185, 180, 0.3);
}

.btn-volver:hover {
  background: linear-gradient(135deg, #51a3a0, #1fb9b4);
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(31, 185, 180, 0.5);
}

.btn-volver:active {
  transform: translateY(0);
}
</style>