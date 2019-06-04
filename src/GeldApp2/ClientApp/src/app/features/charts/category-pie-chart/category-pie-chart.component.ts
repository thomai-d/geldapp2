import { Component, AfterViewInit, Input, OnDestroy } from '@angular/core';
import { GeldAppApi } from 'src/app/api/geldapp-api';
import { fromEvent, Subscription } from 'rxjs';
import { debounceTime } from 'rxjs/operators';
import { Router } from '@angular/router';

@Component({
  selector: 'app-category-pie-chart',
  templateUrl: './category-pie-chart.component.html',
  styleUrls: ['./category-pie-chart.component.css']
})
export class CategoryPieChartComponent implements AfterViewInit, OnDestroy {

  private chartData: any;
  private subscriptions: Subscription[] = [];

  constructor(
    private api: GeldAppApi,
    private router: Router
  ) {
    this.isLoading = true;
  }

  @Input() public accountName: string;

  isLoading = false;

  chart: any;

  async ngAfterViewInit() {
    this.chartData = await this.api.getMonthByCategoryChart(this.accountName);
    await this.refreshChart();

    const showCharts$ = fromEvent(window, 'resize').pipe(debounceTime(500));
    this.subscriptions.push(showCharts$.subscribe(v => this.refreshChart()));
  }

  ngOnDestroy() {
    this.subscriptions.forEach(s => s.unsubscribe());
  }

  async refreshChart() {
    try {

      this.chart = {
        animationEnabled: true,
        animationDuration: 500,
        culture: 'de',
        data: [{
          click: this.onPieClick.bind(this, this.accountName),
          type: 'doughnut',
          radius: '100px',
          startAngle: 180,
          innerRadius: 50,
          indexLabelFontSize: 16,
          indexLabel: '{label}: {y}€',
          toolTipContent: '<b>{label}:</b> {y}€',
          dataPoints: this.chartData,
        }]
      };

      // "Fix" mobile in landscape orientation.
      if (window.innerHeight < 400 && window.innerHeight > 200) {
        this.chart.height = window.innerHeight - 80;
      }
    } finally {
      this.isLoading = false;
    }
  }

  private onPieClick(accountName: string, e: any) {
    const categoryName = e.dataPoint.label;
    if (!categoryName) { return; }

    const now = new Date();
    const queryParams = { q: `!category:'${categoryName}' and month:${now.getMonth()+1} and year:${now.getFullYear()}` };

    this.router.navigate(['/expenses', accountName], { queryParams: queryParams});
  }
}
