using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ReactLiveSoldProject.ServerBL.Base;
using ReactLiveSoldProject.ServerBL.DTOs;
using ReactLiveSoldProject.ServerBL.Infrastructure.Interfaces;
using ReactLiveSoldProject.ServerBL.Models.Notifications;

namespace ReactLiveSoldProject.ServerBL.Infrastructure.Services
{
    public class NotificationService : INotificationService
    {
        private readonly LiveSoldDbContext _context;
        private readonly IMapper _mapper;

        public NotificationService(LiveSoldDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<NotificationDto> CreateNotificationAsync(Guid userId, CreateNotificationDto createDto)
        {
            if (!Enum.TryParse<NotificationType>(createDto.Type, true, out var notificationType))
            {
                notificationType = NotificationType.Info; // Default value if parsing fails
            }

            var notification = new Notification
            {
                UserId = userId,
                Title = createDto.Title,
                Message = createDto.Message,
                Type = notificationType,
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            return _mapper.Map<NotificationDto>(notification);
        }

        public async Task<IEnumerable<NotificationDto>> GetNotificationsForUserAsync(Guid userId)
        {
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            return _mapper.Map<IEnumerable<NotificationDto>>(notifications);
        }

        public async Task<bool> MarkAllAsReadAsync(Guid userId)
        {
            var notificationsToUpdate = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            if (!notificationsToUpdate.Any())
            {
                return true;
            }

            foreach (var notification in notificationsToUpdate)
            {
                notification.IsRead = true;
            }

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> MarkAsReadAsync(Guid notificationId, Guid userId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

            if (notification == null || notification.IsRead)
            {
                return false;
            }

            notification.IsRead = true;
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
