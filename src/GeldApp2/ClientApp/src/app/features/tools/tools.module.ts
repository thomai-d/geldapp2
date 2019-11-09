import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { GlobalImportsModule } from 'src/app/global-imports.module';
import { CategoryPieChartComponent } from './category-pie-chart/category-pie-chart.component';
import { ExpenseHistoryChartComponent } from './expense-history-chart/expense-history-chart.component';
import { ToolsRoutingModule } from './tools-routing.module';
import { ToolsOverviewComponent } from './tools-overview/tools-overview.component';
import { CategoryComparisonComponent } from './category-comparison/category-comparison.component';
import { CategorySelectorComponent } from './controls/category-selector/category-selector.component';
import { ChartComponent } from './controls/chart/chart.component';
import { CsvImportComponent } from './csv-import/csv-import.component';

@NgModule({
  declarations: [
    CategoryPieChartComponent,
    ExpenseHistoryChartComponent,
    ToolsOverviewComponent,
    CategoryComparisonComponent,
    CategorySelectorComponent,
    ChartComponent,
    CsvImportComponent
  ],
  imports: [
    CommonModule,
    GlobalImportsModule,
    ToolsRoutingModule
  ],
  exports: [
    CategoryPieChartComponent
  ]
})
export class ToolsModule { }
