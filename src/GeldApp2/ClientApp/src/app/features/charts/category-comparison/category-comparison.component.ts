import { GeldAppApi } from './../../../api/geldapp-api';
import { Category } from 'src/app/api/model/category';
import { CategoryService } from 'src/app/services/category.service';
import { ActivatedRoute, Router, ParamMap } from '@angular/router';
import { Component, OnInit } from '@angular/core';
import { ChartBaseComponent } from '../chart-base-component';
import { DateLineChart } from 'src/app/api/model/response-types';
import { ChartHelper } from 'src/app/helpers/chart-helper';

@Component({
  selector: 'app-category-comparison',
  templateUrl: './category-comparison.component.html',
  styleUrls: ['./category-comparison.component.css']
})
export class CategoryComparisonComponent
    extends ChartBaseComponent
    implements OnInit {

  private isInitializing = true;

  constructor(
    private categoryService: CategoryService,
    private api: GeldAppApi,
    activatedRoute: ActivatedRoute,
    router: Router) {
    super(activatedRoute, router);
  }

  /* Properties */

  category1: string[] = [];
  category2: string[] = [];
  category3: string[] = [];
  category4: string[] = [];

  categories: Category[] = [];

  expenseChart: any;
  revenueChart: any;

  onSelectionChanged() {
    if (this.isInitializing) {
      return;
    }

    const params = <any>{};
    if (this.category1 && this.category1.length) { params.cat1 = this.category1; }
    if (this.category2 && this.category2.length) { params.cat2 = this.category2; }
    if (this.category3 && this.category3.length) { params.cat3 = this.category3; }
    if (this.category4 && this.category4.length) { params.cat4 = this.category4; }
    this.router.navigate(['.'], { relativeTo: this.activatedRoute, queryParams: params });
  }

  ngOnInit() {
    super.ngOnInit();
    this.activatedRoute.queryParamMap.subscribe(params => {
      setTimeout(() => {
        this.onParamsChanged(params);
        this.refreshChart();
      });
    });
  }

  async onAccountChanged(accountName: string) {
    this.categories = (await this.categoryService.getCategoriesFor(accountName)).data;
    this.expenseChart = null;
    this.revenueChart = null;
  }

  async refreshChart() {
    if (!this.category1.length && !this.category2.length && !this.category3.length && !this.category4.length) {
      return;
    }

    const categories = [];
    if (this.category1 && this.category1.length) { categories.push(this.category1); }
    if (this.category2 && this.category2.length) { categories.push(this.category2); }
    if (this.category3 && this.category3.length) { categories.push(this.category3); }
    if (this.category4 && this.category4.length) { categories.push(this.category4); }

    const data = await this.api.getCompareCategoriesChart(this.accountName, categories);
    this.expenseChart = this.renderChart('Ausgaben', data.expense, true);
    this.revenueChart = this.renderChart('Einnahmen', data.revenue, false);
  }

  private onParamsChanged(paramMap: ParamMap) {
    this.isInitializing = true;
    this.category1 = paramMap.getAll('cat1');
    this.category2 = paramMap.getAll('cat2');
    this.category3 = paramMap.getAll('cat3');
    this.category4 = paramMap.getAll('cat4');
    this.isInitializing = false;
  }

  private renderChart(title: string, data: DateLineChart[], invert: boolean) {

    if (!data.length) {
      return null;
    }

    const months = Math.max(...data.map(l => l.items.length));
    const windowSize = window.innerWidth / 50;
    const monthInterval = Math.max(1, Math.ceil(months / windowSize));

    const chartOptions = this.buildChartOptions(monthInterval, title);
    for (const line of data) {
      chartOptions.data.push({
        click: this.onDataPointClick.bind(this, line.tag), /* The line's tag contains a filter expression */
        type: 'line',
        showInLegend: true,
        name: line.name,
        markerType: 'square',
        xValueFormatString: 'MM/YYYY',
        yValueFormatString: '0.00€',
        dataPoints: ChartHelper.mapToDateItems(line.items, invert),
      });
    }

    // "Fix" mobile in landscape orientation.
    if (window.innerHeight < 400 && window.innerHeight > 200) {
      chartOptions.height = window.innerHeight - 80;
    }

    return chartOptions;
  }

  private buildChartOptions(monthInterval: number, title: string): any {
    return {
      culture: 'de',
      animationEnabled: true,
      animationDuration: 1000,
      theme: 'light2',
      title: {
        text: title,
      },
      axisX: {
        valueFormatString: 'MM/YYYY',
        interval: monthInterval,
        intervalType: 'month',
        crosshair: {
          enabled: true,
          snapToDataPoint: true
        }
      },
      axisY: {
        title: '€',
        crosshair: {
          enabled: false
        }
      },
      toolTip: {
        shared: true
      },
      legend: {
        cursor: 'pointer',
        verticalAlign: 'top',
        horizontalAlign: 'center',
        dockInsidePlotArea: false,
      },
      data: []
    };

  }

  private onDataPointClick(filterStr: any, e: any) {
    const queryStr = `!year:${e.dataPoint.x.getFullYear()} and month:${e.dataPoint.x.getMonth() + 1} and ${filterStr}`;
    this.router.navigate(['/expenses', this.accountName], { queryParams: { q: queryStr, future: true }});
  }
}
