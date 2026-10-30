# 🎁 Amigo Secreto 2026 - Portal Web Interactivo (Blazor Server & SQLite)

Portal web interactivo desarrollado con **Blazor Server** (arquitectura moderna Visual Studio 2026 / .NET 10) para la gestión e intercambio de regalos de **Amigo Secreto**.

---

## 🚀 Características Principales

### 🔍 1. Portal Público de Búsqueda de Participantes
* **Búsqueda en tiempo real e insensible a mayúsculas/minúsculas (Case-Insensitive)**:
  * Permite buscar por **nombre**, **apellido** o ambos (ejemplo: `carlos`, `mendoza`, `maria fernanda`, `valentina`).
  * Normalización con soporte para tildes y diacríticos (ejemplo: buscar `rodriguez` encuentra a `Rodríguez`).
* **Filtros rápidos por Departamento**: Navegación ágil por áreas (Tecnología, Banca Corporativa, Finanzas, Recursos Humanos, Operaciones, etc.).
* **Contador dinámico de coincidencias**: Visualiza al instante cuántas personas coinciden con los criterios de búsqueda.
* **Tarjeta de Participante interactiva**: Con avatar personalizado, iniciales, indicador de cantidad de gustos, cosas que le desagradan y alertas médicas/alergias.

### 📋 2. Panel Deslizante Interactivo de Detalles
Al hacer clic en cualquier tarjeta se abre un panel lateral (*Offcanvas Drawer*) con animaciones fluidas y fondo desenfocado (*glassmorphism*):
* 🎁 **Cosas que le gustan / Ideas de regalo**: Lista estructurada de preferencias y deseos.
* ❌ **Cosas que NO le gustan (Evitar)**: Lista detallada de artículos, colores o regalos a evitar.
* ⚠️ **Alergias y Restricciones**: Alertas de salud (alergias alimentarias a frutos secos, mariscos, intolerancias a la lactosa o alergias a materiales).
* 📏 **Detalles adicionales**: Talla de camisa, calzado, color favorito y notas personalizadas.
* 📋 **Copiar lista de ideas**: Botón de un solo clic que copia el resumen formateado al portapapeles.

### 🛡️ 3. Portal Administrativo Seguro
* **Autenticación de Acceso**:
  * **Usuario:** `admin`
  * **Contraseña:** `bnc2026`
  * Botón de ayuda para auto-completar credenciales de prueba.
* **Dashboard con Métricas**:
  * Total de participantes registrados.
  * Personas con preferencias y gustos cargados.
  * Personas con alergias registradas (alerta de catering y seguridad).
  * Departamentos activos.
* **Gestión Completa (CRUD)**:
  * **Crear nuevo participante**: Formulario intuitivo con datos personales, asignación de departamento, selección de color de avatar en vivo, listas de gustos, disgustos y alergias.
  * **Editar participante**: Modificación en tiempo real de cualquiera de los campos.
  * **Eliminar participante**: Con modal de confirmación y advertencia.
  * **Restablecer datos de prueba**: Botón para restaurar la lista de demostración predeterminada.

---

## 🗄️ Base de Datos SQLite

La aplicación utiliza **SQLite** mediante **Entity Framework Core**:
* Archivo de base de datos local: `amigosecreto.db`
* Inicialización automática al iniciar el proyecto (`DbInitializer.cs`), pre-cargando 8 participantes completos y realistas.
* Uso de `IDbContextFactory<AppDbContext>` para máxima concurrencia y seguridad de hilos en Blazor Server.

---

## 💻 Cómo Ejecutar el Proyecto

### Desde Visual Studio 2026:
1. Abre la solución `AmigoSecreto.sln` o la carpeta del proyecto en Visual Studio 2026.
2. Presiona **F5** o haz clic en **Iniciar Depuración** (`http` o `https`).

### Desde la línea de comandos (.NET CLI):
```bash
# Restaurar dependencias y compilar
dotnet build

# Iniciar la aplicación
dotnet run --launch-profile http
```

La aplicación estará disponible en:
* **Portal de Búsqueda:** `http://localhost:5222`
* **Portal Administrativo:** `http://localhost:5222/admin`
