import * as signalR from '@microsoft/signalr';

let hubConnection = null;

export const initSignalR = (onReceiveNotification, onUnreadCountUpdated) => {
  const token = localStorage.getItem('accessToken');
  if (!token) return null;

  if (!hubConnection) {
    hubConnection = new signalR.HubConnectionBuilder()
      .withUrl('/notificationHub', {
        accessTokenFactory: () => localStorage.getItem('accessToken') || '',
      })
      .withAutomaticReconnect()
      .build();
  }

  // Clear existing handlers to prevent stale callbacks
  hubConnection.off('ReceiveNotification');
  hubConnection.off('UnreadCountUpdated');

  hubConnection.on('ReceiveNotification', (notification) => {
    if (onReceiveNotification) {
      onReceiveNotification(notification);
    }
  });

  hubConnection.on('UnreadCountUpdated', (count) => {
    if (onUnreadCountUpdated !== undefined && onUnreadCountUpdated !== null) {
      onUnreadCountUpdated(count);
    }
  });

  if (hubConnection.state === signalR.HubConnectionState.Disconnected) {
    hubConnection
      .start()
      .then(() => {
        const userId = localStorage.getItem('userId');
        if (userId && hubConnection.state === signalR.HubConnectionState.Connected) {
          hubConnection.invoke('JoinUserGroup', userId).catch(() => {});
        }
      })
      .catch((err) => console.error('SignalR Connection Error: ', err));
  } else if (hubConnection.state === signalR.HubConnectionState.Connected) {
    const userId = localStorage.getItem('userId');
    if (userId) {
      hubConnection.invoke('JoinUserGroup', userId).catch(() => {});
    }
  }

  return hubConnection;
};

export const stopSignalR = () => {
  if (hubConnection) {
    hubConnection.stop();
    hubConnection = null;
  }
};
