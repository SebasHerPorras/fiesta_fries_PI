<template>
    <div class="wrap">

        <header class="header">
            <div class="header-content">
                <div class="display">
                    <div class="logo-box">
                        <span class="f">F</span>
                    </div>
                    <div class="texts">
                        <h1>Mi Empresa</h1>
                        <p>Usuario</p>
                    </div>
                </div>

                <div class="dash-right">
                    <h2>Dashboard Empleado</h2>
                </div>
            </div>
        </header>

        <section class="hero">
            <div class="big-container">

                <div class="img-grid">
                    <div class="img-box">
                        <img src="@/assets/style/grafico1.jpg" alt="Imagen 1" />
                    </div>

                    <div class="img-box">
                        <img src="@/assets/style/grafico1.jpg" alt="Imagen 2" />
                    </div>

                    <div class="img-box">
                        <img src="@/assets/style/grafico1.jpg" alt="Imagen 3" />
                    </div>

                    <div class="img-box">
                        <img src="@/assets/style/grafico1.jpg" alt="Imagen 4" />
                    </div>
                </div>

            </div>
        </section>

    </div>
</template>

<script>
    export default {
        name: "DashboardEmpleadoImagenes",
        data() {
            return {

            };
        },
        mounted() {
            this.venBailalo();
        },
        methods: {
            venBailalo() {
                console.log(localStorage);
            }
        }

    };
</script>

<style src="@/assets/style/DashboardEmpleador.css" scoped></style>
