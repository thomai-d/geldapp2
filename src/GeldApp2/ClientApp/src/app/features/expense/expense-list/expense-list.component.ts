import { BaseComponent } from 'src/app/controls/base-component';
import { ExpenseService, ExpenseResult } from 'src/app/services/expense.service';
import { CacheableItem, ItemState } from 'src/app/services/cache.service';
import { Router, ActivatedRoute, ParamMap } from '@angular/router';
import { GeldAppApi } from 'src/app/api/geldapp-api';
import { Component, OnInit, ViewChild, ElementRef } from '@angular/core';
import { Expense } from 'src/app/api/model/expense';
import { combineLatest, BehaviorSubject, concat, of } from 'rxjs';
import { switchMap, debounceTime, tap, map } from 'rxjs/operators';
import { FormControl } from '@angular/forms';
import { environment } from 'src/environments/environment';
import { DialogService } from 'src/app/services/dialog.service';
import { isOfflineException } from 'src/app/helpers/exception-helper';
import { ToolbarService, ToolbarItem } from 'src/app/services/toolbar.service';
import { ExpenseQueryOptions } from './expense-list-query-options';

@Component({
  selector: 'app-expense-list',
  templateUrl: './expense-list.component.html',
  styleUrls: ['./expense-list.component.css'],
})
export class ExpenseListComponent
 extends BaseComponent
 implements OnInit {

  DATE_STR = ['Januar', 'Februar', 'März', 'April', 'Mai', 'Juni', 'Juli', 'August', 'September', 'Oktober', 'November', 'Dezember'];

  private accountName: string;

  private forceRefreshSub = new BehaviorSubject<any>(null);

  private toolbarSearch: ToolbarItem;

  constructor(
    public expenseService: ExpenseService,
    private dialogService: DialogService,
    private activatedRoute: ActivatedRoute,
    private toolbar: ToolbarService,
    private api: GeldAppApi,
    private router: Router
  ) {
    super();
    this.toolbarSearch = new ToolbarItem('search', async () => {
      this.isSearchEnabled = true;
      setTimeout(() => this.search.nativeElement.focus(), 30);
    }, () => !this.isSearchEnabled);
  }

  // Display data.
  queuedItems: Expense[];
  items: (Expense | string)[];
  expenseData: CacheableItem<Expense[]>;

  // State.
  isLoading: boolean;
  canFetchMore = false;

  // Search form.
  isSearchEnabled = false;
  searchText = new FormControl('');
  includeFuture = new FormControl(false);

  @ViewChild('search') search: ElementRef;

  async ngOnInit() {

    // Search parameters modify the URL.
    const s1 = combineLatest([this.searchText.valueChanges,
                      concat(of(false), this.includeFuture.valueChanges)])
          .pipe(debounceTime(500))
          .subscribe(([a, b]) => {
            const params = <any>{};
            if (!!this.searchText.value) { params.q = this.searchText.value; }
            if (!!this.includeFuture.value) { params.future = true; }
            this.router.navigate(['.'], { relativeTo: this.activatedRoute, queryParams: params});
          });

    this.subscriptions.push(s1);

    // Url modifications trigger a refresh.
    const stream = combineLatest([this.activatedRoute.paramMap, this.activatedRoute.queryParamMap, this.forceRefreshSub])
      .pipe(
        map(([params, queryParams]) => this.buildQueryObject(params, queryParams)),
        tap(opt => this.updateSearchForm(opt)),
        switchMap(opt => this.expenseService.getExpenses(opt.accountName, opt.searchText,
                                                         environment.expenseItemsPerPage, opt.includeFuture)));

    this.subscriptions.push(stream.subscribe(result => {
      this.canFetchMore = result.expenses.data
                          && result.expenses.data.length !== environment.expenseItemsPerPage;

      this.toolbar.setButtons([this.toolbarSearch]);
      this.processResult(result);
    }));
  }

  async onClickExpense(expense: Expense) {
    await this.router.navigate(['expenses', expense.accountName, expense.id]);
  }

  async fetchMore() {
    this.canFetchMore = false;
    this.isLoading = true;
    const fut = this.includeFuture.value === 'true';

    try {
      const moreExpenses = await this.api.getExpenses(this.accountName, this.searchText.value,
        environment.expenseItemsPerPage, this.expenseData.data.length, fut);
      this.expenseData.data.push(...moreExpenses);
      this.canFetchMore = moreExpenses.length === environment.expenseItemsPerPage;
      this.refreshView();
    } catch (ex) {
      if (isOfflineException(ex)) {
        this.dialogService.showError('Der Server ist momentan nicht erreichbar.');
        return;
      }
      this.dialogService.showError('Es ist ein Fehler aufgetreten.');
    } finally {
      this.isLoading = false;
    }
  }

  async sync() {
    const obs = this.expenseService.syncQueuedExpenses(this.accountName);
    obs.subscribe(
      _ => {},
      err => { this.dialogService.showError(err); },
      () => this.dialogService.showSnackbar('Synchronisierung erfolgreich!'));

    await this.dialogService.showProgress(obs);
    this.forceRefreshSub.next(false);
  }

  isHeader(item: string | Expense): boolean {
    const isString = typeof (item) === 'string';
    return isString;
  }

  /* Rx operators */

  private buildQueryObject(params: ParamMap, queryParams: ParamMap): ExpenseQueryOptions {
    return {
      accountName: params.get('accountName'),
      searchText: queryParams.get('q'),
      includeFuture: queryParams.get('future') === 'true',
    };
  }

  private updateSearchForm(opt: ExpenseQueryOptions) {
    if (!!opt.searchText || opt.includeFuture) {
      this.isSearchEnabled = true;
    }

    this.searchText.setValue(opt.searchText);
    this.includeFuture.setValue(opt.includeFuture);
    this.accountName = opt.accountName;

    // Reset data.
    this.isLoading = true;
    this.items = [];
    this.queuedItems = [];
    this.canFetchMore = false;
  }

  /* Private methods */

  private processResult(result: ExpenseResult) {
    this.expenseData = result.expenses;
    this.queuedItems = result.queued;
    this.isLoading = result.expenses.isBackgroundLoading;
    if (result.expenses.data) {
      this.canFetchMore = result.expenses.state === ItemState.Online
                            && result.expenses.data
                            && result.expenses.data.length === environment.expenseItemsPerPage;
      this.refreshView();
    } else {
      this.items = [];
      this.canFetchMore = false;
    }
  }

  private refreshView() {
    let lastElement: Expense = null;

    this.items = [];
    for (const x of this.expenseData.data) {

      if (!lastElement || (lastElement && !this.isSameMonth(lastElement, x))) {
        const date = new Date(x.date);
        this.items.push(`${this.DATE_STR[date.getMonth()]} ${date.getFullYear()}`);
      }

      this.items.push(x);
      lastElement = x;
    }
  }

  private isSameMonth(a: Expense, b: Expense): boolean {
    const e1 = new Date(a.date);
    const e2 = new Date(b.date);
    return e1.getMonth() === e2.getMonth()
      && e1.getFullYear() === e2.getFullYear();
  }
}
