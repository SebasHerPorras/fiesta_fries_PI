<template>
  <div class="wrap">
    <!-- Header -->
    <header class="header">
      <nav class="nav">
        <!-- Brand (logo + nombre) -->
        <div class="display">
          <div class="logo-box">
            <span class="f">F</span>
          </div>
          <div class="texts">
            <h1>Mi Empresa</h1>
            <p>Usuario</p>
          </div>
        </div>

        <!-- Texto a la derecha -->
        <div class="dashboard-texts">
          <h2>Dashboard-Empleado</h2>
          <p>Fecha del último pago</p>
        </div>
      </nav>
    </header>

    <!-- Contenido -->
    <section class="hero">

      <!-- Contenedor grande gris claro -->
      <div class="big-container">

  <!-- GRID 2 filas x 4 columnas -->
  <div class="grid-container">

    <!-- Fila 1 -->
    <div class="grid-item"><p>Salario Bruto</p></div>
    <div class="grid-item"><p>SEM</p></div>
    <div class="grid-item"><p>IVM</p></div>
    <div class="grid-item"><p>LPT</p></div>

    <!-- Fila 2 -->
    <div class="grid-item"><p>Impuesto sobre la renta</p></div>
    <div class="grid-item"><p>Total deducciones</p></div>
    <div class="grid-item"><p>Porcentaje retenido</p></div>
    <div class="grid-item"><p>Salario neto</p></div>

  </div> <!-- Cierra grid-container -->

</div>

       
      <!-- Contenedor para imagen -->
      <div class="image-container">
        <img src="@/assets/style/narutogato.jpg" alt="Imagen aquí" />
      </div>

    </section>
  </div>
</template>

<script>
export default {
  name: "DashboardEmpleado",
};
</script>

<style scoped>
/* ======== Estilos ya existentes del usuario (mantengo todo) ======== */

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

.nav {
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

/******** Nuevo texto del dashboard ********/

.dashboard-texts h2 {
  margin: 0;
  color: white;
  font-size: 22px;
  text-align: right;
}

.dashboard-texts p {
  margin: 4px 0 0;
  color: white;
  opacity: 0.85;
  font-size: 14px;
  text-align: right;
}

/******** Hero ********/

.hero {
  display: flex;
  flex-direction: column;
  align-items: center;
  padding: 48px 64px;
  gap: 40px;
  flex: 1 0 auto;
}

/******** Contenedor grande gris claro ********/

.big-container {
  width: 100%;
  background: #2a2a2a;
  padding: 24px;
  border-radius: 12px;
}

/******** Grid 2 filas × 4 columnas ********/

.grid-container {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  grid-template-rows: repeat(2, auto);
  gap: 16px;
}

.grid-item {
  background: #dcdcdc;
  border: 2px solid #1fb9b4;
  padding: 20px;
  border-radius: 10px;
  text-align: center;
  color: #333;
  font-weight: 600;
}

/******** Contenedor de imagen ********/

.image-container {
  width: 100%;
  background: #2a2a2a;
  padding: 24px;
  border-radius: 12px;
  display: flex;
  justify-content: center;
}

.image-container img {
  max-width: 100%;
  border-radius: 8px;
  border: 1px solid #1fb9b4;
}

/******** Responsivo ********/

@media (max-width: 900px) {
  .grid-container {
    grid-template-columns: repeat(2, 1fr);
  }

  .dashboard-texts h2,
  .dashboard-texts p {
    text-align: center;
  }
}

@media (max-width: 600px) {
  .grid-container {
    grid-template-columns: 1fr;
  }
}
</style>
