import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  Issue,
  IssueSummary,
  CreateIssueRequest,
  CreateIssueResponse,
  AssignIssueRequest,
  User,
  CreateUserRequest,
  AssigneeReport,
} from '../models/api.models';

@Injectable({ providedIn: 'root' })
export class IssuesApiService {
  private readonly http = inject(HttpClient);

  // Issues API (write-side, port 5035 via /api proxy)

  createIssue(request: CreateIssueRequest): Observable<CreateIssueResponse> {
    return this.http.post<CreateIssueResponse>('/api/issues', request);
  }

  getIssue(id: string): Observable<Issue> {
    return this.http.get<Issue>(`/api/issues/${id}`);
  }

  assignIssue(issueId: string, assigneeId: string): Observable<void> {
    return this.http.put<void>(`/api/issues/${issueId}/assign`, {
      issueId,
      assigneeId,
    } as AssignIssueRequest);
  }

  unassignIssue(issueId: string): Observable<void> {
    return this.http.put<void>(`/api/issues/${issueId}/unassign`, { issueId });
  }

  closeIssue(issueId: string): Observable<void> {
    return this.http.put<void>(`/api/issues/${issueId}/close`, { issueId });
  }

  reopenIssue(issueId: string): Observable<void> {
    return this.http.put<void>(`/api/issues/${issueId}/reopen`, { issueId });
  }

  // Users

  createUser(request: CreateUserRequest): Observable<User> {
    return this.http.post<User>('/api/users', request);
  }

  getUser(id: string): Observable<User> {
    return this.http.get<User>(`/api/users/${id}`);
  }

  // Reporting API (read-side, port 5036 via /reporting proxy)

  getIssueSummaries(): Observable<IssueSummary[]> {
    return this.http.get<IssueSummary[]>('/reporting/issues/summaries');
  }

  getIssueSummary(id: string): Observable<IssueSummary> {
    return this.http.get<IssueSummary>(`/reporting/issues/${id}/summary`);
  }

  getAssigneeReports(): Observable<AssigneeReport[]> {
    return this.http.get<AssigneeReport[]>('/reporting/reports/assignees');
  }

  getAssigneeReport(userId: string): Observable<AssigneeReport> {
    return this.http.get<AssigneeReport>(`/reporting/reports/assignees/${userId}`);
  }
}
