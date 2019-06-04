import { Progress } from 'src/app/dialogs/progress-dialog/progress-dialog.component';
import { CategoryService } from '../../../services/category.service';
import { ActivatedRoute, Router } from '@angular/router';
import { GeldAppApi } from '../../../api/geldapp-api';
import { Component, OnInit, OnDestroy } from '@angular/core';
import { Category } from 'src/app/api/model/category';
import { Logger } from 'src/app/services/logger';
import { CacheableItem } from 'src/app/services/cache.service';
import { DialogService } from 'src/app/services/dialog.service';
import { isOfflineException } from 'src/app/helpers/exception-helper';
import { BehaviorSubject } from 'rxjs';

@Component({
  selector: 'app-category-list',
  templateUrl: './category-list.component.html',
  styleUrls: ['./category-list.component.css']
})
export class CategoryListComponent implements OnInit {

  public categories: CacheableItem<Category[]>;

  public linkFromCategory = this.linkFromCategoryImpl.bind(this);

  private accountName: string;

  constructor(
    private categoryService: CategoryService,
    private activatedRoute: ActivatedRoute,
    private dialogService: DialogService,
    private router: Router,
    private log: Logger,
  ) { }

  async ngOnInit() {
    this.activatedRoute.paramMap.subscribe(async params => {
      this.accountName = params.get('accountName');
      if (!this.accountName) { this.router.navigate(['']); }
      await this.refresh();
    });
  }

  async onDelete(categories: Category[]) {
    const message = `${categories.length === 1 ? 'Kategorie' : `${categories.length} Kategorien`} endgültig löschen?`;
    if (!(await this.dialogService.decideImportant(message, 'Ja', true, 'Nein', false))) {
      return;
    }

    const progress = new BehaviorSubject<Progress>({ percentComplete: 0, statusText: 'Löschen...' });
    this.dialogService.showProgress(progress);

    try {
      const total = categories.length;
      let i = 0;
      for (const cat of categories) {
        i++;
        progress.next({ percentComplete: i / total * 100, statusText: `Lösche ${i}/${total}...` });
        await this.categoryService.deleteCategory(this.accountName, cat.name);
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

  async onCreate(name: string) {
    if (!name) {
      return;
    }

    const cat = <Category>{ name: name, subcategories: [] };

    this.log.info('category.list', `Adding category '${name}'`);

    if (this.categories.data
      && this.categories.data.find(c => c.name.localeCompare(name, undefined, { sensitivity: 'base' }) === 0)) {
      await this.dialogService.showError('Die Kategorie gibt es bereits.');
      return;
    }

    try {
      await this.categoryService.addCategory(this.accountName, cat.name);
    } catch (ex) {
      if (isOfflineException(ex)) {
        this.dialogService.showError('Kategorie konnte nicht angelegt werden: Keine Verbindung zum Server.');
        return;
      }

      this.log.error('features.category.category-list', `Error while pushing category '${name}': ${JSON.stringify(ex)}`);
      this.dialogService.showError('Kategorie konnte nicht angelegt werden: Serverfehler. Bitte an Thomas wenden.');

    } finally {
      await this.refresh();
    }
  }

  private async refresh() {
    this.categories = await this.categoryService.getCategoriesFor(this.accountName);
  }

  private linkFromCategoryImpl(category: Category): string[] {
    return ['/settings', 'categories', this.accountName, category.name];
  }
}
