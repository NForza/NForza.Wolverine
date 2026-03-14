import { Component, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { IssuesApiService } from '../services/issues-api.service';

@Component({
  selector: 'app-create-issue',
  imports: [RouterLink, FormsModule],
  template: `
    <a routerLink="/issues" class="back-link">&larr; Back to issues</a>
    <h2>Create Issue</h2>

    <form (ngSubmit)="submit()" class="form">
      <div class="field">
        <label for="originatorId">Originator User ID</label>
        <input id="originatorId" type="text" [(ngModel)]="originatorId" name="originatorId"
               placeholder="GUID of the user creating this issue" class="input" required />
      </div>
      <div class="field">
        <label for="title">Title</label>
        <input id="title" type="text" [(ngModel)]="title" name="title"
               placeholder="Short issue title" class="input" required />
      </div>
      <div class="field">
        <label for="description">Description</label>
        <textarea id="description" [(ngModel)]="description" name="description"
                  placeholder="Describe the issue..." class="input textarea" rows="4" required></textarea>
      </div>

      @if (error()) {
        <p class="error">{{ error() }}</p>
      }

      <div class="actions">
        <button type="submit" class="btn btn-primary" [disabled]="submitting() || !isValid()">
          {{ submitting() ? 'Creating...' : 'Create Issue' }}
        </button>
        <a routerLink="/issues" class="btn btn-secondary">Cancel</a>
      </div>
    </form>
  `,
  styles: `
    .back-link { color: #1a56db; text-decoration: none; font-size: .9rem; }
    .back-link:hover { text-decoration: underline; }
    h2 { margin: 1rem 0; }
    .form { display: flex; flex-direction: column; gap: 1rem; max-width: 500px; }
    .field { display: flex; flex-direction: column; gap: .25rem; }
    .field label { font-weight: 500; font-size: .9rem; }
    .input { padding: .5rem .75rem; border: 1px solid #d1d5db; border-radius: 6px; font-size: .9rem; }
    .textarea { font-family: inherit; resize: vertical; }
    .actions { display: flex; gap: .75rem; margin-top: .5rem; }
  `,
})
export class CreateIssueComponent {
  private readonly api = inject(IssuesApiService);
  private readonly router = inject(Router);

  readonly originatorId = signal('');
  readonly title = signal('');
  readonly description = signal('');
  readonly submitting = signal(false);
  readonly error = signal<string | null>(null);

  isValid(): boolean {
    return this.originatorId().trim().length > 0
      && this.title().trim().length > 0
      && this.description().trim().length > 0;
  }

  submit(): void {
    if (!this.isValid()) return;

    this.submitting.set(true);
    this.error.set(null);
    this.api.createIssue({
      originatorId: this.originatorId(),
      title: this.title(),
      description: this.description(),
    }).subscribe({
      next: (result) => {
        this.submitting.set(false);
        this.router.navigate(['/issues', result.id]);
      },
      error: (err) => {
        this.submitting.set(false);
        this.error.set('Failed to create issue. Check that the originator ID is a valid user.');
        console.error('Create issue failed:', err);
      },
    });
  }
}
