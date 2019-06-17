import { Component, OnInit, OnDestroy } from '@angular/core';
import { environment } from 'src/environments/environment';
import { MediaObserver } from '@angular/flex-layout';
import { Subscription } from 'rxjs';
import { UserService } from 'src/app/services/user.service';

@Component({
  selector: 'app-info',
  templateUrl: './info.component.html',
  styleUrls: ['./info.component.css']
})
export class InfoComponent implements OnInit, OnDestroy {

  private subscriptions: Subscription[] = [];

  constructor(
    private mediaObserver: MediaObserver,
    public userService: UserService
  ) { }

  version: string;
  releaseDate: string;
  media: string;
  mediaQuery: string;
  token: string;

  ngOnInit() {
    const token = localStorage.getItem('authToken');
    if (token) {
      this.token = token.substr(0, 6) + '...' + token.substr(token.length - 6);
    }

    this.version = environment.version;
    this.releaseDate = environment.versionDate;
    this.subscriptions.push(this.mediaObserver.asObservable()
        .subscribe(m => {
          this.media = m ? m[0].mqAlias : '?';
          this.mediaQuery = m ? m[0].mediaQuery : '?';
        }));
  }

  ngOnDestroy() {
    this.subscriptions.forEach(s => s.unsubscribe());
    this.subscriptions = [];
  }

  eval(exp: string) {
// tslint:disable-next-line: no-eval
    return eval(exp);
  }
}
