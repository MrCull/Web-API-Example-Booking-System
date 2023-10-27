using Microsoft.JSInterop;

namespace MyDevTools.Services;

public class ClipboardService(IJSRuntime jsInterop) : IClipboardService
{
    private readonly IJSRuntime _jsInterop = jsInterop;

    public async Task CopyToClipboardAsync(string text)
    {
        await _jsInterop.InvokeVoidAsync("navigator.clipboard.writeText", text);
    }
}
