import { DialogService } from './../../../services/dialog.service';
import { ActivatedRoute, Router } from '@angular/router';
import { Component, OnInit } from '@angular/core';
import { Expense } from 'src/app/api/model/expense';
import { switchMap } from 'rxjs/operators';
import { ExpenseService } from 'src/app/services/expense.service';
import { CacheableItem } from 'src/app/services/cache.service';
import { Location } from '@angular/common';
import { isOfflineException } from 'src/app/helpers/exception-helper';

@Component({
  selector: 'app-edit-expense',
  templateUrl: './edit-expense.component.html',
  styleUrls: ['./edit-expense.component.css']
})
export class EditExpenseComponent implements OnInit {

  public expense: CacheableItem<Expense>;

  constructor(
    private activatedRoute: ActivatedRoute,
    private router: Router,
    private location: Location,
    private dialogService: DialogService,
    private expenseService: ExpenseService) {
  }

  async ngOnInit() {
    this.activatedRoute.paramMap
      .pipe(switchMap(async (params) => {
        if (!params) {
          return undefined;
        }

        const expenseId = +params.get('id');
        const accountName = params.get('accountName');
        try {
          const expense = await this.expenseService.getExpense(accountName, expenseId);
          return expense;
        } catch (ex) {
          if (ex.status === 404) {
            await this.dialogService.showError(
              'Dieser Eintrag existiert nicht.');
            this.location.back();
            return;
          }

          await this.dialogService.showError(
            'Der Eintrag konnte nicht geladen werden. Bitte später noch einmal versuchen oder an Thomas wenden.');
            this.location.back();
        }
      }))
      .subscribe(exp => this.expense = exp);
  }

  async onExpenseConfirmed(expense: Expense) {
    try {
      this.expenseService.saveExpense(expense);
      await this.router.navigate(['/expenses', expense.accountName]);
    } catch (ex) {
      this.dialogService.showError('Beim Speichern ist ein Fehler aufgetreten. Bitte an Thomas wenden.');
    }
  }

  async onDeleteExpense(expense: Expense) {
    try {
    await this.expenseService.deleteExpense(expense);
    await this.router.navigate(['/expenses', expense.accountName]);
    await this.dialogService.showSnackbar('Eintrag gelöscht.');
    } catch (ex) {
      if (isOfflineException(ex)) {
        this.dialogService.showError('Bereits synchronisierte Einträge können nur im Online-Modus gelöscht werden!');
        return;
      }
      this.dialogService.showError('Beim Löschen ist ein Fehler aufgetreten. Bitte an Thomas wenden.');
    }
  }
}
