import { Notification } from '@/types/notification';
import { Bell, CheckCheck, Info, TriangleAlert } from 'lucide-react';
import { cn } from '@/lib/utils';

interface NotificationItemProps {
    notification: Notification;
    onMarkAsRead: (id: string) => void;
}

const iconMap = {
    info: <Info className="h-5 w-5 text-blue-500" />,
    success: <CheckCheck className="h-5 w-5 text-green-500" />,
    warning: <TriangleAlert className="h-5 w-5 text-yellow-500" />,
    error: <TriangleAlert className="h-5 w-5 text-red-500" />,
};

export const NotificationItem = ({ notification, onMarkAsRead }: NotificationItemProps) => {
    const icon = iconMap[notification.type] || <Bell className="h-5 w-5" />;

    return (
        <div
            className={cn(
                'w-full p-3 flex items-start gap-3 border-b border-border/60 last:border-b-0 hover:bg-muted/50 cursor-pointer',
                !notification.isRead && 'bg-primary/5'
            )}
            onClick={() => onMarkAsRead(notification.id)}
        >
            <div className="flex-shrink-0">{icon}</div>
            <div className="flex-grow">
                <p className="font-semibold text-sm">{notification.title}</p>
                <p className="text-xs text-muted-foreground">{notification.message}</p>
                <p className="text-xs text-muted-foreground/80 mt-1">
                    {new Date(notification.createdAt).toLocaleString()}
                </p>
            </div>
            {!notification.isRead && (
                <div className="flex-shrink-0 self-center">
                    <div className="h-2 w-2 rounded-full bg-blue-500"></div>
                </div>
            )}
        </div>
    );
};
