// Services/YeuCauBackgroundService.cs
using Hangfire;
using KTX.Entities;
using Microsoft.EntityFrameworkCore;

namespace KTX.Services
{
    public class YeuCauBackgroundService : IYeuCauBackgroundService
    {
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly IServiceProvider _serviceProvider;

        public YeuCauBackgroundService(
            IBackgroundJobClient backgroundJobClient,
            IServiceProvider serviceProvider)
        {
            _backgroundJobClient = backgroundJobClient;
            _serviceProvider = serviceProvider;
        }

        public void ScheduleAutoComplete(int yeuCauId)
        {
            var random = new Random();
            int days = random.Next(2, 4); // 2 hoặc 3 ngày
            var delay = TimeSpan.FromDays(days);

            _backgroundJobClient.Schedule<IYeuCauBackgroundService>(
                service => service.HoanThanhYeuCauAsync(yeuCauId),
                delay);
        }

        public async Task HoanThanhYeuCauAsync(int yeuCauId)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<SinhVienKtxContext>();

            var yc = await context.YeuCaus.FindAsync(yeuCauId);
            if (yc != null && yc.TrangThaiYc == "Đang xử lý")
            {
                yc.TrangThaiYc = "Đã giải quyết";
                await context.SaveChangesAsync();
            }
        }
    }
}