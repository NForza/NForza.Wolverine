import { Component, effect, inject, OnDestroy, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { IssuesApiService } from '../services/issues-api.service';
import { IssuesHubService } from '../generated/IssuesHub';
import { Issue } from '../models/api.models';

@Component({
  selector: 'app-issue-detail',
  imports: [RouterLink, DatePipe, FormsModule],
  template: `
    <a routerLink="/issues" class="back-link">&larr; Back to issues</a>

    @if (loading()) {
      <p class="muted">Loading issue...</p>
    } @else if (error()) {
      <p class="error">{{ error() }}</p>
    } @else if (issue(); as issue) {
      <div class="issue-header">
        <h2>{{ issue.title }}</h2>
        <span class="badge" [class.open]="issue.isOpen" [class.closed]="!issue.isOpen">
          {{ issue.isOpen ? 'Open' : 'Closed' }}
        </span>
      </div>

      <p class="description">{{ issue.description }}</p>

      <div class="meta">
        <div><strong>Opened:</strong> {{ issue.openedAt | date:'medium' }}</div>
        @if (issue.assigneeId) {
          <div><strong>Assigned to:</strong> {{ issue.assigneeId }}</div>
        } @else {
          <div><strong>Assigned to:</strong> <span class="muted">Unassigned</span></div>
        }
      </div>

      <div class="actions">
        @if (issue.isOpen) {
          <div class="assign-form">
            <input
              type="text"
              [(ngModel)]="assigneeId"
              placeholder="User ID to assign"
              class="input"
            />
            <button class="btn btn-primary" [disabled]="!assigneeId() || actionInProgress()" (click)="assign()">
              Assign
            </button>
          </div>
          <button class="btn btn-secondary" [disabled]="actionInProgress()" (click)="close()">
            Close Issue
          </button>
        } @else {
          <button class="btn btn-primary" [disabled]="actionInProgress()" (click)="reopen()">
            Reopen Issue
          </button>
        }
      </div>

      @if (actionError()) {
        <p class="error">{{ actionError() }}</p>
      }
    }
  `,
  styles: `
    .back-link { color: #1a56db; text-decoration: none; font-size: .9rem; }
    .back-link:hover { text-decoration: underline; }
    .issue-header { display: flex; align-items: center; gap: .75rem; margin: 1rem 0 .5rem; }
    .issue-header h2 { margin: 0; }
    .badge { padding: .2rem .6rem; border-radius: 999px; font-size: .8rem; font-weight: 500; }
    .badge.open { background: #dbeafe; color: #1e40af; }
    .badge.closed { background: #f3f4f6; color: #6b7280; }
    .description { color: #444; line-height: 1.6; }
    .meta { display: flex; flex-direction: column; gap: .25rem; margin-bottom: 1.5rem; font-size: .9rem; }
    .actions { display: flex; flex-wrap: wrap; align-items: center; gap: .75rem; }
    .assign-form { display: flex; gap: .5rem; align-items: center; }
    .input { padding: .4rem .6rem; border: 1px solid #d1d5db; border-radius: 6px; font-size: .9rem; }
  `,
})
export class IssueDetailComponent implements OnInit, OnDestroy {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly api = inject(IssuesApiService);
  private readonly hub = inject(IssuesHubService);

  readonly issue = signal<Issue | null>(null);
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly assigneeId = signal('');
  readonly actionInProgress = signal(false);
  readonly actionError = signal<string | null>(null);

  private issueId = '';

  constructor() {
    // Auto-refresh on SignalR events for this issue
    effect(() => {
      const assigned = this.hub.issueAssigned();
      const unassigned = this.hub.issueUnassigned();
      const closed = this.hub.issueClosed();
      const opened = this.hub.issueOpened();
      if (assigned || unassigned || closed || opened) {
        this.loadIssue();
      }
    });
  }

  ngOnInit(): void {
    this.issueId = this.route.snapshot.paramMap.get('id')!;
    this.hub.subscribeToIssue(this.issueId);
    this.loadIssue();
  }

  ngOnDestroy(): void {
    this.hub.unsubscribeFromIssue(this.issueId);
  }

  assign(): void {
    this.actionInProgress.set(true);
    this.actionError.set(null);
    this.api.assignIssue(this.issueId, this.assigneeId()).subscribe({
      next: () => {
        this.actionInProgress.set(false);
        this.assigneeId.set('');
        this.loadIssue();
      },
      error: (err) => {
        this.actionInProgress.set(false);
        this.actionError.set('Failed to assign issue.');
        console.error('Assign failed:', err);
      },
    });
  }

  close(): void {
    this.actionInProgress.set(true);
    this.actionError.set(null);
    this.api.closeIssue(this.issueId).subscribe({
      next: () => {
        this.actionInProgress.set(false);
        this.loadIssue();
      },
      error: (err) => {
        this.actionInProgress.set(false);
        this.actionError.set('Failed to close issue.');
        console.error('Close failed:', err);
      },
    });
  }

  reopen(): void {
    this.actionInProgress.set(true);
    this.actionError.set(null);
    this.api.reopenIssue(this.issueId).subscribe({
      next: () => {
        this.actionInProgress.set(false);
        this.loadIssue();
      },
      error: (err) => {
        this.actionInProgress.set(false);
        this.actionError.set('Failed to reopen issue.');
        console.error('Reopen failed:', err);
      },
    });
  }

  private loadIssue(): void {
    this.api.getIssue(this.issueId).subscribe({
      next: (issue) => {
        this.issue.set(issue);
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set('Failed to load issue.');
        this.loading.set(false);
        console.error('Load issue failed:', err);
      },
    });
  }
}
