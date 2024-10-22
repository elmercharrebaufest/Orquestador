# Guía de instalación "Orquestador de Dispositivos"

## Prerrequisitos
1. Servidor de Aplicaciones por ambiente
2. Contar con usuarios por ambiente:
    - De servicios
    - Para acceso a servidores
3. Existencia de Bases de datos por ambiente
4. Software de base
5. Conexiones entrantes y salientes por ambiente

---

### 1. Servidor de Aplicaciones por ambiente 

 | Entorno | Nombre de Servidor | Tipo   |
 |:-------:|--------------------|--------|
 | QA      | ARSFVSQWACAPP00    | Windows Server 2022 |
 | PROD    | POR ASIGNAR        | Windows Server 2022 |

Deben tener features y roles configurados. Para el detalle ver sección 'Roles y Features".

---

### 2. Usuarios y Roles por ambiente

**Usuarios de Servicio**

Nota: Los siguientes usuarios no se le vencen las claves:

 | Entorno | Nombre de Usuario | Descripción |
 |:-------:|--------------------|------|
 | QA      | **MOLINOSAGRO\UsrSvcAccesosOrqQA**    | Usuario de aplicación, utilizado para correr el Application Pool y para el Servicio Windows para la conexión con la DB, por el momento Admin. |
 | QA      |**MOLINOSAGRO\UsrSvcTFSAccesosOrqQ** | Usuario de servicio utilizado para desplegar desde TFS, Rol Admin. |
 | PROD    | **MOLINOSAGRO\UsrSvcAccesosOrqPRD**        | Usuario de aplicación, utilizado para correr el Application Pool para la conexión con la DB, por el momento Admin. |
 | PROD    | **MOLINOSAGRO\UsrSvcTFSAccesosOrqP** | Usuario de servicio utilizado para desplegar desde TFS, Rol Admin. |

<br />

**Usuarios con accesos/permisos a servidores**
  
  Se requieren usuarios de dominio individuales para acceso
  - Para acceso por RDP a servidor aplicativo
  - Para acceder a Bases de Datos
  - Para acceder a Repositorio de Código

---

### 3. Existencia de Bases de datos por ambiente
  
   | Entorno | Nombre de Base de datos| Servidor/cluster |
   |:--:|--|--|
   | QA | MoaOrquestadorAccesosQA | Cluster SQL QA (ACCESOSAGLSQA.molinosagro.ad)|
   | PROD | MoaOrquestadorAccesos | Cluster SQL PROD (ACCESOSAGLSPRD.molinosagro.ad)| 

---

### 4. Software de base
   1. Internet Information Services (Version 10.0.20348.1)
   2. Msdeploy (Version 4.0)

---

### 5. Conexiones entrantes y salientes por ambiente
  
Asegurarse que las reglas de firewall permitan las siguientes conexiones:

*Referentes: Paolo Magrini e Iván De Angelis*

Nota: Los usuarios de red que utilizarán la aplicación Orquest.Web serán configurados por la aplicación y de momento son:
   - Mantenimiento de planta
   - TI (Paolo Magrini, Patricio Manna, Gabriel Cayo)
   - Referentes de usuarios (Marian Rabellato)
   - Microinformática
   - Usuarios de VPN para bindar servicios de gestión de aplicación (por ejemplo, proveedores como Baufest, Huenei, etc.)

#### QA - Entrantes

|  | Origen | Destino | Puerto | Detalle |
|:------:|:------:|:-------:|:------:|:--------:|
| *Accesos externos a servidor* |  
| 1 | Usuarios de red | arsfvsqwacapp00 (172.20.250.20) | 3389 | RDP |
| *Accesos externos por Orquest.Web* |  
| 1 | Usuarios de red  | arsfvsqwacapp00 (172.20.250.20) | 80 | Utilizado para el acceso web general |
| 2 | Usuarios de red | arsfvsqwacapp00 (172.20.250.20) | 443 | Utilizado para el acceso web seguro |
| *Servicios expuestos*
| 1 | Aplicación "Control de Accesos" en GSLOACCESOS01 (10.12.42.60) u otros clientes que requieran consumir (ej.: PostMan / SoapUI) | arsfvsqwacapp00 (172.20.250.20) | 8080 | Servicio expuesto por protocolo *http* del Orquestador para realizar acciones sobre dispositivos (ServicioOrquestador, ServicioOrquestadorSAP, ServicioSuscriptor) |
| 2 | Componente Orquestador local (arsfvsqwacapp00 (172.20.250.20)) y remotos (-) | arsfvsqwacapp00 (172.20.250.20) | 8081 | Servicio expuesto por protocolo *net.tcp* del orquestador, para comunicación entre Orquestadores. Aclaración: En Accesos de momentos solo existirá un componente Orquestador |
| *Servicio Intercomunicador* | | | | Aún inexistente para Accesos
| 1 | Servicio intercomunicador  | arsfvsqwacapp00 (172.20.250.20) | 8889 | Utilizado para los servicios a interactuar del Intercomunicador |
| *CI/CD* |
| 1 | GVICTFS01 (10.12.12.59) | arsfvsqwacapp00 (172.20.250.20) | 445 | Utilizado para el servicio SMB cuando se despliega el servicio orquestador y realiza una copia al directorio compartido ([unidad]:\Orquestador) |
| 2 | GVICTFS01 (10.12.12.59) | arsfvsqwacapp00 172.20.250.20 | 135 | RPC Endpoint Mapper. Usado para iniciar la conexión remota a servicios de Windows desde TFS |
| 3 | GVICTFS01 (10.12.12.59) | arsfvsqwacapp00 172.20.250.20 | 49152-65535 | Puertos dinámicos usados por TFS, después de que se negocia la conexión inicial con RPC |
| *Cámaras* |
| 10 | Cámara xxxx | arsfvsqwacapp00 172.20.250.20 | 6666 | La(s) cámara(s) se conectarán a este puerto |

#### QA - Salientes

|  | Origen | Destino | Puerto | Detalle |
|:------:|:------:|:-------:|:------:|:--------:|
| *Base de datos* | | | | |
| 1 | arsfvsqwacapp00 (172.20.250.20) | ACCESOSAGLSQA.molinosagro.ad | 1450 | Servicio Orquestador y Orquest.Web acceden a Base de Datos SQL Server `MoaOrquestadorAccesosQA` |
| *Otros Servicios* | | | | NO REQUERIDOS PARA CONTROL de ACCESOS |
| 1 | arsfvsqwacapp00 (172.20.250.20) | - | - | Reconocimiento de Patente (http://[server]/Orquest.ModuloALPR/ServicioALPR.svc)  |
| 2 | arsfvsqwacapp00 (172.20.250.20) | arsfvsqwacapp00 (172.20.250.20) | 8899 | NIRS - Calidad de granos (net.tcp://localhost:8899/Nova/remoteAPI)  |
| *Servicio Suscriptor* | | | | |
| 1 | arsfvsqwacapp00 (172.20.250.20) | gsloaccesos01 (url completa) | 8080 | (Pedidos_QA/Servicios/ServicioSuscriptor.svc)  |
| *Dispositivos varios y/o Simuladores* | | | | Configurables por Orquest.Web. NO SON FIJOS
| 1 | arsfvsqwacapp00 (172.20.250.20) | 10.10.104.6 | 1880 | Acceso a http://10.10.104.6:1880/ui/  |
| 2..n | arsfvsqwacapp00 (172.20.250.20) | 10.10.104.6 | * | Ver con Paolo Magrini el resto de los dispositivos que son totalmente configurables  |

#### PROD - Entrantes

| Num | Origen | Destino | Puerto | Detalle |
|:------:|:------:|:-------:|:------:|:--------:|
| *Accesos externos a servidor* |  
| 1 | Usuarios de red | POR DEFINIR | 3389 | RDP |
| *Accesos externos por Orquest.Web* |  
| 1 | Usuarios de red  | POR DEFINIR | 80 | Utilizado para el acceso web general |
| 2 | Usuarios de red | POR DEFINIR | 443 | Utilizado para el acceso web seguro |
| *Servicios expuestos*
| 1 | Aplicación "Control de Accesos" en GSLOACCESOS01 (10.12.42.60) u otros clientes que requieran consumir (ej.: PostMan / SoapUI) | POR DEFINIR | 8080 | Servicio expuesto por protocolo *http* del Orquestador para realizar acciones sobre dispositivos (ServicioOrquestador, ServicioOrquestadorSAP, ServicioSuscriptor) |
| 2 | Componente Orquestador local (POR DEFINIR) y remotos (-) | POR DEFINIR | 8081 | Servicio expuesto por protocolo *net.tcp* del orquestador, para comunicación entre Orquestadores. Aclaración: En Accesos de momentos solo existirá un componente Orquestador |
| *Servicio Intercomunicador* | | | | Aún inexistente para Accesos
| 1 | Servicio intercomunicador | POR DEFINIR | 8889 | Utilizado para los servicios a interactuar del Intercomunicador |
| *CI/CD* |
| 1 | GVICTFS01 (10.12.12.59) | POR DEFINIR | 445 | Utilizado para el servicio SMB cuando se despliega el servicio orquestador y realiza una copia al directorio compartido ([unidad]:\Orquestador) |
| 2 | GVICTFS01 (10.12.12.59) | POR DEFINIR | 135 | RPC Endpoint Mapper. Usado para iniciar la conexión remota a servicios de Windows desde TFS |
| 3 | GVICTFS01 (10.12.12.59) | POR DEFINIR | 49152-65535 | Puertos dinámicos usados por TFS, después de que se negocia la conexión inicial con RPC |
| *Cámaras* |
| 10.1 a n | Cámara xxxx | POR DEFINIR | 10006, 10008, 10010, 10011, 10012, 10021, 10023, 10024, 10027, 10041, 10043, 10046, 10047, 10064, 10065, 10079, 10082, 10087, 10093, 10094, 10095, 10111, 10116, 10122, 10123, 10125, 10138, 10139, 10140, 10141, 10142, 10143, 10144, 10145, 10146, 10147, 10148, 10164, 10172, 10179, 10180, 10182, 10185, 10189, 10190, 10201, 10208, 10209, 10210, 10211, 10212, 10237, 10238, 10239, 10240, 10241, 10242, 10247, 10248, 10257 | Cada cámara se conectará a uno de los puertos a la vez |

#### PROD - Salientes
Hacia Control de accesos
Hacia los dispositivos (VLAN de cada dispositivo)

|  | Origen | Destino | Puerto | Detalle |
|:------:|:------:|:-------:|:------:|:--------:|
| *Base de datos* | | | | |
| 1 | POR DEFINIR | ACCESOSAGLSPRD.molinosagro.ad | 1433 | Servicio Orquestador y Orquest.Web acceden a Base de Datos SQL Server `MoaOrquestadorAccesos` |
| *Otros Servicios* | | | | NO REQUERIDOS PARA CONTROL de ACCESOS |
| 1 | POR DEFINIR | - | - | Reconocimiento de Patente (http://[server]/Orquest.ModuloALPR/ServicioALPR.svc)  |
| 2 | POR DEFINIR | POR DEFINIR | 8899 | NIRS - Calidad de granos (net.tcp://localhost:8899/Nova/remoteAPI)  |
| *Servicio Suscriptor* | | | | |
| 1 | POR DEFINIR | gsloaccesos01 (url completa) | 8080 | (Pedidos/Servicios/ServicioSuscriptor.svc)  |
| *Dispositivos varios y/o Simuladores* | | | | Configurables por Orquest.Web. NO SON FIJOS
| 1 | POR DEFINIR | 10.10.104.6 | 1880 | Acceso a http://10.10.104.6:1880/ui/  |
| 2..n | POR DEFINIR | 10.10.104.6 | * | Ver con Paolo Magrini el resto de los dispositivos que son totalmente configurables  |

---

## Instalación desde cero
### Roles and Features
Para que se pueda crear el sitio web para Orquest.web se requieren, además de los puntos mencionados anteriormente cumplir con los siguientes pasos en una intalación desde cero. A continuación, se explicitan 2 maneras de hacerlo: Una automatizada ejecutando un script Powershell y otra de forma manual, con el paso a paso.

#### Ejecución automática

1. **Descargar el Archivo de Features**  
   Descarga el archivo `InstalledFeatures.csv` y colócalo en el directorio `C:\` del servidor en proceso de configuración. El script buscará este archivo en esa ubicación de forma predeterminada.

2. **Ejecución del Script**  
   Ejecuta el script `Install-RolesAndFeatures.ps1` con permisos de administrador. Este script instalará los roles y features necesarios según lo especificado en el archivo `InstalledFeatures.csv`.

3. **Modificación de la Ruta del Archivo (Opcional)**  
   Si deseas utilizar una ruta diferente para el archivo `InstalledFeatures.csv`, ajusta la última línea del script para especificar el nuevo path antes de ejecutar el script.

---

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
   -  **Net Framework 3.5 Features**: (Ver si no está por defecto o si es necesario)
   -  **Net Framework 4.8 Features**:
      - `HTTP - Activation`
---

### Instalar Web Deploy
Además, se requiere la instalación de Web Deploy. A continuación, se explicitan 2 maneras de hacerlo: Una automatizada ejecutando un script Powershell y otra de forma manual, con el paso a paso a paso

#### Ejecución automática

1. **Configuración de la Ruta de Descarga del MSI**  
   El script `Install-MSDeploy.ps1` utiliza una URL predefinida para descargar el instalador MSI de MSDeploy. Es importante verificar que el path indicado en el script siga funcionando. Se recomienda siempre buscar el archivo de instalación en el [sitio oficial de Microsoft](https://www.iis.net/downloads/microsoft/web-deploy) para asegurar su disponibilidad y autenticidad.

2. **Acceso a Internet**  
   Asegúrate de que el servidor tenga acceso a internet para que el script pueda descargar el archivo MSI. En caso de no contar con acceso a internet, descarga previamente el instalador, transfiérelo manualmente al servidor y especifica la ruta en la variable (`$msiPath`), y comenta las líneas 8 y 9 del script, donde se realiza la descarga automática, para evitar errores de conexión.

3. **Ejecución del Script**  
   Ejecuta el script `Install-MSDeploy.ps1` con permisos de administrador. Esto instalará MSDeploy en el servidor utilizando el archivo MSI, ya sea descargado automáticamente o transferido manualmente.

---

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
      - Actualmente dentro del IIS debe de estar la entrada **Windows Authentication** en el valor **Enabled** a nivel de la aplicación.


## Instalación incremental
### Ejecutar pipeline CD
Debe ejecutarse el pipeline correspondiente que desplegará la versión de Orquest.Web y Servicio Orquestador (y opcionalmente la actualización de base de datos) según el ambiente seleccionado

Link: http://gvictfs01.molinosagro.ad:8080/tfs/MOLINOS_AGRO/Orquestador/_release

#### QA
  - (OrquestadorQA - Accesos)
    - http://gvictfs01.molinosagro.ad:8080/tfs/MOLINOS_AGRO/Orquestador/_release?definitionId=11&_a=releases

#### PROD
  - (OrquestadorPROD - Accesos)
    - http://gvictfs01.molinosagro.ad:8080/tfs/MOLINOS_AGRO/Orquestador/_release?definitionId=12&_a=releases