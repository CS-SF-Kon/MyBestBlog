namespace MyBestBlog.Web.Models;

public class ErrorViewModel
{
    public string? RequestId { get; set; }
    public int StatusCode { get; set; }
    public string? Path { get; set; }
    public string? ErrorMessage { get; set; }
    public Exception? Exception { get; set; }

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    public bool ShowPath => !string.IsNullOrEmpty(Path);
    public bool ShowErrorMessage => !string.IsNullOrEmpty(ErrorMessage);
}
