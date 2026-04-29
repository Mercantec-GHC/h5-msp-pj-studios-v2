using System.Net.Http.Json;
using System.Text.Json;
using System.Text;
using Microsoft.JSInterop;

namespace Frontend.Services
{
    public class AuthService
    {
        private const string TokenStorageKey = "authToken";
        private const string UserIdStorageKey = "userId";
        private readonly HttpClient _httpClient;
        private readonly IJSRuntime _jsRuntime;

        public AuthService(HttpClient httpClient, IJSRuntime jsRuntime)
        {
            _httpClient = httpClient;
            _jsRuntime = jsRuntime;
        }

        public async Task<AuthResponse> LoginAsync(string email, string password)
        {
            try
            {
                var request = new { email, password };
                var response = await _httpClient.PostAsJsonAsync("/api/auth/login", request);
                
                if (response.IsSuccessStatusCode)
                {
                    var payload = await response.Content.ReadFromJsonAsync<LoginResponse>();
                    if (string.IsNullOrWhiteSpace(payload?.Token))
                    {
                        return new AuthResponse { Success = false, Message = "Login lykkedes, men token mangler." };
                    }

                    return new AuthResponse
                    {
                        Success = true,
                        Message = "Login lykkedes.",
                        Token = payload.Token
                    };
                }
                
                return new AuthResponse 
                { 
                    Success = false, 
                    Message = await BuildErrorMessageAsync("Login fejlede", response)
                };
            }
            catch (Exception ex)
            {
                return new AuthResponse 
                { 
                    Success = false, 
                    Message = $"Fejl: {ex.Message}" 
                };
            }
        }

        public async Task<AuthResponse> SignUpAsync(string username, string email, string password, string passwordConfirm)
        {
            try
            {
                var request = new { username, email, password, passwordConfirm };
                var response = await _httpClient.PostAsJsonAsync("/api/auth/register", request);
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return new AuthResponse
                    {
                        Success = true,
                        Message = string.IsNullOrWhiteSpace(content) ? "Bruger oprettet." : content
                    };
                }
                
                return new AuthResponse 
                { 
                    Success = false, 
                    Message = await BuildErrorMessageAsync("Signup fejlede", response)
                };
            }
            catch (Exception ex)
            {
                return new AuthResponse 
                { 
                    Success = false, 
                    Message = $"Fejl: {ex.Message}" 
                };
            }
        }

        public async Task SaveTokenAsync(string token)
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", TokenStorageKey, token);
        }

        public async Task<string?> GetTokenAsync()
        {
            return await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", TokenStorageKey);
        }

        public async Task ClearTokenAsync()
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", TokenStorageKey);
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", UserIdStorageKey);
        }

        public async Task SaveUserIdFromTokenAsync(string token)
        {
            try
            {
                var userId = ExtractUserIdFromJwt(token);
                if (!string.IsNullOrWhiteSpace(userId))
                {
                    await _jsRuntime.InvokeVoidAsync("localStorage.setItem", UserIdStorageKey, userId);
                }
            }
            catch
            {
                // Silently fail if unable to decode token
            }
        }

        public async Task<string?> GetUserIdAsync()
        {
            return await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", UserIdStorageKey);
        }

        private static string? ExtractUserIdFromJwt(string token)
        {
            try
            {
                var parts = token.Split('.');
                if (parts.Length != 3)
                    return null;

                var payload = parts[1];
                payload = payload.Replace("-", "+").Replace("_", "/");
                payload += new string('=', (4 - payload.Length % 4) % 4);

                var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(payload));
                using var doc = JsonDocument.Parse(decoded);

                if (doc.RootElement.TryGetProperty("sub", out var subElement))
                {
                    return subElement.GetString();
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        private static async Task<string> BuildErrorMessageAsync(string prefix, HttpResponseMessage response)
        {
            var raw = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(raw))
            {
                return $"{prefix}: {(int)response.StatusCode} {response.StatusCode}";
            }

            try
            {
                using var doc = JsonDocument.Parse(raw);
                if (doc.RootElement.ValueKind == JsonValueKind.String)
                {
                    return $"{prefix}: {doc.RootElement.GetString()}";
                }
            }
            catch
            {
                // Keep raw response if payload is not valid JSON.
            }

            return $"{prefix}: {raw}";
        }

        private sealed class LoginResponse
        {
            public string Token { get; set; } = string.Empty;
        }
    }

    public class AuthResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? Token { get; set; }
        public UserDto? User { get; set; }
    }

    public class UserDto
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}
