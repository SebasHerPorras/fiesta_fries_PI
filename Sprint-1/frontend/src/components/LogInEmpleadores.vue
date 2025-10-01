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

            <aside class="register-card">
                <h2>Formulario Empleador</h2>
                <form id="EmployerLogIn" @submit.prevent="handleSubmit" @reset="handleReset">
                    <label class="input">
                        <input type="text"
                               id="Name"
                               v-model="form.firstName"
                               placeholder="👤 Nombre"
                               required />
                    </label>
                    <div v-if="firstNameError" class="error-msg">{{ firstNameError }}</div>

                    <label class="input">
                        <input type="text"
                               id="SecondNames"
                               v-model="form.secondName"
                               placeholder="👥 Apellidos"
                               required />
                    </label>

                    <label class="input">
                        <input type="text"
                               id="Id"
                               v-model="form.id"
                               placeholder="🆔 Cédula"
                               required />
                    </label>
                    <div v-if="idError" class="error-msg">{{ idError }}</div>
                    <div v-if="idFormatError" class="error-msg">{{ idFormatError }}</div>

                    <label class="input">
                        <input type="email"
                               id="Email"
                               v-model="form.email"
                               placeholder="📧 Email"
                               required />

                    </label>
                    <div v-if="emailError" class="error-msg">{{ emailError }}</div>

                    <label class="input">
                        <input type="date"
                               v-model="form.birthdate"
                               id="BirthDate"
                               required />
                    </label>
                    <div v-if="birthdateError" class="error-msg">{{ birthdateError }}</div>

                    <label class="input">
                        <input type="tel"
                               id="Phone_Number"
                               v-model="form.personalPhone"
                               placeholder="📱 Teléfono"
                               required />
                    </label>

                    <div v-if="numberError" class="error-msg">{{numberError}}</div>

                    <label class="input">
                        <input type="text"
                               id="Home Number"
                               v-model="form.homePhone"
                               placeholder="☎ Teléfono casa" />
                    </label>

                   <div v-if="numberError" class="error-msg">{{numberError}}</div>

                    <label class="input">
                        <input type="password"
                               id="Password"
                               v-model="form.password"
                               placeholder="🔒 Contraseña"
                               required />
                    </label>
                    <div v-if="passwordError" class="error-msg">{{ passwordError }}</div>

                    <label class="input">
                        <input type="password"
                               id="Password_Confirm"
                               v-model="form.passwordConfirm"
                               placeholder="🔒 Confirmar Contraseña"
                               required />
                    </label>
                    <div v-if="passwordConfirmationError" class="error-msg">
                        {{ passwordConfirmationError }}
                    </div>

                    <label class="input">
                        <input type="text"
                               v-model="form.direction"
                               id="Direction"
                               placeholder="📍 Dirección"
                               required />
                    </label>
                    <div v-if="directionError" class="error-msg">{{ directionError }}</div>

                    <div class="buttons">
                        <button class="btn" type="submit">Enviar</button>
                        <button class="btn cancel" @click="returnLogin()">Regresar</button>
                    </div>
                </form>
            </aside>
        </main>

        <!-- Aqpi vamo a dejar el footer -->
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
    export default{
        name: "employerFomr",
        data(){
           return{
               form: {
                 uniqueUser: "",
                 id: "",
                 firstName: "",
                 secondName: "",
                 email: "",
                 personalPhone: "",
                 homePhone: "",
                 birthdate: "",
                 personType: "Empleador",
                 direction: "",
               },
               passwordError:"",
               firstNameError:"",
               birthdateError: "",
               idError: "",
               directionError: "",
               passwordConfirmationError: "",
               emailError: "",
               idFormatError: "",
               numberError: "",
           };
        },

        methods: {
            returnLogin() {
                this.$router.push({ path: "/" });
            },
            validateNumberFormat() {
                if ((this.form.homePhone && /^\d+$/.test(this.form.homePhone))&& (this.form.personalPhone &&  /^\d+$/.test(this.form.personalPhone))&&(this.form.homePhone.length === 8 && this.form.personalPhone.length === 8)) {

                    return true;
                }
                return false;
            },
            ShowNumberError(message) {
                this.numberError = message;
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
            validatePassword(password) {
                const regex = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,16}$/;
                return regex.test(password);
            },
            showpasswordError(message) {
                this.passwordError = message;
            },
            async validateEmail() {
                const validateEmailUrl = `http://localhost:5081/api/user/emailverify?email=${encodeURIComponent(this.form.email)}`;

                const response = await axios.get(validateEmailUrl);

                console.log("Respuesta importante\n");
                console.log(response.data.result)

                return response.data.result === true;

            },
            showEmailError(message) {
                this.emailError = message;
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
                this.emailError = ""
                this.numberError = "";
            },
            showbirthdateError(message) {
                this.birthdateError = message;
            },
            validateDirectionLength() {
                return this.form.direction.length < 200;
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
                const response = await axios.post(validateidurl, ageInt, {headers: { "Content-Type": "application/json" }} );
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
                return name.length >= 3;
            },
            showfisrtNameError(errorMessage) {
                this.firstNameError = errorMessage;
            },


           async handleSubmit(){
                const JSondata = JSON.stringify(this.form,null,2);
                console.log("Datos capaturados correctamente\n");
                console.log(JSondata);
                //Aquí justo es donde tengo que aprender a hacer lo nuevo
                //Ojito qe primero vamos 
               try {
                   this.clearErrors();
                   const createUserUrl = "http://localhost:5081/api/user/create";
                   //Vamo a crear otro Jsoncito para almacenar la data que neceito para la api de user
                   const userData = {
                       Email: this.form.email.trim(),
                       PasswordHash: this.form.password
                   };

                   const event1 = await this.validateEmail();
                   if (!event1) {
                       console.log("Entra aquí\n");
                       this.showEmailError("Este correo elctrónico ya está registrado, ingrese otro");
                       return;
                   }

                   if (!this.validateIDFormat()) {
                       console.log("Entra aquí pepe\n");
                       this.ShowIDFomratError("El formato de la cédula no es correcto");
                       return;
                   } 

                   if (!this.validateNumberFormat()) {
                       this.ShowNumberError("Número no sigue el formato adecuado");
                       return;
                   }


                   if (!(this.isNameValid(this.form.firstName))) {
                       this.showfisrtNameError("El nombre debe de contener al menos 5 carácteres");
                       return;
                   }


                   const event = await this.validateID();
                   if (!event) {
                       return;
                   }

                   if (!(this.isAdult(this.form.birthdate))) {
                       this.showbirthdateError('La fecha de nacimiento es inválida, debes de ser mayor de edad para registrarte en nuestra plataforma');
                       console.log("Entra aquí4\n");
                       return;
                   }

                   if (!this.validateDirectionLength()) {
                       this.showDirectionError("La dirección no puede exceder los 200 carácteres");
                       return;
                   }
                   if (!this.validatePassword(this.form.password)) {
                       this.showpasswordError('La contraseña no cumple con el formato esperado, mínimo 8 caracteres, max 16) (Mínimo 1 char mayúscula, 1 char mínúscula, 1 char especial) ');
                       return;
                   }


                   if (!this.validateConfirmationPassword(this.form.password, this.form.passwordConfirm)) {
                        this.showpasswordConfirmationError("La contraseña debe de coincidir con la original");
                        return;
                   }
                   this.clearErrors();

                    console.log("Va a llegar a la primera conexión\n");
                    const userResponse = await axios.post(createUserUrl, userData);
                    console.log("Conexión exitosa\n");
                    console.log("Usuario:", userResponse.data);

                    // Aquí apartir de la respuesta me traigo el id del usuario para trabajar la persona
                    const userId = userResponse.data.id;

                    this.form.uniqueUser = userId;

                    const personUrl = "http://localhost:5081/api/person/create";

                    const persRes = await axios.post(personUrl, this.form);
                    console.log("Usuario y Persona creados correctamente:");
                    console.log("Persona:", persRes.data);

                    this.$router.push({ path: "/" }).then(() => {
                        alert("El formulario fue compleatado con éxito revise su correo para activar su usuario")
                    });

                } catch (error) {
                    console.log("Error crando usuario o persona",error);
                }


                //Aquí vamos a crear la persona ojito

            },
            handleReset(){

                //ojito tomamos la vaina y la rechazamos 
                this.form = {
                      name: "",
                      secondNames:"",
                      id:"",
                      email: "",
                      date: "",
                      phoneNumber: "",
                      password: "",
                      passwordConfirm:"",
                      direction: "",

                };
            
            },

        },
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
        max-width: 45%;
        margin-top: 40px;
    }
    #EmployerLogIn {
        margin-top: 20px;
        margin-bottom: 20px;
        width: 360px;
        min-height: 220px;
        background: rgb(71, 69, 69);
        border: 1px solid rgba(255, 255, 255, 0.15);
        padding: 22px;
        border-radius: 8px;
        box-shadow: 0 6px 18px rgba(0, 0, 0, 0.35);
        height: 700px;
    }
    .logo-box {
        width: 84px;
        height: 84px;
        background: linear-gradient(180deg, #51a3a0, hsl(178, 77%, 86%));
        border-radius: 16px;
        display: flex;
        align-items: center;
        justify-content: center;
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
        height:800px;
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

    .buttons {
        display: flex;
        gap: 8px;
        margin-top: 40px;
    }

    .btn {
        flex: 1;
        padding: 10px 12px;
        border-radius: 6px;
        border: 0;
        font-weight: 600;
        cursor: pointer;
        background: #1fb9b4;
        color: white;
    }

        .btn.cancel {
            background: #444;
            color: #ccc;
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