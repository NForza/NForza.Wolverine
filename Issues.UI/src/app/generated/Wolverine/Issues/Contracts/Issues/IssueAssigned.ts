export namespace Wolverine.Issues.Contracts.Issues {
	export interface IssueAssigned
	{
		issueId: string;
		assigneeId: string;
		assigneeName: string;
		title: string;
	}
}
