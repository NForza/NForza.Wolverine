import { Injectable, signal } from '@angular/core';
import { HubConnectionBuilder, HubConnection, LogLevel } from '@microsoft/signalr';

export interface IssueEvent {
  eventType: string;
  data: Record<string, unknown>;
  receivedAt: Date;
}

@Injectable({ providedIn: 'root' })
export class SignalRService {
  private connection: HubConnection;

  readonly events = signal<IssueEvent[]>([]);
  readonly connected = signal(false);

  constructor() {
    this.connection = new HubConnectionBuilder()
      .withUrl('/hub/issues')
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Information)
      .build();

    const eventTypes = ['IssueCreated', 'IssueAssigned', 'IssueUnassigned', 'IssueClosed', 'IssueOpened'];
    for (const eventType of eventTypes) {
      this.connection.on(eventType, (data: Record<string, unknown>) => {
        this.events.update(current => [
          { eventType, data, receivedAt: new Date() },
          ...current,
        ]);
      });
    }

    this.connection.onclose(() => this.connected.set(false));
    this.connection.onreconnected(() => this.connected.set(true));

    this.start();
  }

  private async start(): Promise<void> {
    try {
      await this.connection.start();
      this.connected.set(true);
      console.log('SignalR connected');
    } catch (err) {
      console.error('SignalR connection error:', err);
      setTimeout(() => this.start(), 5000);
    }
  }
}
