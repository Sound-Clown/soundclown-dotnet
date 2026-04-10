// @ Services/IToastService.cs
namespace MusicApp.Services;

public enum ToastType { Success, Error, Info }

public interface IToastService
{
    void Show(ToastType type, string message);
}
