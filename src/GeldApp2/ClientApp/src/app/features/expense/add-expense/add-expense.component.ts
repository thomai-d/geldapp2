import { ExpenseService } from './../../../services/expense.service';
import { Router } from '@angular/router';
import { Component, OnInit } from '@angular/core';
import { Expense } from 'src/app/api/model/expense';
import { Logger } from 'src/app/services/logger';
import { DialogService } from 'src/app/services/dialog.service';

@Component({
  selector: 'app-add-expense',
  templateUrl: './add-expense.component.html',
  styleUrls: ['./add-expense.component.css']
})
export class AddExpenseComponent implements OnInit {

  public serverError = '';

  constructor(
    private expenseService: ExpenseService,
    private dialogService: DialogService,
    private log: Logger,
    private router: Router) {
  }

  ngOnInit() {
  }

  async onExpenseConfirmed(expense: Expense) {
    try {
      await this.expenseService.saveExpense(expense);
      await this.router.navigate(['/expenses', expense.accountName]);
    } catch (ex) {
      this.dialogService.showError('Beim Speichern ist ein Fehler aufgetreten. Bitte an Thomas wenden.');
    }
  }
}
