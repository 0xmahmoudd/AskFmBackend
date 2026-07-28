import * as signalR from '@microsoft/signalr';

let hubConnection = null;

export const initSignalR = (onReceiveNotification) => {
  const token = localStorage.getItem('accessToken');
  if (!token) return null;

  if (hubConnection && hubConnection.state === signalR.HubConnectionState.Connected) {
    return hubConnection;
  }

  hubConnection = new signalR.HubConnectionBuilder()
    .withUrl('/notificationHub', {
      accessTokenFactory: () => localStorage.getItem('accessToken') || '',
    })
    .withAutomaticReconnect()
    .build();

  hubConnection.on('ReceiveNotification', (notification) => {
    if (onReceiveNotification) {
      onReceiveNotification(notification);
    }
  });

  hubConnection
    .start()
    .catch((err) => console.error('SignalR Connection Error: ', err));

  return hubConnection;
};

export const stopSignalR = () => {
  if (hubConnection) {
    hubConnection.stop();
    hubConnection = null;
  }
};
