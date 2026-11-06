# Integración con Firebase Authentication y login con Google

Investigación del issue #2 para la app .NET MAUI (net10) **Compra Pública Cerca**.

## 1. Comparativa de opciones

Firebase no tiene un SDK oficial para .NET MAUI.

| Opción | Ventajas | Desventajas | Resultado |
|---|---|---|---|
| Plugin.Firebase.Auth + Plugin.Firebase.Auth.Google | API común para Android e iOS | Auth quitó los proveedores de Google, Facebook y Apple en la versión 4. La versión 5.0.1 se publica para net9 y Auth.Google 3.1.2 se quedó en net8 | Descartada para el login con Google |
| FirebaseAuthentication.net (REST) + `WebAuthenticator` | Menos dependencias nativas | Más código manual: flujo OAuth, redirección y renovación de tokens | Sin probar |
| SDK nativos de Google y Firebase (bindings de NuGet) | Usa los SDK oficiales de cada plataforma | Requiere una implementación por plataforma y ajustar versiones de paquetes | **Elegida** |

## 2. Opción elegida y justificación

Se eligen los **SDK nativos**, detrás de la interfaz `IAuthService`, porque:

- Plugin.Firebase ya no incluye el login con Google en sus versiones actuales.
- La prueba de concepto confirmó que compilan en net10 para Android e iOS.
- Los ViewModels dependen de la interfaz, como pide la arquitectura MVVM del equipo.

Paquetes usados:

| Plataforma | Paquete | Versión |
|---|---|---|
| Android | `Xamarin.Firebase.Auth` | 124.2.0.1 |
| Android | `Xamarin.GooglePlayServices.Auth` | 121.6.0.2 |
| iOS | `AdamE.Firebase.iOS.Auth` | 12.19.3 |
| iOS | `AdamE.Google.iOS.SignIn` | 10.1.0 |

## 3. Restricción de OAuth en WebViews

Google bloquea el inicio de sesión OAuth dentro de WebViews embebidos (error `403 disallowed_useragent`).
El login no debe hacerse en un `WebView` propio. La opción elegida cumple esta regla: usa Google Play Services en Android y una sesión segura del navegador del sistema en iOS.

## 4. Configuración en Firebase Console

1. Crear el proyecto `app-compra-publica`.
2. Definir el nombre público en **Configuración del proyecto → General**. Es el nombre que Google muestra al usuario.
3. Habilitar Google en **Seguridad → Authentication → Método de acceso**.
4. Registrar la app Android con el paquete `com.uisrael.comprapublica`.
5. Agregar el SHA-1 de cada integrante en **Configuración del proyecto → Tus apps → Agregar huella digital**.
6. Descargar `google-services.json` **después** de agregar los SHA-1.
7. Registrar la app iOS con el Bundle ID `com.uisrael.comprapublica`.
8. Descargar `GoogleService-Info.plist`.

> El nombre del paquete no se puede cambiar después de registrar la app.

### Obtener el SHA-1 (macOS)

El keystore de debug se crea con la primera compilación de Android:

```bash
keytool -list -v -keystore "$HOME/Library/Application Support/Xamarin/Mono for Android/debug.keystore" -alias androiddebugkey -storepass android -keypass android
```

Cada integrante tiene su propio keystore, así que cada uno debe registrar su SHA-1.

## 5. Configuración en la app

### Requisitos

- .NET 10 con el workload `maui`.
- Android: API 36 instalada y JDK 21. Si la versión de Java activa falla, indicar la ruta con `JavaSdkDirectory`.
- iOS: Xcode y un simulador.

### Android

- Colocar el archivo en `Platforms/Android/google-services.json`.
- Subir el mínimo de Android a la **API 24**, que exige una librería de Firebase.
- Mantener alineados los paquetes `Ktx` de AndroidX en el `.csproj` para evitar el error de clases duplicadas.
- Ejecutar con `dotnet build -t:Run -f net10.0-android`. Instalar el APK de Debug con `adb install` hace que la app se cierre al abrir.

### iOS

- Colocar el archivo en `Platforms/iOS/GoogleService-Info.plist`.
- Agregar `REVERSED_CLIENT_ID` como URL scheme en `Platforms/iOS/Info.plist`.
- Agregar el permiso de llavero en `Platforms/iOS/Entitlements.plist`.
- Compilar para el simulador con:

```bash
dotnet build -f net10.0-ios -p:RuntimeIdentifier=iossimulator-arm64
```

- Después de cambiar `Info.plist` o `Entitlements.plist`, borrar `bin/Debug/net10.0-ios` y `obj/Debug/net10.0-ios` antes de compilar. La compilación incremental puede dejar la versión anterior dentro de la app.

## 6. Cómo compartir los archivos de Firebase

- `google-services.json` y `GoogleService-Info.plist` están en `.gitignore` y no se suben al repositorio.
- El responsable del issue #2 los comparte por un canal privado del equipo.
- El proyecto compila sin esos archivos, pero el login no funciona hasta agregarlos.

## 7. Prueba de concepto

Flujo: botón "Iniciar sesión con Google" → cuenta de Google → token → Firebase Auth → mostrar nombre, correo y UID.

| Plataforma | Compila | Abre Google | Login completo |
|---|---|---|---|
| iOS (simulador) | Sí | Sí | Sí |
| Android (emulador) | Sí | Sí | Pendiente: falta registrar el SHA-1 |

Código principal: `Views/LoginPage`, `ViewModels/LoginViewModel`, `Services/Interfaces/IAuthService`, `Platforms/Android/AndroidAuthService` y `Platforms/iOS/IosAuthService`.

## 8. Pendientes y decisiones del equipo

- Probar el login completo en Android.
- Probar en un iPhone físico, que requiere una cuenta de desarrollador de Apple.
- Reemplazar `GoogleSignIn` en Android, marcado como obsoleto, evaluando Credential Manager.
- Decidir si el código de prueba entra a `main`, porque el issue pedía solo investigación.
- Confirmar `com.uisrael.comprapublica` como `ApplicationId` definitivo (issue #3).
- Actualizar `CONTRIBUTING.md`: el mínimo de Android pasa de la API 21 a la 24.
- Confirmar si `REVERSED_CLIENT_ID` puede quedar en `Info.plist`, que sí se sube al repositorio.
- Cuando se construya el flujo real, quitar `LoginPage` como página de arranque temporal y usar Shell.