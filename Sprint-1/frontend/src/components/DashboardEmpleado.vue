<template>
    <meta charset="UTF-8">
  <div class="wrap">
    
    <header class="header">
      <nav class="nav">
        
        <div class="display">
          <div class="logo-box">
            <span class="f">F</span>
          </div>
          <div class="texts">
            <h1>{{userName}}</h1>
            <p>{{userRole}}</p>
          </div>
        </div>

        <button @click="volverAtras" class="btn-volver">
          ← Volver
        </button>

        <div class="dashboard-texts">
          <h2>Dashboard-Empleado</h2>
          <p>Fecha del &uacute;ltimo pago</p>
          <p v-if="lastPaymentDay">{{lastPaymentDay}}</p>
        </div>
      </nav>
    </header>

    
    <section class="hero">

     
      <div class="big-container">

  
  <div class="grid-container">

    
    <div class="grid-item"><p>Salario Bruto</p><p v-if="employeeSalary">{{employeeSalary}}₡</p></div>
    <div class="grid-item"><p >Banco Popular Empleado</p><p v-if="deductions.bancoPopular">{{deductions.bancoPopular}}₡</p></div>
    <div class="grid-item"><p>CSS Pensiones</p><p v-if="deductions.cssPensiones">{{deductions.cssPensiones}}₡</p></div>
    <div class="grid-item"><p>Impuesto sobre la renta</p><p v-if="deductions.isr">{{deductions.isr}}₡</p></div>

  
    <div class="grid-item"><p>CSS Salud Empleado</p><p v-if="deductions.cssSalud">{{deductions.cssSalud}}₡</p></div>
    <div class="grid-item"><p>Total deducciones</p><p v-if="totalDeductions">{{totalDeductions}}₡</p></div>
    <div class="grid-item"><p>Porcentaje retenido</p><p v-if="retainedPercentage">{{retainedPercentage}}%</p></div>
    <div class="grid-item"><p>Salario neto</p><p v-if="netSalary">{{netSalary}}₡</p></div>

  </div> 

</div>

       
      
      <div class="image-container">
          <img v-bind:src="imageData" alt="Imagen aquí" />
      </div>

    </section>
  </div>
</template>

<script>
 import axios from "axios";
 import { API_ENDPOINTS } from '../config/apiConfig';

 export default {
  name: "DashboardEmpleado",
        data() {
            return {
                deductions: {
                    bancoPopular: "",
                    cssPensiones: "",
                    cssSalud: "",
                    isr: ""
                },
                date: "",
                lastPaymentDay: "",
                totalDeductions: "",
                retainedPercentage: "",
                netSalary: "",
                employeeSalary: "",
                employeeId: "",
                deductionsMap: {
                    "Banco Popular Empleado": "bancoPopular",
                    "CCSS Pensiones Empleado (IVM)": "cssPensiones",
                    "CCSS Salud Empleado": "cssSalud",
                    "Impuesto sobre la Renta": "isr"
                },
                payrollData: "",
                imageData: "",
                userName: "",
                userRole: "",
            };
        },
        mounted() {
          this.loadUserData();
          this.getEmployeeId();

        },
        
        methods: {
         volverAtras() {
           this.$router.push('/Profile');
         },
         
         async getEmployeeId() {
              let dataTemp = localStorage.getItem("userData");
              let objectTemp = JSON.parse(dataTemp);
              this.employeeId = objectTemp.personaId;
                await this.servePayroll()
            },

         loadUserData() {
             const stored = localStorage.getItem("userData");
             let parsed = null;
             if (stored) {
                 try {
                     parsed = JSON.parse(stored);
                 } catch (e) {
                     console.error("loadUserData: error parsing userData", e);
                 }
             }
             if (parsed) {
                 this.userName = `${parsed.firstName || ""} ${parsed.secondName || ""}`.trim() || this.userName;
                 this.userRole = parsed.personType || parsed.role || this.userRole;
             }
         },
           async serveData() {
               await this.getPayroll();
               this.serveDeductionsLabels();
               await this.getSalaryData();
               this.serveSalaryLabels();
               await this.serveImage();
          },
          async servePayroll() {
               console.log(this.employeeId);
              const payrollUrl = API_ENDPOINTS.LAST_PAYROLL(this.employeeId);
              const response = await axios.get(payrollUrl);
              console.log(response.data);
              if (!response.data) { 
                  this.serveNonPayroll();
                  this.serveNonPayrrollImage();
                  return
              }
              let lastPayroll = response.data;
              lastPayroll = new Date(lastPayroll);
              this.lastPaymentDay = lastPayroll.toISOString().split("T")[0];
              this.serveData();
            },
          serveNonPayroll() {
              this.lastPaymentDay = "Aún no hay registro de pago";
              this.serveNonPayrollLabels(); 
          },

            serveNonPayrollLabels() {
              this.employeeSalary = "Aún no se procesa\n 0";
                this.deductions.bancoPopular = "Aún no se procesa\n 0";
                this.deductions.cssPensiones = "Aún no se procesa\n 0";
                this.deductions.isr = "Aún no se procesa\n 0";
                this.deductions.cssSalud = "Aún no se procesa\n 0";
                this.totalDeductions = "Aún no se procesa\n 0";
                this.retainedPercentage = "Aún no se procesa\n 0";
                this.netSalary = "Aún no se procesa\n 0";
          },
          serveNonPayrrollImage() {
              this.imageData = new URL('@/assets/logo.png', import.meta.url).href
          },
            async getPayroll() {

              const payrollUrl = API_ENDPOINTS.GET_PAYROLL(this.employeeId, this.lastPaymentDay);

              const Pdata = await axios.get(payrollUrl);

                this.payrollData = Pdata.data;
          },

          serveDeductionsLabels() {
                console.log(this.payrollData[0]);
                this.payrollData.forEach(item => {

                    const key = this.deductionsMap[item.deductionName];


                  if (key) {
                      this.deductions[key] = item.deductionAmount;
                  }

                  if (this.deductions.isr === ""){
                      this.deductions.isr = "No aplica\n 0";
                  }
              });
          },
          async getSalaryData() {
              const dashboardDataUrl = API_ENDPOINTS.GET_SALARY_DATA(this.employeeId, this.lastPaymentDay);
              const dataS = await axios.get(dashboardDataUrl);
              this.payrollData = dataS.data;
              console.log(this.payrollData);

          },
            serveSalaryLabels() {
                this.employeeSalary = this.payrollData.crudSalary;
                this.totalDeductions = this.payrollData.totalDeductions; 
                this.retainedPercentage = this.payrollData.reteinedPercentage;
                this.netSalary = this.payrollData.netSalary;
            },

            async serveImage() {
                try {
                    const imageUrl = API_ENDPOINTS.GET_IMAGE(this.employeeId, this.lastPaymentDay);

                    const response = await axios.get(imageUrl, {
                        responseType: 'arraybuffer'  
                    });

                    console.log("Datos recibidos:", response.data);
                    console.log("Tipo de datos:", typeof response.data);

                    const bytes = new Uint8Array(response.data);
                    const base64String = btoa(
                        bytes.reduce((acc, byte) => acc + String.fromCharCode(byte), "")
                    );

                    this.imageData = `data:image/png;base64,${base64String}`;
                    console.log("Imagen convertida correctamente");

                } catch (error) {
                    console.error("Error al cargar la imagen:", error);
             
                }
            }

      }

 };
</script>

<style src="@/assets/style/DashboardEmpleado.css" scoped></style>
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
