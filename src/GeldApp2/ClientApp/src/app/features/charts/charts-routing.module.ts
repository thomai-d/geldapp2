import { CategoryComparisonComponent } from './category-comparison/category-comparison.component';
import { ChartsOverviewComponent } from './charts-overview/charts-overview.component';
import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Routes } from '@angular/router';
import { AuthGuard } from 'src/app/guards/auth.guard';
import { ExpenseHistoryChartComponent } from './expense-history-chart/expense-history-chart.component';

const routes: Routes = [
  {
    path: 'charts/:accountName',
    component: ChartsOverviewComponent,
    data: { changeAccountUrl: ['charts', ':accountName'], title: 'Charts' },
    canActivate: [AuthGuard]
  },
  {
    path: 'charts/:accountName/expense-history',
    component: ExpenseHistoryChartComponent,
    data: { changeAccountUrl: ['charts', ':accountName', 'expense-history'], title: 'Ausgaben-Historie' },
    canActivate: [AuthGuard]
  },
  {
    path: 'charts/:accountName/category-comparison',
    component: CategoryComparisonComponent,
    data: { changeAccountUrl: ['charts', ':accountName', 'category-comparison'], title: 'Kategorievergleich' },
    canActivate: [AuthGuard]
  }
];

@NgModule({
  imports: [
    CommonModule,
    RouterModule.forChild(routes)],
  exports: [RouterModule]
})

export class ChartsRoutingModule { }
