import { Category } from '../api/model/category';
import { CacheService, CacheableItem } from './cache.service';
import { Subscription, Observable, Subject, Subscriber } from 'rxjs';
import { UserService, IUserWithAccounts } from './user.service';
import { Logger } from 'src/app/services/logger';
import { Injectable } from '@angular/core';
import { GeldAppApi } from '../api/geldapp-api';
import { CategoryPredictionResult } from '../api/model/response-types';
import { environment } from 'src/environments/environment';
import { isOfflineException } from '../helpers/exception-helper';

@Injectable({
  providedIn: 'root'
})
export class CategoryService {

  private subscriptions: Subscription[] = [];

  private categoriesByAccount: { [accountName: string]: CacheableItem<Category[]> } = {};
  private fetchCategoryPromises: { [accountName: string]: Promise<Category[]> } = {};

  constructor(
    private log: Logger,
    private api: GeldAppApi,
    private userService: UserService,
    private cache: CacheService
  ) {
  }

  /* Public methods */

  static readonly CategoryCacheKeyFn = (accountName: string) => `services.category.${accountName}`;

  initialize() {
    this.log.debug('services.category', 'Initializing category service');
    const s1 = this.userService.currentUser$.subscribe(async u => this.refresh(u));
    this.subscriptions.push(s1);
  }

  async addCategory(accountName: string, categoryName: string) {
    await this.api.addCategory(accountName, categoryName);
    this.invalidate(accountName);
  }

  async deleteCategory(accountName: string, categoryName: string) {
    await this.api.deleteCategory(accountName, categoryName);
    this.invalidate(accountName);
  }

  async addSubcategory(accountName: string, categoryName: string, subcategoryName: string) {
    await this.api.addSubcategory(accountName, categoryName, subcategoryName);
    this.invalidate(accountName);
  }

  async deleteSubcategory(accountName: string, categoryName: string, subcategoryName: string) {
    await this.api.deleteSubCategory(accountName, categoryName, subcategoryName);
    this.invalidate(accountName);
  }

  async predictCategory(accountName: string, amount: number, created: Date, expenseDate: Date): Promise<CategoryPredictionResult> {
    return this.api.predictCategory(accountName, amount, created, expenseDate);
  }

  getCategoriesFor(accountName: string): Observable<CacheableItem<Category[]>> {
    return new Observable<CacheableItem<Category[]>>(obs => {
      const cacheKey = CategoryService.CategoryCacheKeyFn(accountName);
      const cachedItem = this.cache.get<Category[]>(cacheKey);
      const now = Date.now();
      if (cachedItem && cachedItem.data) {
        obs.next(CacheableItem.live(cachedItem.data));
        if (cachedItem.timestamp + environment.categoryCacheInvalidationIntervalMs > now) {
          obs.complete();
          return;
        }
      }

      this.continueGetCagegoriesFor(obs, accountName, !!cachedItem);
    });
  }

  /* Private methods */

  private async continueGetCagegoriesFor(obs: Subscriber<CacheableItem<Category[]>>, accountName: string, hasProvidedData: boolean) {
      try {
        const categories = await this.fetchCategories(accountName);
        obs.next(CacheableItem.live(categories));
        obs.complete();
      } catch (ex) {

        if (hasProvidedData) {
          obs.complete();
          return;
        }

        if (isOfflineException(ex)) {
          obs.next(CacheableItem.offline());
          obs.complete();
          return;
        }

        this.log.error('services.category', `Error while fetching categories for ${accountName}: ${JSON.stringify(ex)}`);
        obs.next(CacheableItem.error(ex.message));
        obs.complete();
        return;
      }
  }

  private async refresh(user: IUserWithAccounts) {
    this.log.debug('services.category', 'User changed. Refreshing categories...');
    if (!user) {
      this.categoriesByAccount = {};
      this.fetchCategoryPromises = {};
      return;
    }

    const fetchAll = user.accounts.map(accountName => this.refreshCategories(accountName));
    await Promise.all(fetchAll);
  }

  private invalidate(accountName: string) {
    const cacheKey = CategoryService.CategoryCacheKeyFn(accountName);
    delete this.categoriesByAccount[accountName];
    delete this.fetchCategoryPromises[accountName];
    this.cache.clear(cacheKey);
    this.refreshCategories(accountName);
  }

  private async refreshCategories(accountName: string) {
    try {
      this.log.debug('services.category', `Refreshing categories for ${accountName}...`);
      const categories = await this.fetchCategories(accountName);
      this.categoriesByAccount[accountName] = CacheableItem.live(categories);
    } catch (ex) {
      this.log.errorWithException('services.category', `Fetching categories for ${accountName} failed`, ex);
    }
  }

  private async fetchCategories(accountName: string): Promise<Category[]> {
    const cacheKey = CategoryService.CategoryCacheKeyFn(accountName);
    let runningFetch = this.fetchCategoryPromises[accountName];
    if (!runningFetch) {
      runningFetch = this.api.getCategories(accountName);
      this.fetchCategoryPromises[accountName] = runningFetch;
    }

    try {
      const categories = await runningFetch;
      this.cache.set(cacheKey, categories);
      return categories;
    } finally {
      this.fetchCategoryPromises[accountName] = undefined;
    }
  }
}

// TODO Unittests:
// Logout will clear the cache.
// Changing user will refresh the cache.
// Exceptions while refreshing caused by user change are swallowed.
// Getting categories for user will use the cache first if available and < Threshold.
// Getting categories with stale cache item will fetch it and update the cache.
// Getting categories with no cache item will fetch it and update the cache.
// Any error while fetching category returns cached item if possible.
// Offline error while fetching categories return an offline item.
// Generic error while fetching categories return an error item.
// Invalidate will clear the cache and immediately refresh the account.
// Fetch request while already fetching will reuse the old promise.
// Failed fetch request will dispose the promise.
// Deleting category will invoke the api and invalidate the cache.
// Deleting subcategory will invoke the api and invalidate the cache.
// Adding category will invoke the api and invalidate the cache.
// Adding subcategory will invoke the api and invalidate the cache.