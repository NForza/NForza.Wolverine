import { Component, inject } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { IssuesHubService } from './generated/IssuesHub';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  template: `
    <nav class="nav">
      <span class="brand">Issues Tracker</span>
      <div class="links">
        <a routerLink="/issues" routerLinkActive="active" [routerLinkActiveOptions]="{ exact: true }">Issues</a>
        <a routerLink="/assignees" routerLinkActive="active">Assignees</a>
        <a routerLink="/events" routerLinkActive="active">Events</a>
      </div>
      <span class="status" [class.connected]="hub.connected()">
        {{ hub.connected() ? 'Live' : 'Offline' }}
      </span>
    </nav>
    <main>
      <router-outlet />
    </main>
  `,
  styles: `
    :host { display: block; font-family: system-ui, -apple-system, sans-serif; color: #1a1a1a; }
    .nav { display: flex; align-items: center; gap: 1.5rem; padding: .75rem 1.5rem; border-bottom: 1px solid #e0e0e0; background: #fafafa; }
    .brand { font-weight: 700; font-size: 1.1rem; }
    .links { display: flex; gap: 1rem; }
    .links a { color: #555; text-decoration: none; font-size: .9rem; padding: .25rem .5rem; border-radius: 4px; }
    .links a:hover { color: #1a56db; }
    .links a.active { color: #1a56db; background: #eef2ff; font-weight: 500; }
    .status { margin-left: auto; padding: .2rem .6rem; border-radius: 999px; font-size: .75rem; font-weight: 500; background: #fee; color: #c00; }
    .status.connected { background: #efe; color: #060; }
    main { max-width: 960px; margin: 1.5rem auto; padding: 0 1.5rem; }
    .muted { color: #888; font-style: italic; }
    .error { color: #c00; }
    .btn { display: inline-block; padding: .4rem .9rem; border: 1px solid #d1d5db; border-radius: 6px; font-size: .9rem; text-decoration: none; cursor: pointer; background: white; }
    .btn-primary { background: #1a56db; color: white; border-color: #1a56db; }
    .btn-primary:hover { background: #1e40af; }
    .btn-primary:disabled { opacity: .5; cursor: not-allowed; }
    .btn-secondary { background: #f3f4f6; color: #374151; }
    .btn-secondary:hover { background: #e5e7eb; }
    .btn-secondary:disabled { opacity: .5; cursor: not-allowed; }
  `,
})
export class App {
  protected readonly hub = inject(IssuesHubService);
}
