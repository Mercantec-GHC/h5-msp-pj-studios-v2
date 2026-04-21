using System.Net.Http.Json;

namespace Frontend.Services
{
    public class AuthService
    {
        private readonly HttpClient _httpClient;

        public AuthService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<AuthResponse> LoginAsync(string email, string password)
        {
            try
            {
                var request = new { email, password };
                var response = await _httpClient.PostAsJsonAsync("/api/auth/login", request);
                
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<AuthResponse>() 
                        ?? new AuthResponse { Success = false, Message = "Invalid response" };
                }
                
                return new AuthResponse 
                { 
                    Success = false, 
                    Message = $"Login failed: {response.StatusCode}" 
                };
            }
            catch (Exception ex)
            {
                return new AuthResponse 
                { 
                    Success = false, 
                    Message = $"Error: {ex.Message}" 
                };
            }
        }

        public async Task<AuthResponse> SignUpAsync(string fullName, string email, string password)
        {
            try
            {
                var request = new { fullName, email, password };
                var response = await _httpClient.PostAsJsonAsync("/api/auth/signup", request);
                
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<AuthResponse>() 
                        ?? new AuthResponse { Success = false, Message = "Invalid response" };
                }
                
                return new AuthResponse 
                { 
                    Success = false, 
                    Message = $"Sign up failed: {response.StatusCode}" 
                };
            }
            catch (Exception ex)
            {
                return new AuthResponse 
                { 
                    Success = false, 
                    Message = $"Error: {ex.Message}" 
                };
            }
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
