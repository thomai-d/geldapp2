import { Logger } from 'src/app/services/logger';
import { BehaviorSubject, Observable, timer } from 'rxjs';
import { GeldAppApi } from '../api/geldapp-api';
import { Injectable } from '@angular/core';
import { JwtHelperService } from '@auth0/angular-jwt';
import { isOfflineException } from '../helpers/exception-helper';
import { Router } from '@angular/router';

export interface IGeldAppToken {
  username: string;
  userid: number;
  accounts: (string | string[]);
}

export interface IUserWithAccounts {
  userName: string;
  accounts: string[];
}

export enum LoginResult {
  Success,
  Rejected,
  Error,
  Offline,
}

/// Encapsulates user related functionality.
@Injectable({ providedIn: 'root' })
export class UserService {

  readonly refreshTokenDelayMs = 10000;     // 10 sec
  readonly refreshTokenIntervalMs = 300000; // 5 mins

  private currentUserSubject = new BehaviorSubject<IUserWithAccounts>(null);
  private tokenExpirySubject = new BehaviorSubject<Date>(null);

  constructor(
    private api: GeldAppApi,
    private jwt: JwtHelperService,
    private router: Router,
    private log: Logger) {

    this.currentUser$ = this.currentUserSubject.asObservable();
    this.tokenExpiry$ = this.tokenExpirySubject.asObservable();

    timer(this.refreshTokenDelayMs, this.refreshTokenIntervalMs)
      .subscribe(_ => this.refreshToken());

    const tokenStr = localStorage.getItem('authToken');
    if (tokenStr) {
      try {
      this.setToken(tokenStr);
      } catch {
        localStorage.removeItem('authToken');
      }
    }
  }

  public currentUser: IUserWithAccounts;
  public currentUser$: Observable<IUserWithAccounts>;
  public tokenExpiry$: Observable<Date>;

  async loginAsync(username: string, password: string): Promise<LoginResult> {
    try {
      localStorage.clear();
      const response = await this.api.login(username, password);

      const tokenString = response.token;
      localStorage.setItem('refreshToken', response.refreshToken);
      this.setToken(tokenString);

      return LoginResult.Success;
    } catch (ex) {
      if (isOfflineException(ex)) { return LoginResult.Offline; }
      if (ex.status === 401) { return LoginResult.Rejected; }
      this.log.error('services.login-service', `Login failed: ${JSON.stringify(ex)}`);
      return LoginResult.Error;
    }
  }

  logout() {
    this.log.debug('services.user', 'Logout');
    localStorage.clear();
    this.currentUser = null;
    this.currentUserSubject.next(undefined);
    this.router.navigate(['/login'], { queryParams: { redirect: '/' }});
  }

  isAuthenticated(): boolean {
    const token = localStorage.getItem('authToken');
    if (!token || this.jwt.isTokenExpired(token)) {
      return false;
    }

    return true;
  }

  public async refreshToken() {
    const refreshToken = localStorage.getItem('refreshToken');
    if (!refreshToken) { return; }

    try {
      const result = await this.api.refreshToken(refreshToken);
      this.setToken(result.token);
    } catch {
      this.log.warn('services.user', 'Could not refresh token.');
    }
  }

  private setToken(tokenString: string) {
    const token = this.jwt.decodeToken(tokenString);
    const expiry = this.jwt.getTokenExpirationDate(tokenString);
    localStorage.setItem('authToken', tokenString);
    this.log.info('services.user', `Got token for ${token.username}, valid until ${expiry}`);
    if (!token.username || !token.accounts || !token.userid) {
      throw new Error(`Invalid token: ${JSON.stringify(token)}`);
    }

    const accounts = Array.isArray(token.accounts) ? token.accounts : [token.accounts];
    this.currentUser = { userName: token.username, accounts: accounts };
    this.currentUserSubject.next(this.currentUser);
    this.tokenExpirySubject.next(expiry);
  }
}
