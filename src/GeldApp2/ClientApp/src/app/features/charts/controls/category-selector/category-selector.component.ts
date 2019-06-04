import { Category } from 'src/app/api/model/category';
import { Component, OnInit, Input, OnChanges, Output, EventEmitter } from '@angular/core';

@Component({
  selector: 'app-category-selector',
  templateUrl: './category-selector.component.html',
  styleUrls: ['./category-selector.component.css']
})
export class CategorySelectorComponent {

  private _selection: string[] = [];

  constructor() { }

  @Input() categories: Category[];
  @Input() label: string;
  @Output() selectionChange = new EventEmitter<string[]>();

  selectSubcategory = false;

  set selection(value: string[]) {
    if (this._selection === value) { return; }

    this._selection = value;
    this.selectionChange.emit(value);
  }

  @Input()
  get selection() {
    return this._selection;
  }

  toggleMode() {
    this.selectSubcategory = !this.selectSubcategory;
    this.selection = [];
  }

}
