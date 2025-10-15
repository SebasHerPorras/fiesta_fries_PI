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
            <form id="EmployerLogIn" @submit.prevent="handleSubmit" @reset="handleReset">
                <h2 style="color: #eee; margin: 0 0 20px; font-weight: 600; font-size: 18px; text-align: center;">Formulario Empleado</h2>

                <div class="field-group">
                    <label class="input">
                        <input type="text"
                               id="Name"
                               v-model="form.firstName"
                               placeholder="👤 Nombre*"
                               required />
                    </label>
                    <div v-if="firstNameError" class="error-msg">{{ firstNameError }}</div>
                </div>

                <div class="field-group">
                    <label class="input">
                        <input type="text"
                               id="SecondNames"
                               v-model="form.secondName"
                               placeholder="👥 Apellidos*"
                               required />
                    </label>
                    <div v-if="secondNameError" class="error-msg">{{ secondNameError }}</div>
                </div>

                <div class="field-group">
                    <label class="input">
                        <input type="text"
                               id="Id"
                               v-model="form.id"
                               placeholder="🆔 Cédula*"
                               required />
                    </label>
                    <div v-show="idError" class="error-msg">{{ idError }}</div>
                    <div v-show="idFormatError" class="error-msg">{{ idFormatError }}</div>

                </div>

                <div class="field-group">
                    <label class="input">
                        <input type="date"
                               v-model="form.birthdate"
                               id="BirthDate"
                               required />
                    </label>
                    <div v-show="birthdateError" class="error-msg">{{ birthdateError }}</div>
                </div>

                <div class="field-group">
                    <label class="input">
                        <input type="text"
                               id="email"
                               v-model="form.email"
                               placeholder="📧 Email*"
                               required />
                    </label>
                    <div v-show="emailError" class="error-msg">{{ emailError }}</div>
                </div>


                <div class="field-group">
                    <label class="input">
                        <input type="text"
                               v-model="form.position"
                               placeholder="👔 Puesto*"
                               required />
                    </label>

                    <div v-show="positionError" class="error-msg">{{ positionError }}</div>
                </div>

                <div class="field-group">
                    <label class="input">
                        <select v-model="form.employmentType" required>
                            <option disabled value="">Tipo de Contrato</option>
                            <option value="Tiempo Completo">Tiempo Completo</option>
                            <option value="Medio Tiempo">Medio Tiempo</option>
                            <option value="Por Horas">Por Horas</option>
                        </select>
                    </label>
                </div>

                <div class="field-group">
                    <div class="input">
                        <select id="role" name="role" v-model="payMethod" required>
                            <option value="" disabled selected style="color: #ece6e6ff">Método de pago</option>
                            <option value="cash" style="color: #ece6e6ff"> Efectivo</option>
                            <option value=" bankAccount" style="color: #ece6e6ff">Cuenta de Banco</option>
                        </select>
                    </div>
                </div>

                <div class="field-group">
                    <label class="input">
                        <input type="text" v-model="form.departament" placeholder="🧑‍💼 Departamento*" required />
                    </label>
                    <div v-show="departamentError" class="error-msg">{{ departamentError }}</div>
                </div>

                <div class="field-group">
                    <label class="input">
                        <input type="text"
                               v-model="form.salary"
                               placeholder="💵 Salario Colones*"
                               required />
                    </label>
                    <div v-show="salaryError" class="error-msg">{{ salaryError }}</div>
                </div>

                <div class="field-group">
                    <label class="input">
                        <input type="tel"
                               id="Phone_Number"
                               v-model="form.personalPhone"
                               placeholder="📱 Teléfono*"
                               required />
                    </label>
                    <div v-show="numberError" class="error-msg">{{numberError}}</div>
                </div>

                <div class="field-group">
                    <label class="input">
                        <input type="text"
                               id="Home Number"
                               v-model="form.homePhone"
                               placeholder="☎ Teléfono casa" />
                    </label>

                    <div v-show="secondPhoneError" class="error-msg">{{secondPhoneError}}</div>
                </div>

                <div class="field-group">
                    <label class="input">
                        <input type="password"
                               id="Password"
                               v-model="form.password"
                               placeholder="🔒 Contraseña*"
                               required />
                    </label>
                    <div v-show="passwordError" class="error-msg">{{ passwordError }}</div>
                </div>

                <div class="field-group">
                    <label class="input">
                        <input type="password"
                               id="Password_Confirm"
                               v-model="form.passwordConfirm"
                               placeholder="🔒 Confirmar Contraseña*"
                               required />
                    </label>
                    <div v-show="passwordConfirmationError" class="error-msg">
                        {{ passwordConfirmationError }}
                    </div>

                </div>

                <div class="field-group">

                    <label class="input">
                        <input type="text"
                               v-model="form.direction"
                               id="Direction"
                               placeholder="📍 Dirección*"
                               required />
                    </label>
                    <div v-if="directionError" class="error-msg">{{ directionError }}</div>
                </div>

                <div class="buttons-row">
                    <button class="btn btn-secondary" @click="volverAlHome"> ← Volver</button>
                    <button class="btn btn-primary" type="submit">Crear</button>
                </div>

                <div class="field-group">
                    <div class="message"
                         v-if="successMessage"
                         :class="{ 'error': !isError, 'succes': isError }">
                        {{ successMessage }}
                    </div>
                </div>
</form>
        </main>

        <!-- Aquí vamo a dejar el footer -->
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
    import axios from 'axios';

    export default {
        name: "RegistrarEmpleado",
        data() {
            return {
                userName: "Cargando...",
                userRole: "",
                companies: [],
                selectedCompany: null,
                form: {
                    email: "",
                    position: "",
                    employmentType: "",
                    salary: "",
                    hireDate: "",
                    departament: "",
                    idCompny: "",
                    uniqueUser: "",
                    id: "",
                    firstName: "",
                    secondName: "",
                    personalPhone: "",
                    homePhone: "",
                    birthdate: "",
                    personType: "Empleado",
                    direction: "",
                    password: "",
                    passwordConfirmation:"",
                },
                emailError: "",
                payMethod: "",
                passwordError: "",
                firstNameError: "",
                birthdateError: "",
                idError: "",
                directionError: "",
                passwordConfirmationError: "",
                email: "",
                workstation: "",
                employmentType: "",
                idFormatError: "",
                numberError: "",
                salaryError: "",
                date: "",
                idC: "",
                departament: "",
                secondPhoneError: "",
                positionError: "",
                departamentError: "",
                secondNameError: "",
                successMessage: "",
                isError: false,
            };
        },
        mounted() {
            this.loadUserFromLocalStorage();
            this.obtenerEmpresaSeleccionada();
        },
        methods: {
            volverAlHome() {
              this.$router.go(-1)
            },
            succesMessageD(message) {
                this.successMessage = message;
            },
            validateSecondName() {
                if (this.form.secondName.length > 30) {
                    const message = "Los apellidos no pueden exceder los 30 carácteres";
                    this.showSecondNameError(message);
                    return false;
                }
                return true;
            },
            showSecondNameError(message) {
                this.secondNameError = message;
            },
            validatePosition() {
                if (this.form.position.length > 30) {
                    const message = "La longitud del puesto no debe de exceder los 30 carácteres";
                    this.showPositionErrorMessage(message);
                    return false;
                }

                return true;
            },
            showPositionErrorMessage(message) {
                this.positionError = message;
            },
            validateDepartamentError() {
                if (this.form.departament.length > 30) {
                    const message = "La longitud del departamento no debe de exceder los 30 caracteres";
                    this.showDepartamentError(message);
                    return false;
                }
                return true;
            },
            showDepartamentError(message) {
                this.departamentError = message;
            },
            clearAll() {
                this.form.email = "";
                this.form.position = "";
                this.form.employmentType = "";
                this.form.salary = "";
                this.form.hireDate = "";
                this.form.departament = "";
                this.form.idCompny = "";
                this.form.uniqueUser = "";
                this.form.id = "";
                this.form.firstName = "";
                this.form.secondName = "",
                this.form.personalPhone = "";
                this.form.homePhone = "";
                this.form.birthdate = "";
                this.form.personType = "Empleado";
                this.form.direction = "";
                this.form.password = "";
                this.form.passwordConfirmation = "";
            },
            validateNumberFormat() {
                if ((this.form.personalPhone && /^\d+$/.test(this.form.personalPhone)) && (this.form.personalPhone.length === 8)) {

                    return true;
                }
                return false;
            },
            validateSecondNumberFormat() {
                if ((this.form.homePhone && /^\d+$/.test(this.form.homePhone)) && (this.form.homePhone.length === 8)){
                   return true;
                }
                return false;
            },
            validateSalary() {
                console.log(this.form.salary);
                if (!isNaN(this.form.salary)) {
                    const salaryN = parseInt(this.form.salary);
                    if ((salaryN >= 368000) ) {
                        return true;
                    }else {
                      const message = "El salario no es permitido ya que no cumple con lo establecido con la ley costarricense";
                       this.showErrorSalary(message);
                        return false;
                    }
                }else {
                  const message = "El salario debe de ser un valor númerico";
                   this.showErrorSalary(message);
                    return false;
                }
            },
        showErrorSalary(message) {
            this.salaryError = message;
        },
        ShowNumberError(message) {
                this.numberError = message;
         },
        showSecondNumberError(message) {
          this.secondPhoneError = message;
        },
            validateIDFormat() {
                const id = this.form.id;

                if (id && /^\d+$/.test(id) && (id.length === 9 || id.length === 12)) {
                    return true;
                }
                return false;
            },
            ShowIDFomratError(message) {
                this.idFormatError = message;
            },
            getDepartament() {
                const get = this.$route.query.departamento || "";
                this.departament = get;
                console.log(this.departament);
            },
            getIdcrl() {
                const get = this.$route.query.idC || "";
                this.idC = get;
                console.log(this.idC);
            },
            getDateUrl() {
                const getDateFromUrl = this.$route.query.fechaC || "";
                this.date = getDateFromUrl;
                console.log(this.date);
            },
            getSalaryUrl() {
                const getSalaryUrl = this.$route.query.salario || "";
                this.salary = getSalaryUrl;
                console.log(this.salary);
            },
            getType() {
                const getTypeUrl = this.$route.query.tipoEmpleo || "";
                this.employmentType = getTypeUrl;
                console.log(getTypeUrl);
            },
            getEmailUrl() {
                const getEmailFromUrl = this.$route.query.email || "";
                this.email = getEmailFromUrl;
                console.log(this.email);
            },
            getPuestoURL() {
                const getPuestoFromUrl = this.$route.query.puesto || "";
                this.workstation = getPuestoFromUrl;
                console.log(getPuestoFromUrl);
            },
            validatePassword(password) {
                const regex = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,16}$/;
                return regex.test(password);
            },
            showpasswordError(message) {
                this.passwordError = message;
            },
            validateConfirmationPassword(firstPassword, secondPassword) {
                if (firstPassword == secondPassword) {
                    return true;
                } else {
                    return false;
                }
            },
            showpasswordConfirmationError(message) {
                this.passwordConfirmationError = message;
            },
            isAdult(birthDate) {
                //El día de hoy

                const today = new Date();
                const birth = new Date(birthDate);
                console.log(today);
                console.log(birth);
                let age = today.getFullYear() - birth.getFullYear();
                const m = today.getMonth() - birth.getMonth();
                if (m < 0 || (m === 0 && today.getDate() < birth.getDate())) {
                    age--;
                }
                console.log(age);
                return age >= 18;

            },
            clearErrors() {
                this.passwordError = "";
                this.firstNameError = "";
                this.birthdateError = "";
                this.idError = "";
                this.directionError = "";
                this.passwordConfirmationError = "";
                this.idFormatError = "";
                this.emailError = "";
                this.numberError = "";
                this.salaryError = "";
                this.secondPhoneError = "";
                this.secondPhoneError = "";
                this.departamentError = "";
                this.positionError = "";
            },
            showEmailError(message) {
                this.emailError = message;
            },
            showbirthdateError(message) {
                this.birthdateError = message;
            },
            validateDirectionLength() {
                return this.form.direction.length < 148;
            },
            showDirectionError(message) {
                this.directionError = message;
            },
            async validateID() {
                // tengo que llamar aquí a la api
                const validateidurl = "http://localhost:5081/api/idverification/idvalidate";
                console.log("Entra aquí jijij");
                let ageInt = parseInt(this.form.id, 10);
                console.log(ageInt);
                const response = await axios.post(validateidurl, ageInt, { headers: { "Content-Type": "application/json" } });
                console.log("pasa de aquí\n");

                console.log(response);
                if (response.data.result) {
                    // tengo que llamar al método que haga return ojito
                    this.showidError();

                    return false;
                }

                return true;
            },
            showidError() {
                this.idError = "Este id ya está registrado, ingrse otro";
            },

            isNameValid(name) {
                name = name.trim();
                if (name.length > 30) {
                    const messageL = "El nombre no debe de exceder una longitud de 30 caracteres";
                    this.showfisrtNameError(messageL);
                    return false;
                } else if (name.length< 2) {
                    const message = "El nombre debe de contener al menos 2 caracteres";
                    this.showfisrtNameError(message);
                    return false;
                }
                return true;
                
            },
            showfisrtNameError(errorMessage) {
                this.firstNameError = errorMessage;
            },

            obtenerEmpresaSeleccionada() {
                const empresaData = localStorage.getItem('selectedCompany');
                if (empresaData) {
                    try {
                        this.selectedCompany = JSON.parse(empresaData);
                        console.log('Empresa seleccionada:', this.selectedCompany);
                    } catch (error) {
                        console.error('Error al parsear empresa seleccionada:', error);
                        this.mostrarError('Error al obtener empresa seleccionada');
                    }
                } else {
                    this.mostrarError('No hay empresa seleccionada. Volviendo a Inicio...');
                    setTimeout(() => {
                        this.$router.push('/Profile');
                    }, 2000);
                }
            },
            loadUserFromLocalStorage() {
                const stored = localStorage.getItem("userData");
                if (!stored) {
                    this.$router.push("/"); // si no hay sesión, redirige
                    return;
                }
                const userData = JSON.parse(stored);

                this.userName = `${userData.firstName} ${userData.secondName}`;
                this.userRole = userData.personType; // Ej: "Empleador" o "Empleado"
            },
            async validateEmail() {
                const validateEmailUrl = `http://localhost:5081/api/user/emailverify?email=${encodeURIComponent(this.form.email)}`;

                const response = await axios.get(validateEmailUrl);

                console.log("Respuesta importante\n");
                console.log(response.data.result)

                return response.data.result === true;

            },
            async handleSubmit() {
                this.clearErrors();
                this.validateEmail();
                if (this.emailError) return;

                console.log("Datos del empleado a registrar:", this.form);

                // Simulación de guardado
                let empleados = JSON.parse(localStorage.getItem("empleados")) || [];
                empleados.push(this.form);
                localStorage.setItem("empleados", JSON.stringify(empleados));
                console.log("Entra");

                try {
                    this.clearErrors();
                    this.isError = false;

                     const userData = {
                       Email: this.form.email.trim(),
                       PasswordHash: this.form.password
                    };

                    let status = true;
                    const emailVEvent = await this.validateEmail();
                    if (!emailVEvent) {
                        console.log("Entra aquí\n");
                        this.showEmailError("Este correo elctrónico ya está registrado, ingrese otro");
                        status = false;
                    }

                    if (!this.validateSalary()) {
                        status = false;
                    }

                    if (!this.validateSecondNumberFormat()) {
                        this.showSecondNumberError("El formato del teléfono no es válido");
                        status = false;
                    }

                    if (!this.validateIDFormat()) {
                        console.log("Entra aquí pepe\n");
                        this.ShowIDFomratError("El formato de la cédula no es correcto");
                        status = false;
                    }


                    if (!this.validateNumberFormat()) {
                        this.ShowNumberError("Número no sigue el formato adecuado");
                        status = false;
                    }

                    if (!(this.isNameValid(this.form.firstName))) {
                        status = false;
                    }

                    const idVerification = await this.validateID();
                    if (!idVerification) {
                        status = false;
                    }

                    if (!(this.isAdult(this.form.birthdate))) {
                        this.showbirthdateError('La fecha de nacimiento es inválida, debes de ser mayor de edad para registrarte en nuestra plataforma');
                        console.log("Entra aquí4\n");
                        status = false;
                    }
                    if (!this.validateDirectionLength()) {
                        this.showDirectionError("La dirección no puede exceder los 200 carácteres");
                        status = false;
                    }

                    if (!this.validatePassword(this.form.password)) {
                        this.showpasswordError('La contraseña no cumple con el formato esperado, mínimo 8 caracteres, max 16) (Mínimo 1 char mayúscula, 1 char mínúscula, 1 char especial) ');
                        status = false;
                    }

                    if (!this.validateConfirmationPassword(this.form.password, this.form.passwordConfirm)) {
                        this.showpasswordConfirmationError("La contraseña debe de coincidir con la original");
                        status = false;
                    }

                    if (!this.validateDepartamentError()) {
                        status = false;
                    }

                    if (!this.validateSecondName()) {
                        status = false;
                    }

                    if (!this.validatePosition()) {
                        status = false;
                    }

                    if (!status) {
                        const error_message = "Ocurrió un error Verifique los mensajes de error e intentelo de nuevo";
                        this.succesMessageD(error_message);
                        setTimeout(() => {
                            this.succesMessageD("");
                        }, 4000);
                        return;
                    }

                    this.clearErrors();

                    const userUrl = "http://localhost:5081/api/user/createEmployer";

                    let userResponse = await axios.post(userUrl, userData);

                    let userID = userResponse.data.id;

                    const personData = {
                        uniqueUser: userID,
                        id: this.form.id,
                        firstName: this.form.firstName,
                        secondName: this.form.secondName,
                        email: this.form.email,
                        personalPhone: this.form.personalPhone,
                        homePhone: this.form.homePhone,
                        birthdate: this.form.birthdate,
                        personType: "Empleado",
                        direction: this.form.direction,
                    };
                    const personUrl = "http://localhost:5081/api/person/create";

                    const persRes = await axios.post(personUrl, personData);
                    console.log(persRes.data);

                    const cId = this.selectedCompany.cedulaJuridica;

                    console.log(cId);

                    const fechaC = new Date().toISOString();

                    console.log(fechaC);

                    const empleado = {
                        personaId: parseInt(this.form.id),
                        firstName: this.form.firstName,
                        secondName: this.form.secondName,
                        birthdate: this.form.birthdate,
                        direction: this.form.direction,
                        personalPhone: this.form.personalPhone,
                        homePhone: this.form.homePhone,
                        personType: this.form.personType,
                        userEmail: this.form.email,
                        userPassword: this.form.password,
                        position: this.form.position,
                        employmentType: this.form.employmentType,
                        salary: parseInt(this.form.salary),
                        hireDate: fechaC,
                        idCompny: parseInt(cId),
                        departament: this.form.departament,
                    };

                    const EmpleadoUrl = "http://localhost:5081/api/Empleado/create-with-person";

                    const EmpleadoRes = await axios.post(EmpleadoUrl, empleado);
                    console.log("Usuario y Persona creados correctamente:");
                    console.log("Persona:", EmpleadoRes.data);
                    this.isError = true;
                    const SuccessM = "El Empleado fue registrado con éxito";
                    this.succesMessageD(SuccessM);

                    setTimeout(() => {
                        this.volverAlHome();
                    }, 2000);

                    this.clearAll();

                } catch (error) {
                    console.log("Error Creando al nuevo empleador", error);
                }
            },

            onCompanyChange() {
                if (this.selectedCompany) {
                    this.saveSelectedCompany();
                    // Redirigir a la página de administración de empresas
                    this.$router.push('/PageEmpresaAdmin');
                }
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

    .brand {
        display: flex;
        align-items: center;
        gap: 18px;
        max-width: 55%;
        margin-bottom: 150px;
        margin-top: 220px;
    }

    #EmployerLogIn {
        width: 400px;
        min-height: 220px;
        background: rgb(71, 69, 69);
        border: 1px solid rgba(255, 255, 255, 0.15);
        padding: 25px;
        border-radius: 8px;
        box-shadow: 0 6px 18px rgba(0, 0, 0, 0.35);
    }


    .buttons-row {
        display: flex;
        gap: 12px;
        margin-top: 10px;
    }

        .buttons-row .btn {
            width: 50%;
            padding: 10px 12px;
            font-size: 14px;
        }

    .btn-primary {
        background: #1fb9b4;
        color: white;
    }

        .btn-primary:hover:not(:disabled) {
            background: #1aa8a4;
        }

    .btn-secondary {
        background: #6c757d;
        color: white;
    }

    .btn-secondary:hover:not(:disabled) {
            background: #5a6268;
    }


    .btn {
        border-radius: 6px;
        border: 0;
        font-weight: 600;
        cursor: pointer;
        transition: background 0.3s;
        text-align: center;
    }

        .btn:disabled {
            opacity: 0.6;
            cursor: not-allowed;
        }

    .logo-box {
        width: 84px;
        height: 84px;
        background: linear-gradient(180deg, #51a3a0, hsl(178, 77%, 86%));
        border-radius: 16px;
        display: flex;
        align-items: center;
        justify-content: center;
        flex-shrink: 0;
    }

        .logo-box .f {
            font-weight: 800;
            font-size: 44px;
            color: white;
        }

    .texts h1 {
        margin: 0;
        font-size: 34px;
    }

    .texts p {
        margin: 6px 0 0;
        color: #bdbdbd;
    }

    .register-card {
        width: 410px;
        background: rgb(71, 69, 69);
        border: 1px solid rgba(255, 255, 255, 0.15);
        padding: 24px;
        border-radius: 10px;
        box-shadow: 0 6px 18px rgba(0, 0, 0, 0.35);
        height: 800px;
    }

        .register-card h2 {
            color: #eee;
            margin: 0 0 16px;
            font-weight: 600;
            font-size: 18px;
            text-align: center;
        }

    .input {
        display: flex;
        align-items: center;
        padding: 10px 12px;
        border-radius: 6px;
        background: rgba(0, 0, 0, 0.25);
        border: 1px solid rgba(255, 255, 255, 0.06);
        margin-bottom: 30px;
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

    footer {
        background: #fff;
        padding: 28px 64px;
        border-top: 1px solid #eee;
        color: #8b8b8b;
        display: flex;
        align-items: center;
        justify-content: space-between;
    }

    .socials {
        display: flex;
        gap: 12px;
    }

    .message {
        margin-top: 15px;
        text-align: center;
        font-size: 14px;
        padding: 0;
        color: #9fe6cf;
    }

        .message.success {
            color: #9fe6cf;
        }
        .message.error {
            color: #ff6b6b;
        }
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

    .error-msg {
        margin-top: 6px; 
        color: #ff6b6b;
        font-size: 13px;
        text-align: left;
        min-height: 16px;
        transition: opacity 0.3s ease;
    }

    .field-group {
        margin-bottom: 30px; 
    }
    .input {
        margin-bottom: 0;
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