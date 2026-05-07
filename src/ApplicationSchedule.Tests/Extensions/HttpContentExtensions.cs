using System.Text.Json;

namespace ApplicationSchedule.Tests.Extensions;

/// <summary>
/// Extensiones para HttpContent para facilitar la lectura de respuestas JSON dinámicas en tests.
/// </summary>
public static class HttpContentExtensions
{
    /// <summary>
    /// Lee el contenido como un objeto dinámico (utiliza System.Text.Json internamente).
    /// </summary>
    public static async Task<dynamic?> ReadAsAsync<T>(this HttpContent content)
    {
        var json = await content.ReadAsStringAsync();
        if (string.IsNullOrEmpty(json))
            return null;

        var document = JsonDocument.Parse(json);
        return document.RootElement;
    }

    /// <summary>
    /// Lee el contenido como un objeto dinámico (JSON).
    /// </summary>
    public static async Task<dynamic?> ReadAsDynamicAsync(this HttpContent content)
    {
        var json = await content.ReadAsStringAsync();
        if (string.IsNullOrEmpty(json))
            return null;

        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.Clone();
    }
}
