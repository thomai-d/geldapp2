import { ExpenseService } from './../../../services/expense.service';
import { CacheableItem } from 'src/app/services/cache.service';
import { Router, ActivatedRoute, ParamMap } from '@angular/router';
import { GeldAppApi } from '../../../api/geldapp-api';
import { Component, OnInit, OnDestroy, ViewChild, ElementRef } from '@angular/core';
import { Expense } from 'src/app/api/model/expense';
import { Subscription, combineLatest, BehaviorSubject, concat, of } from 'rxjs';
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
export class ExpenseListComponent implements OnInit, OnDestroy {

  DATE_STR = ['Januar', 'Februar', 'März', 'April', 'Mai', 'Juni', 'Juli', 'August', 'September', 'Oktober', 'November', 'Dezember'];

  private subscriptions: Subscription[] = [];

  private accountName: string;

  private forceRefreshSub = new BehaviorSubject<any>(null);

  private toolbarSearch: ToolbarItem;

  constructor(
    private dialogService: DialogService,
    private activatedRoute: ActivatedRoute,
    private expenseService: ExpenseService,
    private toolbar: ToolbarService,
    private api: GeldAppApi,
    private router: Router
  ) {
    this.toolbarSearch = new ToolbarItem('search', async () => {
      this.isSearchEnabled = true;
      setTimeout(() => this.search.nativeElement.focus(), 30);
    }, () => !this.isSearchEnabled);
  }

  public QueuedItems: Expense[];

  public Items: (Expense | string)[];

  public isLoading: boolean;

  public isSearchEnabled = false;
  public searchText = new FormControl('');
  public includeFuture = new FormControl(false);

  public error: string;

  public canFetchMore = false;

  public expenseData: CacheableItem<Expense[]>;

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
      this.canFetchMore = result.data && result.data.length !== environment.expenseItemsPerPage;
      this.toolbar.setButtons([this.toolbarSearch]);
      this.processResult(result);
    }));
  }

  ngOnDestroy(): void {
    this.subscriptions.forEach(obs => obs.unsubscribe());
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
    this.QueuedItems = this.expenseService.getQueuedNewExpenses(this.accountName);
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
    this.Items = [];
    this.QueuedItems = [];
    this.canFetchMore = false;
  }

  /* Private methods */

  private processResult(result: CacheableItem<Expense[]>) {
    this.expenseData = result;
    this.QueuedItems = this.expenseService.getQueuedNewExpenses(this.accountName);
    this.isLoading = result.isBackgroundLoading;
    if (result.data) {
      this.canFetchMore = result.data && result.data.length === environment.expenseItemsPerPage;
      this.refreshView();
      this.error = null;
    } else {
      this.Items = [];
      this.error = result.error;
      this.canFetchMore = false;
    }
  }

  private refreshView() {
    let lastElement: Expense = null;

    this.Items = [];
    for (const x of this.expenseData.data) {

      if (!lastElement || (lastElement && !this.isSameMonth(lastElement, x))) {
        const date = new Date(x.date);
        this.Items.push(`${this.DATE_STR[date.getMonth()]} ${date.getFullYear()}`);
      }

      this.Items.push(x);
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
