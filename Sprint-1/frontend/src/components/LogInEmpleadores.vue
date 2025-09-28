<template>
        <div class="main_container">
            <form id="EmployerLogIn" @submit.prevent ="handleSubmit" @reset =" handleReset">
             <h3 id="title">Formulario Empleador</h3>
                <label for="Name"> 
                    <input type="text" id="Name" v-model ="form.firstName" placeholder="Nombre" required>
                    <div v-if="firstNameError" style="color: #ff6b6b; font-size: 13px; margin-bottom: 8px"> {{firstNameError}}</div>
                </label>
                <label for="SecondNames">
                    <input type="text" id="SecondNames" v-model="form.secondName" placeholder="Apelllidos" required/>
                </label>
                <label for="Id">
                    <input type="text" id="Id" v-model="form.id" placeholder="cédula" required/>
                    <div v-if="idError"  style="color: #ff6b6b; font-size: 13px; margin-bottom: 8px"> {{idError}} </div>
                 </label>
                <label for="Email"> 
                    <input type="email" id="Email" v-model = "form.email" placeholder="Email" required />
                </label>
                <label for ="BirthDate">
                    <input type="date" v-model ="form.birthdate" id="BirthDate" required />
                    <div v-if="birthdateError"  style="color: #ff6b6b; font-size: 13px; margin-bottom: 8px"> {{birthdateError}}</div>
                </label>
                <label for="Phone_Number">
                <input type="tel" id="Phone_Number" v-model ="form.personalPhone" required placeholder="teléfono"/>
                </label>
                <label for="Home Number">
                    <input type="text" id="Home Number" v-model="form.homePhone" placeholder="Teléfono casa"/>
                </label>
                <label for="Password">
                <input type="password" id="Password" v-model = "form.password" required placeholder="Contraseña" />
                      <div v-if="passwordError"  style="color: #ff6b6b; font-size: 13px; margin-bottom: 8px" >{{passwordError}}</div>
                </label>
                <label for="Password_Confirm">
                    <input type="password" id="Password_Confirm" v-model ="form.passwordConfirm" required placeholder="Confirmar Contraseña" />
                      <div v-if="passwordConfirmationError" style="color: #ff6b6b; font-size: 13px; margin-bottom: 8px" >{{passwordConfirmationError}}</div>
                </label>
                <label for="Direction">
                <input type="text" v-model ="form.direction" id="Direction" required placeholder="Dirección" />
                    <div v-if="directionError" style="color: #ff6b6b; font-size: 13px; margin-bottom: 8px"> {{directionError}}</div>
                </label>
                <div class ="Bottons_container">
                    <input type="submit" value="Enviar" id="Submit-btn" />
                    <input type="reset" value="Cancelar" id="Restart-btn" />
                </div>
            </form>
        </div>
        <footer>
                <div>©2025 Fiesta Fries</div>
                <div class="socials">
                    <!-- Enlaces a redes sociales (solo íconos, no funcionales, de momento ojito) --> 
                    <a href="#" aria-label="Facebook">f</a>
                    <a href="#" aria-label="LinkedIn">in</a>
                    <a href="#" aria-label="YouTube">▶</a>
                    <a href="#" aria-label="Instagram">✶</a>
                </div>
            </footer>
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
               passwordConfirmationError:"",
           };
        },

        methods: {
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
                return name.length > 5;
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
                   const createUserUrl = "http://localhost:5081/api/user/create";
                   //Vamo a crear otro Jsoncito para almacenar la data que neceito para la api de user
                   const userData = {
                       Email: this.form.email.trim(),
                       PasswordHash: this.form.password
                   };


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


<style>
 .main_container{
        display: grid;
        place-items: center;
        margin-top: 20px;
        background-color: #1e1e1e;
        text-align: center;
  }

    label {
        color: white;
        margin-top:5px;
    }

    input {
        align-items: center;
        padding: 10px 12px;
        border-radius: 6px;
        background: rgba(0, 0, 0, 0.25);
        border: 1px solid rgba(255, 255, 255, 0.06);
        color: #ece6e6ff;
        width: 210px;
    }

    footer {
        display: flex;
        flex-direction: column; 
        align-items: center; 
        gap: 10px;
        text-align: center; 
        padding: 10px 0;
    }

    #EmployerLogIn {
        margin-top:20px;
        margin-bottom: 20px;
        width: 360px;
        min-height: 220px;
        background: rgb(71, 69, 69);
        border: 1px solid rgba(255, 255, 255, 0.15);
        padding: 22px;
        border-radius: 8px;
        box-shadow: 0 6px 18px rgba(0, 0, 0, 0.35);
        height:680px;
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

    .Bottons_container{
        margin-top: 20px;
        display: flex;
        gap: 5px;
    }

    #Submit-btn {
        color: white;
        background-color: #1fb9b4;
    }

    #Restart-btn {
        color: #bdbdbd;
        background-color: white;
    }


 #title {
   color: #bdbdbd;
   margin-bottom: 20px;

  }

</style>