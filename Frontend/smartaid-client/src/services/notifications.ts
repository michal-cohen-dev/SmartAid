import {
  HubConnection,
  HubConnectionBuilder,
  LogLevel,
} from '@microsoft/signalr';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL?.replace(/\/+$/, '');

export function createNotificationsConnection(
  token: string
): HubConnection {
  if (!API_BASE_URL) {
    throw new Error('כתובת השרת לא הוגדרה.');
  }

  return new HubConnectionBuilder()
    .withUrl(`${API_BASE_URL}/hubs/notifications`, {
      accessTokenFactory: () => token,
    })
    .withAutomaticReconnect()
    .configureLogging(LogLevel.Information)
    .build();
}