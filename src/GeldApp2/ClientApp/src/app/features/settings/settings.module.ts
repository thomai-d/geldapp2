import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SettingsRoutingModule } from './settings-routing.module';
import { CategoryListComponent } from './category-list/category-list.component';
import { ListEditComponent } from './controls/list-edit/list-edit.component';
import { SubcategoryListComponent } from './subcategory-list/subcategory-list.component';
import { GlobalImportsModule } from 'src/app/global-imports.module';
import { SettingsOverviewComponent } from './settings-overview/settings-overview.component';
import { InfoComponent } from './info/info.component';
import { ChangePasswordComponent } from './change-password/change-password.component';
import { UserSettingsComponent } from './user-settings/user-settings.component';

@NgModule({
  declarations: [
    ListEditComponent,
    CategoryListComponent,
    SubcategoryListComponent,
    SettingsOverviewComponent,
    InfoComponent,
    ChangePasswordComponent,
    UserSettingsComponent,
  ],
  imports: [
    GlobalImportsModule,
    CommonModule,
    SettingsRoutingModule
  ]
})
export class SettingsModule { }
