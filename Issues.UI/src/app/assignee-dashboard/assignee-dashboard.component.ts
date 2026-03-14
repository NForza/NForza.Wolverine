import { Component, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { IssuesApiService } from '../services/issues-api.service';
import { AssigneeReport } from '../models/api.models';

@Component({
  selector: 'app-assignee-dashboard',
  imports: [RouterLink],
  template: `
    <h2>Assignee Dashboard</h2>

    @if (loading()) {
      <p class="muted">Loading reports...</p>
    } @else if (error()) {
      <p class="error">{{ error() }}</p>
    } @else if (reports().length === 0) {
      <p class="muted">No assignee data yet. Assign issues to users to see reports here.</p>
    } @else {
      <div class="cards">
        @for (report of reports(); track report.id) {
          <div class="card">
            <h3>{{ report.assigneeName }}</h3>
            <p class="summary">{{ report.issues.length }} issue(s)</p>
            @if (report.issues.length > 0) {
              <ul class="issue-list">
                @for (issue of report.issues; track issue.issueId) {
                  <li>
                    <a [routerLink]="['/issues', issue.issueId]">{{ issue.title }}</a>
                    <span class="badge" [class.open]="issue.status === 'Open'" [class.closed]="issue.status === 'Closed'">
                      {{ issue.status }}
                    </span>
                  </li>
                }
              </ul>
            }
          </div>
        }
      </div>
    }
  `,
  styles: `
    h2 { margin: 0 0 1rem; }
    .cards { display: grid; grid-template-columns: repeat(auto-fill, minmax(300px, 1fr)); gap: 1rem; }
    .card { border: 1px solid #e0e0e0; border-radius: 8px; padding: 1rem 1.25rem; }
    .card h3 { margin: 0 0 .25rem; font-size: 1.1rem; }
    .summary { margin: 0 0 .75rem; color: #666; font-size: .9rem; }
    .issue-list { list-style: none; padding: 0; margin: 0; }
    .issue-list li { display: flex; align-items: center; justify-content: space-between; padding: .35rem 0; border-top: 1px solid #f0f0f0; }
    .issue-list a { color: #1a56db; text-decoration: none; font-size: .9rem; }
    .issue-list a:hover { text-decoration: underline; }
    .badge { padding: .1rem .4rem; border-radius: 999px; font-size: .75rem; font-weight: 500; }
    .badge.open { background: #dbeafe; color: #1e40af; }
    .badge.closed { background: #f3f4f6; color: #6b7280; }
  `,
})
export class AssigneeDashboardComponent implements OnInit {
  private readonly api = inject(IssuesApiService);

  readonly reports = signal<AssigneeReport[]>([]);
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);

  ngOnInit(): void {
    this.api.getAssigneeReports().subscribe({
      next: (reports) => {
        this.reports.set(reports);
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set('Failed to load assignee reports.');
        this.loading.set(false);
        console.error('Load assignee reports failed:', err);
      },
    });
  }
}
