import { filter } from 'rxjs/operators';
import { Injectable, Output, OnDestroy } from '@angular/core';
import { Observable, BehaviorSubject, Subscription } from 'rxjs';
import { Router, NavigationEnd, ActivationEnd, RoutesRecognized } from '@angular/router';

export class ToolbarItem {

  constructor(icon: string, action: () => Promise<void>, isVisible?: () => boolean, isEnabled?: () => boolean) {
    this.isVisible = isVisible ? isVisible : () => true;
    this.isEnabled = isEnabled ? isEnabled : () => true;
    this.icon = icon;
    this.action = action;
  }

  isVisible: () => boolean;

  isEnabled: () => boolean;

  action: () => Promise<void>;

  icon: string;

  isExecuting: boolean;

}

@Injectable({
  providedIn: 'root'
})
export class ToolbarService implements OnDestroy {

  private titleSubject = new BehaviorSubject<string>('GeldApp2');
  private buttonsSubject = new BehaviorSubject<ToolbarItem[]>([]);

  private subscriptions: Subscription[] = [];

  constructor(
    private router: Router
  ) {
    this.title$ = this.titleSubject.asObservable();
    this.buttons$ = this.buttonsSubject.asObservable();

    this.subscriptions.push(this.router.events
                                .pipe(filter(ev => ev instanceof NavigationEnd))
                                .subscribe(ev => this.onNavigated(ev)));

    this.subscriptions.push(this.router.events
                                .pipe(filter(ev => ev instanceof RoutesRecognized))
                                .subscribe(ev => this.onBeforeNavigated(ev)));
  }

  title$: Observable<string>;
  buttons$: Observable<ToolbarItem[]>;

  setTitle(title: string) {
    this.titleSubject.next(title);
  }

  setButtons(buttons: ToolbarItem[]) {
    this.buttonsSubject.next(buttons);
  }

  ngOnDestroy() {
    this.subscriptions.forEach(s => s.unsubscribe());
    this.subscriptions = [];
  }

  private onBeforeNavigated(ev: any) {
    this.buttonsSubject.next([]);
  }

  private onNavigated(ev: any) {
    const data = this.router.routerState.snapshot.root.firstChild.data;
    if (data && data.title) {
      this.setTitle(data.title);
    } else {
      this.setTitle('GeldApp2');
    }
  }
}
