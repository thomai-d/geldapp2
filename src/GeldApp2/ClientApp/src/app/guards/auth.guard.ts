import { Injectable } from '@angular/core';
import { CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot, UrlTree, Router } from '@angular/router';
import { Observable } from 'rxjs';
import { UserService } from '../services/user.service';
import { Logger } from '../services/logger';

@Injectable({
  providedIn: 'root'
})
export class AuthGuard implements CanActivate {
  constructor(
    private userService: UserService,
    private log: Logger,
    private router: Router
  ) { }

  canActivate(
    next: ActivatedRouteSnapshot,
    state: RouterStateSnapshot): Observable<boolean | UrlTree> | Promise<boolean | UrlTree> | boolean | UrlTree {

    if (this.userService.isAuthenticated()) {
      return true;
    }

    if (next && state) {
      this.log.info('guards.auth', 'User not authenticated. Redirecting...');

      // Add redirect information.
      const params = {};
      if (location.pathname !== '/') {
        params['redirect'] = location.pathname;
      }

      this.router.navigate(['login'], { queryParams: params});
    }

    return false;
  }
}
