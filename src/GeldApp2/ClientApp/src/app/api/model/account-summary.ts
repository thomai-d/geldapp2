export class AccountSummary {

    accountName: string;

    totalExpenses: number;

    // Number of locally queued items for this account. Only used in the client.
    itemsToSync: number;

}
