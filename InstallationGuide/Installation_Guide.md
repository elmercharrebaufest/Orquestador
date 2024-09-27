# Nombre Aplicación: Orquestador

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

- **(80)**: Utilizado para el acceso web general.  
- **(443)**: Utilizado para el acceso web seguro.  
- **(8081)**: Utilizado para el servicio net.tcp. (Comunicación entre Orquestadores)  
- **(8080)**: Utilizado para los servicios expuestos del Orquestador/ServicioSuscriptor.  
- **(8889)**: Utilizado para los servicios a interactuar del Intercomunicador.  

### Puertos de Cámaras

#### PROD
- (10006, 10008, 10010, 10011, 10012, 10021, 10023, 10024, 10027, 10041, 10043, 10046, 10047, 10064, 10065, 10079, 10082, 10087, 10093, 10094, 10095, 10111, 10116, 10122, 10123, 10125, 10138, 10139, 10140, 10141, 10142, 10143, 10144, 10145, 10146, 10147, 10148, 10164, 10172, 10179, 10180, 10182, 10185, 10189, 10190, 10201, 10208, 10209, 10210, 10211, 10212, 10237, 10238, 10239, 10240, 10241, 10242, 10247, 10248, 10257).  

#### QA
- (6666).

### Usuarios y Roles QA
- **UsrSvcAccesosOrqQA**: Usuario de aplicación, utilizado para correr el Application Pool para la conexión con la DB, por el momento Admin.  
- **UsrSvcTFSAccesosOrqQA**: Usuario de servicio utilizado para desplegar desde TFS, Rol Admin.  

### Usuarios y Roles PROD
- **UsrSvcAccesosOrqPRD**: Usuario de aplicación, utilizado para correr el Application Pool para la conexión con la DB, por el momento Admin.  
- **UsrSvcTFSAccesosOrqPRD**: Usuario de servicio utilizado para desplegar desde TFS, Rol Admin.  


### Roles and Features  
- Correr script `ps1` para la instalación de roles y features necesarios.  
---

## Instalación

### Orquestador

- **Carpetas compartidas**  
  - Crear carpeta compartida necesaria para poder desplegar el servicio Orquestador, `\\Servername\Orquestador` (G:\Orquestador).  
  - Otorgarle permisos read/write al usuario `molinosagro\tfs_servicio` (tfs_servicio representa la cuenta con la que corre el agente) a la carpeta compartida `\\Servername\Orquestador` para que pueda realizar remove/copy cuando se despliega el servicio. (Ver de realizarlo por PS).

- **Desplegar pipeline CD**  
  - (OrquestadorQA - Accesos).  

### Orquest Web

- Crear directorios de aplicación.  
- Crear Application Pool.  
- Crear Site "Scato".  
- Crear Virtual Application "Scato/Orquest.web" apuntando a la carpeta G:\Scato\Orquest.web.  

- Correr script PowerShell `moa_create_site_apppooll_IIS.ps1`.  
- Configurar en el IIS el método de autenticación a mano (ver de incorporarlo al script).  
- Desplegar pipeline CD (OrquestadorQA - Accesos).  

### Despliegues
- Ejecutar el pipeline.

### Instalar el rol de IIS y habilitar las características necesarias

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


### Instalar Web Deploy

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

### Configurar MSDeployAgentService (Web Deployment Agent Service)

1. **Verificar que el servicio de Web Deploy está habilitada**:
   - Abre una terminal de comandos como administrador y ejecuta el siguiente comando para verificar que el servicio de **MSDeployAgentService** este habilitado:

   ```cmd
   sc query msdepsvc
   ```

   Esto debería mostrar que el servicio **MSDEPLOYAGENTSERVICE** está en estado **RUNNING**.

2. **Iniciar el servicio si no está en ejecución**:
   - Si el servicio no está ejecutándose, puedes iniciarlo manualmente:

   ```cmd
   net start msdepsvc
   ```

3. **Configurar el servicio para que se inicie automáticamente**:
   - Para asegurarte de que el servicio está siempre disponible después de reiniciar el servidor, configúralo para que se inicie automáticamente:

   ```cmd
   sc config msdepsvc start= auto
   ```

### Verificar el firewall

1. **Verificar reglas de firewall**:
   - Asegúrate de que la regla de firewall permita las conexiones entrantes en el puerto **80** para **MSDEPLOYAGENTSERVICE**.

