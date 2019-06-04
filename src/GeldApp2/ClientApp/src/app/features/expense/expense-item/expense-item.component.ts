import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { Expense } from 'src/app/api/model/expense';

@Component({
  selector: 'app-expense-item',
  templateUrl: './expense-item.component.html',
  styleUrls: ['./expense-item.component.css']
})
export class ExpenseItemComponent implements OnInit {

  @Input() expense: Expense;
  @Output() click = new EventEmitter<Expense>();

  constructor() { }

  ngOnInit() {
  }

  onClick() {
    this.click.emit(this.expense);
  }

  formatDate(dateStr: string): string {
    const date = new Date(dateStr);
    const month = ('0' + (date.getMonth() + 1)).slice(-2);
    const day = ('0' + (date.getDate())).slice(-2);
    return `${date.getFullYear()}-${month}-${day}`;
  }
}
