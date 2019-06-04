import { AccountService } from './services/account.service';
import { UserService } from './services/user.service';
import { Component, ViewChild, HostListener, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { ExpenseService } from './services/expense.service';
import { DialogService } from './services/dialog.service';
import { MediaObserver } from '@angular/flex-layout';
import { MatSidenav } from '@angular/material';
import { AuthGuard } from './guards/auth.guard';
import * as Hammer from 'hammerjs';

const MenuSlideIn_Threshold = 15;
const MenuSlideIn_Area = 100;

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {

  @ViewChild('sidenav') sideNavigation: MatSidenav;

  title = 'GeldApp2';
  userName = '';
  isSmallScreen = true;

  constructor(
    public auth: AuthGuard,
    private userService: UserService,
    private accountService: AccountService,
    private expenseService: ExpenseService,
    private dialogService: DialogService,
    private router: Router,
    private mediaObserver: MediaObserver
    ) {
      this.mediaObserver.asObservable()
          .subscribe(m => {
            this.isSmallScreen = !!m.find(i => i.mqAlias === 'xs' || i.mqAlias === 'sm');
          });
      this.userService.currentUser$
          .subscribe(u => this.userName = u ? u.userName : '');
    }

  ngOnInit() {
    const hammertime = new Hammer(document.querySelector('body'));
    hammertime.on('panleft', e => this.closeSideNav(e));
    hammertime.on('panright', e => this.openSideNav(e));
  }

  gotoExpenses() {
    this.router.navigate(['/expenses', this.accountService.lastSelectedAccountName]);
  }

  gotoCharts() {
    this.router.navigate(['/charts', this.accountService.lastSelectedAccountName]);
  }

  onAccountChanged() {
    if (this.isSmallScreen) {
      this.sideNavigation.close();
    }
  }

  onMenuItemClick() {
    if (this.isSmallScreen) {
      this.sideNavigation.close();
    }
  }

  closeSideNav(event: any) {
    if (this.isSmallScreen && event.deltaX < -MenuSlideIn_Threshold) {
       this.sideNavigation.close();
    }
  }

  openSideNav(event: any) {
    if (this.isSmallScreen
      && (event.center.x - event.deltaX < MenuSlideIn_Area)
      && event.deltaX > MenuSlideIn_Threshold) {
       this.sideNavigation.open();
    }
  }

  toggleSideNav() {
    if (this.isSmallScreen) {
      this.sideNavigation.toggle();
    }
  }

  async logout() {

    // Message if unsynchronized expenses.
    const queuedStuff = this.expenseService.getNumberOfQueuedExpenses();
    if (queuedStuff) {
      const result = await this.dialogService.decideImportant(
        `Es sind noch ${queuedStuff} unsynchronisierte Einträge vorhanden. Ausloggen würde diese Einträge löschen! Fortfahren?`,
        'Ja', true, 'Nein', false);
      if (!result) {
        return;
      }
    }

    this.userService.logout();
  }
}
