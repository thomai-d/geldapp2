import { Logger } from 'src/app/services/logger';
import { Injectable } from '@angular/core';
import { CacheableItem, CacheService, ItemState } from './cache.service';
import { Expense } from '../api/model/expense';
import { GeldAppApi } from '../api/geldapp-api';
import { isOfflineException } from '../helpers/exception-helper';
import { DialogService } from './dialog.service';
import { Progress } from '../dialogs/progress-dialog/progress-dialog.component';
import { Observable, Subject, Observer } from 'rxjs';
import { UserService } from './user.service';

@Injectable({ providedIn: 'root' })
export class ExpenseService {

  // Timespan in [ms] within which the cached expense list is reported as 'online'.
  static readonly AssumeOnlineTimeMs = 60000;

  // Local expenses contain expenses which were modified in offline mode.
  // Expense.Ids < 0 indicate expenses which are new, others are modified versions of existing ex
  private queuedExpensesCacheKey = (accountName: string) => `services.expense.${accountName}.queue`;

  // Latest expenses are not modified and reflect the state of the last server request.
  private latestExpensesCacheKey = (accountName: string) => `services.expense.${accountName}.latest-expenses`;

  constructor(
    private userService: UserService,
    private cache: CacheService,
    private api: GeldAppApi,
    private dialogService: DialogService,
    private log: Logger
  ) { }

  /* Public methods */

  public getNumberOfQueuedExpenses(): number {
    if (!this.userService.currentUser) {
      return 0;
    }

    let total = 0;
    for (const accountName of this.userService.currentUser.accounts) {
      total = total + this.getQueuedExpenses(accountName).length;
    }

    return total;
  }

  public async getExpense(accountName: string, expenseId: number): Promise<CacheableItem<Expense>> {

    const latestCacheKey = this.latestExpensesCacheKey(accountName);
    const localExpensesCacheKey = this.queuedExpensesCacheKey(accountName);

    // Return from local queue.
    const cachedItem = this.cache.get<Expense[]>(localExpensesCacheKey);
    if (cachedItem && cachedItem.data) {
      const ret = cachedItem.data.find(i => i.id === expenseId);
      if (ret) {
        this.log.debug('services.expense-service', `Returned expense ${expenseId} from local queue.`);
        return CacheableItem.cached(ret, cachedItem.timestamp);
      }
    }

    try {
      // Read from server.
      const expense = await this.api.getExpense(accountName, expenseId);
      return CacheableItem.live(expense);
    } catch (ex) {

      if (isOfflineException(ex)) {
        // Return from local cache if possible.
        const localItem = this.cache.get<Expense[]>(latestCacheKey);
        if (localItem && localItem.data) {
          const ret = localItem.data.find(i => i.id === expenseId);
          if (ret) {
            this.log.debug('services.expense-service', `Returned expense ${expenseId} from cache.`);
            return CacheableItem.cached(ret, localItem.timestamp);
          }
        }
      }

      this.log.error('services.expense-service', `Exception while getting expense ${expenseId}: ${JSON.stringify(ex)}`);
      throw ex;
    }
  }

  // Returns all new expenses that are queued.
  public getQueuedNewExpenses(accountName: string): Expense[] {

    const cacheKey = this.queuedExpensesCacheKey(accountName);
    const cachedItem = this.cache.get<Expense[]>(cacheKey);
    if (cachedItem) {
      return cachedItem.data.filter(e => e.id < 0);
    }

    return [];
  }

  // Returns all (including edited existing) expenses that are queued.
  public getQueuedExpenses(accountName: string): Expense[] {

    const cacheKey = this.queuedExpensesCacheKey(accountName);
    const cachedItem = this.cache.get<Expense[]>(cacheKey);
    if (cachedItem) {
      return cachedItem.data;
    }

    return [];
  }

  private getCachedExpenses(accountName: string) {
    const cacheKey = this.latestExpensesCacheKey(accountName);
    const cachedItem = this.cache.get<Expense[]>(cacheKey);
    if (cachedItem) {
      cachedItem.data = this.replaceWithLocal(accountName, cachedItem.data);
    }
    return cachedItem;
  }

  // Fetches the expenses from the server, replacing local expenses.
  // Queued expenses are not returned.
  public getExpenses(accountName: string, searchText: string, limit: number, includeFuture: boolean)
                  : Observable<CacheableItem<Expense[]>> {

    return new Observable<CacheableItem<Expense[]>>(subj => {

    if (!searchText) {
      const cachedItem = this.getCachedExpenses(accountName);
      if (cachedItem) {
        if (cachedItem.isNewerThan(ExpenseService.AssumeOnlineTimeMs)) {
          cachedItem.state = ItemState.Online;
        } else {
          cachedItem.isBackgroundLoading = true;
        }

        subj.next(cachedItem);
      }
    } else {
      subj.next(CacheableItem.live([], true));
    }

      this.fetchExpensesFromServer(subj, accountName, searchText, limit, includeFuture);
    });
  }

  private async fetchExpensesFromServer(
    subj: Observer<CacheableItem<Expense[]>>,
    accountName: string,
    searchText: string,
    limit: number,
    includeFuture: boolean) {

    try {
      const cacheKey = this.latestExpensesCacheKey(accountName);
      const expenses = await this.api.getExpenses(accountName, searchText, limit, 0, includeFuture);

      if (!searchText) {
        this.cache.set(cacheKey, expenses);
      }

      const finalExpenses = this.replaceWithLocal(accountName, expenses);
      subj.next(CacheableItem.live<Expense[]>(finalExpenses));
      subj.complete();
    } catch (ex) {


      if (isOfflineException(ex)) {
        const cachedItem = this.getCachedExpenses(accountName);
        if (cachedItem) {
          subj.next(cachedItem);
        } else {
          subj.next(CacheableItem.offline());
        }
        return;
      }

      if (ex.status === 400 && ex.error && ex.error.errorType === 'FilterParseException') {
        subj.next(CacheableItem.error('Ungültiger Filterausdruck'));
        return;
      }

      this.log.error('services.expense', `Error while fetching expenses: ${JSON.stringify(ex)}`);
      subj.error(CacheableItem.error('Unbekannter Fehler'));
    }
  }

  public async deleteExpense(expense: Expense) {
    // Not yet synced.
    if (expense.id < 0) {
      this.removeFromLocalQueue(expense.accountName, expense.id);
      return;
    }

    await this.api.deleteExpense(expense);
  }

  // Saves the expense by directly pushing it to the server.
  // Fallback is storing it locally and pushing it later.
  public async saveExpense(expense: Expense) {
      try {
        if (expense.id <= 0) {
          this.log.info('services.expense', `Creating expense for ${expense.categoryName} > ${expense.subcategoryName}`);
          await this.api.createExpense(expense);
        } else {
          this.log.info('services.expense', `Updating expense ${expense.id} for ${expense.categoryName} > ${expense.subcategoryName}`);
          await this.api.updateExpense(expense);
        }

        this.removeFromLocalQueue(expense.accountName, expense.id);
        this.dialogService.showSnackbar('Eintrag wurde in der Cloud gespeichert.');
      } catch (ex) {
        if (isOfflineException(ex)) {
          this.saveExpenseLocally(expense);
          this.dialogService.showSnackbar('Eintrag wurde lokal gespeichert und wird später übertragen.', 5000);
          return;
        }

        this.log.error('services.expense-service', `Error pushing expense to server: ${JSON.stringify(ex)}`);
        throw ex;
      }
  }

  public syncQueuedExpenses(accountName: string): Observable<Progress> {

    const queued = this.getQueuedExpenses(accountName);
    const progress = new Subject<Progress>();

    let current = 0;
    const total = queued.length;
    let errors = 0;

    const statusFn = (): Progress => ({ percentComplete: total / current * 100, statusText: `Synchronisiere ${current} von ${total}...` });
    progress.next(statusFn());

    setTimeout(async () => {
      for (const item of queued) {
        try {
          if (item.id < 0) {
            await this.api.createExpense(item);
          } else {
            await this.api.updateExpense(item);
          }
          this.removeFromLocalQueue(accountName, item.id);
        } catch (ex) {
          errors++;
          this.log.error('services.expense-service', `Error while syncing expense: ${JSON.stringify(ex)}`);
        }

        progress.next(statusFn());
        current++;
      }

      if (errors) {
        progress.error(`${errors} ${errors === 1 ? 'Eintag konnte' : 'Einträge konnten'} nicht synchronisiert werden.
                       \nBesteht eine Internetverbindung?`);
      } else {
        progress.complete();
      }
    }, 0);

    return progress;
  }

  /* Private methods */

  private saveExpenseLocally(expense: Expense) {

    const cacheKey = this.queuedExpensesCacheKey(expense.accountName);

    let expenseQueue: Expense[] = [];
    const localItem = this.cache.get<Expense[]>(cacheKey);
    if (localItem && localItem.state === ItemState.Cached) {
      expenseQueue = localItem.data;
    }

    if (!expense.id) {
      // Set id if not yet done.
      const newExpenseIds = expenseQueue.filter(e => e.id < 0).map(e => e.id);
      const nextId = newExpenseIds.length > 0 ? Math.min(...newExpenseIds) - 1 : -1;
      expense.id = nextId;
    } else {
      // remove expense of same id.
      expenseQueue = expenseQueue.filter(e => e.id !== expense.id);
    }

    expenseQueue.push(expense);
    this.cache.set(cacheKey, expenseQueue);
    this.log.info('services.expense-service', `Expense ${expense.id} is kept local.`);
  }

  private removeFromLocalQueue(accountName: string, expenseId: number) {
    const cacheKey = this.queuedExpensesCacheKey(accountName);
    const localItem = this.cache.get<Expense[]>(cacheKey);
    if (!localItem) { return; }

    const items = localItem.data.filter(i => i.id !== expenseId);
    this.cache.set(cacheKey, items);
  }

  private replaceWithLocal(accountName: string, expenses: Expense[])
          : Expense[] {
    const cacheKey = this.queuedExpensesCacheKey(accountName);
    const localItem = this.cache.get<Expense[]>(cacheKey);
    if (!localItem) { return expenses; }

    expenses = expenses.map(exp => {
      const localExp = localItem.data.find(lx => lx.id === exp.id);
      return localExp ? localExp : exp;
    });

    return expenses;
  }
}
