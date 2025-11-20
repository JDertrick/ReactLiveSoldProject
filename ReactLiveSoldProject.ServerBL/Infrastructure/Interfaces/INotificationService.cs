using ReactLiveSoldProject.ServerBL.DTOs;
using ReactLiveSoldProject.ServerBL.Models.Notifications;

namespace ReactLiveSoldProject.ServerBL.Infrastructure.Interfaces
{
    public interface INotificationService
    {
        Task<IEnumerable<NotificationDto>> GetNotificationsForUserAsync(Guid userId);
        Task<bool> MarkAsReadAsync(Guid notificationId, Guid userId);
        Task<bool> MarkAllAsReadAsync(Guid userId);
        Task<NotificationDto> CreateNotificationAsync(Guid userId, CreateNotificationDto createDto);
    }
}
