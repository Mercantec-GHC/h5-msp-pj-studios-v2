using System.Net.Http.Json;
using System.Text.Json;

namespace Frontend.Services
{
    public sealed class PictureOfDayDto
    {
        public string Title { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string Explanation { get; set; } = string.Empty;
        public string MediaType { get; set; } = string.Empty;
    }

    public class PictureService
    {
        private readonly HttpClient _http;
        // Use Picsum's public list endpoint to get a random image entry
        private const string PicsumListUrl = "https://picsum.photos/v2/list?page=1&limit=1";

        public PictureService(HttpClient http)
        {
            _http = http;
        }

        public async Task<PictureOfDayDto?> GetPictureOfDayAsync()
        {
            try
            {
                var list = await _http.GetFromJsonAsync<JsonElement[]>(PicsumListUrl);
                if (list == null || list.Length == 0)
                    return null;

                var item = list[0];
                var author = item.TryGetProperty("author", out var a) ? a.GetString() ?? string.Empty : string.Empty;
                var download = item.TryGetProperty("download_url", out var d) ? d.GetString() ?? string.Empty : string.Empty;

                return new PictureOfDayDto
                {
                    Title = "Billede af " + author,
                    Url = download,
                    Explanation = string.Empty,
                    MediaType = "image"
                };
            }
            catch
            {
                return null;
            }
        }
    }
}
