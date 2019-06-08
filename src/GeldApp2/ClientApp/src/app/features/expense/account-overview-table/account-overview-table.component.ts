import { DialogService } from 'src/app/services/dialog.service';
import { CacheableItem, ItemState } from './../../../services/cache.service';
import { ChartSummaryService } from '../../../services/chart-summary.service';
import { AccountSummary } from './../../../api/model/account-summary';
import { Component, OnInit, OnDestroy } from '@angular/core';
import { fromEvent, Subscription, range, Observable, interval, of, timer } from 'rxjs';
import { debounceTime, map, delay, take } from 'rxjs/operators';
import { ExpenseService } from 'src/app/services/expense.service';

@Component({
  selector: 'app-account-overview-table',
  templateUrl: './account-overview-table.component.html',
  styleUrls: ['./account-overview-table.component.css']
})
export class AccountOverviewTableComponent implements OnInit, OnDestroy {

  private subscriptions: Subscription[] = [];

  constructor(
    private chartService: ChartSummaryService,
    private expenseService: ExpenseService,
    private dialogService: DialogService
  ) { }

  showCharts = false;

  accountSummary: CacheableItem<AccountSummary[]>;

  ngOnInit() {
    const sub = this.chartService.getSummaryMonth()
      .subscribe(data => {
        this.accountSummary = data;
        this.updateSyncableItems();
        if (this.accountSummary.state === ItemState.Online) {
          this.showCharts = document.body.offsetWidth > 600;
          const showCharts$ = fromEvent(window, 'resize').pipe(debounceTime(500)).pipe(map(_ => document.body.offsetWidth > 600));
          this.subscriptions.push(showCharts$.subscribe(v => this.showCharts = v));
        }
      });

   this.subscriptions.push(sub);
  }

  ngOnDestroy() {
    this.subscriptions.forEach(s => s.unsubscribe());
    this.subscriptions = [];
  }

  getSyncItems(accountName: string): number {
    return this.expenseService.getQueuedExpenses(accountName).length;
  }

  async sync(accountName: string) {
    const obs = this.expenseService.syncQueuedExpenses(accountName);
    obs.subscribe(
      _ => {},
      err => { this.dialogService.showError(err); },
      () => this.dialogService.showSnackbar('Synchronisierung erfolgreich!'));

    await this.dialogService.showProgress(obs);
    this.updateSyncableItems();
  }

  private updateSyncableItems() {
    if (this.accountSummary) {
      for (const account of this.accountSummary.data) {
        account.itemsToSync = this.expenseService.getQueuedExpenses(account.accountName).length;
      }
    }
  }
}
