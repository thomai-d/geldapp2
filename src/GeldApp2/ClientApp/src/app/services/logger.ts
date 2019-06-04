import { Injectable } from '@angular/core';
import { Router, NavigationEnd } from '@angular/router';

@Injectable({
  providedIn: 'root'
})
export class Logger {

  constructor(private router: Router) { 
    this.router.events.subscribe(ev => {
      if (!(ev instanceof NavigationEnd)) { return; }

      const e = ev as NavigationEnd;
      this.info('routing', `Navigated to ${e.url}`);
    });
  }

  public error(path: string, message: string) {
// tslint:disable-next-line: no-console
    console.error(`${path}: ${message}`);
  }

  public warn(path: string, message: string) {
// tslint:disable-next-line: no-console
    console.warn(`${path}: ${message}`);
  }

  public info(path: string, message: string) {
// tslint:disable-next-line: no-console
    console.info(`${path}: ${message}`);
  }

  public debug(path: string, message: string) {
// tslint:disable-next-line: no-console
    console.debug(`${path}: ${message}`);
  }
}
