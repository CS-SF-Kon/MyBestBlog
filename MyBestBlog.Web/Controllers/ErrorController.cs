using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MyBestBlog.Web.Models;

namespace MyBestBlog.Web.Controllers;

[AllowAnonymous]
public class ErrorController : Controller
{
    private readonly ILogger<ErrorController> _logger;

    public ErrorController(ILogger<ErrorController> logger)
    {
        _logger = logger;
    }

    [Route("Error/{statusCode:int}")]
    public IActionResult HttpStatusCodeHandler(int statusCode)
    {
        var statusCodeData = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();

        _logger.LogWarning(
            $"Status Code: {statusCode} - Path: {statusCodeData?.OriginalPath}"
        );

        return statusCode switch
        {
            404 => View("NotFound"),
            403 => View("Forbidden"),
            _ => View("Error", new ErrorViewModel
            {
                StatusCode = statusCode,
                Path = statusCodeData?.OriginalPath
            })
        };
    }

    [Route("Error")]
    public IActionResult HandleError()
    {
        var exceptionHandlerPathFeature =
            HttpContext.Features.Get<IExceptionHandlerPathFeature>();

        _logger.LogError($"Path: {exceptionHandlerPathFeature?.Path} - " +
            $"Error: {exceptionHandlerPathFeature?.Error}");

        return View("Error", new ErrorViewModel
        {
            StatusCode = 500,
            Path = exceptionHandlerPathFeature?.Path,
            ErrorMessage = "Произошла внутренняя ошибка сервера"
        });
    }
}