# 🕒 Sistema de Control de Fichaje (Compliance España 2025)

Sistema de registro de jornada laboral desarrollado en **.NET 9** y **ASP.NET Core MVC**. Diseñado para cumplir con el **Real Decreto-ley 8/2019** de España, garantizando inmutabilidad, trazabilidad y geolocalización.

## 🚀 Características Principales

*   **Arquitectura Inmutable:** Los registros de tiempo nunca se sobrescriben (Event Sourcing simplificado).
*   **Geolocalización Real:** Guarda coordenadas GPS precisas y genera enlaces directos a Google Maps.
*   **Validación de Estado:** Impide fichajes incoherentes (ej. no permite salir si no has entrado).
*   **Notificaciones SMTP:**
    *   Correo automático al fichar salida.
    *   **Vigilante Automático (Background Service):** Detecta si un empleado olvidó fichar salida después de 8 horas.
*   **Reportes:** Descarga inmediata de historial en formato CSV (Excel).

## 🛠️ Tecnologías

*   **Backend:** .NET 9 (C#), Entity Framework Core.
*   **Frontend:** Razor Pages (MVC), Bootstrap 5, JavaScript Geolocation API.
*   **Base de Datos:** SQL Server (Compatible con LocalDB y Azure SQL).
*   **Seguridad:** User Secrets para manejo de credenciales SMTP.

---

## ⚡ Guía de Instalación Rápida

Sigue estos pasos para levantar el proyecto en tu máquina local:

### 1. Requisitos Previos
*   Tener instalado el [.NET SDK 9.0](https://dotnet.microsoft.com/download).
*   Tener SQL Server Express o LocalDB habilitado.

### 2. Clonar el repositorio
git clone https://github.com/javier008l/SistemaFichaje.git
cd SistemaFichaje

### 2.1. Instalar Dependencias (NuGet)
Restaura todas las librerías necesarias (Entity Framework, SQL Server, Herramientas de diseño):
dotnet restore


Si es la primera vez que trabajas con bases de datos en este PC, instala la herramienta global de EF:
dotnet tool install --global dotnet-ef


### 3. Configurar Base de Datos
El proyecto viene configurado para usar `LocalDB` por defecto.
1.  Abre la terminal en la carpeta del proyecto.
2.  Ejecuta las migraciones para crear la base de datos y la tabla `FichajeEventos`:

dotnet ef database update

*(Si da error, asegúrate de tener las herramientas de EF instaladas: `dotnet tool install --global dotnet-ef`)*

### 4. Configurar el Correo (SMTP)
Este proyecto usa **User Secrets** para no exponer contraseñas en GitHub. Para que el envío de correos funcione:

1.  Abre la terminal en la carpeta del proyecto.
2.  Ejecuta estos comandos con TUS credenciales reales (ej. Gmail App Password):

dotnet user-secrets init
dotnet user-secrets set "GmailSettings:Email" "tu_correo@gmail.com"
dotnet user-secrets set "GmailSettings:AppPassword" "tu_contraseña_de_aplicacion_16_caracteres"
dotnet user-secrets set "GmailSettings:SenderName" "Sistema RRHH"


### 5. Ejecutar la aplicación
dotnet watch run

La aplicación estará disponible en `http://localhost:5209` (o el puerto que indique la consola).

---

## 🧪 Cómo probar la Geolocalización (GPS)

Si estás probando desde un PC de escritorio, es probable que la ubicación sea imprecisa (basada en IP). Para simular ubicación exacta:

1.  Abre la web en Chrome/Brave.
2.  Abre las **DevTools** (`F12`).
3.  Presiona `Ctrl + Shift + P` y escribe **"Sensors"**.
4.  En la pestaña "Location", selecciona una ciudad (ej. "London").
5.  Ficha en la web. El registro guardará las coordenadas de Londres.

---

## 📋 Estructura del Proyecto

*   `/Controllers`: Lógica de negocio (`FichajeController.cs`).
*   `/Services`: Lógica de fondo (`EmailService.cs` y el vigilante `VigilanteService.cs`).
*   `/Models`: Definición de datos (`FichajeEvento.cs` usando GUIDs).
*   `/Views`: Interfaz de usuario Razor.

## 🤝 Integración

El sistema está diseñado como un módulo satélite. El `FichajeController` utiliza una variable `_usuarioSimulado` (Guid) para pruebas.
> **Para Producción:** Reemplaza `_usuarioSimulado` por `User.FindFirst(ClaimTypes.NameIdentifier)?.Value` para leer el ID del usuario logueado en tu sistema de autenticación real.

---
Hecho con .NET
