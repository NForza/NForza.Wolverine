import { Component, inject } from '@angular/core';
import { DatePipe } from '@angular/common';
import { SignalRService, IssueEvent } from './signalr.service';

@Component({
  selector: 'app-event-list',
  imports: [DatePipe],
  template: `
    <div class="header">
      <h1>Issue Events</h1>
      <span class="status" [class.connected]="signalr.connected()">
        {{ signalr.connected() ? 'Connected' : 'Disconnected' }}
      </span>
    </div>

    @if (signalr.events().length === 0) {
      <p class="empty">No events received yet. Waiting for issue activity...</p>
    } @else {
      <ul class="events">
        @for (event of signalr.events(); track event.receivedAt) {
          <li class="event" [attr.data-type]="event.eventType">
            <div class="event-header">
              <span class="event-type">{{ event.eventType }}</span>
              <time>{{ event.receivedAt | date:'HH:mm:ss' }}</time>
            </div>
            <div class="event-data">{{ formatData(event) }}</div>
          </li>
        }
      </ul>
    }
  `,
  styles: `
    :host { display: block; max-width: 720px; margin: 2rem auto; font-family: system-ui, sans-serif; }
    .header { display: flex; align-items: center; justify-content: space-between; margin-bottom: 1rem; }
    h1 { margin: 0; font-size: 1.5rem; }
    .status { padding: .25rem .75rem; border-radius: 999px; font-size: .8rem; background: #fee; color: #c00; }
    .status.connected { background: #efe; color: #060; }
    .empty { color: #888; font-style: italic; }
    .events { list-style: none; padding: 0; }
    .event { border: 1px solid #e0e0e0; border-radius: 8px; padding: .75rem 1rem; margin-bottom: .5rem; }
    .event-header { display: flex; justify-content: space-between; margin-bottom: .25rem; }
    .event-type { font-weight: 600; color: #1a56db; }
    time { color: #888; font-size: .85rem; }
    .event-data { font-family: monospace; font-size: .85rem; color: #444; word-break: break-all; }
  `,
})
export class EventListComponent {
  protected readonly signalr = inject(SignalRService);

  protected formatData(event: IssueEvent): string {
    return JSON.stringify(event.data);
  }
}
