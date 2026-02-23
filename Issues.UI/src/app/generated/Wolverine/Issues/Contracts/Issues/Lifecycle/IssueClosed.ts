export namespace Wolverine.Issues.Contracts.Issues.Lifecycle {
	export interface IssueClosed
	{
		issueId: string;
		assigneeId: string;
		closed: string;
	}
}
