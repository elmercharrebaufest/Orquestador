# Orquestador de Dispositivos - AI Agent Instructions

## Project Overview
Enterprise device orchestration system for Molinos Agro managing industrial IoT devices (scales, barriers, sensors, cameras, ALPR, meteorological stations, etc.) across manufacturing facilities. Built with .NET Framework 4.7.2, Entity Framework 6, WCF services, and Windows Services architecture.

## Architecture Components

### 3-Tier Architecture
1. **Molinos.Orquest.Servidor** - Windows Service that hosts WCF services and manages device drivers
2. **Molinos.Orquest.Web** - ASP.NET MVC 5 web application for device management and monitoring
3. **Molinos.Orquest.Database** - SQL Server database project for configuration and state

### Core Layer Structure
- **Molinos.Orquest.Dominio** - Domain entities, commands (e.g., `ComandoEjecutar`, `ComandoSuscribir`), DTOs, and exceptions
- **Molinos.Orquest.Drivers** - Device driver interfaces (`IDriver`, `IDriverBarrera`, `IDriverCamara`, etc.)
- **Molinos.Orquest.DriversImpl** - Concrete driver implementations for physical devices
- **Molinos.Orquest.Servicios** - Business logic and orchestration services (`IServicioOrquestador`, `AdministradorSuscripciones`)
- **Molinos.Orquest.Repositorio** - Entity Framework data access layer with `RepositorioEF`
- **Molinos.Orquest.Dependencias** - Ninject DI configuration (`OrquestNinjectModule`, `OrquestWebNinjectModule`)
- **Molinos.Orquest.ModuloALPR** - ALPR (Automatic License Plate Recognition) specialized module

## Key Patterns & Conventions

### Command Pattern for Device Operations
All device operations use command objects inheriting from `Comando` base class:
- `ComandoEjecutar` - Base for execution commands (e.g., `EjecutarAperturaBarrera`, `EjecutarTomarFoto`)
- `ComandoSuscribir` - Subscribe to device events
- `ComandoCancelarSuscripcion` - Unsubscribe from events
- Commands use `[DataContract]` and `[KnownType("TiposDeComandos")]` for WCF serialization

### Driver Architecture
- All drivers implement `IDriver` interface with `Inicializar()`, `VerificarDispositivo()`, and `EventoDriver` event
- Device-specific interfaces extend `IDriver` (e.g., `IDriverBarrera`, `IDriverCamara`)
- Drivers loaded dynamically via `DriverFactory` using Ninject
- Exception hierarchy: `DriverException` → `ComandoDriverException`, `ConexionDispositivoDriverException`

### Configuration-Driven Devices
Device configs stored in DB as entities (`ConfigBarrera`, `ConfigCamara`, etc.) inheriting from `ConfigDispositivo`. Each device type has its own configuration entity with specific properties.

### Service Endpoints
- **Windows Service**: Exposes WCF services at `net.tcp://[machine]:8081/ServicioOrquestador` (inter-orchestrator) and `http://[machine]:8080/ServicioOrquestador` (external clients)
- **Web App**: Consumes orchestrator service via `IServicioOrquestador` client proxy
- **SAP Integration**: Separate endpoint `http://[machine]:8080/ServicioOrquestadorSAP`

## Development Workflows

### Building & Running
```powershell
# Build entire solution
MSBuild.exe Molinos.Orquestador.sln /p:Configuration=Debug

# Local installation (builds + deploys)
cd Molinos.Orquest.Build
Local.Instalar.bat  # Runs build.proj + deploy.proj

# Run Windows Service in DEBUG mode
# Service automatically shows MessageBox when compiled in DEBUG configuration
# See Molinos.Orquest.Servidor\Program.cs
```

### Testing
- Unit tests in **Molinos.Orquest.Test** using NUnit (`[Test]` attributes)
- E2E tests in **Molinos.Orquest.E2ETests** using Playwright with Node.js
- Test coverage via OpenCover and ReportGenerator (configured in `build.targets`)
```bash
cd Molinos.Orquest.E2ETests
npm install
npx playwright install
npm test
```

### Database Updates
- Use SQL Server Database Project (**Molinos.Orquest.Database.sqlproj**)
- Multiple publish profiles for environments (QA, PROD)
- Entity Framework migrations not used - schema managed via SQL project

## Critical Conventions

### Naming Standards
- Device codes stored and compared in **UPPERCASE** (see `Comando.CodigoDispositivo` setter)
- Controllers follow `{DeviceType}Controller` pattern (e.g., `BarreraController`, `CamaraController`)
- All inherit from `BaseController` for common auth/logging

### Dependency Injection
- Ninject used throughout (version configured in packages.config)
- `OrquestNinjectModule` for Windows Service DI
- `OrquestWebNinjectModule` for Web Application DI
- Singleton services: `IServicioOrquestador`, `IDriverFactory`, `IAdministradorSuscripciones`
- Transient repositories: `IRepositorio`, `DbContext`

### Configuration
- Connection string: `OrquestadorDb` in App.config/Web.config
- Key settings:
  - `TiempoReintentoReconexion` - Driver reconnection retry interval (ms)
  - `TiempoPing` - Device ping interval (ms)
  - `maxNotificacions` - Max notifications before throttling (minutes)
  - `TimeoutSuscripciones` - Subscription timeout (seconds)

### Error Handling
- Custom exceptions in Dominio (e.g., `DispositivoNoEncontradoException`, `DriverNoEncontradoException`)
- `DriverException` hierarchy for driver-specific errors
- Web controllers catch and display errors via `ViewBag.Mensaje`

## Integration Points

### External Systems
- **SAP**: Consumes `IServicioOrquestadorSAP` for SAP-specific operations
- **Control de Accesos**: Client application consuming orchestrator services
- **ITC Controllers (PLCs)**: Via `IDriverItc` for industrial controllers
- **ALPR Module**: Separate web module for license plate recognition cameras

### Inter-Service Communication
- `net.tcp` binding for orchestrator-to-orchestrator communication
- `basicHttpBinding` for web-to-service and SAP integration
- Windows Authentication (`Ntlm`) for web services
- Subscription pattern for event-driven device notifications

## Common Tasks

### Adding New Device Type
1. Create config entity in `Molinos.Orquest.Dominio\Entidades\Config{DeviceType}.cs`
2. Define driver interface in `Molinos.Orquest.Drivers\IDriver{DeviceType}.cs` extending `IDriver`
3. Implement driver in `Molinos.Orquest.DriversImpl`
4. Create execution commands in `Molinos.Orquest.Dominio\Comandos\Ejecutar*.cs`
5. Add web controller in `Molinos.Orquest.Web\Controllers\{DeviceType}Controller.cs`
6. Register driver in DI configuration (`OrquestNinjectModule`)

### Debugging Device Communication
- Enable TCP logging: Set `DriverBalanzaPuerto.LoguearTCP=true` in App.config
- Check `log4net.config` for log output configuration
- Service logs available when running in DEBUG mode (see `Program.cs` MessageBox pattern)

### Deployment
- MSBuild-based deployment via `build.proj` and `deploy.proj`
- Web Deploy packages generated in `obj\{Configuration}\Package`
- Installation scripts in **InstallationGuide/** for server setup
- Requires IIS, Windows Services, SQL Server access (see `Installation_Guide.md`)

## Solution Configuration
- **Debug**: Local development with SQL Express
- **Jenkins**: CI/CD configuration with test coverage
- **Release**: Production build

## Important Files
- [InstallationGuide/Installation_Guide.md](InstallationGuide/Installation_Guide.md) - Production deployment guide
- [Molinos.Orquest.Dependencias/OrquestNinjectModule.cs](Molinos.Orquest.Dependencias/OrquestNinjectModule.cs) - DI wiring
- [Molinos.Orquest.Drivers/IDriver.cs](Molinos.Orquest.Drivers/IDriver.cs) - Core driver interface
- [Molinos.Orquest.Servicios/IServicioOrquestador.cs](Molinos.Orquest.Servicios/IServicioOrquestador.cs) - Main orchestration service contract
- [Molinos.Orquest.Build/build.targets](Molinos.Orquest.Build/build.targets) - Build paths and configurations
