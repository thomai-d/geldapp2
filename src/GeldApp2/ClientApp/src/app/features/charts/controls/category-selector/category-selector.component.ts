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
    if (this.compareFn(value, this._selection)) {
      return;
    }

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

  compareFn(a: string[], b: string[]): boolean {
    if (!a && !b) {
      return true;
    }

    if (!a || !b) {
      return false;
    }

    if (a.length !== b.length) {
      return false;
    }

    for (let n = 0; n < a.length; n++) {
      if (a[n] !== b[n]) {
        return false;
      }
    }

    return true;
  }

}
