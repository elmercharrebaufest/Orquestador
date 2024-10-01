# Guía de instalación "Servicio Orquestador de Dispositivos"

### Servidor QA
- ARSFVSQWACAPP00

### Servidor PROD
- POR ASIGNAR

### Prerrequisitos
- **SO**:  
  - Windows Server 2022  
- **Software base con versiones**:  
  - Internet Information Services (Version 10.0.20348.1)
  - msdeploy

### Puertos
Asegurarse de que la regla de firewall permita las conexiones entrantes en los siguientes puertos
- **(80)**: Utilizado para el acceso web general.  
- **(443)**: Utilizado para el acceso web seguro.  
- **(8081)**: Utilizado para el servicio net.tcp. (Comunicación entre Orquestadores)  
- **(8080)**: Utilizado para los servicios expuestos del Orquestador/ServicioSuscriptor.  
- **(8889)**: Utilizado para los servicios a interactuar del Intercomunicador.  
- **(445)**: Utilizado para el servicio SMB cuando se despliega el servicio orquestador y realiza una copia al directorio compartido G:\Orquestador.
- **(135)**: (RPC Endpoint Mapper) Usado para iniciar la conexión remota a servicios de Windows desde TFS.
- **(49152-65535)**: Puertos dinámicos usados después de que se negocia la conexión inicial con RPC.

### Puertos de Cámaras

#### QA
- (6666).

#### PROD
- (10006, 10008, 10010, 10011, 10012, 10021, 10023, 10024, 10027, 10041, 10043, 10046, 10047, 10064, 10065, 10079, 10082, 10087, 10093, 10094, 10095, 10111, 10116, 10122, 10123, 10125, 10138, 10139, 10140, 10141, 10142, 10143, 10144, 10145, 10146, 10147, 10148, 10164, 10172, 10179, 10180, 10182, 10185, 10189, 10190, 10201, 10208, 10209, 10210, 10211, 10212, 10237, 10238, 10239, 10240, 10241, 10242, 10247, 10248, 10257).  


### Usuarios y Roles QA
- **MOLINOSAGRO\UsrSvcAccesosOrqQA**: Usuario de aplicación, utilizado para correr el Application Pool para la conexión con la DB, por el momento Admin.
- **MOLINOSAGRO\UsrSvcTFSAccesosOrqQA**: Usuario de servicio utilizado para desplegar desde TFS, Rol Admin.

### Usuarios y Roles PROD
- **MOLINOSAGRO\UsrSvcAccesosOrqPRD**: Usuario de aplicación, utilizado para correr el Application Pool para la conexión con la DB, por el momento Admin.
- **MOLINOSAGRO\UsrSvcTFSAccesosOrqPRD**: Usuario de servicio utilizado para desplegar desde TFS, Rol Admin.


### Roles and Features
Para que se pueda crear el sitio web para Orquest.web se requieren ciertos prerequisitos. A continuación, se explicitan 2 maneras de hacerlo: Una automatizada ejecutando un script Powershell y otra de forma manual, con el paso a paso a paso

#### Ejecución automática  
- Ejecutar el script `Install-RolesAndFeatures.ps1`, con permisos de administrador, para la instalación de roles y features necesarios.

#### Ejecución manual 
1. **Abrir Server Manager**:
   - Haz clic en el icono de **Server Manager** en la barra de tareas o abre desde el menú de inicio.

2. **Agregar roles y características**:
   - En el panel de Server Manager, selecciona **Agregar roles y características**.
   - En el asistente que aparece, haz clic en **Siguiente** hasta llegar a la selección de roles.

3. **Seleccionar el rol de IIS (Servidor Web)**:
   - Marca la casilla de **Servidor Web (IIS)** y continúa haciendo clic en **Siguiente**.
   - En la sección de **Caracteristicas**, no es necesario agregar ninguna adicional en este paso, así que sigue avanzando.

4. **Seleccionar las caracteristicas de IIS**:
   - En la página de selección de **Servicios de Rol**, asegúrate de habilitar las siguientes caracteristicas:
     - **Desarrollo de aplicaciones**:
       - `ASP.NET 4.8`
       - `Net Extensibility 4.8`
       - `ISAPI Extensions`
       - `ISAPI Filters`
     - **Seguridad**:
       - `Windows Authentication`
     - **Herramientas de administracion**:
       - `Consola de administración de IIS`
       - `IIS Management Scripts and Tools`
     - **Compatibilidad con versiones anteriores**:
       - `Compatibilidad con IIS 6 Metabase y configuración` (necesario para algunas funciones de compatibilidad con versiones anteriores y Web Deploy).

5. **Instalar el rol**:
   - Haz clic en **Instalar** para comenzar la instalación del rol de IIS con las características seleccionadas.

5. **Instalar features**:
   -  **Net Framework 3.5 Features**:
   -  **Net Framework 4.8 Features**:
      - `HTTP - Activation`
---

### Instalar Web Deploy
Además, se requiere la instalación de Web Deploy. A continuación, se explicitan 2 maneras de hacerlo: Una automatizada ejecutando un script Powershell y otra de forma manual, con el paso a paso a paso

#### Ejecución automática
- Ejecutar el script `Install-MSDeploy.ps1`, con permisos de administrador.

#### Ejecución manual
1. **Descargar Web Deploy 3.6**:
   - Descarga la versión más reciente de **Web Deploy** desde el [sitio oficial de Microsoft](https://www.iis.net/downloads/microsoft/web-deploy).
   - Elige la opción **Web Deploy 3.x** que sea compatible con tu servidor (en general, 3.6 es compatible con Windows Server 2022).

2. **Instalar Web Deploy**:
   - Durante la instalación, selecciona las siguientes opciones:
     - **IIS Deployment Handler**: Para permitir despliegues automáticos a IIS.
     - **Remote Agent Service**: Para habilitar el servicio de agente remoto (**MSDeployAgentService**) que permite despliegues remotos.
     - **Client**: Si también deseas que el servidor pueda realizar despliegues hacia otros servidores.

3. **Finalizar la instalación**:
   - Completa la instalación y asegúrate de que Web Deploy se haya instalado correctamente.

4. **Configurar MSDeployAgentService (Web Deployment Agent Service)**
   4.1. **Verificar que el servicio de Web Deploy esté habilitado**: Abre una terminal de comandos como administrador y ejecuta el siguiente comando para verificar que el servicio de **Web Deployment Agent Service** este habilitado:
 
   ```powershell
   Get-Service -Name MsDepSvc
   ```
 
   Esto debería mostrar que el servicio **Web Deployment Agent Service** está en estado **RUNNING**.

   4.2.  **Iniciar el servicio si no está en ejecución**: Si el servicio no está ejecutándose, puedes iniciarlo manualmente:
 
   ```powershell
   Start-Service -Name MsDepSvc
   ```

   4.3. **Configurar el servicio para que se inicie automáticamente**: Para asegurarte de que el servicio está siempre disponible después de reiniciar el servidor, configúralo para que se inicie automáticamente:
 
   ```powershell
   Set-Service -Name MsDepSvc -StartupType Automatic
   ```

### Precondiciones adicionales para instalación de Servicio Orquestador

- Ejecutar script con permisos de administrador `Create-SharedFolder.ps1`
   - Este script creará carpeta compartida `\\Servername\Orquestador` (G:\Orquestador), para que pueda realizar despliegue 
  - Otorgará permisos "Full Access" (hay que ver si se puede cambiar)  para poder realizar remove/copy cuando se despliega el servicio al usuario `molinosagro\tfs_servicio` (tfs_servicio representa la cuenta con la que corre el agente) a la carpeta compartida
    - A menos que se utilice un agente con el usuario  UsrSvcTFSAccesosOrq* debe seguir siendo utilizando tfs_servicio), sino habría que cambiar el pipeline (¿podríamos dejar esta actividad para después?)

### Precondiciones adicionales para instalación de Orquest.Web

- Ejecutar script con permisos de administrador `Create-AppPoolAndSites.ps1`. Este script permitirá 
   - Crear directorios de aplicación.  
   - Crear Application Pool.  
   - Crear Site "Scato".  
   - Crear Virtual Application "Scato/Orquest.web" apuntando a la carpeta G:\Scato\Orquest.web.
   - Configurar en el IIS el método de autenticación a mano (ver de incorporarlo al script).  


### Instalación
#### Ejecutar pipeline CD
#### QA
  - (OrquestadorQA - Accesos).

  
#### PROD
  - (OrquestadorPROD - Accesos).