using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using CompraPublicCercaMobile.Models;
using CompraPublicCercaMobile.Models.Backend;
using CompraPublicCercaMobile.Services.Interfaces;

namespace CompraPublicCercaMobile.Services;

public sealed class BackendAuthClient : IBackendAuthClient
{
    private readonly HttpClient _http;

    public BackendAuthClient(HttpClient http)
    {
        _http = http;
        // Sin esto Laravel respondería errores como páginas HTML en vez de JSON.
        _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task<int> SendVerificationCodeAsync(string email, CancellationToken cancellationToken = default)
    {
        var response = await SendAsync<SendCodeRequest, SendCodeResponse>(
            HttpMethod.Post, "auth/email/send-code", new SendCodeRequest(email),
            BackendJsonContext.Default.SendCodeRequest, BackendJsonContext.Default.SendCodeResponse, null, cancellationToken);
        return response.ResendIn;
    }

    public async Task<string> VerifyCodeAsync(string email, string code, CancellationToken cancellationToken = default)
    {
        var response = await SendAsync<VerifyCodeRequest, VerifyCodeResponse>(
            HttpMethod.Post, "auth/email/verify-code", new VerifyCodeRequest(email, code),
            BackendJsonContext.Default.VerifyCodeRequest, BackendJsonContext.Default.VerifyCodeResponse, null, cancellationToken);
        return response.VerificationToken;
    }

    public async Task<int> SendPasswordResetCodeAsync(string email, CancellationToken cancellationToken = default)
    {
        var response = await SendAsync<SendCodeRequest, SendCodeResponse>(
            HttpMethod.Post, "auth/password/send-code", new SendCodeRequest(email),
            BackendJsonContext.Default.SendCodeRequest, BackendJsonContext.Default.SendCodeResponse, null, cancellationToken);
        return response.ResendIn;
    }

    public async Task<string> VerifyPasswordResetCodeAsync(string email, string code, CancellationToken cancellationToken = default)
    {
        var response = await SendAsync<VerifyCodeRequest, VerifyResetCodeResponse>(
            HttpMethod.Post, "auth/password/verify-code", new VerifyCodeRequest(email, code),
            BackendJsonContext.Default.VerifyCodeRequest, BackendJsonContext.Default.VerifyResetCodeResponse, null, cancellationToken);
        return response.ResetToken;
    }

    public Task<AuthResult> ResetPasswordAsync(string email, string resetToken, string password, string passwordConfirmation, string deviceName, CancellationToken cancellationToken = default) =>
        AuthAsync("auth/password/reset", new ResetPasswordRequest(email, resetToken, password, passwordConfirmation, deviceName),
            BackendJsonContext.Default.ResetPasswordRequest, cancellationToken);

    public Task<AuthResult> RegisterAsync(Registration r, string deviceName, CancellationToken cancellationToken = default) =>
        AuthAsync("auth/register", new RegisterRequest(
                r.FirstNames, r.LastNames, r.Email, r.Password, r.PasswordConfirmation, r.VerificationToken,
                r.Province, NullIfEmpty(r.Address), NullIfEmpty(r.PostalCode), r.BirthDate?.ToString("yyyy-MM-dd"), deviceName),
            BackendJsonContext.Default.RegisterRequest, cancellationToken);

    public Task<AuthResult> LoginAsync(string email, string password, string deviceName, CancellationToken cancellationToken = default) =>
        AuthAsync("auth/login", new LoginRequest(email, password, deviceName),
            BackendJsonContext.Default.LoginRequest, cancellationToken);

    public Task<AuthResult> LoginWithGoogleAsync(string firebaseIdToken, string deviceName, string? province, CancellationToken cancellationToken = default) =>
        AuthAsync("auth/google", new GoogleLoginRequest(firebaseIdToken, deviceName, province),
            BackendJsonContext.Default.GoogleLoginRequest, cancellationToken);

    public async Task<AuthUser> GetMeAsync(string token, CancellationToken cancellationToken = default)
    {
        var response = await SendAsync<object, UserEnvelope>(
            HttpMethod.Get, "me", null, null, BackendJsonContext.Default.UserEnvelope, token, cancellationToken);
        return response.Data.ToAuthUser();
    }

    public async Task<AuthUser> UpdateProvinceAsync(string token, string province, CancellationToken cancellationToken = default)
    {
        var response = await SendAsync<UpdateProfileRequest, UserEnvelope>(
            HttpMethod.Patch, "me", new UpdateProfileRequest(province),
            BackendJsonContext.Default.UpdateProfileRequest, BackendJsonContext.Default.UserEnvelope, token, cancellationToken);
        return response.Data.ToAuthUser();
    }

    public async Task LogoutAsync(string token, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "auth/logout");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        using var response = await SendRawAsync(request, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    private async Task<AuthResult> AuthAsync<TRequest>(
        string path, TRequest body, JsonTypeInfo<TRequest> requestType, CancellationToken cancellationToken)
    {
        var response = await SendAsync(HttpMethod.Post, path, body, requestType,
            BackendJsonContext.Default.AuthTokenResponse, null, cancellationToken);
        return new AuthResult(response.Token, response.User.ToAuthUser(), response.IsNewUser);
    }

    private async Task<TResponse> SendAsync<TRequest, TResponse>(
        HttpMethod method,
        string path,
        TRequest? body,
        JsonTypeInfo<TRequest>? requestType,
        JsonTypeInfo<TResponse> responseType,
        string? token,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(method, path);
        if (body is not null && requestType is not null)
        {
            request.Content = JsonContent.Create(body, requestType);
        }

        if (token is not null)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        using var response = await SendRawAsync(request, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);

        try
        {
            return await response.Content.ReadFromJsonAsync(responseType, cancellationToken)
                ?? throw new JsonException("Respuesta vacía.");
        }
        catch (JsonException)
        {
            throw new BackendApiException(response.StatusCode, "El servidor respondió algo inesperado. Inténtalo de nuevo.");
        }
    }

    private async Task<HttpResponseMessage> SendRawAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        try
        {
            return await _http.SendAsync(request, cancellationToken);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException && !cancellationToken.IsCancellationRequested)
        {
            // Solo en Debug: la causa real (IP equivocada, permiso de red local, ATS…) en la salida de Rider.
            System.Diagnostics.Debug.WriteLine(
                $"[API] {request.Method} {new Uri(_http.BaseAddress!, request.RequestUri!)} falló: {ex}");
            throw new BackendApiException(null, "No pudimos conectar con el servidor. Revisa tu conexión e inténtalo de nuevo.");
        }
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        ErrorResponse? error = null;
        try
        {
            error = await response.Content.ReadFromJsonAsync(BackendJsonContext.Default.ErrorResponse, cancellationToken);
        }
        catch (JsonException)
        {
            // Respuesta sin JSON (p. ej. un proxy): se usa el mensaje genérico de abajo.
        }

        var fieldErrors = error?.Errors?
            .Where(e => e.Value.Length > 0)
            .ToDictionary(e => e.Key, e => e.Value[0]);

        var message = response.StatusCode switch
        {
            HttpStatusCode.TooManyRequests => TooManyRequestsMessage(response),
            HttpStatusCode.Unauthorized when error?.Message is null or "Unauthenticated." => "Tu sesión venció. Inicia sesión de nuevo.",
            HttpStatusCode.UnprocessableEntity when fieldErrors is { Count: > 0 } => fieldErrors.Values.First(),
            // Un 500 es un fallo no previsto: su mensaje puede traer detalles internos (con APP_DEBUG
            // activo, la excepción completa), así que nunca se muestra. Los 503 sí son mensajes
            // pensados para el usuario (p. ej. «No pudimos enviar el correo…»).
            HttpStatusCode.InternalServerError => "El servidor tuvo un problema. Inténtalo en unos minutos.",
            >= HttpStatusCode.InternalServerError when error?.Message is null or "Server Error" => "El servidor tuvo un problema. Inténtalo en unos minutos.",
            _ => error?.Message ?? "Algo salió mal. Inténtalo de nuevo."
        };

        throw new BackendApiException(response.StatusCode, message, fieldErrors);
    }

    private static string TooManyRequestsMessage(HttpResponseMessage response)
    {
        var wait = response.Headers.RetryAfter?.Delta;
        return wait is { } delta
            ? $"Demasiados intentos. Espera {Math.Ceiling(delta.TotalSeconds)} segundos e inténtalo de nuevo."
            : "Demasiados intentos. Espera un momento e inténtalo de nuevo.";
    }

    private static string? NullIfEmpty(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
