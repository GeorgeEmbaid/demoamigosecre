# 🤖 AGENT.md - Contexto y Guía de Continuación en Casa
## Proyecto: Portal Interactivo Amigo Secreto BNC 2026

Este documento contiene toda la información técnica, arquitectónica y operativa para que cualquier desarrollador o agente de Inteligencia Artificial retome y continúe el trabajo en este repositorio desde casa.

---

## 📌 1. Resumen Ejecutivo del Proyecto
El **Portal Amigo Secreto BNC 2026** es una aplicación web interactiva desarrollada con **Blazor Server** (.NET 10 / Visual Studio 2026) y **SQLite** diseñada para el intercambio navideño corporativo de los colaboradores del **Banco Nacional de Crédito (BNC)**.

### Características Principales Implementadas:
1. **Login Unificado para Participantes y Administrador (Página Principal `/`):**
   - Existe una **única pantalla de inicio de sesión** en `/`:
     - Si se ingresa con las credenciales del **Administrador (`admin` / `bnc2026`)**, el sistema inicia sesión en modo administrativo, redirige al Panel de Gestión (`/admin`) y habilita en la barra superior la conmutación libre entre el **Directorio (`/`)** y el **Panel Admin (`/admin`)**.
     - Si se ingresa con las credenciales de un **Participante BNC**, el sistema inicia sesión con el perfil de ese colaborador y muestra el directorio de los 51 participantes junto con su barra de autogestión.
   - **Dominio de Correo Corporativo:** `@bncenlinea.net` (ej. `gembaid@bncenlinea.net`).
   - **Regla de Usuario por Defecto:** Inicial del primer nombre + primer apellido en minúscula sin acentos (ej. `George Embaid` -> `gembaid`, `Sarahy Barragán` -> `sbarragan`).
   - **Regla de Contraseña por Defecto:** Primer apellido en minúscula sin acentos seguido de `2026` (ej. `embaid2026`, `barragan2026`).
2. **Autogestión del Participante ("Mi Perfil, Foto y Regalo Deseado"):**
   - Al iniciar sesión en `/`, el participante ve su barra personal en la parte superior con acceso directo para configurar su ficha:
     - 📸 **Subir su Foto de Perfil (`ProfilePhotoPath`)**: Almacenada en `wwwroot/uploads/profiles/` para que su Amigo Secreto lo reconozca fácilmente.
     - 🎁 **Subir Imagen del Regalo Deseado (`GiftImagePath`) + URL Referencial (`GiftReferenceUrl`)**: Almacenada en `wwwroot/uploads/gifts/` junto con un enlace clicable (Amazon, MercadoLibre, tienda en línea, etc.).
     - ✅ **Cosas que le gustan (`Likes`)**: Ideas y preferencias de regalo.
     - 🚫 **Cosas que NO le gustan (`Dislikes`)**: Artículos o gustos que prefiere evitar.
     - ⚠️ **Alergias (`Allergies`)**, tallas y mensaje para su amigo secreto.
3. **Directorio y Buscador en Tiempo Real (`/`):**
   - Muestra los 51 participantes con su foto de perfil, indicadores de gustos, regalo sugerido y alertas de alergias.
   - Búsqueda instantánea por nombre o apellido (*case-insensitive* e insensible a acentos/tildes).
   - Al seleccionar cualquier participante se abre el panel lateral deslizante (`PersonDetailModal.razor`) mostrando su foto, regalo deseado con imagen y botón de URL externa, cosas que le gustan, cosas que no le gustan y alergias.
4. **Portal Administrativo Independiente (`/admin`):**
   - El usuario administrativo está separado de los participantes para labores de soporte y organización:
     - **Usuario:** `admin`
     - **Contraseña:** `bnc2026`
   - Permite crear/eliminar participantes y apoyar a cualquier colaborador editando su información, sus fotos, sus gustos y **modificando su usuario de acceso (`Username`) y su contraseña (`Password`)** en caso de que lo solicite.

---

## 👥 2. Credenciales de los 51 Participantes BNC (`@bncenlinea.net`)

| # | Participante | Correo Corporativo | Usuario | Contraseña |
|---|---|---|---|---|
| 1 | Sarahy Barragán | `sbarragan@bncenlinea.net` | `sbarragan` | `barragan2026` |
| 2 | Wilmary Maduro | `wmaduro@bncenlinea.net` | `wmaduro` | `maduro2026` |
| 3 | Leonardo Chacón | `lchacon@bncenlinea.net` | `lchacon` | `chacon2026` |
| 4 | Alix Puello | `apuello@bncenlinea.net` | `apuello` | `puello2026` |
| 5 | Keynner Aguilar | `kaguilar@bncenlinea.net` | `kaguilar` | `aguilar2026` |
| 6 | Carlos Nuñez | `cnunez@bncenlinea.net` | `cnunez` | `nunez2026` |
| 7 | Sorianyely Morin | `smorin@bncenlinea.net` | `smorin` | `morin2026` |
| 8 | Pablo Barboza | `pbarboza@bncenlinea.net` | `pbarboza` | `barboza2026` |
| 9 | Jesús Gomes | `jgomes@bncenlinea.net` | `jgomes` | `gomes2026` |
| 10 | George Embaid | `gembaid@bncenlinea.net` | `gembaid` | `embaid2026` |
| 11 | Kevin Solorzano | `ksolorzano@bncenlinea.net` | `ksolorzano` | `solorzano2026` |
| 12 | Erick Adames | `eadames@bncenlinea.net` | `eadames` | `adames2026` |
| 13 | Julián Esteves | `jesteves@bncenlinea.net` | `jesteves` | `esteves2026` |
| 14 | Susana Escobar | `sescobar@bncenlinea.net` | `sescobar` | `escobar2026` |
| 15 | Jhon Erice | `jerice@bncenlinea.net` | `jerice` | `erice2026` |
| 16 | Jesús Noguera | `jnoguera@bncenlinea.net` | `jnoguera` | `noguera2026` |
| 17 | José Sanabria | `jsanabria@bncenlinea.net` | `jsanabria` | `sanabria2026` |
| 18 | Katherine Mendible | `kmendible@bncenlinea.net` | `kmendible` | `mendible2026` |
| 19 | Darwin García | `dgarcia@bncenlinea.net` | `dgarcia` | `garcia2026` |
| 20 | Gabriel Tagliafico | `gtagliafico@bncenlinea.net` | `gtagliafico` | `tagliafico2026` |
| 21 | Kervin Azuaje | `kazuaje@bncenlinea.net` | `kazuaje` | `azuaje2026` |
| 22 | Albert Rangel | `arangel@bncenlinea.net` | `arangel` | `rangel2026` |
| 23 | Rosbel Rodríguez | `rrodriguez@bncenlinea.net` | `rrodriguez` | `rodriguez2026` |
| 24 | Miguel Suarez | `msuarez@bncenlinea.net` | `msuarez` | `suarez2026` |
| 25 | Edinson Cabello | `ecabello@bncenlinea.net` | `ecabello` | `cabello2026` |
| 26 | Jesús Colina | `jcolina@bncenlinea.net` | `jcolina` | `colina2026` |
| 27 | Yldemaro Romero | `yromero@bncenlinea.net` | `yromero` | `romero2026` |
| 28 | Junior Romero | `jromero@bncenlinea.net` | `jromero` | `romero2026` |
| 29 | Rafael Lozano | `rlozano@bncenlinea.net` | `rlozano` | `lozano2026` |
| 30 | Andrés Parra | `aparra@bncenlinea.net` | `aparra` | `parra2026` |
| 31 | Yhusleika Molina | `ymolina@bncenlinea.net` | `ymolina` | `molina2026` |
| 32 | Victor Obelmejía | `vobelmejia@bncenlinea.net` | `vobelmejia` | `obelmejia2026` |
| 33 | Enrique Sanchez | `esanchez@bncenlinea.net` | `esanchez` | `sanchez2026` |
| 34 | Jonathan Colucci | `jcolucci@bncenlinea.net` | `jcolucci` | `colucci2026` |
| 35 | Michael Bastidas | `mbastidas@bncenlinea.net` | `mbastidas` | `bastidas2026` |
| 36 | Lileska Romero | `lromero@bncenlinea.net` | `lromero` | `romero2026` |
| 37 | Jose Manuel Quintal | `jquintal@bncenlinea.net` | `jquintal` | `quintal2026` |
| 38 | Jose Luis Reyes | `jreyes@bncenlinea.net` | `jreyes` | `reyes2026` |
| 39 | Greadary Garcia | `ggarcia@bncenlinea.net` | `ggarcia` | `garcia2026` |
| 40 | Sheyla Tovar | `stovar@bncenlinea.net` | `stovar` | `tovar2026` |
| 41 | Abraham Figueroa | `afigueroa@bncenlinea.net` | `afigueroa` | `figueroa2026` |
| 42 | Karilyn Carvajal | `kcarvajal@bncenlinea.net` | `kcarvajal` | `carvajal2026` |
| 43 | Nestor Villegas | `nvillegas@bncenlinea.net` | `nvillegas` | `villegas2026` |
| 44 | Gustavo Suarez | `gsuarez@bncenlinea.net` | `gsuarez` | `suarez2026` |
| 45 | Eduardo Lobo | `elobo@bncenlinea.net` | `elobo` | `lobo2026` |
| 46 | Pablo Romero | `promero@bncenlinea.net` | `promero` | `romero2026` |
| 47 | Yessika Mata | `ymata@bncenlinea.net` | `ymata` | `mata2026` |
| 48 | Alexander Diaz | `adiaz@bncenlinea.net` | `adiaz` | `diaz2026` |
| 49 | Zenon Avariano | `zavariano@bncenlinea.net` | `zavariano` | `avariano2026` |
| 50 | Tokio Franco | `tfranco@bncenlinea.net` | `tfranco` | `franco2026` |
| 51 | Luis Rendon | `lrendon@bncenlinea.net` | `lrendon` | `rendon2026` |

---

## 🗄️ 3. Base de Datos SQLite (`amigosecreto.db`) y Archivos Subidos
- **Archivo:** `amigosecreto.db` ubicado en la raíz del proyecto.
- **Imágenes subidas:** Se guardan en `wwwroot/uploads/profiles/` (fotos de perfil) y `wwwroot/uploads/gifts/` (fotos de regalos deseados).
- **Control de versiones:** `amigosecreto.db` está incluido en Git (`!amigosecreto.db` en [.gitignore](.gitignore)).
- **Auto-migración:** `DbInitializer.cs` verifica automáticamente que la tabla `People` contenga las columnas `Username`, `Password`, `ProfilePhotoPath`, `GiftImagePath` y `GiftReferenceUrl`, actualizando el esquema y las credenciales `@bncenlinea.net` si detecta una versión previa.

---

## 💻 4. Instrucciones para Ejecutar en Casa

```bash
# 1. Clonar o actualizar el repositorio
git pull origin develop

# 2. Compilar la solución
dotnet build AmigoSecreto.slnx

# 3. Iniciar el servidor local
dotnet run --launch-profile http
```

### URLs de Acceso Local:
- **Portal de Participantes (Login individual):** `http://localhost:5222`
- **Portal Administrativo (`admin` / `bnc2026`):** `http://localhost:5222/admin`
