export interface ChartOfAccountDto {
    id: string;
    accountCode: string;
    accountName: string;
    accountType: "Asset" | "Liability" | "Equity" | "Revenue" | "Expense";
    systemAccountType?: string;
    description: string;
    isActive: boolean;
}

export interface CreateChartOfAccountDto {
    accountCode: string;
    accountName: string;

    accountType: "Asset" | "Liability" | "Equity" | "Revenue" | "Expense";
    description?: string;
}

export interface JournalEntryLineDto {
    id: string;
    accountId: string;
    accountName: string;
    accountCode: string;
    debit: number;
    credit: number;
}

export interface JournalEntryDto {
    id: string;
    entryDate: string;
    description: string;
    referenceNumber: string;
    lines: JournalEntryLineDto[];
}
