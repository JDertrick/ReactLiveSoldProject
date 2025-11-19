import { Bell, Loader2 } from 'lucide-react';
import { Popover, PopoverContent, PopoverTrigger } from '@/components/ui/popover';
import { Button } from '@/components/ui/button';
import { useNotifications } from '@/hooks/useNotifications';
import { NotificationItem } from './NotificationItem';
import { ScrollArea } from '@/components/ui/scroll-area';

export const NotificationDropdown = () => {
    const { notifications, unreadCount, markAsRead, markAllAsRead, loading } = useNotifications();

    return (
        <Popover>
            <PopoverTrigger asChild>
                <Button variant="ghost" size="icon" className="relative">
                    <Bell className="h-5 w-5" />
                    {unreadCount > 0 && (
                        <span className="absolute top-1 right-1 flex h-4 w-4 items-center justify-center rounded-full bg-red-500 text-xs text-white">
                            {unreadCount}
                        </span>
                    )}
                </Button>
            </PopoverTrigger>
            <PopoverContent className="w-80 p-0" align="end">
                <div className="p-3 flex justify-between items-center">
                    <h4 className="font-semibold text-lg">Notifications</h4>
                    {loading && <Loader2 className="h-5 w-5 animate-spin" />}
                </div>
                <ScrollArea className="h-[300px]">
                    {!loading && notifications.length === 0 ? (
                        <div className="p-4 text-center text-sm text-muted-foreground">
                            You have no new notifications.
                        </div>
                    ) : (
                        notifications.map(notification => (
                            <NotificationItem
                                key={notification.id}
                                notification={notification}
                                onMarkAsRead={() => markAsRead(notification.id)}
                            />
                        ))
                    )}
                </ScrollArea>
                <div className="p-2 border-t border-border/60 text-center">
                    <Button variant="link" size="sm" onClick={markAllAsRead} disabled={unreadCount === 0}>
                        Mark all as read
                    </Button>
                </div>
            </PopoverContent>
        </Popover>
    );
};