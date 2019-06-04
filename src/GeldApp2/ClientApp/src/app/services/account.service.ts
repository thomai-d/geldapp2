import { UserService, IUserWithAccounts } from './user.service';
import { Injectable } from '@angular/core';
import { Observable, BehaviorSubject, combineLatest, Subscription } from 'rxjs';
import { Router, NavigationEnd } from '@angular/router';
import { filter } from 'rxjs/operators';
import { Logger } from './logger';

@Injectable({
  providedIn: 'root'
})
export class AccountService {
  static readonly lastSelectedAccountNameCacheKey = 'services.account.lastSelectedAccount';

  private _lastSelectedAccountName: string;
  private lastSelectedAccountNameSub = new BehaviorSubject<string>(null);

  private subscriptions: Subscription[] = [];

  constructor(
    private userService: UserService,
    private router: Router,
    private log: Logger) {

    this.lastSelectedAccountName$ = this.lastSelectedAccountNameSub.asObservable();

    const obs1 = userService.currentUser$.subscribe(_ => this.refresh());
    const obs2 = router.events.pipe(filter(e => e instanceof NavigationEnd)).subscribe(_ => this.refresh());
    this.subscriptions.push(obs1, obs2);
  }

  /* Properties */

  get lastSelectedAccountName() { return this._lastSelectedAccountName; }
  lastSelectedAccountName$: Observable<string>;

  /* Methods */

  selectAccount(accountName: string) {
    if (this._lastSelectedAccountName === accountName) {
      return;
    }

  /* Rx helpers */

    this._lastSelectedAccountName = accountName;
    this.lastSelectedAccountNameSub.next(accountName);
  }

  /* Methods */

  private refresh() {
    if (!this.userService.currentUser) {
      this.selectAccount(null);
      return;
    } else {
      const newAccountName = this.determineCurrentAccountName();
      this.selectAccount(newAccountName);
    }
  }

  private determineCurrentAccountName(): string {
    const accounts = this.userService.currentUser.accounts;

    // Account name set by current route?
    const routerRoot = this.router.routerState.snapshot.root;
    if (routerRoot.firstChild) {
      const accountName = routerRoot.firstChild.paramMap.get('accountName');
      if (accountName && accounts.includes(accountName)) {
        if (this.lastSelectedAccountName !== accountName) {
          this.log.debug('services.account-service', `Current account is now ${accountName} (url-parameter).`);
        }
        return accountName;
      }
    }

    // Is there a preferred account?
    const lastAccountName = this.lastSelectedAccountName;
    if (lastAccountName && accounts.includes(lastAccountName)) {
      return lastAccountName;
    }

    // Select the first.
    if (accounts.length) {
      const firstAccountName = accounts[0];
      if (this.lastSelectedAccountName !== firstAccountName) {
        this.log.debug('services.account-service', `Current account is now ${firstAccountName} (first-in-list).`);
      }
      return firstAccountName;
    }
  }
}
