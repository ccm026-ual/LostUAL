# LostUAL

Aplicación para la gestión de objetos perdidos en la Universidad de Almería.  
Desarrollada para la asignatura "Desarrollo Web/Móvil" del Máster en Ingeniería Informática por la Universidad de Almería.

## Instalación

### Requisitos 

* Visual Studio 2022 Community
* .NET 9 SDK

### Instalación

1) Clonar repositorio
   
```
git clone https://github.com/ccm026-ual/LostUAL
```
2) Ejecutar proyecto

Si se carga la solución en Visual Studio, se puede lanzar directamente seleccionando el perfil Proyecto (que lanza Api y Web) como elemento de inicio y dándole a run.

<img width="199" height="47" alt="image" src="https://github.com/user-attachments/assets/753f1b4a-70fb-42ee-b6ab-f3bb1baaec5d" />  

Si se prefiere lanzar la solución sin tener que recurrir a Visual Studio, se pueden ejecutar por línea de comandos de manera separada el proyecto para la Api y el proyecto para la Web desde la raíz de la solución:

* Api:

```
dotnet run --project .\LostUAL.Api\LostUAL.Api.csproj
```

* Web:

```
dotnet run --project .\LostUAL.Web\LostUAL.Web.csproj
```
La Api se encuentra en https://localhost:7178. Swagger se encuentra disponible en https://localhost:7178/swagger  
La Web se encuentra disponible en https://localhost:7211  

## Uso  
### Acceso 

La aplicación ya cuenta con una pequeña base de datos con usuarios y posts creados para comprobar la funcionalidad. Las credenciales para los usuarios y roles son:  

| Rol | Email | Contraseña |
|----------|:----------:|:----------:|
| Usuario        |    usuario{N}@ual.es (donde N es número entero)     | Usuario1!        |
| Admin/mod        | admin_lostual@ual.es        | Admin123!        |

Si se prefiere partir de una base de datos vacía con solo el Admin/mod definido, ejecutar desde una ventana de comandos:

```
dotnet ef database drop -p LostUAL.Data -s LostUAL.Api  
dotnet ef database update -p LostUAL.Data -s LostUAL.Api  
```

### Funcionalidades principales   

**Usuarios**
* Creación de posts de encontrado/perdido
* Reclamación de posts de otros usuarios
* Gestión de las reclamaciones (aceptar reclamación, chat con el interesado, confirmación de resolución, etc...)
* Posibilidad de reportar posts y chats con contenido inapropiado

**Admin/mod**
* Todas las funcionalidades de Usuarios
* Gestión de los reportes (desestimar, cerrar post, bloquear usuarios, etc...)

### Flujos de estado principales

**Estados de un post**
<img width="894" height="484" alt="image" src="https://github.com/user-attachments/assets/8f32cd7d-b100-46fa-b8ce-4bc16c08836d" />

**Estados de una reclamación**
<img width="904" height="520" alt="image" src="https://github.com/user-attachments/assets/c1eb7204-8855-47ad-984f-61bfd546411c" />

**Estados de un reporte**
<img width="923" height="396" alt="image" src="https://github.com/user-attachments/assets/e1734d6d-a2b5-4cb0-ba6c-5e9ec78d0cec" />

### Estructura de la Web  

Un usuario tiene acceso a diferentes servicios dependiendo de si está autenticado o si es o no mod.

**Usuario sin autenticar**

* **Página principal (/home):** muestra preview de últimos posts publicados y manda al login.
* **Login (/login):** iniciar sesión.
* **Registro (/register):** registro de un nuevo usuario.
* **AboutUs (/about):** información sobre LostUAL.

**Usuario autenticado**

* **Página principal (/home):** muestra posts publicados, con filtros según el tipo de post, el estado, fecha de publicación, categoría...
* **Detalle del post (/posts/{id}:** muestra los detalles de un post concreto, permitiendo realizar acciones (reclamar post, cerrar post, reportar post...)
* **Nuevo post (/new-post):** formulario para publicar un nuevo post.
* **Mis posts (/my-posts):** panel de gestión de los posts publicados por el usuario: filtros por tipo, estado... y ver reclamaciones asociados a cada uno.
* **Mis reclamaciones (/my-claims):** panel de gestión de las reclamaciones realizadas sobre otros posts: avisos de nuevo mensaje recibido, acceso a los chats, filtros por estado, tipo...
* **Reclamaciones recibidas (/inblox-claims):** panel de gestión de reclamaciones recibidas para los posts del usuario: avisos de nuevo mensaje recibido, acceso a los chats, filtros por estado, tipo...
* **Chat (/conversations/{id}):** acceso al chat propio de cada reclamación: permite realizar acciones como aceptar una reclamación, rechazar una reclamación, reportar...
* **Mi cuenta (/account):** datos de la cuenta y posibilidad de modificar contraseña y email asociados.

**Admin/Moderador**

* **Todos los servicios de Usuario**
* **Moderación (/reports)**: panel de gestión de los distintos reportes recibidos
* **Detalle del post (Moderación)(/posts/{id}?postReportId={id}**: detalle del post con la información del reporte asociado y acciones referentes a él (desestimar, cerrar post, bloquear usuario y cerrar post)
* **Chat (Moderación) (/conversations/{id}?reportId={id}**: copia del chat reportado con la información del reprote asociado y acciones referentes a él (desestimar, bloquear usuario)








