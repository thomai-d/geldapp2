import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { GlobalImportsModule } from 'src/app/global-imports.module';
import { CategoryPieChartComponent } from './category-pie-chart/category-pie-chart.component';
import { ExpenseHistoryChartComponent } from './expense-history-chart/expense-history-chart.component';
import { ChartsRoutingModule } from './charts-routing.module';
import { ChartsOverviewComponent } from './charts-overview/charts-overview.component';
import { CategoryComparisonComponent } from './category-comparison/category-comparison.component';
import { CategorySelectorComponent } from './controls/category-selector/category-selector.component';
import { ChartComponent } from './controls/chart/chart.component';

@NgModule({
  declarations: [CategoryPieChartComponent, ExpenseHistoryChartComponent, ChartsOverviewComponent, CategoryComparisonComponent, CategorySelectorComponent, ChartComponent],
  imports: [
    CommonModule,
    GlobalImportsModule,
    ChartsRoutingModule
  ],
  exports: [
    CategoryPieChartComponent
  ]
})
export class ChartsModule { }
