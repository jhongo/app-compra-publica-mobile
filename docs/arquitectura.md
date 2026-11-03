# Compra Pública Cerca — Investigación de arquitectura

## 1. CommunityToolkit.Mvvm frente a INotifyPropertyChanged

Proyecto: Compra Pública Cerca | Plataforma: .NET MAUI + C# | Alcance: 3 semanas

### Objetivo de la investigación

Determinar cómo administrar los cambios de datos y las acciones de las pantallas con el patrón MVVM, sin repetir código innecesario.

### Comparación de alternativas

INotifyPropertyChanged es la interfaz que informa a XAML cuando cambia una propiedad. Al implementarla manualmente, cada propiedad debe notificar el cambio. CommunityToolkit.Mvvm utiliza esa misma base, pero genera automáticamente propiedades y comandos mediante atributos.

| Criterio | Manual | CommunityToolkit.Mvvm |
| --- | --- | --- |
| Propiedades | Notificación escrita a mano | [ObservableProperty] |
| Botones y acciones | Comandos creados manualmente | [RelayCommand] |
| Mantenimiento | Más código repetitivo | Más compacto y uniforme |
| Dependencias | Sin paquete adicional | Requiere NuGet |

### Ejemplo para la búsqueda de procesos

```csharp
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

public partial class SearchViewModel : ObservableObject
{
    [ObservableProperty] private string keyword = "";
    [RelayCommand]
    private void Search() { /* Solicitar datos al servicio */ }
}
```

El Toolkit genera PalabraClave y BuscarCommand. La pantalla puede enlazar un campo de texto a la propiedad y un botón al comando. El ejemplo no consulta aún SERCOP: esa operación se implementará en el servicio correspondiente.

### Decisión para el proyecto

Usar CommunityToolkit.Mvvm en todos los ViewModels. Reducirá trabajo durante las tres semanas y mantendrá un estilo común entre los integrantes. Los métodos de consulta HTTP no estarán dentro de la View, sino en los servicios.

Referencia: Microsoft Learn — MVVM Toolkit, ObservableProperty y RelayCommand.

## 2. Inyección de dependencias en MauiProgram.cs

Proyecto: Compra Pública Cerca | Plataforma: .NET MAUI + C# | Alcance: 3 semanas

### ¿Para qué sirve?

La inyección de dependencias permite que una pantalla reciba su ViewModel y que este reciba los servicios necesarios sin crearlos con new. En .NET MAUI, estas dependencias se registran normalmente en MauiProgram.cs.

### Singleton y Transient

| Registro | Funcionamiento | Uso propuesto |
| --- | --- | --- |
| Singleton | Comparte una misma instancia durante la vida del contenedor. | Cliente HTTP / servicio de sesión sin estado mutable inseguro. |
| Transient | Crea una instancia cada vez que se solicita. | Pantallas y ViewModels de login, búsqueda y detalle. |

### Ejemplo de configuración

```csharp
// Dentro de CreateMauiApp(), antes de builder.Build()
builder.Services.AddHttpClient<IApiService, ApiService>();
builder.Services.AddSingleton<ISessionService, SessionService>();
builder.Services.AddTransient<SearchViewModel>();
builder.Services.AddTransient<SearchPage>();
builder.Services.AddTransient<ProcessDetailViewModel>();
builder.Services.AddTransient<ProcessDetailPage>();
```

AddHttpClient registra un cliente HTTP administrado para la API interna. Las interfaces y clases del ejemplo deberán crearse antes de compilar; se ajustarán al código real del equipo.

### Ejemplo de uso en una pantalla

```csharp
public partial class SearchPage : ContentPage
{
    public SearchPage(SearchViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
```

### Decisión para el proyecto

Registrar servicios compartidos o clientes HTTP con el ciclo de vida apropiado, y Views/ViewModels como Transient. La aplicación móvil llamará a la API interna; solo el backend accederá a PostgreSQL o consultará a SERCOP. No se guardarán credenciales en código fuente.

Referencia: Microsoft Learn — Dependency injection en .NET MAUI y service lifetimes en .NET.

## 3. Navegación con Shell y paso de parámetros

Proyecto: Compra Pública Cerca | Plataforma: .NET MAUI + C# | Alcance: 3 semanas

### ¿Qué resuelve Shell?

Shell organiza la navegación mediante rutas. El usuario iniciará sesión, buscará oportunidades y abrirá el detalle de un proceso. El identificador OCID permite solicitar al backend la información del proceso correcto.

### Flujo principal

LoginPage → SearchPage → DetalleProcesoPage → enlace al portal oficial de SERCOP. El acceso a FavoritosPage estará disponible desde la navegación principal.

### Definición de una ruta

```csharp
// En Constants/NavigationRoutes.cs
public static class NavigationRoutes
{
    public const string ProcessDetail = "detalle";
}

// En AppShell.xaml.cs
Routing.RegisterRoute(NavigationRoutes.ProcessDetail, typeof(ProcessDetailPage));
```

### Envío de un identificador desde la búsqueda

```csharp
await Shell.Current.GoToAsync(NavigationRoutes.ProcessDetail,
    new ShellNavigationQueryParameters
    {
        { "ocid", selectedOcid }
    });
```

### Recepción en el ViewModel de detalle

```csharp
public partial class ProcessDetailViewModel : BaseViewModel,
    IQueryAttributable
{
    public void ApplyQueryAttributes(
        IDictionary<string, object> query)
    {
        if (query.TryGetValue("ocid", out var value))
        {
            var ocid = value?.ToString() ?? "";
            // Consultar el proceso mediante IProcessService.
        }
    }
}
```

IQueryAttributable recibe los parámetros sin depender de QueryProperty. Una vez validado el OCID, el ViewModel llamará a un servicio para recuperar el detalle; no consultará directamente la base de datos.

### Decisión para el proyecto

Usar Shell con rutas claras, GoToAsync e IQueryAttributable. Enviar identificadores, no objetos extensos, para facilitar una navegación predecible y una carga actualizada de los datos.

Referencia: Microsoft Learn — .NET MAUI Shell navigation.

## 4. Estructura de carpetas y responsabilidades

Proyecto: Compra Pública Cerca | Plataforma: .NET MAUI + C# | Alcance: 3 semanas

### Organización propuesta

Se propone separar la aplicación móvil del backend. En el móvil se seguirá MVVM y en el backend se concentrará la consulta de SERCOP, la lógica de negocio y el acceso a PostgreSQL.

```text
CompraPublicaCerca/
├── mobile/CompraPublicaCerca/
│   ├── Models/        (Proceso, Usuario, Favorito)
│   ├── Views/         (LoginPage, RegistroPage, BusquedaPage,
│   │                   DetalleProcesoPage, FavoritosPage)
│   ├── ViewModels/    (BaseViewModel y uno por pantalla)
│   ├── Services/      (ApiService, AuthService, ProcesoService,
│   │                   FavoritosService e interfaces)
│   ├── Utils/         (utilidades reutilizables)
│   ├── Converters/    (conversiones para XAML, si se necesitan)
│   ├── Constants/     (rutas y valores no secretos)
│   ├── Resources/     (estilos, imagenes, fuentes)
│   ├── Platforms/     (configuracion Android)
│   ├── App.xaml
│   ├── AppShell.xaml
│   └── MauiProgram.cs
├── backend/CompraPublicaCerca.Api/
│   ├── Controllers/
│   ├── Models/
│   ├── DTOs/
│   ├── Data/
│   ├── Services/
│   └── Program.cs
├── tests/
├── docs/arquitectura.md
└── README.md
```

### Reglas de organización

- Views: interfaz XAML; evitar consultas HTTP o lógica de negocio en code-behind.
- ViewModels: propiedades y comandos de la pantalla; consumen servicios mediante interfaces.
- Services: comunicación con la API interna y tareas compartidas.
- Models y DTOs: estructuras de datos; los DTOs del backend definen las respuestas JSON.
- Utils, Converters y Constants: crear solo lo necesario; nunca almacenar secretos.
- Las carpetas auxiliares pueden permanecer vacías hasta que exista una necesidad concreta.
### Decisión para el proyecto

Adoptar esta estructura desde el inicio y usar nombres coherentes: SearchPage, SearchViewModel e IProcessService. El equipo podrá desarrollar pantallas y backend en paralelo sin mezclar responsabilidades.

Referencia: Microsoft Learn — .NET MAUI architecture y MVVM.

## 5. BaseViewModel: carga, título y errores

Proyecto: Compra Pública Cerca | Plataforma: .NET MAUI + C# | Alcance: 3 semanas

### ¿Qué es y para qué se utilizará?

BaseViewModel es una clase compartida por los ViewModels. Evita repetir propiedades que necesitan varias pantallas: título, estado de carga y mensaje de error. Hereda de ObservableObject, provisto por CommunityToolkit.Mvvm.

### Implementación mínima

```csharp
using CommunityToolkit.Mvvm.ComponentModel;

public partial class BaseViewModel : ObservableObject
{
    [ObservableProperty] private bool isBusy;
    [ObservableProperty] private string title = "";
    [ObservableProperty] private string errorMessage = "";
}
```

### Ejemplo de uso en una búsqueda

```csharp
public partial class SearchViewModel : BaseViewModel
{
    private readonly IProcessService _processes;
    public SearchViewModel(IProcessService processes)
    {
        _processes = processes;
        Title = "Búsqueda de procesos";
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        if (IsBusy) return;
        try {
            IsBusy = true;
            ErrorMessage = "";
            // Solicitar procesos al servicio.
        }
        catch (Exception) { ErrorMessage = "Error de conexión."; }
        finally { IsBusy = false; }
    }
}
```

El fragmento es ilustrativo y requiere los using y la interfaz IProcessService del proyecto. No se deben mostrar mensajes técnicos sensibles al usuario; se registrarán los detalles del error en el lugar adecuado.

### Decisión para el proyecto

Mantener BaseViewModel pequeña, con IsBusy, Title y ErrorMessage. La autenticación, las consultas a SERCOP y el acceso a datos seguirán en los servicios; una búsqueda sin resultados no se tratará como error de conexión.

Referencia: Microsoft Learn — ObservableObject y ObservableProperty.

## 6. Bindings compilados con x:DataType

Proyecto: Compra Pública Cerca | Plataforma: .NET MAUI + C# | Alcance: 3 semanas

### ¿Qué es un binding compilado?

Los bindings enlazan controles XAML con propiedades y comandos del ViewModel. Al definir x:DataType, .NET MAUI puede compilar y verificar muchos de esos enlaces, ayudando a descubrir nombres mal escritos y mejorando el rendimiento.

### Ejemplo en la pantalla de búsqueda

```xml
<ContentPage
    xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
    xmlns:vm="clr-namespace:CompraPublicCercaMobile.ViewModels"
    x:Class="CompraPublicCercaMobile.Views.SearchPage"
    x:DataType="vm:SearchViewModel">

    <VerticalStackLayout Padding="20" Spacing="12">
        <Label Text="{Binding Title}" />
        <Entry Text="{Binding Keyword}" />
        <Button Text="Buscar"
                Command="{Binding SearchCommand}" />
        <ActivityIndicator IsRunning="{Binding IsBusy}" />
        <Label Text="{Binding ErrorMessage}" />
    </VerticalStackLayout>
</ContentPage>
```

Para que compile, SearchViewModel debe exponer PalabraClave y BuscarCommand, y la pantalla debe recibir dicho ViewModel como BindingContext. x:DataType indica el tipo esperado, pero no asigna por sí mismo el BindingContext.

### Listas de procesos

Cuando se muestre una colección, cada DataTemplate deberá declarar x:DataType="models:Proceso" (tras definir su xmlns). Así el compilador interpretará Titulo, Institucion y Estado como propiedades de cada proceso, no del ViewModel de la página.

### Decisión para el proyecto

Declarar x:DataType en todas las páginas y plantillas de elementos, revisar advertencias XAML y probar los bindings antes de integrar cambios al repositorio. Esto disminuye errores de interfaz en el plazo de tres semanas.

Referencia: Microsoft Learn — Compiled bindings en .NET MAUI.

## 7. Decisiones técnicas MVVM

Proyecto: Compra Pública Cerca | Plataforma: .NET MAUI + C# | Alcance: 3 semanas

| Tema | Decisión para el equipo |
| --- | --- |
| CommunityToolkit.Mvvm | Usar ObservableObject, [ObservableProperty] y [RelayCommand] para evitar la implementación manual repetitiva de INotifyPropertyChanged. |
| Inyección de dependencias | Registrar componentes en MauiProgram.cs: Singleton para servicios compartidos sin estado mutable por pantalla y Transient para páginas y ViewModels. |
| Navegación Shell | Definir rutas en AppShell; utilizar GoToAsync() y pasar el identificador OCID al detalle. Recibirlo con IQueryAttributable. |
| BaseViewModel | Heredar de ObservableObject y compartir IsBusy, Title y ErrorMessage. La lógica de negocio permanecerá en los servicios. |
| Bindings compilados | Declarar x:DataType para cada ViewModel en XAML y para el tipo de elemento en cada DataTemplate. |
