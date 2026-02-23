import { Component } from '@angular/core';
import { EventListComponent } from './event-list.component';

@Component({
  selector: 'app-root',
  imports: [EventListComponent],
  template: `<app-event-list />`,
  styles: `
    :host { display: block; padding: 1rem; }
  `,
})
export class App {}
