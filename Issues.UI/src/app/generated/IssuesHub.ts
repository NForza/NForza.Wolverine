import { Injectable, signal } from '@angular/core';
import { HubConnectionBuilder, HubConnection, LogLevel } from '@microsoft/signalr';
import { IssueCreated } from './Wolverine/Issues/Contracts/Issues/IssueCreated';
import { IssueAssigned } from './Wolverine/Issues/Contracts/Issues/IssueAssigned';
import { IssueUnassigned } from './Wolverine/Issues/Contracts/Issues/IssueUnassigned';
import { IssueClosed } from './Wolverine/Issues/Contracts/Issues/Lifecycle/IssueClosed';
import { IssueOpened } from './Wolverine/Issues/Contracts/Issues/Lifecycle/IssueOpened';

export interface HubEvent<T> {
  eventType: string;
  data: T;
  receivedAt: Date;
}

@Injectable({ providedIn: 'root' })
export class IssuesHubService {
  private connection: HubConnection;
  private subscribedIssueIds = new Set<string>();

  readonly issueCreated = signal<IssueCreated | null>(null);
  readonly issueAssigned = signal<IssueAssigned | null>(null);
  readonly issueUnassigned = signal<IssueUnassigned | null>(null);
  readonly issueClosed = signal<IssueClosed | null>(null);
  readonly issueOpened = signal<IssueOpened | null>(null);
  readonly allEvents = signal<HubEvent<unknown>[]>([]);
  readonly connected = signal(false);

  constructor() {
    this.connection = new HubConnectionBuilder()
      .withUrl('/hub/issues')
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Information)
      .build();

    this.connection.on('IssueCreated', (data: IssueCreated) => {
      this.issueCreated.set(data);
      this.allEvents.update(current => [
        { eventType: 'IssueCreated', data, receivedAt: new Date() },
        ...current,
      ]);
    });
    this.connection.on('IssueAssigned', (data: IssueAssigned) => {
      this.issueAssigned.set(data);
      this.allEvents.update(current => [
        { eventType: 'IssueAssigned', data, receivedAt: new Date() },
        ...current,
      ]);
    });
    this.connection.on('IssueUnassigned', (data: IssueUnassigned) => {
      this.issueUnassigned.set(data);
      this.allEvents.update(current => [
        { eventType: 'IssueUnassigned', data, receivedAt: new Date() },
        ...current,
      ]);
    });
    this.connection.on('IssueClosed', (data: IssueClosed) => {
      this.issueClosed.set(data);
      this.allEvents.update(current => [
        { eventType: 'IssueClosed', data, receivedAt: new Date() },
        ...current,
      ]);
    });
    this.connection.on('IssueOpened', (data: IssueOpened) => {
      this.issueOpened.set(data);
      this.allEvents.update(current => [
        { eventType: 'IssueOpened', data, receivedAt: new Date() },
        ...current,
      ]);
    });

    this.connection.onclose(() => this.connected.set(false));
    this.connection.onreconnected(() => {
      this.connected.set(true);
      this.resubscribe();
    });
    this.start();
  }

  async subscribeToIssue(issueId: string): Promise<void> {
    this.subscribedIssueIds.add(issueId);
    if (this.connected()) {
      await this.connection.invoke('JoinGroup', issueId);
    }
  }

  async unsubscribeFromIssue(issueId: string): Promise<void> {
    this.subscribedIssueIds.delete(issueId);
    if (this.connected()) {
      await this.connection.invoke('LeaveGroup', issueId);
    }
  }

  private async resubscribe(): Promise<void> {
    for (const issueId of this.subscribedIssueIds) {
      await this.connection.invoke('JoinGroup', issueId);
    }
  }

  private async start(): Promise<void> {
    try {
      await this.connection.start();
      this.connected.set(true);
      await this.resubscribe();
    } catch (err) {
      console.error('SignalR connection error:', err);
      setTimeout(() => this.start(), 5000);
    }
  }
}

