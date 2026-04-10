using Microsoft.JSInterop;

namespace MusicApp.Services;

public class ToastService : IToastService
{
    private readonly IJSRuntime _js;

    public ToastService(IJSRuntime js)
    {
        _js = js;
    }

    public async void Show(ToastType type, string message)
    {
        var typeStr = type switch
        {
            ToastType.Success => "success",
            ToastType.Error => "error",
            _ => "info"
        };
        await _js.InvokeVoidAsync("showToast", typeStr, message);
    }
}
