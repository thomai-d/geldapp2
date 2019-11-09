import { Expense } from 'src/app/api/model/expense';
import { Injectable, Inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { LoginResult } from './model/loginResult';
import { Logger } from '../services/logger';
import { AccountSummary } from './model/account-summary';
import { Category } from './model/category';
import { ChangePassword, CompareCategoryChartOptions } from './model/request-types';
import { ExpenseRevenueLineChartsDto, CategoryPredictionResult } from './model/response-types';
import { UserSummary } from './model/user-summary';
import { IImportedExpense } from './model/imported-expense';

/// Encapsulates every single call to the web-API.
@Injectable({ providedIn: 'root' })
export class GeldAppApi {
  constructor(
    @Inject('BASE_URL') private url: string,
    private http: HttpClient,
    private log: Logger
  ) { }

  async login(username: string, password: string): Promise<LoginResult> {
    this.log.debug('api', `${username} logging in...`);
    const request = { username, password };
    const response = await this.http.post<LoginResult>(`${this.url}api/auth/login`, request).toPromise();
    return response;
  }

  async refreshToken(refreshToken: string): Promise<LoginResult> {
    this.log.debug('api', `Refreshing token...`);
    const response = await this.http.get<LoginResult>(`${this.url}api/auth/refresh?token=${refreshToken}`).toPromise();
    return response;
  }

  async getCategories(accountName: string): Promise<Category[]> {
    this.log.debug('api', `getting category info for ${accountName}...`);
    accountName = encodeURIComponent(accountName);
    return <Category[]>await this.http.get(`${this.url}api/account/${accountName}/categories`).toPromise();
  }

  async getAccountSummaryMonth(): Promise<AccountSummary[]> {
    this.log.debug('api', `getting account summaries...`);
    return <AccountSummary[]>await this.http.get(`${this.url}api/accounts/summary/month`).toPromise();
  }

  async createExpense(expense: Expense): Promise<void> {
    this.log.debug('api', `creating expense...`);
    const accountName = encodeURIComponent(expense.accountName);
    await this.http.post(`${this.url}api/account/${accountName}/expenses`, expense).toPromise();
  }

  async importCsv(accountName: string, file): Promise<void> {
    this.log.debug('api', `Uploading csv...`);
    accountName = encodeURIComponent(accountName);
    const data = new FormData();
    data.append('csvFile', file);
    await this.http.post(`${this.url}api/account/${accountName}/import/csv`, data).toPromise();
  }

  async handleImportedExpense(accountName: string, importedExpense: IImportedExpense): Promise<void> {
    this.log.debug('api', `Handling imported expense ${importedExpense.id}...`);
    accountName = encodeURIComponent(accountName);
    await this.http.post(`${this.url}api/account/${accountName}/imports/${importedExpense.id}/handle`, {}).toPromise();
  }

  async linkImportedExpenseToExpense(accountName: string, importedExpense: IImportedExpense, expense: Expense): Promise<void> {
    this.log.debug('api', `Linking imported expense ${importedExpense.id} to expense ${expense.id}...`);
    accountName = encodeURIComponent(accountName);

    const params = new HttpParams()
                      .set('importedExpenseId', importedExpense.id.toString())
                      .set('relatedExpenseId', expense.id.toString());

    await this.http.post(`${this.url}api/account/${accountName}/import/link`,
                          {}, { params: params }).toPromise();
  }

  async getUnhandledImportedExpenses(accountName: string): Promise<IImportedExpense[]> {
    this.log.debug('api', `getting unhandled imported expenses...`);
    accountName = encodeURIComponent(accountName);

    const importedExpenses = <IImportedExpense[]>await this.http.get(`${this.url}api/account/${accountName}/imports/unhandled`).toPromise();
    return importedExpenses;
  }

  async updateExpense(expense: Expense): Promise<void> {
    this.log.debug('api', `updating expense ${expense.id}...`);
    const accountName = encodeURIComponent(expense.accountName);
    const url = `${this.url}api/account/${accountName}/expense/${expense.id}`;
    await this.http.put(url, expense).toPromise();
  }

  async deleteExpense(expense: Expense): Promise<void> {
    this.log.debug('api', `deleting expense ${expense.id}...`);
    const accountName = encodeURIComponent(expense.accountName);
    const url = `${this.url}api/account/${accountName}/expense/${expense.id}`;
    await this.http.delete(url).toPromise();
  }

  async deleteCategory(accountName: string, categoryName: string): Promise<void> {
    this.log.debug('api', `deleting category ${categoryName} for ${accountName}...`);
    accountName = encodeURIComponent(accountName);
    categoryName = encodeURIComponent(categoryName);
    const url = `${this.url}api/account/${accountName}/category/${categoryName}`;
    await this.http.delete(url).toPromise();
  }

  async deleteSubCategory(accountName: string, categoryName: string, subcategoryName: string): Promise<void> {
    this.log.debug('api', `deleting category ${categoryName}, subcategory ${subcategoryName} for ${accountName}...`);
    accountName = encodeURIComponent(accountName);
    categoryName = encodeURIComponent(categoryName);
    subcategoryName = encodeURIComponent(subcategoryName);
    const url = `${this.url}api/account/${accountName}/category/${categoryName}/${subcategoryName}`;
    await this.http.delete(url).toPromise();
  }

  async getExpenses(accountName: string, searchText: string, limit: number = 50, offset: number = 0, includeFuture: boolean)
  : Promise<Expense[]> {
    this.log.debug('api', `getting expenses... (q = ${searchText}, f = ${includeFuture})`);
    accountName = encodeURIComponent(accountName);

    let params = new HttpParams()
                      .set('offset', offset.toString())
                      .set('limit', limit.toString())
                      .set('includeFuture', includeFuture.toString());

    if (searchText) {
      params = params.set('q', searchText);
    }

    const expenses = <Expense[]>await this.http.get(`${this.url}api/account/${accountName}/expenses`, { params: params }).toPromise();

    return expenses;
  }

  async getExpensesRelatedToImportedExpense(accountName: string, importedExpense: IImportedExpense)
  : Promise<Expense[]> {
    this.log.debug('api', `getting expenses for imported expense ${importedExpense.id}...`);
    accountName = encodeURIComponent(accountName);

    const params = new HttpParams()
                      .set('relatedToImportedExpense', importedExpense.id.toString());

    const expenses = <Expense[]>await this.http.get(`${this.url}api/account/${accountName}/expenses`, { params: params }).toPromise();

    return expenses;
  }

  async getExpense(accountName: string, expenseId: number): Promise<Expense> {
    this.log.debug('api', `getting expense ${expenseId}...`);
    accountName = encodeURIComponent(accountName);
    const expense = <Expense>await this.http.get(`${this.url}api/account/${accountName}/expense/${expenseId}`).toPromise();
    return expense;
  }

  async addCategory(accountName: string, categoryName: string): Promise<void> {
    this.log.debug('api', `adding category ${categoryName} to ${accountName}...`);
    accountName = encodeURIComponent(accountName);
    await this.http.post(`${this.url}api/account/${accountName}/categories`, {
      categoryName: categoryName
    }).toPromise();
  }

  async addSubcategory(accountName: string, categoryName: string, subcategoryName: string): Promise<void> {
    this.log.debug('api', `adding subcategory ${subcategoryName} to ${categoryName} of ${accountName}...`);
    accountName = encodeURIComponent(accountName);
    categoryName = encodeURIComponent(categoryName);
    subcategoryName = encodeURIComponent(subcategoryName);
    await this.http.put(
      `${this.url}api/account/${accountName}/category/${categoryName}/${subcategoryName}`, {})
       .toPromise();
  }

  async getMonthByCategoryChart(accountName: string, year?: number, month?: number): Promise<any> {
    this.log.debug('api', `getting month-by-category chart for ${accountName}...`);
    accountName = encodeURIComponent(accountName);

    if (year && month) {
      return await this.http.get(`${this.url}api/account/${accountName}/charts/month-by-category?month=${month}&year=${year}`).toPromise();
    } else {
      return await this.http.get(`${this.url}api/account/${accountName}/charts/month-by-category`).toPromise();
    }
  }

  async getExpenseHistoryChart(accountName: string): Promise<any> {
    this.log.debug('api', `getting expense-history chart for ${accountName}...`);
    accountName = encodeURIComponent(accountName);
    return await this.http.get(`${this.url}api/account/${accountName}/charts/expense-history`).toPromise();
  }

  async getRevenueHistoryChart(accountName: string): Promise<any> {
    this.log.debug('api', `getting revenue-history chart for ${accountName}...`);
    accountName = encodeURIComponent(accountName);
    return await this.http.get(`${this.url}api/account/${accountName}/charts/revenue-history`).toPromise();
  }

  async changePassword(oldPassword: string, newPassword: string): Promise<void> {
    const cmd = <ChangePassword>{ oldPassword, newPassword };
    this.log.debug('api', 'Setting new password');
    await this.http.post(`${this.url}api/auth/changePassword`, cmd).toPromise();
  }

  async getCompareCategoriesChart(accountName: string, categories: string[][]): Promise<ExpenseRevenueLineChartsDto> {
    this.log.debug('api', `getting compare categories chart for ${accountName}`);
    accountName = encodeURIComponent(accountName);
    const opt = new CompareCategoryChartOptions();
    for (const catArray of categories) {
      opt.categories.push({ categoryName: catArray[0], subcategoryName: catArray[1] });
    }

    const url = `${this.url}api/account/${accountName}/charts/compare-category`;
    return <ExpenseRevenueLineChartsDto>await this.http.post(url, opt).toPromise();
  }

  async getAppVersion(): Promise<string> {
    this.log.debug('api', 'Getting app version...');
    const url = `${this.url}api/app/version`;
    const result = <any>await this.http.get(url).toPromise();
    return <string>(result.version);
  }

  async getUserSummary(): Promise<UserSummary[]> {
    this.log.debug('api', 'Getting user summary...');
    const url = `${this.url}api/users`;
    return <UserSummary[]>(await this.http.get(url).toPromise());
  }

  // 200: Success
  // 422: User error (with message in body)
  // 403: Forbidden
  // 401: Unauthorized
  // 500: Server error
  async createUser(name: string, password: string, createDefaultAccount: boolean): Promise<void> {
    this.log.debug('api', `Creating user ${name}`);
    const url = `${this.url}api/users`;
    await this.http.post(url, { name, password, createDefaultAccount}).toPromise();
  }

  // 200: Success
  // 204: No data for account
  // 500: Server error
  async predictCategory(accountName: string, amount: number, created: Date, expenseDate: Date): Promise<CategoryPredictionResult> {
    this.log.debug('api', `predicting category for ${accountName} with amount ${amount}...`);
    accountName = encodeURIComponent(accountName);
    const url = `${this.url}api/account/${accountName}/categories/predict`;
    const params = {
      amount: amount.toString(),
      created: created.toISOString(),
      expenseDate: expenseDate.toISOString()
    };

    const response = await this.http.get(url, { params: params, observe: 'response' }).toPromise();
    if (response.status === 200) {
      const result = <CategoryPredictionResult>response.body;
      result.success = true;
      return result;
    }

    return { success: false, category: '', subcategory: '' };
  }
}
