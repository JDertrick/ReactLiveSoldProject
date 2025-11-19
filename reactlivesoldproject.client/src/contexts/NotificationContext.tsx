import React, { createContext, useState, ReactNode, useMemo, useEffect, useCallback } from 'react';
import { Notification } from '@/types/notification';
import { useAuthStore } from '@/store/authStore';
import { apiClient } from '@/services/api';

interface NotificationContextType {
  notifications: Notification[];
  markAsRead: (id: string) => Promise<void>;
  markAllAsRead: () => Promise<void>;
  unreadCount: number;
  loading: boolean;
}

export const NotificationContext = createContext<NotificationContextType | undefined>(undefined);

interface NotificationProviderProps {
  children: ReactNode;
}

export const NotificationProvider: React.FC<NotificationProviderProps> = ({ children }) => {
  const [notifications, setNotifications] = useState<Notification[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const { user } = useAuthStore();

  const fetchNotifications = useCallback(async () => {
    if (!user) return;
    setLoading(true);
    try {
      const response = await apiClient.get<Notification[]>('/notifications');
      setNotifications(response.data);
    } catch (error) {
      console.error('Failed to fetch notifications', error);
    } finally {
      setLoading(false);
    }
  }, [user]);

  useEffect(() => {
    fetchNotifications();
  }, [fetchNotifications]);

  const markAsRead = async (id: string) => {
    const notification = notifications.find(n => n.id === id);
    if(notification && !notification.isRead) {
        setNotifications(prev =>
            prev.map(n => (n.id === id ? { ...n, isRead: true } : n))
          );
        try {
            await apiClient.post(`/notifications/${id}/read`);
        } catch (error) {
            console.error('Failed to mark notification as read', error);
            // Revert optimistic update
            setNotifications(prev =>
                prev.map(n => (n.id === id ? { ...n, isRead: false } : n))
            );
        }
    }
  };

  const markAllAsRead = async () => {
    const unreadNotifications = notifications.filter(n => !n.isRead);
    if (unreadNotifications.length === 0) return;
    
    const originalNotifications = [...notifications];
    setNotifications(prev => prev.map(n => ({ ...n, isRead: true })));

    try {
        await apiClient.post('/notifications/read-all');
    } catch (error) {
        console.error('Failed to mark all notifications as read', error);
        setNotifications(originalNotifications);
    }
  };

  const unreadCount = useMemo(() => {
    return notifications.filter(n => !n.isRead).length;
  }, [notifications]);

  const contextValue = useMemo(() => ({
    notifications,
    markAsRead,
    markAllAsRead,
    unreadCount,
    loading,
  }), [notifications, loading]);

  return (
    <NotificationContext.Provider value={contextValue}>
      {children}
    </NotificationContext.Provider>
  );
};