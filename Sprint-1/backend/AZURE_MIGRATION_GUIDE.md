# Guía de Migración a Azure SQL Database
## Fiesta Fries - Backend ASP.NET Core

### 📋 Resumen de Cambios Implementados

Se ha actualizado la aplicación backend para funcionar correctamente con **Azure SQL Database** usando un schema personalizado `Fiesta_Fries_DB` dentro de la base de datos `G02-2025-II-DB`.

---

## ✅ Cambios Completados

### 1. **Actualización de Paquetes NuGet**
- ✅ Migrado de `System.Data.SqlClient` (obsoleto) a `Microsoft.Data.SqlClient 5.2.2`
- ✅ Agregado `AspNetCore.HealthChecks.SqlServer 8.0.2` para health checks

### 2. **Connection String Optimizado**
**Archivo:** `appsettings.json`

```json
"UserContext": "Server=ucr-bd.database.windows.net;Database=G02-2025-II-DB;User Id=grupo2;Password=Grupo02-2025$#;TrustServerCertificate=True;Encrypt=True;Connection Timeout=60;Min Pool Size=5;Max Pool Size=100;"
```

**Mejoras agregadas:**
- `Connection Timeout=60` - Aumentado para conexiones lentas a Azure
- `Min Pool Size=5` - Pool mínimo de conexiones
- `Max Pool Size=100` - Pool máximo para mejor performance

### 3. **SqlConnectionFactory Mejorado**
**Archivo:** `Infrastructure/SqlConnectionFactory.cs`

**Cambios:**
- ✅ Usa `Microsoft.Data.SqlClient`
- ✅ Implementa logging con `ILogger`
- ✅ Try-catch para capturar errores de conexión
- ✅ Mensajes de error detallados para diagnóstico

### 4. **Scripts SQL Actualizados**
**Ubicación:** `queries/`

**Cambios aplicados a 41 de 42 archivos SQL:**
- ✅ `CREATE DATABASE Fiesta_Fries_DB;` → Comentado (ya existe G02-2025-II-DB)
- ✅ `USE Fiesta_Fries_DB;` → `USE [G02-2025-II-DB];`
- ✅ Todas las tablas ahora usan prefijo: `[Fiesta_Fries_DB].[NombreTabla]`
- ✅ Backup creado en: `queries/backup_original_scripts/`

**Ejemplo de cambio:**
```sql
-- ANTES
CREATE TABLE Empleado (id int PRIMARY KEY)

-- DESPUÉS
CREATE TABLE [Fiesta_Fries_DB].[Empleado] (id int PRIMARY KEY)
```

### 5. **Repositorios C# Actualizados**
**Ubicación:** `Repositories/`

**Cambios aplicados a 17 de 22 archivos:**
- ✅ Todas las consultas SQL ahora usan prefijo de schema
- ✅ Backup creado en: `Repositories/backup_original_files/`

**Ejemplos de cambios:**
```csharp
// ANTES
"SELECT * FROM Empleado WHERE id = @id"
"INSERT INTO Payroll (PeriodDate, CompanyId) VALUES (@Date, @CompanyId)"

// DESPUÉS
"SELECT * FROM [Fiesta_Fries_DB].[Empleado] WHERE id = @id"
"INSERT INTO [Fiesta_Fries_DB].[Payroll] (PeriodDate, CompanyId) VALUES (@Date, @CompanyId)"
```

### 6. **Health Checks Implementados**
**Archivo:** `Program.cs`

**Endpoint agregado:** `GET /health`

**Respuesta JSON:**
```json
{
  "status": "Healthy",
  "checks": [
    {
      "name": "Azure SQL Database",
      "status": "Healthy",
      "description": null,
      "duration": 45.2
    }
  ],
  "totalDuration": 45.2
}
```

---

## 🚀 Pasos Siguientes para Implementar

### **Paso 1: Ejecutar Script de Creación de Schema en Azure**

**Archivo:** `queries/000_CreateSchema_Fiesta_Fries_DB.sql`

**Herramientas sugeridas:**
- Azure Data Studio
- SQL Server Management Studio (SSMS)
- Azure Portal Query Editor

**Instrucciones:**
1. Conectarte a `ucr-bd.database.windows.net`
2. Seleccionar base de datos `G02-2025-II-DB`
3. Ejecutar el script completo
4. Verificar que el schema se creó correctamente

**Resultado esperado:**
```
✓ Schema [Fiesta_Fries_DB] creado exitosamente
✓ Usuario [grupo2] configurado con default schema [Fiesta_Fries_DB]
```

---

### **Paso 2: Configurar Firewall de Azure SQL**

**En Azure Portal:**

1. Ir a: `ucr-bd` SQL Server
2. Navegar a: **Settings → Networking → Firewall rules**
3. Agregar reglas:
   - ✅ **Allow Azure services**: ON (para deployments en Azure)
   - ✅ **Tu IP pública**: Agregar para desarrollo local

**Ejemplo:**
```
Rule Name: MyDevMachine
Start IP: 201.123.45.67
End IP: 201.123.45.67
```

**Verificar conectividad:**
```powershell
Test-NetConnection -ComputerName ucr-bd.database.windows.net -Port 1433
```

---

### **Paso 3: Ejecutar Scripts SQL en Orden**

**Ubicación:** `queries/`

**Orden recomendado:**
1. `000_CreateSchema_Fiesta_Fries_DB.sql` ✅ (ya ejecutado)
2. `SCRUM-23_DB_and_Tabla-User_1.sql` (tabla User)
3. `SCRUM-7-Tabla-Persona_2.sql` (tabla Persona)
4. `SCRUM-8_DB_and_Tabla-Empresa_3.sql` (tabla Empresa)
5. `SCRUM-15_Tabla_Empleados_4.sql` (tabla Empleado)
6. ... continuar con los demás en orden numérico

**Opción automatizada (ejecutar todos):**
```powershell
# En PowerShell
cd c:\sebas-dev_windows\fiesta_fries_PI\Sprint-1\backend\queries

# Conectar y ejecutar todos los scripts (requiere sqlcmd)
Get-ChildItem -Filter "*.sql" | 
  Where-Object { $_.Name -ne "000_CreateSchema_Fiesta_Fries_DB.sql" } |
  Sort-Object Name |
  ForEach-Object {
    Write-Host "Ejecutando: $($_.Name)" -ForegroundColor Cyan
    sqlcmd -S ucr-bd.database.windows.net -d G02-2025-II-DB -U grupo2 -P "Grupo02-2025$#" -i $_.FullName
  }
```

---

### **Paso 4: Probar la Aplicación Localmente**

**Iniciar el backend:**
```powershell
cd c:\sebas-dev_windows\fiesta_fries_PI\Sprint-1\backend
dotnet run
```

**Probar health check:**
```powershell
# En PowerShell
Invoke-RestMethod -Uri "http://localhost:5081/health" -Method Get | ConvertTo-Json -Depth 3
```

**Resultado esperado:**
```json
{
  "status": "Healthy",
  "checks": [
    {
      "name": "Azure SQL Database",
      "status": "Healthy"
    }
  ]
}
```

**Si falla el health check:**
- Verificar firewall de Azure SQL
- Revisar connection string en `appsettings.json`
- Verificar que el usuario `grupo2` tiene permisos

---

### **Paso 5: Probar Endpoints de la API**

**Swagger UI:**
```
http://localhost:5081/swagger
```

**Endpoints sugeridos para probar:**
1. `GET /api/Country` - Listar países (tabla simple)
2. `GET /api/Empresa` - Listar empresas
3. `GET /api/Empleado` - Listar empleados
4. `POST /api/Empleado` - Crear empleado

**Ejemplo con cURL:**
```bash
curl -X GET "http://localhost:5081/api/Country" -H "accept: application/json"
```

---

## 🔍 Verificación de Tablas en Azure

**Verificar que las tablas están en el schema correcto:**

```sql
USE [G02-2025-II-DB];
GO

-- Ver todas las tablas en el schema Fiesta_Fries_DB
SELECT 
    SCHEMA_NAME(schema_id) AS SchemaName,
    name AS TableName,
    create_date AS CreatedDate
FROM sys.tables
WHERE SCHEMA_NAME(schema_id) = 'Fiesta_Fries_DB'
ORDER BY name;

-- Contar registros en una tabla específica
SELECT COUNT(*) AS TotalEmpleados 
FROM [Fiesta_Fries_DB].[Empleado];
```

---

## 📦 Estructura de Archivos Modificados

```
backend/
├── appsettings.json                    ✅ (connection string optimizado)
├── backend.csproj                      ✅ (paquetes actualizados)
├── Program.cs                          ✅ (health checks agregados)
├── Infrastructure/
│   └── SqlConnectionFactory.cs         ✅ (Microsoft.Data.SqlClient + logging)
├── queries/
│   ├── 000_CreateSchema_Fiesta_Fries_DB.sql  ✅ (nuevo)
│   ├── UpdateScriptsForAzureSchema.ps1       ✅ (script automatización)
│   ├── backup_original_scripts/              ✅ (backups)
│   └── [41 archivos .sql actualizados]       ✅
└── Repositories/
    ├── UpdateRepositoriesForAzureSchema.ps1  ✅ (script automatización)
    ├── backup_original_files/                ✅ (backups)
    └── [17 archivos .cs actualizados]        ✅
```

---

## ⚠️ Notas Importantes

### **1. Default Schema Configurado**
El usuario `grupo2` tiene `Fiesta_Fries_DB` como default schema, lo que significa:
- ✅ Consultas **CON** prefijo: `SELECT * FROM [Fiesta_Fries_DB].[Empleado]` → **Funciona**
- ✅ Consultas **SIN** prefijo: `SELECT * FROM Empleado` → **También funciona** (usa default schema)

### **2. Stored Procedures**
Los stored procedures pueden necesitar ajustes manuales en:
- JOINs internos
- Referencias a funciones escalares (ej: `dbo.Fn_ObtenerHoras`)

**Revisar especialmente:**
- `019_SCRUM-54_SP_getEmployees.sql`
- `029_SCRUM-103_SP_GetPayrollFullReport.sql`
- `033_SCRUM-103_SP_GetPayrollEmployeeReport.sql`

### **3. Backups Disponibles**
Si algo sale mal, todos los archivos originales están respaldados:
- SQL scripts: `queries/backup_original_scripts/`
- Repositorios C#: `Repositories/backup_original_files/`

**Restaurar backups:**
```powershell
# Restaurar scripts SQL
Copy-Item "c:\sebas-dev_windows\fiesta_fries_PI\Sprint-1\backend\queries\backup_original_scripts\*" `
          "c:\sebas-dev_windows\fiesta_fries_PI\Sprint-1\backend\queries\" -Force

# Restaurar repositorios
Copy-Item "c:\sebas-dev_windows\fiesta_fries_PI\Sprint-1\backend\Repositories\backup_original_files\*" `
          "c:\sebas-dev_windows\fiesta_fries_PI\Sprint-1\backend\Repositories\" -Force
```

### **4. Ambiente Development vs Production**
Actualmente `appsettings.Development.json` sigue apuntando a localhost:
```json
"UserContext": "Server=localhost;Database=Fiesta_Fries_DB;Trusted_Connection=True;"
```

**Para desarrollo local con Azure:**
Actualiza `appsettings.Development.json` con el mismo connection string de `appsettings.json`

---

## 🛡️ Seguridad - Mejora Futura

**Actualmente:** Password en texto plano en `appsettings.json`

**Recomendación:** Migrar a **Azure Key Vault**

**Pasos futuros:**
1. Crear Key Vault en Azure
2. Almacenar connection string como secret
3. Actualizar `Program.cs`:
```csharp
builder.Configuration.AddAzureKeyVault(
    new Uri($"https://{keyVaultName}.vault.azure.net/"),
    new DefaultAzureCredential());
```

O usar **User Secrets** para desarrollo:
```powershell
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:UserContext" "Server=ucr-bd..."
```

---

## 📞 Troubleshooting

### **Error: Login failed for user 'grupo2'**
**Solución:**
- Verificar password en connection string
- Revisar firewall de Azure SQL
- Verificar que el usuario existe en la base de datos

### **Error: Invalid object name 'Empleado'**
**Solución:**
- Ejecutar el script `000_CreateSchema_Fiesta_Fries_DB.sql`
- Verificar que las tablas están en el schema correcto
- Ejecutar los scripts de creación de tablas

### **Error: Cannot open database 'G02-2025-II-DB'**
**Solución:**
- Verificar que la base de datos existe en Azure Portal
- Revisar nombre de la base de datos en connection string

### **Timeout connecting to SQL Server**
**Solución:**
- Verificar firewall rules en Azure
- Aumentar `Connection Timeout` en connection string
- Verificar conectividad de red

---

## ✅ Checklist Final

Antes de considerar la migración completa:

- [ ] Schema `Fiesta_Fries_DB` creado en Azure SQL
- [ ] Usuario `grupo2` configurado con default schema
- [ ] Firewall de Azure SQL configurado
- [ ] Todos los scripts SQL ejecutados sin errores
- [ ] Health check devuelve status `Healthy`
- [ ] Endpoints de la API funcionan correctamente
- [ ] Stored procedures probados manualmente
- [ ] Queries complejas validadas
- [ ] Performance aceptable comparado con localhost
- [ ] Logs de errores revisados

---

## 📚 Recursos Adicionales

- [Azure SQL Database Documentation](https://docs.microsoft.com/azure/azure-sql/)
- [Microsoft.Data.SqlClient GitHub](https://github.com/dotnet/SqlClient)
- [ASP.NET Core Health Checks](https://docs.microsoft.com/aspnet/core/host-and-deploy/health-checks)
- [Dapper with Azure SQL](https://github.com/DapperLib/Dapper)

---

**Fecha de migración:** 26 de noviembre de 2025  
**Versión:** Backend ASP.NET Core 8.0  
**Base de datos:** Azure SQL Database - G02-2025-II-DB  
**Schema:** Fiesta_Fries_DB
