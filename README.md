# SmartPantry

Repositorio del Trabajo Práctico Integrador de Desarrollo de Software 2026 - UTN FRCU.  
Solución monolítica en capas basada en **ABP Framework (Layered)** con frontend en **Angular** y persistencia mediante **Entity Framework Core**.

## Integrantes
- Bogado Florencia (@florenciabogado)
- Canela Miranda (@MirandaCanela)
- Gomez Serena (@seregomez)
- Torres Sofia (@sofitorres)

---

## Requisitos Previos
Para compilar y ejecutar la solución en un entorno local, se requiere contar con las siguientes herramientas instaladas:

- **.NET 10 SDK** y **Visual Studio 2022 / 2026** (con la carga de trabajo *Desarrollo de ASP.NET y web*).
- **Node.js 24 LTS** (versión `24.15.0` o superior). Comprobar con:
  ```bash
  node --version
  ```
- **Yarn 1.22.x**. Comprobar con:
  ```bash
  yarn --version
  ```
- **SQL Server LocalDB** o **SQL Server Express** instalado localmente.
- **SQL Server Management Studio (SSMS)** para inspeccionar la base de datos local.
- **ABP CLI** o **ABP Studio Desktop**. Comprobar con:
  ```bash
  dotnet tool install -g Volo.Abp.Studio.Cli
  ```
- **Git**. Comprobar con:
  ```bash
  git --version
  ```

---

## Configuración Local de Base de Datos
La cadena de conexión a la base de datos local se configura en la sección `ConnectionStrings:Default` de los siguientes dos archivos:
- `src/SmartPantry.DbMigrator/appsettings.json`
- `src/SmartPantry.HttpApi.Host/appsettings.json`

### Cadena de conexión utilizada (LocalDB)
```json
"ConnectionStrings": {
  "Default": "Server=(localdb)\\MSSQLLocalDB;Database=SmartPantry;Trusted_Connection=True;TrustServerCertificate=True"
}
```

> **Nota de seguridad:** No versionar credenciales ni contraseñas. En caso de utilizar un servidor remoto o usuario SQL con contraseña, emplear la variable de entorno `ConnectionStrings__Default` o el almacenamiento seguro de **User Secrets** de .NET:
> ```bash
> dotnet user-secrets set "ConnectionStrings:Default" "<cadena-de-conexion>"
> ```

---

## Puesta en Marcha

### 1. Restaurar dependencias y paquetes de ABP
Desde la raíz del repositorio ejecutar:
```powershell
abp install-libs
dotnet restore .\SmartPantry.slnx
```

### 2. Crear y migrar la Base de Datos (DbMigrator)
Ejecutar el proyecto migrador para crear la base local y aplicar la siembra de datos iniciales:
```powershell
dotnet run --project .\src\SmartPantry.DbMigrator
```

### 3. Iniciar el Backend (API Host)
En una terminal ejecutar:
```powershell
dotnet run --project .\src\SmartPantry.HttpApi.Host
```
- **Swagger UI:** https://localhost:44328/swagger  
*(En la primera ejecución en una máquina nueva, ejecutar previamente `dotnet dev-certs https --trust` para confiar en el certificado SSL de desarrollo).*

### 4. Iniciar el Frontend (Angular)
En una segunda terminal, ingresar a la carpeta `angular`, instalar paquetes y levantar el servidor:
```powershell
cd angular
yarn install
yarn start
```
- **Aplicación Web:** http://localhost:4200

---

## Verificación y Tests

### Backend (.NET)
Comandos para compilar la solución y ejecutar las pruebas automatizadas:
```powershell
dotnet build .\SmartPantry.slnx
dotnet test .\SmartPantry.slnx
```

### Frontend (Angular)
Comandos para compilar el frontend y ejecutar las pruebas en modo headless:
```powershell
cd angular
yarn build
yarn test --watch=false --browsers=ChromeHeadless

