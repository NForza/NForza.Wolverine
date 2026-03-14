import { Routes } from '@angular/router';
import { IssueListComponent } from './issue-list/issue-list.component';
import { IssueDetailComponent } from './issue-detail/issue-detail.component';
import { CreateIssueComponent } from './create-issue/create-issue.component';
import { AssigneeDashboardComponent } from './assignee-dashboard/assignee-dashboard.component';
import { EventListComponent } from './event-list.component';

export const routes: Routes = [
  { path: '', redirectTo: 'issues', pathMatch: 'full' },
  { path: 'issues', component: IssueListComponent },
  { path: 'issues/new', component: CreateIssueComponent },
  { path: 'issues/:id', component: IssueDetailComponent },
  { path: 'assignees', component: AssigneeDashboardComponent },
  { path: 'events', component: EventListComponent },
];
