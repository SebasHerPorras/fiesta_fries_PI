// error handler, variable necesaria para invocar los errores
var createError = require("http-errors");
// módulos necesarios
// Eexpress se usa para crear el servidor y manejar rutas
var express = require("express");
// path se usa para manejar rutas de archivos y directorios
var path = require("path");
// cookie-parser se usa para manejar cookies en las solicitudes HTTP
var cookieParser = require("cookie-parser");
// morgan se usa para registrar las solicitudes HTTP en la consola morgan es un middleware de registro de solicitudes HTTP para Node.js
var logger = require("morgan");
// importando las rutas definidas en otros archivos
var indexRouter = require("./routes/index");
var usersRouter = require("./routes/users");

// Swagger, usa swagger-ui-express y swagger-jsdoc, que son herramientas para documentar APIs RESTful
// definidas por express
var swaggerUi = require("swagger-ui-express");
var swaggerJsDoc = require("swagger-jsdoc");

// creando la aplicación express
var app = express();

// Swagger config
const swaggerOptions = {
  definition: {
    openapi: "3.0.0",
    info: {
      title: "API Seguro Privado",
      version: "1.0.0",
      description: "API para cálculo de deducciones de un seguro privado",
    },
  },
  apis: ["./routes/*.js"],
};

// Generando la documentación Swagger
const swaggerDocs = swaggerJsDoc(swaggerOptions);
app.use("/api-docs", swaggerUi.serve, swaggerUi.setup(swaggerDocs));

// view engine setup
app.set("views", path.join(__dirname, "views"));
app.set("view engine", "jade");

// 
app.use(logger("dev"));
app.use(express.json());
app.use(express.urlencoded({ extended: false }));
app.use(cookieParser());
app.use(express.static(path.join(__dirname, "public")));

// Middleware token, excepto para Swagger
app.use((req, res, next) => {
  if (req.path.startsWith("/api-docs")) {
    return next(); // dejar Swagger público
  }

  // validar token
  const token = req.headers["authorization"] || req.query.token;
  if (!token || token !== "12345") {
    return res
      .status(403)
      .json({ error: "Forbidden: token inválido o ausente" });
  }
  next();
});

// rutas con index y users
app.use("/", indexRouter);
app.use("/users", usersRouter);

// catch 404 and forward to error handler
app.use(function (req, res, next) {
  next(createError(404));
});

// error handler
app.use(function (err, req, res, next) {
  // set locals, only providing error in development
  res.locals.message = err.message;
  res.locals.error = req.app.get("env") === "development" ? err : {};

  // render the error page
  res.status(err.status || 500);
  res.render("error");
});

module.exports = app;
