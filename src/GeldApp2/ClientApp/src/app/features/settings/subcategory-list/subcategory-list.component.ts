import { Category } from './../../../api/model/category';
import { Logger } from 'src/app/services/logger';
import { ActivatedRoute, Router } from '@angular/router';
import { Component, OnInit } from '@angular/core';
import { CategoryService } from 'src/app/services/category.service';
import { CacheableItem } from 'src/app/services/cache.service';
import { DialogService } from 'src/app/services/dialog.service';
import { isOfflineException } from 'src/app/helpers/exception-helper';
import { BehaviorSubject } from 'rxjs';
import { Progress } from 'src/app/dialogs/progress-dialog/progress-dialog.component';
import { ToolbarService } from 'src/app/services/toolbar.service';

@Component({
  selector: 'app-subcategory-list',
  templateUrl: './subcategory-list.component.html',
  styleUrls: ['./subcategory-list.component.css']
})
export class SubcategoryListComponent implements OnInit {

  public categories: CacheableItem<Category[]>;

  public canAdd = false;
  public subcategories: string[];
  public categoryName: string;

  private accountName: string;

  constructor(
    private toolbarService: ToolbarService,
    private activatedRoute: ActivatedRoute,
    private categoryService: CategoryService,
    private dialogService: DialogService,
    private router: Router,
    private log: Logger
  ) { }

  async ngOnInit() {
    this.activatedRoute.paramMap.subscribe(async params => {
      this.accountName = params.get('accountName');
      this.categoryName = params.get('categoryName');
      if (!this.accountName || !this.categoryName) { this.router.navigate(['']); }
      this.toolbarService.setTitle(this.categoryName);
      await this.refresh();
    });
  }

  async onCreate(name: string) {
    if (!name) {
      return;
    }

    this.log.info('subcategory.list', `Adding subcategory '${name}'`);

    if (this.subcategories
      && this.subcategories.find(s => s.localeCompare(name, undefined, { sensitivity: 'base' }) === 0)) {
      await this.dialogService.showError('Die Kategorie gibt es bereits.');
      return;
    }

    try {
      this.subcategories.push(name);
      await this.categoryService.addSubcategory(this.accountName, this.categoryName, name);
    } catch (ex) {
      if (isOfflineException(ex)) {
        this.dialogService.showError('Unterkategorie konnte nicht angelegt werden: Keine Verbindung zum Server.');
        return;
      }

      this.log.error('features.category.subcategory-list', `Error while pushing category '${name}': ${JSON.stringify(ex)}`);
      this.dialogService.showError('Unterkategorie konnte nicht angelegt werden: Serverfehler. Bitte an Thomas wenden.');

    } finally {
      await this.refresh();
    }
  }

  async onDelete(subcategories: string[]) {
    const message = `${subcategories.length === 1 ? 'Kategorie' : `${subcategories.length} Kategorien`} endgültig löschen?`;
    if (!(await this.dialogService.decideImportant(message, 'Ja', true, 'Nein', false))) {
      return;
    }

    const progress = new BehaviorSubject<Progress>({ percentComplete: 0, statusText: 'Löschen...' });
    this.dialogService.showProgress(progress);

    try {
      const total = subcategories.length;
      let i = 0;
      for (const cat of subcategories) {
        i++;
        progress.next({ percentComplete: i / total * 100, statusText: `Lösche ${i}/${total}...` });
        await this.categoryService.deleteSubcategory(this.accountName, this.categoryName, cat);
      }
    } catch (ex) {
      if (isOfflineException(ex)) {
        await this.dialogService.showError('Löschen ist im Offlinemodus nicht möglich. Sorry.');
        return;
      }
      await this.dialogService.showError('Es ist ein Fehler aufgetreten. Bitte an Thomas wenden.');
    } finally {
      progress.complete();
      await this.refresh();
    }
  }

  async refresh() {
    this.categories = await this.categoryService.getCategoriesFor(this.accountName).toPromise();
    const category = this.categories.data ? this.categories.data.find(c => c.name === this.categoryName) : null;
    if (!category) {
      this.subcategories = [];
      this.canAdd = false;
      return;
    }

    this.subcategories = category.subcategories;
    this.canAdd = true;
  }
}
