import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { AuthGuard } from 'src/app/guards/auth.guard';
import { ExpenseListComponent } from './expense-list/expense-list.component';
import { AddExpenseComponent } from './add-expense/add-expense.component';
import { EditExpenseComponent } from './edit-expense/edit-expense.component';

const routes: Routes = [
  {
    path: 'expenses/:accountName',
    component: ExpenseListComponent,
    data: { changeAccountUrl: ['expenses', ':accountName'], title: 'Ausgaben - Übersicht' },
    canActivate: [AuthGuard]
  },
  {
    path: 'expenses/:accountName/new',
    component: AddExpenseComponent,
    data: { changeAccountUrl: ['expenses', ':accountName', 'new'], title: 'Neuer Eintrag' },
    canActivate: [AuthGuard]
  },
  {
    path: 'expenses/:accountName/:id',
    component: EditExpenseComponent,
    data: { changeAccountUrl: ['expenses', ':accountName'] },
    canActivate: [AuthGuard]
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class ExpenseRoutingModule { }
