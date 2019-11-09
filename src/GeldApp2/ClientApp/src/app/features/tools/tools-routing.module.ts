import { CategoryComparisonComponent } from './category-comparison/category-comparison.component';
import { ToolsOverviewComponent } from './tools-overview/tools-overview.component';
import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Routes } from '@angular/router';
import { AuthGuard } from 'src/app/guards/auth.guard';
import { ExpenseHistoryChartComponent } from './expense-history-chart/expense-history-chart.component';
import { CsvImportComponent } from './csv-import/csv-import.component';

const routes: Routes = [
  {
    path: 'tools/:accountName',
    component: ToolsOverviewComponent,
    data: { changeAccountUrl: ['tools', ':accountName'], title: 'Werkzeuge' },
    canActivate: [AuthGuard]
  },
  {
    path: 'tools/:accountName/expense-history',
    component: ExpenseHistoryChartComponent,
    data: { changeAccountUrl: ['tools', ':accountName', 'expense-history'], title: 'Ausgaben-Historie' },
    canActivate: [AuthGuard]
  },
  {
    path: 'tools/:accountName/category-comparison',
    component: CategoryComparisonComponent,
    data: { changeAccountUrl: ['tools', ':accountName', 'category-comparison'], title: 'Kategorievergleich' },
    canActivate: [AuthGuard]
  },
  {
    path: 'tools/:accountName/csv-import',
    component: CsvImportComponent,
    data: { changeAccountUrl: ['tools', ':accountName', 'csv-import'], title: 'CSV Import' },
    canActivate: [AuthGuard]
  }
];

@NgModule({
  imports: [
    CommonModule,
    RouterModule.forChild(routes)],
  exports: [RouterModule]
})

export class ToolsRoutingModule { }
