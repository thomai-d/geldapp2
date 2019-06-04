import { Component, OnInit, Input, AfterViewInit, OnChanges } from '@angular/core';
import { ChartHelper } from 'src/app/helpers/chart-helper';
import * as CanvasJS from 'src/libs/canvasjs.min';
import { refreshDescendantViews } from '@angular/core/src/render3/instructions';

@Component({
  selector: 'app-chart',
  templateUrl: './chart.component.html',
  styleUrls: ['./chart.component.css']
})
export class ChartComponent implements AfterViewInit, OnChanges {

  private isInitialized = false;

  private currentChart: any;

  constructor() { }

  @Input() data: any;

  chartId = ChartHelper.createChartId();

  ngAfterViewInit() {
    this.isInitialized = true;
    this.refresh();
  }

  ngOnChanges() {
    this.refresh();
  }

  private refresh() {
    if (!this.isInitialized) { return; }

    setTimeout(() => {
      if (this.data) {
        this.currentChart = new CanvasJS.Chart(this.chartId, this.data);
        this.currentChart.render();
      } else if (this.currentChart) {
        this.currentChart.destroy();
        this.currentChart = null;
      }
    }, 0);
  }
}