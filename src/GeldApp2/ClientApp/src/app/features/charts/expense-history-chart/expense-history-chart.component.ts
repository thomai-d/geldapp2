import { ChartHelper } from './../../../helpers/chart-helper';
import { Component } from '@angular/core';
import { GeldAppApi } from 'src/app/api/geldapp-api';
import { Router, ActivatedRoute } from '@angular/router';
import { ChartBaseComponent } from '../chart-base-component';

@Component({
  selector: 'app-expense-history-chart',
  templateUrl: './expense-history-chart.component.html',
  styleUrls: ['./expense-history-chart.component.css']
})
export class ExpenseHistoryChartComponent extends ChartBaseComponent {

  constructor(
    private api: GeldAppApi,
    activatedRoute: ActivatedRoute,
    router: Router
  ) {
    super(activatedRoute, router);
  }

  chartData: any;

  async onAccountChanged(accountName: string) {
    await this.refreshChart();
  }

  async refreshChart() {

    const [expenseDataRaw, revenueDataRaw] = <[][]>await Promise.all([
                    this.api.getExpenseHistoryChart(this.accountName),
                    this.api.getRevenueHistoryChart(this.accountName)]);

    const expenseData = ChartHelper.mapToDateItems(expenseDataRaw, true);
    const revenueData = ChartHelper.mapToDateItems(revenueDataRaw);

    const months = Math.max(expenseData.length, revenueData.length);
    const windowSize = window.innerWidth / 50;
    const monthInterval = Math.max(1, Math.ceil(months / windowSize));

    this.chartData = <any>{
      animationEnabled: true,
      animationDuration: 1000,
      culture: 'de',
      theme: 'light2',
      title: {
        text: 'Ausgaben- / Einnahmen'
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
      data: [{
        click: this.onDataPointClick.bind(this, true),
        type: 'line',
        showInLegend: true,
        name: 'Ausgaben',
        markerType: 'square',
        xValueFormatString: 'MM/YYYY',
        yValueFormatString: '0.00€',
        color: '#F08080',
        dataPoints: expenseData
      },
      {
        click: this.onDataPointClick.bind(this, false),
        type: 'line',
        showInLegend: true,
        name: 'Einnahmen',
        lineDashType: 'dash',
        xValueFormatString: 'MM/YYYY',
        yValueFormatString: '0.00€',
        dataPoints: revenueData
      }]
    };

    // "Fix" mobile in landscape orientation.
    if (window.innerHeight < 400 && window.innerHeight > 200) {
      this.chartData.height = window.innerHeight - 80;
    }
  }

  private onDataPointClick(isExpense: boolean, e: any) {
    const amountFilter = isExpense ? 'amount<0' : 'amount>0';
    const queryStr = `!year:${e.dataPoint.x.getFullYear()} and month:${e.dataPoint.x.getMonth() + 1} and ${amountFilter}`;
    this.router.navigate(['/expenses', this.accountName], { queryParams: { q: queryStr, future: true }});
  }
}
