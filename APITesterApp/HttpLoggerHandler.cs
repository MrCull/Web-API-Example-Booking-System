public class HttpLoggingHandler : DelegatingHandler
{
    public HttpLoggingHandler(HttpMessageHandler innerHandler) : base(innerHandler)
    {
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Request: {request.Method} {request.RequestUri}");

        // Optionally log the request headers and body
        foreach (KeyValuePair<string, IEnumerable<string>> header in request.Headers)
        {
            Console.WriteLine($"{header.Key}: {string.Join(", ", header.Value)}");
        }

        if (request.Content != null)
        {
            string requestBody = await request.Content.ReadAsStringAsync();
            Console.WriteLine($"Request Body: {requestBody}");
        }

        System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
        HttpResponseMessage response = await base.SendAsync(request, cancellationToken);
        stopwatch.Stop();

        Console.WriteLine($"Response: {response.StatusCode} in {stopwatch.ElapsedMilliseconds}ms");

        // Optionally log the response headers and body
        foreach (KeyValuePair<string, IEnumerable<string>> header in response.Headers)
        {
            Console.WriteLine($"{header.Key}: {string.Join(", ", header.Value)}");
        }

        if (response.Content != null)
        {
            string responseBody = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Response Body: {responseBody}");
        }

        return response;
    }
}
