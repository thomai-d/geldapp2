import { ChartsModule } from './../charts/charts.module';
import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ExpenseRoutingModule } from './expense-routing.module';
import { GlobalImportsModule } from 'src/app/global-imports.module';

import { ExpenseListComponent } from './expense-list/expense-list.component';
import { AddExpenseComponent } from './add-expense/add-expense.component';
import { ExpenseFormComponent } from './controls/expense-form/expense-form.component';
import { EditExpenseComponent } from './edit-expense/edit-expense.component';
import { AccountOverviewTableComponent } from './account-overview-table/account-overview-table.component';
import { ExpenseItemComponent } from './expense-item/expense-item.component';

@NgModule({
  declarations: [
    ExpenseListComponent,
    AddExpenseComponent,
    ExpenseFormComponent,
    EditExpenseComponent,
    AccountOverviewTableComponent,
    ExpenseItemComponent,
  ],
  imports: [
    GlobalImportsModule,
    CommonModule,
    ExpenseRoutingModule,
    ChartsModule
  ],
  exports: [
    AccountOverviewTableComponent
  ]
})
export class ExpenseModule { }
