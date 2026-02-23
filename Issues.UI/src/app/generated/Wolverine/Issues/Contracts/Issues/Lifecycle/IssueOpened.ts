export namespace Wolverine.Issues.Contracts.Issues.Lifecycle {
	export interface IssueOpened
	{
		issueId: string;
		assigneeId: string;
		reopened: string;
	}
}
