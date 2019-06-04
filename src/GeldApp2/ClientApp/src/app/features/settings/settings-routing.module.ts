import { ChangePasswordComponent } from './change-password/change-password.component';
import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { AuthGuard } from 'src/app/guards/auth.guard';

import { CategoryListComponent } from './category-list/category-list.component';
import { SubcategoryListComponent } from './subcategory-list/subcategory-list.component';
import { SettingsOverviewComponent } from './settings-overview/settings-overview.component';
import { InfoComponent } from './info/info.component';

const routes: Routes = [
  {
    path: 'settings',
    component: SettingsOverviewComponent,
    canActivate: [AuthGuard]
  },
  {
    path: 'settings/categories/:accountName',
    component: CategoryListComponent,
    data: { changeAccountUrl: ['settings', 'categories', ':accountName'], title: 'Hauptkategorien' },
    canActivate: [AuthGuard]
  },
  {
    path: 'settings/categories/:accountName/:categoryName',
    component: SubcategoryListComponent,
    data: { changeAccountUrl: ['settings', 'categories', ':accountName'] },
    canActivate: [AuthGuard]
  },
  {
    path: 'settings/info',
    data: { title: 'Info' },
    component: InfoComponent,
    canActivate: [AuthGuard]
  },
  {
    path: 'settings/password',
    data: { title: 'Passwort ändern' },
    component: ChangePasswordComponent,
    canActivate: [AuthGuard]
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class SettingsRoutingModule { }
