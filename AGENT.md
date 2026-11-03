# 🤖 AGENT.md - Contexto y Guía de Continuación en Casa
## Proyecto: Portal Interactivo Amigo Secreto BNC 2026

Este documento contiene toda la información técnica, arquitectónica y operativa para que cualquier desarrollador o agente de Inteligencia Artificial retome y continúe el trabajo en este repositorio desde casa.

---

## 📌 1. Resumen Ejecutivo del Proyecto
El **Portal Amigo Secreto BNC 2026** es una aplicación web interactiva desarrollada con **Blazor Server** (.NET 10 / Visual Studio 2026) y **SQLite** diseñada para el intercambio navideño corporativo de los colaboradores del **Banco Nacional de Crédito (BNC)**.

### Características Principales Implementadas:
1. **Buscador en Tiempo Real (Página Principal `/`):**
   - Búsqueda instantánea mientras se escribe (*debounced input*).
   - Insensible a mayúsculas y minúsculas (*case-insensitive*).
   - Normalización de acentos y tildes (ej. `gomez` encuentra a `Gómez`).
   - Contador dinámico de resultados y vista limpia sin filtros innecesarios.
2. **Panel Deslizante de Preferencias (*Offcanvas Drawer*):**
   - Al seleccionar cualquier participante se despliega un panel lateral (en escritorio) o vista inferior tipo *Bottom Sheet* (en móvil).
   - 🎁 **Cosas que le gustan / Ideas de regalo**: Deseos y sugerencias.
   - 🚫 **Cosas que NO le gustan (Evitar)**: Restricciones para no equivocarse.
   - ⚠️ **Alergias y Restricciones Médicas**: Alertas destacadas en ámbar.
   - 👕 **Tallas y Notas**: Talla de ropa, calzado, color favorito y mensaje.
   - 📋 **Copiar Resumen**: Botón de un solo clic que copia la *wishlist* formateada al portapapeles.
3. **Portal Administrativo Protegido (`/admin`):**
   - Autenticación con credenciales:
     - **Usuario:** `admin`
     - **Contraseña:** `bnc2026`
   - Dashboard con métricas (total participantes, con gustos, con alergias, áreas).
   - CRUD completo: Crear, Editar datos/preferencias, Eliminar con confirmación.
   - Herramienta para restaurar datos muestra si fuera necesario.
4. **Diseño Visual e Identidad Corporativa BNC:**
   - Colores oficiales: **Azul Marino BNC (`#002855`, `#001428`)** y **Naranja BNC (`#FF6600`)**.
   - Logo oficial BNC en [wwwroot/images/BNC_Logo.png](wwwroot/images/BNC_Logo.png) y respaldo en CDN CloudFront.
   - Iconos limpios vectoriales SVG (sin emojis genéricos) con [Components/Shared/SvgIcon.razor](Components/Shared/SvgIcon.razor).
   - Experiencia de usuario altamente responsive adaptada a móviles, tablets y monitores grandes.

---

## 👥 2. Participantes Registrados en la Base de Datos (51 Colaboradores)
La base de datos SQLite tiene precargados los 51 participantes de la lista oficial:

1. Sarahy Barragán
2. Wilmary Maduro
3. Leonardo Chacón
4. Alix Puello
5. Keynner Aguilar
6. Carlos Nuñez
7. Sorianyely Morin
8. Pablo Barboza
9. Jesús Gomes
10. George Embaid
11. Kevin Solorzano
12. Erick Adames
13. Julián Esteves
14. Susana Escobar
15. Jhon Erice
16. Jesús Noguera
17. José Sanabria
18. Katherine Mendible
19. Darwin García
20. Gabriel Tagliafico
21. Kervin Azuaje
22. Albert Rangel
23. Rosbel Rodríguez
24. Miguel Suarez
25. Edinson Cabello
26. Jesús Colina
27. Yldemaro Romero
28. Junior Romero
29. Rafael Lozano
30. Andrés Parra
31. Yhusleika Molina
32. Victor Obelmejía
33. Enrique Sanchez
34. Jonathan Colucci
35. Michael Bastidas
36. Lileska Romero
37. Jose Manuel Quintal
38. Jose Luis Reyes
39. Greadary Garcia
40. Sheyla Tovar
41. Abraham Figueroa
42. Karilyn Carvajal
43. Nestor Villegas
44. Gustavo Suarez
45. Eduardo Lobo
46. Pablo Romero
47. Yessika Mata
48. Alexander Diaz
49. Zenon Avariano
50. Tokio Franco
51. Luis Rendon

---

## 🗄️ 3. Base de Datos SQLite (`amigosecreto.db`)
- **Archivo:** `amigosecreto.db` ubicado en la raíz del proyecto.
- **ORM:** Entity Framework Core SQLite (`Microsoft.EntityFrameworkCore.Sqlite` v10.0.12).
- **Control de versiones:** Este archivo está explícitamente incluido en el repositorio Git (mediante `!amigosecreto.db` en [.gitignore](.gitignore)), de modo que cualquier modificación o carga de gustos se preserva al clonar en casa.
- **Concurrencia en Blazor Server:** Se utiliza `IDbContextFactory<AppDbContext>` en [Program.cs](Program.cs) y [Services/PersonService.cs](Services/PersonService.cs) para garantizar que las múltiples operaciones asíncronas de Blazor Server no colisionen sobre un mismo contexto.

---

## 🔑 4. Credenciales del Portal Administrativo
- **Ruta:** `/admin` (ejemplo: `http://localhost:5222/admin`)
- **Usuario:** `admin`
- **Contraseña:** `bnc2026`
- **Servicio de Validación:** [Services/AdminAuthService.cs](Services/AdminAuthService.cs).

---

## 💻 5. Instrucciones para Ejecutar en Casa

### Requisitos Previos:
- .NET SDK 9.0 o 10.0 instalado.
- Visual Studio 2026 (o Visual Studio 2022 v17.12+) o VS Code con C# Dev Kit.

### Comandos de Ejecución:
```bash
# 1. Clonar el repositorio
git clone <URL_DEL_REPOSITORIO>
cd AmigoSecreto

# 2. Restaurar dependencias y compilar
dotnet build AmigoSecreto.slnx

# 3. Iniciar el servidor local
dotnet run --launch-profile http
```

### URLs de Acceso Local:
- **Portal de Búsqueda:** `http://localhost:5222`
- **Portal Administrativo:** `http://localhost:5222/admin`

---

## 📂 6. Estructura de Archivos Clave

| Archivo / Carpeta | Propósito |
|---|---|
| `AmigoSecreto.slnx` | Solución oficial en formato moderno de Visual Studio 2026. |
| `Program.cs` | Configuración de Blazor Server, SQLite, DataProtection y DI. |
| `amigosecreto.db` | Base de datos SQLite que contiene los participantes y preferencias. |
| `Models/Person.cs` | Modelo con campos personales, tallas, gustos, alergias y métodos auxiliares. |
| `Data/AppDbContext.cs` | Contexto de Entity Framework Core. |
| `Data/DbInitializer.cs` | Semilla inicial de los 51 colaboradores BNC. |
| `Services/IPersonService.cs` y `PersonService.cs` | Lógica de negocio, CRUD y búsqueda case-insensitive sin acentos. |
| `Services/IAdminAuthService.cs` y `AdminAuthService.cs` | Autenticación administrativa (`admin` / `bnc2026`). |
| `Components/Pages/Home.razor` | Vista principal del buscador de participantes. |
| `Components/Pages/Admin.razor` | Vista del portal administrativo con métricas y CRUD. |
| `Components/Shared/PersonDetailModal.razor` | Panel deslizante lateral (offcanvas/bottom sheet) de preferencias. |
| `Components/Shared/PersonEditModal.razor` | Modal para crear/editar participantes y definir preferencias. |
| `Components/Shared/BncLogo.razor` | Componente con el logo corporativo del BNC y fallback a CDN. |
| `Components/Shared/SvgIcon.razor` | Biblioteca de iconos vectoriales SVG limpios. |
| `Components/Layout/MainLayout.razor` | Layout principal con barra superior BNC responsiva y footer. |
| `wwwroot/app.css` | Hoja de estilos con variables de color BNC y reglas responsive. |
| `wwwroot/images/BNC_Logo.png` | Archivo del logo oficial del BNC (256x256 px). |

---

## 🎯 7. Tareas y Próximos Pasos Recomendados en Casa
1. **Completar gustos y alergias de los colaboradores:**
   - Iniciar sesión en `/admin` y abrir cada participante para cargar sus gustos específicos, cosas a evitar y si tiene alergias a algún alimento o fragancia.
2. **Posibles mejoras adicionales:**
   - Módulo de Sorteo Automático / Algoritmo de Emparejamiento de Amigo Secreto (evitando parejas mutuas o del mismo equipo si se desea).
   - Envío de notificaciones por correo o generación de enlace secreto cifrado para cada participante.
   - Opción para exportar el reporte de participantes y alergias a Excel / PDF.
