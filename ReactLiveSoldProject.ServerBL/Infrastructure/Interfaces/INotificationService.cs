using ReactLiveSoldProject.ServerBL.DTOs;
using ReactLiveSoldProject.ServerBL.Models.Notifications;

namespace ReactLiveSoldProject.ServerBL.Infrastructure.Interfaces
{
    public interface INotificationService
    {
        Task<IEnumerable<NotificationDto>> GetNotificationsForUserAsync(string userId);
        Task<bool> MarkAsReadAsync(Guid notificationId, string userId);
        Task<bool> MarkAllAsReadAsync(string userId);
        Task<NotificationDto> CreateNotificationAsync(string userId, CreateNotificationDto createDto);
    }
}
