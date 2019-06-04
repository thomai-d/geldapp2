import { UserService, LoginResult } from '../../services/user.service';

import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { Logger } from 'src/app/services/logger';
import { DialogService } from 'src/app/services/dialog.service';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent implements OnInit {

  constructor(
    private authService: UserService,
    private router: Router,
    private route: ActivatedRoute,
    private log: Logger,
    private dialogService: DialogService
  ) { }

  public isAuthenticated = false;

  public isLoggingIn = false;

  ngOnInit() {
    this.isAuthenticated = this.authService.isAuthenticated();
    if (this.isAuthenticated) {
      this.router.navigate(['']);
    }
  }

  async onLogin(username: string, password: string) {
    this.isLoggingIn = true;
    const loginState = await this.authService.loginAsync(username, password);
    this.isLoggingIn = false;

    if (loginState !== LoginResult.Success) {
      this.log.warn('pages.login', `Authentication failed for ${username}`);

      switch (loginState) {
        case LoginResult.Offline:
          await this.dialogService.showError('Der Login ist aus Sicherheitsgründen nur mit Internetverbindung möglich.');
          return;
        case LoginResult.Error:
          await this.dialogService.showError('Es ist ein Fehler aufgetreten. Bitte versuche es später noch einmal.');
          return;
        case LoginResult.Rejected:
          await this.dialogService.showError('Sorry! Du kommst hier net rein.');
          return;
      }
    }

    this.log.info('pages.login', `Authenticated ${username}`);

    const redirectPath = this.route.snapshot.queryParamMap.get('redirect');
    if (redirectPath) {
      this.router.navigate([redirectPath]);
    } else {
      this.router.navigate(['']);
    }
  }
}
