import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

import { AuthGuard } from './guards/auth.guard';

import { HomeComponent } from './home/home.component';
import { LoginComponent } from './pages/login/login.component';
import { NotFoundComponent } from './pages/not-found/not-found.component';
import { InfoComponent } from './features/settings/info/info.component';

@NgModule({
  declarations: [],
  imports: [
    CommonModule,
    RouterModule.forRoot([
      {
        path: '',
        component: HomeComponent,
        pathMatch: 'full',
        data: { title: 'Übersicht' },
        canActivate: [AuthGuard]
      },
      {
        path: 'login',
        component: LoginComponent
      },
      {
        path: '**',
        data: { title: '¯\_(ツ)_/¯' },
        component: NotFoundComponent,
        canActivate: [AuthGuard]
      }
    ]),
  ],
  exports: [
    RouterModule
  ]
})
export class RoutingModule { }
