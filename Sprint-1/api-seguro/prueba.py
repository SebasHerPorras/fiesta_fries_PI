import requests
import json

# Variables de entrada
url_base = "http://localhost:3000/seguro-privado"
edad = 30  # Cambiado de fecha_nacimiento a edad
dependientes = 2
token = "12345"

# Parámetros y headers
params = {
  "edad": edad,  # Cambiado el parámetro a 'edad'
  "dependientes": dependientes
}

headers = {
  "Authorization": token
}

print("Realizando solicitud a la API...")
print("Los datos enviados son:")
print(json.dumps(params, indent=2))
print("resultado esperado: 50000")

# Llamada a la API
response = requests.get(url_base, params=params, headers=headers)

# Convertir la respuesta a JSON
data = response.json()

# Mostrar resultado
print(json.dumps(data, indent=2))
