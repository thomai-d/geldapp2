import { ChartHelper } from './helpers/chart-helper';
import { CategoryService } from 'src/app/services/category.service';
import { UpdateService } from './services/update.service';
import { BrowserModule } from '@angular/platform-browser';
import { NgModule, LOCALE_ID } from '@angular/core';
import { registerLocaleData } from '@angular/common';
import localeDE from '@angular/common/locales/de';
import { JwtModule } from '@auth0/angular-jwt';
import { RoutingModule } from './app-routing.module';

import { AppComponent } from './app.component';
import { HomeComponent } from './home/home.component';
import { LoginComponent } from './pages/login/login.component';

import { GlobalImportsModule } from './global-imports.module';
import { AccountSelectorComponent } from './controls/account-selector/account-selector.component';
import { ExpenseModule } from './features/expense/expense.module';
import { NotFoundComponent } from './pages/not-found/not-found.component';
import { SettingsModule } from './features/settings/settings.module';
import { ToolsModule } from './features/tools/tools.module';
import { ServiceWorkerModule } from '@angular/service-worker';
import { environment } from '../environments/environment';
import { ErrorDialogComponent } from './dialogs/error-dialog/error-dialog.component';
import { ProgressDialogComponent } from './dialogs/progress-dialog/progress-dialog.component';
import { DecisionDialogComponent } from './dialogs/decision-dialog/decision-dialog.component';
import { TextInputDialogComponent } from './dialogs/text-input-dialog/text-input-dialog.component';
import { ToolbarComponent } from './controls/toolbar/toolbar.component';
import { HTTP_INTERCEPTORS } from '@angular/common/http';
import { UnauthorizedInterceptor } from './guards/unauthorized.interceptor';
import { HAMMER_GESTURE_CONFIG, HammerGestureConfig } from '@angular/platform-browser';
import { EnterNumberComponent } from './dialogs/enter-number/enter-number.component';

export class MyHammerConfig extends HammerGestureConfig {
    buildHammer(element: HTMLElement) {
      const mc = new Hammer(element, {
        touchAction: 'pan-y'
      });
      return mc;
    }
}

export function tokenGetter() {
  return localStorage.getItem('authToken');
}

@NgModule({
  declarations: [
    AppComponent,
    HomeComponent,
    LoginComponent,
    AccountSelectorComponent,
    NotFoundComponent,
    ErrorDialogComponent,
    ProgressDialogComponent,
    DecisionDialogComponent,
    TextInputDialogComponent,
    ToolbarComponent,
    EnterNumberComponent,
  ],
  imports: [
    GlobalImportsModule,
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    ExpenseModule,
    SettingsModule,
    RoutingModule,
    JwtModule.forRoot({
      config: {
        tokenGetter: tokenGetter,
        whitelistedDomains: [
          'localhost:5000',
          'geldapp.tmaisel.de',
          '192.168.0.20:5000'
        ]
      }
    }),
    ToolsModule,
    ServiceWorkerModule.register('ngsw-worker.js', { enabled: environment.production }),
  ],
  entryComponents: [
    ErrorDialogComponent,
    ProgressDialogComponent,
    DecisionDialogComponent,
    TextInputDialogComponent,
    EnterNumberComponent
  ],
  providers: [
    { provide: LOCALE_ID, useValue: 'de' },
    { provide: HTTP_INTERCEPTORS, useClass: UnauthorizedInterceptor, multi: true },
    { provide: HAMMER_GESTURE_CONFIG, useClass: MyHammerConfig }
  ],
  bootstrap: [AppComponent]
})
export class AppModule {
  constructor(
    updateService: UpdateService,
    categoryService: CategoryService
  ) {
    registerLocaleData(localeDE);
    updateService.check();

    categoryService.initialize();

    ChartHelper.init();
  }
}
