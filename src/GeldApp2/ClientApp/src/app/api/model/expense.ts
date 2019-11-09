import { ExpenseType } from './expense-type';

export class Expense {

  id = 0;

  accountName = '';

  amount = 0.0;

  categoryName = '';

  subcategoryName = '';

  details = '';

  // Date as ISO string.
  date = '';

  type: ExpenseType = ExpenseType.expense;

  // Date as ISO string.
  created = '';

  createdBy = '';

  // Date as ISO string.
  lastModified = '';

  lastModifiedBy = '';

  // Defined in the CreateExpenseCommand.
  handlesImportedExpenseId: number | undefined;
}
