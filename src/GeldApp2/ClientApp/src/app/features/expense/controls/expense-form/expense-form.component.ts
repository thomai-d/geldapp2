import { CategoryService } from '../../../../services/category.service';
import { ActivatedRoute, Router } from '@angular/router';
import { Category } from '../../../../api/model/category';
import { Component, OnInit, OnDestroy, Input, EventEmitter, Output, ViewChild } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { Expense } from 'src/app/api/model/expense';
import { Subscription, combineLatest } from 'rxjs';
import { ExpenseType } from 'src/app/api/model/expense-type';
import { DialogService } from 'src/app/services/dialog.service';
import { ToolbarService, ToolbarItem } from 'src/app/services/toolbar.service';
import { GeldAppApi } from 'src/app/api/geldapp-api';
import { switchMap, combineAll, startWith, filter, debounceTime, distinctUntilChanged } from 'rxjs/operators';

@Component({
  selector: 'app-expense-form',
  templateUrl: './expense-form.component.html',
  styleUrls: ['./expense-form.component.css']
})
export class ExpenseFormComponent implements OnInit, OnDestroy {

  private selectedCategory: Category;

  private formChangedSubscriptions: Subscription[] = [];

  private observables: Subscription[] = [];

  private saveButton: ToolbarItem;
  private editButton: ToolbarItem;
  private deleteButton: ToolbarItem;

  public expenseForm = this.formBuilder.group({
    amount: ['', Validators.required],
    categoryName: ['', Validators.required],
    subcategoryName: ['', Validators.required],
    details: '',
    date: ['', Validators.required],
    type: ['expense', Validators.required]
  });

  constructor(
    private categoryService: CategoryService,
    private activatedRoute: ActivatedRoute,
    private dialogService: DialogService,
    private toolbarService: ToolbarService,
    private router: Router,
    private formBuilder: FormBuilder) {

    this.saveButton = new ToolbarItem('done', () => this.onSubmit(),
                                              () => this.isInEditMode && this.isToolbarVisible, () => this.expenseForm.valid);
    this.editButton = new ToolbarItem('edit', async () => this.startEditMode(),
                                                    () => this.expense && !this.isInEditMode && this.isToolbarVisible);
    this.deleteButton = new ToolbarItem('delete', () => this.onDelete(),
                                                  () => !this.isInEditMode && !!this.expense && this.isToolbarVisible);
  }

  @Input() public expense: Expense;

  @Output() public confirmed = new EventEmitter<Expense>();

  @Output() public delete = new EventEmitter<Expense>();

  @ViewChild('createdDate') datePicker: any;

  public accountName: string;

  public categories: Category[] = [];

  public subcategories: string[] = [];

  public serverError: string;

  public isToni = false;

  public isInEditMode = false;

  public isToolbarVisible = true;

  public expenseTypeDisplay = '';

  async ngOnInit() {
    combineLatest([this.activatedRoute.paramMap, this.activatedRoute.fragment])
      .subscribe(async ([params, fragment]) => {
        this.accountName = params.get('accountName');
        this.isInEditMode = fragment === 'edit';
        if (!this.accountName) { this.router.navigate(['']); }
        await this.refresh();
        this.toolbarService.setButtons([ this.saveButton, this.editButton, this.deleteButton ]);
      });
  }

  ngOnDestroy() {
    this.disableFormChangedEvents();
    this.observables.forEach(s => s.unsubscribe());
  }

  async onSubmit() {
    const form = <Expense>this.expenseForm.value;

    const expense = new Expense();
    for (const prop of Object.keys(expense)) {
      expense[prop] = form[prop];
    }

    if (expense.type === ExpenseType.expense
      || expense.type === ExpenseType.regularExpense) {
        expense.amount = -expense.amount;
      }

    expense.accountName = this.accountName;
    expense.id = this.expense ? this.expense.id : 0;

    if (this.expense) {
      expense.created = this.expense.created;
    }

    const date = <Date>this.expenseForm.get('date').value;
    const year = date.getFullYear();
    const month = (date.getMonth() + 1).toString().padStart(2, '0');
    const day = (date.getDate()).toString().padStart(2, '0');
    expense.date = `${year}-${month}-${day}`;

    const now = new Date();

    if (!this.expense) {
      expense.created = now.toISOString();
    }

    expense.lastModified = now.toISOString();

    this.confirmed.next(expense);
    this.isInEditMode = false;
    this.isToolbarVisible = false;
  }

  async onDelete() {
    if (!this.expense) {
      return;
    }

    const result = await this.dialogService.decideImportant('Ausgabe wirklich endgültig löschen?', 'Ja', true, 'Nein', false);
    if (!result) {
      return;
    }

    this.delete.emit(this.expense);
  }

  formatDateTimeString(input: string) {
    const date = new Date(input);
    const year = date.getFullYear();
    const month = ('0' + (date.getMonth() + 1)).slice(-2);
    const day = ('0' + (date.getDate())).slice(-2);
    const time = date.toLocaleTimeString();
    return `${year}-${month}-${day} ${time}`;
  }

  typeToDisplayString(type: string): string {
    if (type === 'expense') { return 'Ausgabe'; }
    if (type === 'revenue') { return 'Einnahme'; }
    if (type === 'regularExpense') { return 'Regelmäßige Ausgabe'; }
    if (type === 'regularRevenue') { return 'Regelmäßige Einnahme'; }
    return '?';
  }

  onDateInputClick() {
    if (this.isInEditMode) {
      this.datePicker.open();
    }
  }

  async onSubtractClick() {
    const subtract = await this.dialogService.enterNumber('Abziehen?');
    if (subtract > 0) {
      const currentAmount = this.expenseForm.get('amount').value;
      if (currentAmount > subtract) {
        const result = currentAmount - subtract;
        this.expenseForm.get('amount').setValue(this.formatAmount(result));
      }
    }
  }

  private async refresh() {

    this.enableFormChangedEvents();
    this.isToni = this.accountName === 'Toni';

    if (this.expense) {
      const categories = await (this.categoryService.getCategoriesFor(this.accountName).toPromise());
      this.categories = categories.data ? categories.data : [];
      this.expenseForm.get('categoryName').setValue(null);

      this.loadExpense(this.expense);
      this.toolbarService.setTitle(this.isInEditMode ? 'Eintrag bearbeiten' : 'Eintrag ansehen');
    } else {

      const today = new Date(Date.now());
      this.expenseForm.get('date').setValue(today);
      this.toolbarService.setTitle('Neuer Eintrag');
      this.isInEditMode = true;

      this.categoryService.getCategoriesFor(this.accountName)
        .subscribe(categories => {
          this.categories = categories.data ? categories.data : [];
          this.expenseForm.get('categoryName').setValue(null);
        });

      this.setupCategoryPrediction();
    }
  }

  private setupCategoryPrediction() {
      this.observables.push(combineLatest([
        this.expenseForm.get('amount').valueChanges.pipe(startWith(this.expenseForm.get('type').value), distinctUntilChanged()),
        this.expenseForm.get('type').valueChanges.pipe(startWith(this.expenseForm.get('type').value), distinctUntilChanged()),
        this.expenseForm.get('date').valueChanges.pipe(startWith(this.expenseForm.get('date').value), distinctUntilChanged())])
        .pipe(
          filter(([amount]) => amount > 0 && this.isPredictionEnabled()),
          debounceTime(500),
          switchMap(async ([amount, type, date]) => {
            if (type === ExpenseType.expense || type === ExpenseType.regularExpense) {
              amount = -amount;
            }
            return await this.categoryService.predictCategory(this.accountName, amount, new Date(Date.now()), date);
        }),
        filter(r => r.success && this.isPredictionEnabled()))
        .subscribe(result => {
          const category = this.categories.find(cat => cat.name === result.category);
          if (category) {
            this.expenseForm.get('categoryName').setValue(result.category);
            if (category.subcategories.find(sub => sub === result.subcategory)) {
              this.expenseForm.get('subcategoryName').setValue(result.subcategory);
            }
          }
        }));
  }

  private isPredictionEnabled(): boolean {
    return this.expenseForm.get('categoryName').pristine
      && this.expenseForm.get('subcategoryName').pristine;
  }

  private startEditMode() {
    if (!this.expense) {
      return;
    }

    this.expenseForm.reset();
    this.router.navigate([], { relativeTo: this.activatedRoute, fragment: 'edit' });
  }

  private enableFormChangedEvents() {
    this.formChangedSubscriptions.push(this.expenseForm.get('categoryName')
      .valueChanges.subscribe(this.onSelectedCategoryChanged.bind(this)));
    this.formChangedSubscriptions.push(this.expenseForm.get('type')
      .valueChanges.subscribe(type => {
        this.expenseTypeDisplay = this.typeToDisplayString(type);
      }));
  }

  private disableFormChangedEvents() {
    this.formChangedSubscriptions.forEach(s => s.unsubscribe());
    this.formChangedSubscriptions = [];
  }

  private loadExpense(expense: Expense) {
    this.expenseForm.get('categoryName').setValue(expense.categoryName);
    this.onSelectedCategoryChanged(expense.categoryName);

    this.expenseForm.get('subcategoryName').setValue(expense.subcategoryName);
    this.expenseForm.get('amount').setValue(this.formatAmount(expense.amount));
    this.expenseForm.get('details').setValue(expense.details);
    this.expenseForm.get('date').setValue(new Date(expense.date));
    this.expenseForm.get('type').setValue(expense.type);
  }

  private formatAmount(amount: number): string {
    return Math.abs(Math.round(amount * 100) / 100).toFixed(2);
  }

  private onSelectedCategoryChanged(categoryName: string) {

    this.selectedCategory = this.categories.find(c => c.name === categoryName);
    if (!this.selectedCategory) {
      this.subcategories = [];
      this.expenseForm.get('subcategoryName').reset();
      return;
    }

    this.subcategories = this.selectedCategory.subcategories;
    this.expenseForm.get('subcategoryName').reset();
  }

}
