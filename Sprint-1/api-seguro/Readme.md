# API Seguro Privado

API construida con Express.js usando Express Generator.  
Permite calcular las deducciones mensuales de un seguro según edad y cantidad de dependientes.  
Incluye seguridad básica con token y documentación interactiva con Swagger UI.

---

## Cómo se hizo el proyecto

1. **Generar proyecto base con Express Generator:**

  npm install -g express-generator
  express seguro-privado --no-view
  cd seguro-privado
  npm install

2. **Instalar dependencias adicionales:**

  npm install swagger-ui-express swagger-jsdoc

3. **Configurar Swagger en `app.js` para documentar la API.**

4. **Middleware de seguridad:**
  - Todos los endpoints requieren un token (Authorization header o `?token=12345` en query).
  - Se excluye `/api-docs` para que Swagger sea público.

5. **Endpoint principal:** `/seguro-privado` implementado en `routes/index.js`.

---

## Cómo levantar el proyecto

1. Abrir terminal en la carpeta del proyecto.
2. Instalar dependencias:

  `npm install`

3. Levantar servidor:

  `npm start`

4. Por defecto corre en:  
  http://localhost:3000

---

## Documentación Swagger

- Disponible en navegador:  
  http://localhost:3000/api-docs
- Permite ver todos los endpoints, parámetros, respuestas y probar la API directamente.

---

## Seguridad

- Token requerido en headers:  
  Authorization: 12345
- O como query param (solo para pruebas):  
  ?token=12345
- Sin token → 403 Forbidden

---

## Endpoint principal

**Función:** calcular_deducciones(edad, dependientes, token)

**Parámetros:**
- edad (integer, obligatorio): Edad del asegurado
- dependientes (integer, obligatorio): Cantidad de dependientes
- token (string, obligatorio si no está en variable global)

**Respuesta esperada (200 OK):**

{
  "deductions": [
    { "type": "ER", "Amount": 100000 },
    { "type": "EE", "Amount": 50000 }
  ]
}

---

## Uso desde Python

import requests
import json

def calcular_deducciones(edad, dependientes, token="12345"):
    url_base = "http://localhost:3000/seguro-privado"
    params = {"edad": edad, "dependientes": dependientes}
    headers = {"Authorization": token}
    response = requests.get(url_base, params=params, headers=headers)
    
    try:
        data = response.json()
        print(json.dumps(data, indent=4))
        return data
    except ValueError:
        print("La respuesta no es JSON válido:")
        print(response.text)
        return None

# Ejemplo correcto
resultado = calcular_deducciones(35, 2)

# Ejemplo sin token
resultado = calcular_deducciones(35, 2, token="")

# Ejemplo con parámetros faltantes
resultado = calcular_deducciones("", 2)

---

## Uso desde JavaScript / Vue.js

const axios = require('axios');

async function calcularDeducciones(edad, dependientes, token="12345") {
    try {
        const res = await axios.get("http://localhost:3000/seguro-privado", {
            params: { edad, dependientes },
            headers: { Authorization: token }
        });
        console.log(JSON.stringify(res.data, null, 4));
        return res.data;
    } catch (err) {
        if (err.response) {
            console.error(`Error ${err.response.status}:`, err.response.data);
        } else {
            console.error(err.message);
        }
        return null;
    }
}

// Ejemplo correcto
calcularDeducciones(35, 2);

// Ejemplo sin token
calcularDeducciones(35, 2, "");

// Ejemplo con parámetros faltantes
calcularDeducciones("", 2);

---

## Casos de error

1. **Sin token**
  - Código de respuesta: 403 Forbidden
  - JSON de error:
    {
      "error": "Forbidden: token inválido o ausente"
    }

2. **Faltan parámetros**
  - Código de respuesta: 400 Bad Request
  - JSON de error:
    {
      "error": "Faltan parámetros"
    }
3. **Parámetros inválidos**
  - Edad < 18 o no numérica
  - Dependientes < 0 o no numérico
  - Código de respuesta: 400 Bad Request
  - JSON de error:
    {
      "error": "Edad inválida"
    }
    o
    {
      "error": "Dependientes inválido"
    }
---

## Resumen

- Proyecto base: Express Generator (--no-view)
- Endpoint principal: /seguro-privado
- Seguridad: token obligatorio (header Authorization o query ?token=)
- Documentación interactiva: Swagger UI (/api-docs)
- Casos de uso: cálculo de deducciones según edad y dependientes, manejo de errores
- Consumo desde código: se puede usar como función en Python o encapsular en funciones JS/Vue para integración con frontend
