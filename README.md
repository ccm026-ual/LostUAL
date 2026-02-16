# LostUAL

Aplicación para la gestión de objetos perdidos en la Universidad de Almería.  
Desarrollada para la asignatura "Desarrollo Web/Móvil" del Máster en Ingeniería Informática por la Universidad de Almería.

## Instalación

**Requisitos:**  

* Visual Studio 2022 Community
* .NET 9 SDK

**Instalación:**

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

