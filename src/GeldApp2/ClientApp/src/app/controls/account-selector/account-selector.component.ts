import { UserService } from '../../services/user.service';
import { Component, OnInit, OnDestroy, EventEmitter, Output } from '@angular/core';
import { ChartSummaryService } from 'src/app/services/chart-summary.service';
import { Router, NavigationEnd } from '@angular/router';
import { Logger } from 'src/app/services/logger';
import { Subscription } from 'rxjs';
import { AccountService } from 'src/app/services/account.service';
import { filter } from 'rxjs/operators';

type buildUrlFunc = (accountName: string) => string[];

/// Component used in the header which detects if the current url path contains
/// an :accountName parameter. If so, it is shown and provides a dropdown to select
/// a different account.
@Component({
  selector: 'app-account-selector',
  templateUrl: './account-selector.component.html',
  styleUrls: ['./account-selector.component.css']
})
export class AccountSelectorComponent implements OnInit, OnDestroy {

  private _selectedAccountName: string;

  private buildUrlFunc: buildUrlFunc;

  private subscriptions: Subscription[] = [];

  constructor(
    private accountService: AccountService,
    private userService: UserService,
    private router: Router) {
  }

  /* Properties */

  allAccountNames: string[];

  isSelectable: boolean;

  @Output() accountChanged = new EventEmitter<string>();

  get selectedAccountName() {
    return this._selectedAccountName;
  }

  set selectedAccountName(value: string) {
    this.accountChanged.emit(value);
    this._selectedAccountName = value;
    this.accountService.selectAccount(value);
    if (this.buildUrlFunc) {
      const url = this.buildUrlFunc(value);
      this.router.navigate(url);
    }
  }

  /* Methods */

  async ngOnInit() {
    this.subscriptions.push(
      this.router.events
          .pipe(filter(ev => ev instanceof NavigationEnd))
          .subscribe(async _ => { await this.refresh(); }));

    this.subscriptions.push(
      this.userService.currentUser$.subscribe(async u => {
        if (u) {
          await this.refresh();
        } else {
          this.isSelectable = false;
        }
      }));

      this.subscriptions.push(
        this.accountService.lastSelectedAccountName$
          .subscribe(v => this._selectedAccountName = v)
      );

      await this.refresh();
  }

  ngOnDestroy() {
    this.subscriptions.forEach(s => s.unsubscribe());
    this.subscriptions = [];
  }

  private async refresh() {

    const firstChildRoute = this.router.routerState.snapshot.root.firstChild;

    if (!this.userService.isAuthenticated() || !firstChildRoute) {
      this.isSelectable = false;
      this.buildUrlFunc = null;
      this.allAccountNames = [];
      return;
    }

    const userName = this.userService.currentUser.userName;
    this.allAccountNames = this.userService.currentUser.accounts;

    // We've not visited an account link.
    const routeData = firstChildRoute.data;
    const changeAccountUrl = <string[]>routeData['changeAccountUrl'];
    if (!changeAccountUrl) {
      this.isSelectable = false;
      this.buildUrlFunc = null;
      return;
    }

    // We've visited an account link.
    this.isSelectable = true;
    this.buildUrlFunc = function(accountName: string) {
      const newUrl = [];
      changeAccountUrl.forEach(seg => {
        if (seg === ':accountName') {
          newUrl.push(accountName);
        } else {
          newUrl.push(seg);
        }
      });
      return newUrl;
    };
  }
}
