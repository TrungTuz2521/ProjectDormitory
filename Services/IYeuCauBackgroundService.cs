// Services/IYeuCauBackgroundService.cs
namespace KTX.Services
{
    public interface IYeuCauBackgroundService
    {
        void ScheduleAutoComplete(int yeuCauId);
        Task HoanThanhYeuCauAsync(int yeuCauId);
    }
}