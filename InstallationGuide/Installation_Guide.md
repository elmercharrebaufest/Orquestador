Nombre Aplicación Orquestador

Ambiente QA
- Servidor

Prerequisitos
- SO
   Windows 2022 
- Software de base con versiones
   Versión de IIS
   Framework
   msdeploy

- Puertos 

- Roles and Features 

   - Correr script ps1 para la instalacion de roles y features necesarios. 

- Usuarios y Roles
   -UsrSvcAccesosOrqQA (Usuario de aplicacion, utilizado para correr el Application Pool, por el momento Admin)
   -UsrSvcTFSAccesosOrqQA (Usuario de servicio utilizado para desplegar desde TFS, Rol Admin).


Instalación

Orquestador

- Carpetas compartidas 
   -Crear carpeta compartida necesaria para poder desplegar el servicio Orquestador, \\Servername\Orquestador

   - Otogarle permisos read/write al usuario molinosagro\tfs_servicio a la carpeta compartida "\\Servername\Orquestador" para que pueda realizar remove/copy cuando se despliega el servicio. (Ver de realizarlo por PS)

- Desplegar pipeline CD (OrquestadorQA - Accesos).


Orquest Web

- Crear directorios de aplicacion
- Crear Application Pool
- Crear Site "Scato"
- Crear Virtual Aplicattion "Scato/Orquest.web"

Correr script powershell moa_create_site_apppooll_IIS.ps1

- Configurar en el IIS metodo de autenticacion a mano. (Ver de incoporarlo al script)
- Desplegar pipeline CD (OrquestadorQA - Accesos).


- Despliegues
 - Ejecutar el pipeline


Base de Datos


### Paso 1: Instalar el rol de IIS y habilitar las caracter�sticas necesarias

1. **Abrir Server Manager**:
   - Haz clic en el �cono de **Server Manager** en la barra de tareas o abre desde el men� de inicio.

2. **Agregar roles y caracter�sticas**:
   - En el panel de Server Manager, selecciona **Agregar roles y caracter�sticas**.
   - En el asistente que aparece, haz clic en **Siguiente** hasta llegar a la selecci�n de roles.

3. **Seleccionar el rol de IIS (Servidor Web)**:
   - Marca la casilla de **Servidor Web (IIS)** y contin�a haciendo clic en **Siguiente**.
   - En la secci�n de **Caracter�sticas**, no es necesario agregar ninguna adicional en este paso, as� que sigue avanzando.

4. **Seleccionar las caracter�sticas de IIS**:
   - En la p�gina de selecci�n de **Servicios de Rol**, aseg�rate de habilitar las siguientes caracter�sticas:
     - **Funciones HTTP comunes**:
       - `Documentos predeterminados`
       - `Redirecci�n de HTTP` (si lo necesitas)
     - **Desarrollo de aplicaciones**:
       - `ASP.NET 4.5`
       - `Extensiones .NET Framework 4.5`
       - `ISAPI Extensions`
       - `ISAPI Filters`
     - **Seguridad**:
       - `Windows Authentication`
     - **Herramientas de administraci�n**:
       - `Console de administraci�n de IIS`
       - `IIS Management Scripts and Tools`
     - **Compatibilidad con versiones anteriores**:
       - `Compatibilidad con IIS 6 Metabase y configuraci�n` (necesario para algunas funciones de compatibilidad con versiones anteriores y Web Deploy).

5. **Instalar el rol**:
   - Haz clic en **Instalar** para comenzar la instalaci�n del rol de IIS con las caracter�sticas seleccionadas.

### Paso 2: Instalar .NET Framework 4.5 y los componentes necesarios

1. **Instalar .NET Framework 4.5**:
   - Aseg�rate de que **.NET Framework 4.5** est� instalado en el servidor. Si no est� instalado, puedes agregarlo usando el mismo asistente de **Agregar roles y caracter�sticas**.
   - En la secci�n de **Caracter�sticas**, selecciona `.NET Framework 4.5` si no est� ya habilitado.

2. **Instalar el paquete de .NET Framework 4.5** (si no est� presente):
   - Descarga el instalador de `.NET Framework 4.5` desde el sitio de Microsoft si no est� ya instalado y sigue los pasos para su instalaci�n.

### Paso 3: Instalar Web Deploy

1. **Descargar Web Deploy 3.6**:
   - Descarga la versi�n m�s reciente de **Web Deploy** desde el [sitio oficial de Microsoft](https://www.iis.net/downloads/microsoft/web-deploy).
   - Elige la opci�n **Web Deploy 3.x** que sea compatible con tu servidor (en general, 3.6 es compatible con Windows Server 2022).

2. **Instalar Web Deploy**:
   - Durante la instalaci�n, selecciona las siguientes opciones:
     - **IIS Deployment Handler**: Para permitir despliegues autom�ticos a IIS.
     - **Remote Agent Service**: Para habilitar el servicio de agente remoto (**MSDeployAgentService**) que permite despliegues remotos.
     - **Client**: Si tambi�n deseas que el servidor pueda realizar despliegues hacia otros servidores.

3. **Finalizar la instalaci�n**:
   - Completa la instalaci�n y aseg�rate de que Web Deploy se haya instalado correctamente.

### Paso 4: Configurar MSDeployAgentService (Web Deployment Agent Service)

1. **Verificar que el servicio de Web Deploy est� habilitado**:
   - Abre una terminal de comandos como administrador y ejecuta el siguiente comando para verificar que el servicio de **MSDeployAgentService** est� habilitado:

   ```cmd
   sc query msdepsvc
   ```

   Esto deber�a mostrar que el servicio **MSDEPLOYAGENTSERVICE** est� en estado **RUNNING**.

2. **Iniciar el servicio si no est� en ejecuci�n**:
   - Si el servicio no est� ejecut�ndose, puedes iniciarlo manualmente:

   ```cmd
   net start msdepsvc
   ```

3. **Configurar el servicio para que se inicie autom�ticamente**:
   - Para asegurarte de que el servicio est� siempre disponible despu�s de reiniciar el servidor, config�ralo para que se inicie autom�ticamente:

   ```cmd
   sc config msdepsvc start= auto
   ```


### Paso 6: Verificar el firewall

2. **Verificar reglas de firewall**:
   - Aseg�rate de que la regla de firewall permita las conexiones entrantes en el puerto **80** para **MSDEPLOYAGENTSERVICE**.

### Paso 7: Desplegar la aplicaci�n 

### Resumen:

1. Instalar IIS con las caracter�sticas necesarias para .NET 4.5.
2. Instalar Web Deploy y habilitar **MSDeployAgentService**.
3. Configurar permisos para el usuario de despliegue (administrador o **Web Deploy Administrators**).
4. Asegurar que el puerto 80 est� habilitado en el firewall.
5. Ejecutar pipeline CD OrquestadorPRD - Accesos