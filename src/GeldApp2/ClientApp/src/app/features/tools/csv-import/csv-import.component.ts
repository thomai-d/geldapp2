import { Component, OnInit, ElementRef, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { GeldAppApi } from 'src/app/api/geldapp-api';
import { IImportedExpense } from 'src/app/api/model/imported-expense';
import { trigger, state, transition, style, animate } from '@angular/animations';
import { Expense } from 'src/app/api/model/expense';
import { delay } from 'q';
import { ToolbarItem, ToolbarService } from 'src/app/services/toolbar.service';
import { DialogService } from 'src/app/services/dialog.service';
import { isOfflineException } from 'src/app/helpers/exception-helper';
import { Logger } from 'src/app/services/logger';

@Component({
  selector: 'app-csv-import',
  templateUrl: './csv-import.component.html',
  styleUrls: ['./csv-import.component.css'],
  animations: [
    trigger('detailExpand', [
      state('collapsed', style({height: '0px', minHeight: '0'})),
      state('expanded', style({height: '*'})),
      transition('expanded <=> collapsed', animate('225ms cubic-bezier(0.4, 0.0, 0.2, 1)')),
    ]),
  ],
})
export class CsvImportComponent implements OnInit {

  private accountName: string;
  private toolbarImportCsv: ToolbarItem;

  constructor(
    private activatedRoute: ActivatedRoute,
    private router: Router,
    private toolbar: ToolbarService,
    private dialogService: DialogService,
    private api: GeldAppApi,
    private log: Logger
  ) { }

  importedExpenses: IImportedExpense[];
  selectedElement: IImportedExpense | null;
  columnsToDisplay = [ 'bookingDay', 'partner', 'amount', 'buttons' ];

  @ViewChild('importCsvInput', { static: false }) importCsvInput: ElementRef;

  ngOnInit() {
    this.activatedRoute.paramMap.subscribe(async params => {
      this.accountName = params.get('accountName');
      if (!this.accountName) { this.router.navigate(['']); }
      this.onAccountChanged(this.accountName);
    });

    this.toolbarImportCsv = new ToolbarItem('play_for_work',
      async () => {
        this.importCsvInput.nativeElement.value = '';
        this.importCsvInput.nativeElement.click();
      }, () => true, () => true);

    this.toolbar.setButtons([this.toolbarImportCsv]);
  }

  async onSelectElement(importedExpense: IImportedExpense) {
    this.selectedElement = importedExpense;
    if (!this.selectedElement
      || this.selectedElement.relatedExpenses) {
      return;
    }

    try {
      const expenses = await this.api.getExpensesRelatedToImportedExpense(this.accountName, this.selectedElement);
      this.selectedElement.relatedExpenses = expenses;
    } catch (ex) {
      if (isOfflineException(ex)) {
        this.dialogService.showError('Keine Verbindung zum Server.');
        return;
      }
      this.dialogService.showError('Beim Laden ist ein Fehler aufgetreten.');
      this.log.errorWithException('tools.csv-import', 'error while loading', ex);
    }
  }

  async onImportCsv(event) {
    const file = event.target.files[0] as File;
    if (!file) {
      return;
    }

    try {
      await this.api.importCsv(this.accountName, file);
      await this.onAccountChanged(this.accountName);
      this.dialogService.showSnackbar('Import erfolgreich.', 2000);
    } catch (ex) {
      if (isOfflineException(ex)) {
        this.dialogService.showError('Keine Verbindung zum Server.');
        return;
      }
      this.dialogService.showError('Beim Importieren ist ein Fehler aufgetreten.');
      this.log.errorWithException('tools.csv-import', 'error while importing', ex);
    }
  }

  async onCreateNew(importedExpense: IImportedExpense) {
    const params = {
      handleImportedExpense: importedExpense.id,
      amount: importedExpense.amount,
      date: importedExpense.bookingDay
    };
    this.router.navigate(['/expenses', this.accountName, 'new'], { queryParams: params});
  }

  async onDelete(importedExpense: IImportedExpense) {
    try {
      await this.api.handleImportedExpense(this.accountName, importedExpense);
      this.importedExpenses = this.importedExpenses.filter(i => i !== importedExpense);
      this.dialogService.showSnackbar('Speichern erfolgreich.', 2000);
    } catch (ex) {
      if (isOfflineException(ex)) {
        this.dialogService.showError('Keine Verbindung zum Server.');
        return;
      }
      this.dialogService.showError('Beim speichern ist ein Fehler aufgetreten.');
      this.log.errorWithException('tools.csv-import', 'error while handling', ex);
    }
  }

  async onLinkWithExpense(importedExpense: IImportedExpense, expense: Expense) {
    try {
      await this.api.linkImportedExpenseToExpense(this.accountName, importedExpense, expense);
      this.importedExpenses = this.importedExpenses.filter(i => i !== importedExpense);
      this.dialogService.showSnackbar('Speichern erfolgreich.', 2000);
    } catch (ex) {
      if (isOfflineException(ex)) {
        this.dialogService.showError('Keine Verbindung zum Server.');
        return;
      }
      this.dialogService.showError('Beim Speichern ist ein Fehler aufgetreten.');
      this.log.errorWithException('tools.csv-import', 'error while linking', ex);
    }
  }

  private async onAccountChanged(accountName: string) {
    this.importedExpenses = await this.api.getUnhandledImportedExpenses(accountName);
  }

}
