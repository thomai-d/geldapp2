import { Expense } from './expense';

export interface IImportedExpense {

  id: number;

  // Date.
  bookingDay: string;

  // Valuta.
  valuta: string;

  // Date.
  imported: string;

  type: string;

  partner: string;

  accountNumber: string;

  bankingCode: string;

  amount: number;

  debteeId: string;

  detail: string;

  reference1: string;

  reference2: string;

  isHandled: boolean;

  expense: Expense;

  // Added for csv-import-UI.
  relatedExpenses: Expense[];

}
