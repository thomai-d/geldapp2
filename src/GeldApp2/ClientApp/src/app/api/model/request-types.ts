import { Category } from './category';

export interface ChangePassword {
  oldPassword: string;
  newPassword: string;
}

export class CompareCategoryChartOptions {
  categories: CategoryFilterItem[] = [];
}

export interface CategoryFilterItem {
  categoryName: string;
  subcategoryName: string;
}