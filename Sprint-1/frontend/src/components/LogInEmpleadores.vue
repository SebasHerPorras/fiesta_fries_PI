<template>
        <div class="main_container">
            <form id="EmployerLogIn" @submit.prevent ="handleSubmit" @reset =" handleReset">
             <h3 id="title">Formulario Empleador</h3>
                <label for="Name"> 
                    <input type="text" id="Name" v-model ="form.firstName" placeholder="Nombre" required>
                </label>
                <label for="SecondNames">
                    <input type="text" id="SecondNames" v-model="form.secondName" placeholder="Apelllidos" required/>
                </label>
                <label for="Id">
                    <input type="text" id="Id" v-model="form.id" placeholder="cédula"/>
                 </label>
                <label for="Email"> 
                    <input type="email" id="Email" v-model = "form.email" placeholder="Email" required />
                </label>
                <label for ="BirthDate">
                    <input type="date" v-model ="form.birthdate" id="BirthDate" required />
                </label>
                <label for="Phone_Number">
                <input type="tel" id="Phone_Number" v-model ="form.personalPhone" required placeholder="teléfono"/>
                </label>
                <label for="Home Number">
                    <input type="text" id="Home Number" v-model="form.homePhone" placeholder="Teléfono casa"/>
                </label>
                <label for="Password">
                <input type="password" id="Password" v-model = "form.password" required placeholder="Contraseña" />
                </label>
                <label for="Password_Confirm">
                    <input type="password" id="Password_Confirm" v-model ="form.passwordConfirm" required placeholder="Confirmar Contraseña" />
                </label>
                <label for="Direction">
                <input type="text" v-model ="form.direction" id="Direction" required placeholder="Dirección" />
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
                 active: 0
             },
           };
        },

        methods: {
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