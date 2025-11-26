<template>
    <div class="wrap">

        <!-- Encabezado -->
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

                <!-- Texto alineado a la derecha -->
                <div class="dash-right">
                    <h2>Dashboard Empleado</h2>
                </div>
            </div>
        </header>

        <!-- Contenedor principal -->
        <section class="hero">
            <div class="big-container">

                <!-- Contenedores 2x2 -->
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

    .header {
        background: rgba(0, 0, 0, 0.25);
        border-bottom: 1px solid rgba(255, 255, 255, 0.15);
        padding: 16px 64px;
    }

    .header-content {
        display: flex;
        justify-content: space-between;
        align-items: center;
    }

    .display {
        display: flex;
        align-items: center;
        gap: 18px;
    }

    .logo-box {
        width: 50px;
        height: 50px;
        background: linear-gradient(180deg, #51a3a0, hsl(178, 77%, 86%));
        border-radius: 16px;
        display: flex;
        align-items: center;
        justify-content: center;
    }

        .logo-box .f {
            font-weight: 800;
            font-size: 24px;
            color: white;
        }

    .dash-right h2 {
        color: white;
        font-size: 22px;
        margin: 0;
    }

    .hero {
        display: flex;
        flex-direction: column;
        align-items: center;
        padding: 48px 64px;
        gap: 40px;
        flex: 1 0 auto;
    }

    .big-container {
        width: 100%;
        background: #2a2a2a;
        padding: 24px;
        border-radius: 12px;
    }

    .img-grid {
        display: grid;
        grid-template-columns: repeat(2, 1fr);
        gap: 20px;
    }

    .img-box {
        background: #1e1e1e;
        border: 2px solid #1fb9b4;
        padding: 16px;
        border-radius: 12px;
        display: flex;
        justify-content: center;
        align-items: center;
    }

        .img-box img {
            width: 100%;
            border-radius: 8px;
        }

    @media (max-width: 700px) {
        .img-grid {
            grid-template-columns: 1fr;
        }
    }
</style>
