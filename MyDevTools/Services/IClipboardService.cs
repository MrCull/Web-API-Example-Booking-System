
namespace MyDevTools.Services;

public interface IClipboardService
{
    Task CopyToClipboardAsync(string text);
}