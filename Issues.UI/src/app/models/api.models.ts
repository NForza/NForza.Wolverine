export interface IssueSummary {
  id: string;
  title: string;
  status: string;
  assigneeId: string | null;
  assigneeName: string | null;
  created: string;
  eventCount: number;
}

export interface Issue {
  id: string;
  assigneeId: string | null;
  originatorId: string | null;
  title: string;
  description: string;
  isOpen: boolean;
  openedAt: string;
  tasks: IssueTask[];
}

export interface IssueTask {
  id: string;
  title: string;
  description: string;
  started: string | null;
  finished: string;
}

export interface CreateIssueRequest {
  originatorId: string;
  title: string;
  description: string;
}

export interface CreateIssueResponse {
  id: string;
  title: string;
  description: string;
}

export interface AssignIssueRequest {
  issueId: string;
  assigneeId: string;
}

export interface User {
  id: string;
  email: string;
  name: string;
}

export interface CreateUserRequest {
  name: string;
  email: string;
}

export interface AssigneeReport {
  id: string;
  assigneeName: string;
  issues: AssigneeIssue[];
}

export interface AssigneeIssue {
  issueId: string;
  title: string;
  status: string;
}
