import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, RouterStateSnapshot, UrlTree, CanActivate, Router } from '@angular/router';
import { Observable } from 'rxjs';
import { UserService } from 'src/app/services/user.service';
import { Logger } from 'src/app/services/logger';

@Injectable({
  providedIn: 'root'
})
export class AdminGuard implements CanActivate {
  constructor(
    private userService: UserService,
    private log: Logger,
    private router: Router
  ) { }

  canActivate(
    next: ActivatedRouteSnapshot,
    state: RouterStateSnapshot): Observable<boolean | UrlTree> | Promise<boolean | UrlTree> | boolean | UrlTree {

    if (this.userService.isAuthenticated() && this.userService.currentUser.isAdmin) {
      return true;
    }

    if (next && state) {
      this.log.info('guards.auth', 'User is not admin. Redirecting...');
      this.router.navigate(['login']);
    }

    return false;
  }
}
