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

      <form id="EmployerLogIn" @submit.prevent="handleSubmit">
          <h2 style="color: #eee; margin: 0 0 20px; font-weight: 600; font-size: 18px; text-align: center;">
              Añadir Horas
          </h2>

          <div class="field-group">
              <label class="input">
                  <input type="week" id="semana" v-model="form.selectedWeek" @change="onWeekSelected" />
              </label>
          </div>

          <div class="field-group">
              <div class="message" v-if="form.weekHoursLabel">{{form.weekHoursLabel}} </div>
          </div>

          <div class="field-group">
              <div class="error-msg" v-if="weekHoursErrorMessage">{{weekHoursErrorMessage}} </div>
          </div>

          <div class="field-group">
              <div class="input">
                  <select id="role" name="role" v-model="form.selectedDay" required :disabled="!form.selectedWeek || isErrorWeek" @change="onDaySelected">
                      <option value="" disabled selected style="color: #ece6e6ff">Día de la semana</option>
                      <option value="Lunes" style="color: #ece6e6ff"> Lunes</option>
                      <option value="Martes" style="color: #ece6e6ff">Martes</option>
                      <option value="Miercoles" style="color: #ece6e6ff">Miércoles</option>
                      <option value="Jueves" style="color: #ece6e6ff">Jueves</option>
                      <option value="Viernes" style="color: #ece6e6ff">Viernes</option>
                  </select>
              </div>
          </div>

          <div class="field-group">
              <div class="message" v-if="form.daysHoursLabel">{{form.daysHoursLabel}} </div>
          </div>

          <div class="field-group">
              <div class="error-msg" v-if="DaysHoursErrorMessage">{{DaysHoursErrorMessage}} </div>
          </div>

          <div class="field-group">
              <label class="input">
                  <select id="hoursCount" name="hoursCount" v-model="form.hoursCount" required :disabled="!form.selectedDay || isErrorHours">
                      <option id="defaultOption" value="" disabled selected style="color: #ece6e6ff">Cantidad Horas</option>
                  </select>
              </label>

          </div>

          <div class="field-group">
              <div class="message" v-if="form.weekHoursMessage">{{form.weekHoursMessage}} </div>
          </div>

          <div class="buttons-row">
              <button class="btn btn-secondary" @click="getBacktoHome"> ← Volver</button>
              <button class="btn btn-primary" type="submit">Añadir</button>
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
import { API_ENDPOINTS } from "../config/apiConfig";
import axios from "axios";

export default {
        name: "RegisterHoras",
        data() {
            return {
                employeeHireDate: "",
                employeeId: "",
                weekHours: "",
                daysHours: "",
                isErrorWeek: true,
                weekHoursCount: "",
                weekHoursErrorMessage: "",
                isErrorDay: true,
                DaysHoursErrorMessage: "",
                isErrorHours: "",
                form: {
                    weekHoursMessage: "",
                    selectedDay: "",
                    selectedWeek: "",
                    weekStartDate: "",
                    selectedDate: "",
                    hoursCount: "",
                    daysHoursLabel: "",
                    weekHoursLabel: "",
                }
            };
        },
        mounted() {
            this.getEmployeeId();
            this.getEmployeeHireDate().then(() => {
                this.makeWeekCalc();
            });
        },

        methods: {
            getEmployeeId() {
                let dataTemp = localStorage.getItem("userData");
                let objectTemp = JSON.parse(dataTemp);
                this.employeeId = objectTemp.personaId;
            },

            getBacktoHome() {
                this.$router.go(-1)
            },

            async getEmployeeHireDate() {
                const dateUrl = API_ENDPOINTS.HIRE_DATE(this.employeeId);
                const response = await axios.get(dateUrl);
                let dateTemp = response.data;
                dateTemp = new Date(dateTemp);
                this.employeeHireDate = dateTemp.toISOString().split("T")[0];
            },

            makeWeekCalc() {
                const hireDate = new Date(this.employeeHireDate);
                const today = new Date();

                const hireWeek = this.getISOWeek(hireDate);
                const hireYear = hireDate.getFullYear();

                const currentWeek = this.getISOWeek(today);
                const currentYear = today.getFullYear();

                const inputWeek = document.getElementById("semana");

                inputWeek.min = `${hireYear}-W${String(hireWeek).padStart(2, "0")}`;
                inputWeek.max = `${currentYear}-W${String(currentWeek).padStart(2, "0")}`;
            },

            getISOWeek(date) {
                const tempDate = new Date(date.getTime());
                tempDate.setHours(0, 0, 0, 0);
                tempDate.setDate(tempDate.getDate() + 3 - ((tempDate.getDay() + 6) % 7));
                const week1 = new Date(tempDate.getFullYear(), 0, 4);
                return (1 + Math.round(((tempDate.getTime() - week1.getTime()) / 86400000 - 3 + ((week1.getDay() + 6) % 7)) / 7));
            },

            async onWeekSelected() {
                if (!this.form.selectedWeek) return;

                this.setWeekErrorMessage("");
                this.form.selectedDay = "";
                this.setDayErrorMessage("");
                this.isErrorWeek = true;

                const [year, week] = this.form.selectedWeek.split("-W").map(Number);
                const monday = this.getMondayOfWeek(year, week);
                this.form.weekStartDate = monday;

                await this.getWeekCount();
            },

            async getWeekCount() {
                const week_pivot = this.form.weekStartDate.toISOString().split("T")[0];
                this.weekHours = week_pivot;

                try {
                    const weekHoursUrl = API_ENDPOINTS.WORK_WEEK_HOURS(week_pivot, this.employeeId);
                    const response = await axios.get(weekHoursUrl);
                    this.weekHoursCount = response.data.hours_count;

                    if (!this.validateWeekHours()) {
                        this.setWeekErrorMessage("No se pueden agregar más horas a esta semana");
                        this.setWeekHourLabel("");
                        this.isErrorWeek = true;
                        return;
                    }

                    this.setWeekHourLabel("Cantiadad de horas registradas esta semana: " + this.weekHoursCount);
                    this.isErrorWeek = false;
                } catch (error) {
                    this.setWeekErrorMessage("Error al obtener horas de la semana");
                }
            },

            setWeekHourLabel(message) {
                this.form.weekHoursLabel = message;
            },

            validateWeekHours() {
                if (this.weekHoursCount >= 45) {
                    return false;
                }
                return true;
            },

            setWeekErrorMessage(message) {
                this.weekHoursErrorMessage = message;
            },

            async onDaySelected() {
                if (!this.form.selectedWeek || !this.form.selectedDay) return;

                this.setDayErrorMessage("");
                this.isErrorHours = true;

                const daysMap = {
                    Lunes: 0,
                    Martes: 1,
                    Miercoles: 2,
                    Jueves: 3,
                    Viernes: 4,
                };

                const offset = daysMap[this.form.selectedDay];
                const selectedDate = new Date(this.form.weekStartDate);
                selectedDate.setDate(selectedDate.getDate() + offset);
                this.form.selectedDate = selectedDate.toISOString().split("T")[0];

                if (!this.validateHireDate()) {
                    this.setHoursLabelContent("");
                    this.isErrorHours = true;
                    return;
                }

                await this.getDayHours();
            },

            setDayErrorMessage(message) {
                this.DaysHoursErrorMessage = message;
            },

            validateDaySelected() {
                if (this.daysHours >= 9) {
                    this.isErrorDay = true;
                    return true;
                }
                this.isErrorDay = false;
                return false;
            },

            validateHireDate() {
                const startDate = new Date(this.employeeHireDate);
                const selectedDate = new Date(this.form.selectedDate);
                startDate.setHours(0, 0, 0, 0);
                selectedDate.setHours(0, 0, 0, 0);

                const today = new Date();
                today.setHours(0, 0, 0, 0);

                if (selectedDate < startDate) {
                    this.setDayErrorMessage("No se pueden ingresar horas previas a la fecha de contratación");
                    return false;
                }

                if (selectedDate > today) {
                    this.setDayErrorMessage("No se pueden ingresar horas posteriores al día actual");
                    return false;
                }

                return true;
            },

            validateHoursDayCount() {
                if (this.daysHours >= 9) {
                    return false;
                }
                return true;
            },

            async getDayHours() {
                const urlDayHours = API_ENDPOINTS.WORK_DAY_HOURS(this.weekHours, this.form.selectedDate, this.employeeId);

                try {
                    const hoursResponse = await axios.get(urlDayHours);
                    this.daysHours = hoursResponse.data.hours_count;

                    if (!this.validateHoursDayCount()) {
                        this.setDayErrorMessage("Este día ya contiene la cantidad máxima de horas añadibles");
                        return;
                    }

                    this.setHoursLabelContent("Cantiadad de horas registradas en este día: " + this.daysHours);
                    this.isErrorHours = false;
                    this.AddHourOptions();
                } catch (error) {
                    this.setDayErrorMessage("Error al obtener horas del día");
                }
            },

            setHoursLabelContent(message) {
                this.form.daysHoursLabel = message;
            },

            AddHourOptions() {
                let hoursSelect = document.getElementById("hoursCount");
                let defaultSetting = document.getElementById("defaultOption");
                hoursSelect.innerHTML = "";
                hoursSelect.appendChild(defaultSetting);
                let hoursOptions = 9 - parseInt(this.daysHours);

                for (let i = 1; i <= hoursOptions; i++) {
                    let hourOption = document.createElement("option");
                    hourOption.value = i;
                    hourOption.textContent = i + " hora(s)";
                    hourOption.style.color = "#ece6e6ff";
                    hourOption.style.background = "#1e1e1e";
                    hoursSelect.appendChild(hourOption);
                }
            },

            getMondayOfWeek(year, week) {
                const simple = new Date(year, 0, 1 + (week - 1) * 7);
                const dow = simple.getDay();
                const monday = new Date(simple);
                const diff = (dow <= 4 ? dow - 1 : dow - 8);
                monday.setDate(simple.getDate() - diff);
                return monday;
            },

            async handleSubmit() {
                const urlAddHours = API_ENDPOINTS.ADD_WORK_DAY_HOURS(this.weekHours, this.form.selectedDate, this.form.hoursCount, this.employeeId);

                try {
                    await axios.get(urlAddHours);
                    this.setSuccessMessage();
                } catch (error) {
                    this.form.weekHoursMessage = "Error al registrar las horas";
                }
            },

            setSuccessMessage() {
                this.form.weekHoursMessage = "Horas Registradas con éxito!";

                setTimeout(() => {
                    this.clearPage();
                    this.form.weekHoursMessage = "";
                }, 2000);
            },

            clearPage() {
                this.setHoursLabelContent("");
                this.setWeekHourLabel("");
                this.form.selectedWeek = "";
                this.form.selectedDay = "";
                this.form.hoursCount = "";
            },
        }
};
</script>

<style scoped src="@/assets/style/RegisterHoursStyle.css"></style>