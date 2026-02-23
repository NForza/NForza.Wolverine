export namespace Wolverine.Issues.Contracts.Issues {
	export interface IssueCreated
	{
		id: string;
		originatorId: string;
		title: string;
		description: string;
		openedAt: string;
	}
}
