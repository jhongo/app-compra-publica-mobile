# Guía de contribución

Esta guía explica cómo trabajamos en **CompraPublicCercaMobile**. Léela antes de tu primer cambio: así los 4 trabajamos en el mismo repositorio sin pisarnos.

> **Estado:** propuesta inicial (Issue #4). Las secciones marcadas con 🟡 siguen en discusión y pueden cambiar cuando el equipo las apruebe.

---

## Índice

1. [Requisitos del entorno](#1-requisitos-del-entorno)
2. [Primeros pasos](#2-primeros-pasos)
3. [Flujo de trabajo (Git)](#3-flujo-de-trabajo-git)
4. [Convención de ramas](#4-convención-de-ramas)
5. [Convención de commits](#5-convención-de-commits)
6. [Pull Requests y revisión](#6-pull-requests-y-revisión)
7. [Issues y tablero](#7-issues-y-tablero)
8. [Arquitectura y estructura de carpetas](#8-arquitectura-y-estructura-de-carpetas-) 🟡
9. [Convenciones de código](#9-convenciones-de-código)
10. [Secretos y archivos de configuración](#10-secretos-y-archivos-de-configuración)
11. [Definición de terminado](#11-definición-de-terminado-definition-of-done)

---

## 1. Requisitos del entorno

| Herramienta | Versión / nota |
|---|---|
| .NET SDK | **10.0** |
| Workload MAUI | `dotnet workload install maui` |
| IDE | JetBrains Rider o Visual Studio 2022+ (VS Code con la extensión .NET MAUI también sirve) |
| Android | Android SDK y un emulador (API 21 o superior) |
| iOS / MacCatalyst | macOS con Xcode (solo si vas a probar en Apple) |

Para comprobar la instalación:

```bash
dotnet --version
dotnet workload list   # debe aparecer "maui"
```

## 2. Primeros pasos

```bash
git clone https://github.com/jhongo/app-compra-publica-mobile.git
cd app-compra-publica-mobile
dotnet restore
dotnet build -f net10.0-android
```

Para ejecutar en Android desde la terminal:

```bash
dotnet build -t:Run -f net10.0-android
```

También puedes abrir `CompraPublicCercaMobile.csproj` en tu IDE y ejecutarlo desde ahí.

## 3. Flujo de trabajo (Git)

Usamos **GitHub Flow**:

- `main` siempre debe compilar y estar estable.
- **Nadie hace push directo a `main`.** Todo entra por Pull Request.
- Cada tarea sale de un issue y tiene su propia rama.

```
main ──●─────────────●──────────●──▶
        \           /          /
         feature/12-login-google
                    \          /
                     fix/15-crash-inicio
```

Pasos para cada tarea:

```bash
# 1. Actualiza main
git checkout main
git pull origin main

# 2. Crea tu rama desde main
git checkout -b feature/12-login-google

# 3. Trabaja y haz commits pequeños
git add .
git commit -m "feat(auth): agregar botón de login con Google"

# 4. Antes de abrir el PR, trae los últimos cambios de main
git fetch origin
git rebase origin/main      # o: git merge origin/main

# 5. Sube tu rama y abre el PR en GitHub
git push -u origin feature/12-login-google
```

## 4. Convención de ramas

Formato: `tipo/<número-issue>-descripcion-corta` (en minúsculas y con guiones).

| Tipo | Uso | Ejemplo |
|---|---|---|
| `feature/` | Nueva funcionalidad | `feature/12-login-google` |
| `fix/` | Corrección de un error | `fix/15-crash-inicio` |
| `docs/` | Solo documentación | `docs/4-guia-contribucion` |
| `refactor/` | Reestructurar sin cambiar el comportamiento | `refactor/20-base-viewmodel` |
| `chore/` | Configuración, dependencias, CI | `chore/8-github-actions` |
| `research/` | Investigación o prueba de concepto | `research/2-firebase-poc` |

## 5. Convención de commits

Usamos [Conventional Commits](https://www.conventionalcommits.org/es/v1.0.0/):

```
<tipo>(<ámbito opcional>): <descripción en imperativo y minúsculas>
```

| Tipo | Cuándo usarlo |
|---|---|
| `feat` | Nueva funcionalidad |
| `fix` | Corrección de un error |
| `docs` | Documentación |
| `style` | Formato, sin cambios de lógica |
| `refactor` | Cambio de código que no agrega ni corrige nada |
| `test` | Agregar o corregir pruebas |
| `chore` | Configuración, paquetes, build, CI |

Ejemplos:

```
feat(auth): agregar login con Google
fix(home): corregir crash cuando no hay conexión
docs: agregar guía de contribución
chore(deps): agregar CommunityToolkit.Mvvm
```

Recomendaciones:
- Haz commits pequeños con un solo propósito.
- Para cerrar un issue al hacer merge, escribe en el PR o en el commit: `Closes #12`.

## 6. Pull Requests y revisión

1. Abre el PR **hacia `main`**.
2. Título con el mismo formato de los commits: `feat(auth): login con Google`.
3. En la descripción incluye:
   - **Qué** cambia y **por qué**.
   - El issue relacionado (`Closes #12`).
   - **Cómo probarlo** (pasos).
   - Capturas o video si hay cambios visuales.
4. Pide revisión a **al menos 1 compañero**. Se necesita **1 aprobación** para hacer merge.
5. El autor del PR hace el merge después de la aprobación, con **Squash and merge**.
6. Borra la rama después del merge.

**Al revisar:**
- Descarga la rama y pruébala si el cambio es funcional.
- Comenta con respeto y de forma concreta; sugiere alternativas.
- Revisa que se cumpla la [definición de terminado](#11-definición-de-terminado-definition-of-done).

## 7. Issues y tablero

- **Todo trabajo empieza con un issue.** Si no existe, créalo.
- Asígnate el issue antes de empezar, para que nadie más lo tome.
- Usamos **GitHub Projects** (Kanban): `Por hacer` → `En progreso` → `En revisión` → `Terminado`.

Etiquetas:

| Etiqueta | Significado |
|---|---|
| `research` | Investigación, sin código de producción |
| `architecture` | Decisiones de arquitectura |
| `setup` | Configuración del proyecto |
| `auth` / `firebase` | Autenticación e integración con Firebase |
| `devops` | CI/CD y flujo de trabajo |
| `bug` | Error |
| `enhancement` | Mejora o nueva funcionalidad |

Un buen issue tiene: **objetivo**, **tareas o preguntas**, **entregable** y **criterio de aceptación**.

## 8. Arquitectura y estructura de carpetas 🟡

> Pendiente de aprobar en el **Issue #1**. Esta es la propuesta inicial.

Usamos el patrón **MVVM** (Model-View-ViewModel) con **CommunityToolkit.Mvvm** e inyección de dependencias en `MauiProgram.cs`.

```
CompraPublicCercaMobile/
├── Views/          # Páginas XAML (LoginPage.xaml)
├── ViewModels/     # Lógica de presentación (LoginViewModel.cs)
├── Models/         # Entidades y DTOs
├── Services/       # Acceso a datos y APIs (interfaces + implementaciones)
│   └── Interfaces/
├── Utils/          # Helpers y extensiones
├── Converters/     # Value converters de XAML
├── Constants/      # Constantes y rutas de navegación
├── Resources/      # Estilos, fuentes, imágenes
└── Platforms/      # Código específico de cada plataforma
```

Reglas:
- **La View no tiene lógica.** El code-behind (`.xaml.cs`) solo recibe el ViewModel por constructor y lo asigna al `BindingContext`.
- **El ViewModel no conoce la View.** Se comunica con ella mediante bindings y comandos.
- **Los ViewModels dependen de interfaces** (`IAuthService`), no de implementaciones, para poder probarlos.
- Toda View, ViewModel y Service se registra en `MauiProgram.cs`.
- La navegación se hace con **Shell** y las rutas se definen en `Constants`.

## 9. Convenciones de código

**C#**
- `PascalCase` para clases, métodos y propiedades. `camelCase` para variables locales y parámetros. `_camelCase` para campos privados.
- Interfaces con el prefijo `I`: `IAuthService`.
- Métodos asíncronos con el sufijo `Async`, y nunca `async void` (salvo en eventos).
- Un archivo por clase, con el mismo nombre que la clase.
- Mantener `Nullable` activo y no ignorar las advertencias.

**Nombres MVVM**
- Cada View tiene su ViewModel con el mismo nombre: `LoginPage` ↔ `LoginViewModel`.
- Services: `AuthService` implementa `IAuthService`.

**XAML**
- Declara `x:DataType` en cada página para tener bindings compilados.
- Colores y estilos en `Resources/Styles`, nunca valores fijos en la página.
- Los textos visibles para el usuario se centralizan (no se repiten en varias páginas).

**Idioma:** el código (clases, métodos, variables) va en **inglés**. Los commits, issues, PRs y la documentación van en **español**.

## 10. Secretos y archivos de configuración

⚠️ **Nunca subas al repositorio:**
- `google-services.json` (Android, Firebase)
- `GoogleService-Info.plist` (iOS, Firebase)
- Claves de API, tokens, contraseñas, keystores (`*.keystore`, `*.jks`)

Estos archivos los comparte el responsable del Issue #2 por un canal privado del equipo y se agregan al `.gitignore`.

Si subiste un secreto por error, **avisa enseguida al equipo**. Borrarlo en un commit nuevo no basta, porque queda en el historial: hay que rotar la clave.

## 11. Definición de terminado (Definition of Done)

Un issue está **terminado** cuando:

- [ ] El código compila sin errores (`dotnet build -f net10.0-android`).
- [ ] No agrega advertencias nuevas importantes.
- [ ] Se probó en un emulador o dispositivo.
- [ ] Sigue la arquitectura y las convenciones de esta guía.
- [ ] Tiene pruebas, si aplica (ViewModels y Services).
- [ ] El PR fue aprobado por al menos 1 compañero.
- [ ] Se hizo merge a `main` y el issue se cerró.

---

¿Dudas o propuestas para mejorar esta guía? Abre un issue con la etiqueta `docs`.
