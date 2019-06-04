import { Expense } from 'src/app/api/model/expense';
import { Injectable, Inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { LoginResult } from './model/loginResult';
import { Logger } from '../services/logger';
import { AccountSummary } from './model/account-summary';
import { Category } from './model/category';
import { ChangePassword, CompareCategoryChartOptions } from './model/request-types';
import { ExpenseRevenueLineChartsDto } from './model/response-types';

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
    this.log.debug('api', `getting expenses...`);
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
}
