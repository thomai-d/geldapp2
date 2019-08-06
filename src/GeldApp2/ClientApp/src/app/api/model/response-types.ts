export interface ExpenseRevenueLineChartsDto {
  expense: DateLineChart[];
  revenue: DateLineChart[];
}

export interface DateLineChart {
  name: string;

  items: DateChartItem[];

  tag: any;
}

export interface DateChartItem {
  // DateTime
  x: string;
  y: number;
}

export interface CategoryPredictionResult {
  category: string;
  subcategory: string;
  success: boolean;
}
