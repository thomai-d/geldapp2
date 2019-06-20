import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { Injectable } from '@angular/core';
import { UserService } from '../services/user.service';
import { catchError } from 'rxjs/operators';
import { DialogService } from '../services/dialog.service';

@Injectable()
export class UnauthorizedInterceptor implements HttpInterceptor {

  constructor(
    private userService: UserService,
    private dialogService: DialogService
    ) {
  }

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    return next.handle(req)
      .pipe(catchError(err => {
        if (err.status === 401 && this.userService.isAuthenticated()) {
          this.dialogService.showError('Authentifizierung fehlgeschlagen. Du wirst abgemeldet.')
            .then(_ => this.userService.logout());
        }

        return throwError(err);
      }));
  }
}