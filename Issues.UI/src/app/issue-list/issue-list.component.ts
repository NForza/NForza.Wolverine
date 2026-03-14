import { Component, effect, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { DatePipe } from '@angular/common';
import { IssuesApiService } from '../services/issues-api.service';
import { IssuesHubService } from '../generated/IssuesHub';
import { IssueSummary } from '../models/api.models';

@Component({
  selector: 'app-issue-list',
  imports: [RouterLink, DatePipe],
  template: `
    <div class="toolbar">
      <h2>Issues</h2>
      <a routerLink="/issues/new" class="btn btn-primary">+ New Issue</a>
    </div>

    @if (loading()) {
      <p class="muted">Loading issues...</p>
    } @else if (error()) {
      <p class="error">{{ error() }}</p>
    } @else if (issues().length === 0) {
      <p class="muted">No issues yet. Create one to get started.</p>
    } @else {
      <table class="table">
        <thead>
          <tr>
            <th>Title</th>
            <th>Status</th>
            <th>Assignee</th>
            <th>Created</th>
            <th>Events</th>
          </tr>
        </thead>
        <tbody>
          @for (issue of issues(); track issue.id) {
            <tr>
              <td><a [routerLink]="['/issues', issue.id]">{{ issue.title }}</a></td>
              <td>
                <span class="badge" [class.open]="issue.status === 'Open'" [class.closed]="issue.status === 'Closed'">
                  {{ issue.status }}
                </span>
              </td>
              <td>{{ issue.assigneeName ?? 'Unassigned' }}</td>
              <td>{{ issue.created | date:'short' }}</td>
              <td>{{ issue.eventCount }}</td>
            </tr>
          }
        </tbody>
      </table>
    }
  `,
  styles: `
    .toolbar { display: flex; align-items: center; justify-content: space-between; margin-bottom: 1rem; }
    h2 { margin: 0; }
    .table { width: 100%; border-collapse: collapse; }
    .table th, .table td { text-align: left; padding: .5rem .75rem; border-bottom: 1px solid #e0e0e0; }
    .table th { font-size: .85rem; color: #666; text-transform: uppercase; letter-spacing: .05em; }
    .table a { color: #1a56db; text-decoration: none; font-weight: 500; }
    .table a:hover { text-decoration: underline; }
    .badge { padding: .15rem .5rem; border-radius: 999px; font-size: .8rem; font-weight: 500; }
    .badge.open { background: #dbeafe; color: #1e40af; }
    .badge.closed { background: #f3f4f6; color: #6b7280; }
  `,
})
export class IssueListComponent implements OnInit {
  private readonly api = inject(IssuesApiService);
  private readonly hub = inject(IssuesHubService);

  readonly issues = signal<IssueSummary[]>([]);
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);

  constructor() {
    // Auto-refresh when a new issue is created via SignalR
    effect(() => {
      const created = this.hub.issueCreated();
      if (created) {
        this.loadIssues();
      }
    });
  }

  ngOnInit(): void {
    this.loadIssues();
  }

  private loadIssues(): void {
    this.loading.set(true);
    this.api.getIssueSummaries().subscribe({
      next: (issues) => {
        this.issues.set(issues);
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set('Failed to load issues. Make sure the APIs are running.');
        this.loading.set(false);
        console.error('Failed to load issues:', err);
      },
    });
  }
}
