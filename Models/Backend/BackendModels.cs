using System.Text.Json.Serialization;

namespace CompraPublicCercaMobile.Models.Backend;

// Contratos JSON de la API Laravel. Los nombres siguen snake_case por BackendJsonContext.

internal sealed record SendCodeRequest(string Email);

internal sealed record SendCodeResponse(string Message, int ExpiresIn, int ResendIn);

internal sealed record VerifyCodeRequest(string Email, string Code);

internal sealed record VerifyCodeResponse(string Message, string VerificationToken);

internal sealed record RegisterRequest(
    string FirstNames,
    string LastNames,
    string Email,
    string Password,
    string PasswordConfirmation,
    string VerificationToken,
    string Province,
    string? Address,
    string? PostalCode,
    string? BirthDate,
    string DeviceName);

internal sealed record LoginRequest(string Email, string Password, string DeviceName);

internal sealed record VerifyResetCodeResponse(string Message, string ResetToken);

internal sealed record ResetPasswordRequest(
    string Email,
    string ResetToken,
    string Password,
    string PasswordConfirmation,
    string DeviceName);

internal sealed record GoogleLoginRequest(string IdToken, string DeviceName, string? Province);

internal sealed record UpdateProfileRequest(string Province);

internal sealed record UserDto(
    long Id,
    string FirstNames,
    string LastNames,
    string Email,
    string? Province,
    bool HasPassword,
    bool GoogleLinked)
{
    public AuthUser ToAuthUser() => new(Id, FirstNames, LastNames, Email, Province, HasPassword, GoogleLinked);
}

internal sealed record AuthTokenResponse(string Token, DateTimeOffset TokenExpiresAt, bool IsNewUser, UserDto User);

/// <summary>Laravel envuelve los JsonResource en "data".</summary>
internal sealed record UserEnvelope(UserDto Data);

/// <summary>Errores de Laravel: mensaje general y, en 422, errores por campo.</summary>
internal sealed record ErrorResponse(string? Message, Dictionary<string, string[]>? Errors);

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower,
    PropertyNameCaseInsensitive = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(SendCodeRequest))]
[JsonSerializable(typeof(SendCodeResponse))]
[JsonSerializable(typeof(VerifyCodeRequest))]
[JsonSerializable(typeof(VerifyCodeResponse))]
[JsonSerializable(typeof(RegisterRequest))]
[JsonSerializable(typeof(LoginRequest))]
[JsonSerializable(typeof(VerifyResetCodeResponse))]
[JsonSerializable(typeof(ResetPasswordRequest))]
[JsonSerializable(typeof(GoogleLoginRequest))]
[JsonSerializable(typeof(UpdateProfileRequest))]
[JsonSerializable(typeof(AuthTokenResponse))]
[JsonSerializable(typeof(UserEnvelope))]
[JsonSerializable(typeof(ErrorResponse))]
internal sealed partial class BackendJsonContext : JsonSerializerContext
{
}
