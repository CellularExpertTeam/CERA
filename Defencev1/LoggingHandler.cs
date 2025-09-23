using Microsoft.Extensions.Logging;
using System.Net;

namespace Defencev1;

public class LoggingHandler : DelegatingHandler
{
    private readonly ILogger _logger;
    public LoggingHandler(HttpMessageHandler innerHandler, ILogger<LoggingHandler> logger) : base(innerHandler)
    {
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"➡️ {request.Method} {request.RequestUri}");

        if (request.Content != null)
        {
            var requestBody = await request.Content.ReadAsStringAsync();
            _logger.LogInformation($"Request Body: {requestBody}");
        }

        HttpResponseMessage response;
        try
        {
            response = await base.SendAsync(request, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError($"❌ Connection Failed: {ex.Message}");

            response = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
            {
                RequestMessage = request,
                Content = new StringContent($"Connection failed: {ex.Message}")
            };
        }

        _logger.LogInformation($"⬅️ {response.StatusCode} {request?.RequestUri}");

        if (response.Content != null)
        {
            var responseBody = await response.Content.ReadAsStringAsync();

            string truncatedBody = responseBody.Trim();
            if (!string.IsNullOrEmpty(responseBody) && responseBody.Length > 600)
            {
                truncatedBody = responseBody.Substring(0, 500) + "... (truncated)";
            }
            else if (string.IsNullOrEmpty(responseBody))
            {
                truncatedBody = "[No content]";
            }

            _logger.LogInformation($"Response Body: {truncatedBody}\n");
        }

        return response;
    }
}