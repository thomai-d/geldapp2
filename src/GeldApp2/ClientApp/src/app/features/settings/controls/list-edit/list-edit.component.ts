import { ToolbarService, ToolbarItem } from './../../../../services/toolbar.service';
import { Component, OnInit, Input, TemplateRef, ContentChild, Output, EventEmitter, ViewChild } from '@angular/core';
import { MatMenuTrigger, MatSelectionList } from '@angular/material';
import { DialogService } from 'src/app/services/dialog.service';

@Component({
  selector: 'app-list-edit',
  templateUrl: './list-edit.component.html',
  styleUrls: ['./list-edit.component.css']
})
export class ListEditComponent implements OnInit {

  private toolbarAdd: ToolbarItem;
  private toolbarBeginDelete: ToolbarItem;
  private toolbarAcceptDelete: ToolbarItem;
  private toolbarCancelDelete: ToolbarItem;

  constructor(
    private dialogService: DialogService,
    private toolbar: ToolbarService
  ) {
    this.toolbarAdd = new ToolbarItem('add', () => this.onAdd(), () => !this.isInDeleteMode);
    this.toolbarBeginDelete = new ToolbarItem('delete_sweep', () => this.enterDeleteMode(), () => !this.isInDeleteMode);
    this.toolbarAcceptDelete = new ToolbarItem('delete', () => this.acceptDeleteMode(), () => this.isInDeleteMode);
    this.toolbarCancelDelete = new ToolbarItem('cancel', () => this.abortDeleteMode(), () => this.isInDeleteMode);
  }

  @Input() items = [];

  @Input() routerLinkFunc: (item: any) => string[];

  @Output() deleteRequested = new EventEmitter<any>();

  @Output() createRequested = new EventEmitter<string>();

  @Input() placeholder: string;

  @ContentChild(TemplateRef, { static: false }) template;

  @ViewChild('itemList', { static: false }) itemList: MatSelectionList;

  isInDeleteMode = false;

  ngOnInit() {
    this.toolbar.setButtons([
      this.toolbarAdd,
      this.toolbarBeginDelete,
      this.toolbarAcceptDelete,
      this.toolbarCancelDelete
    ]);
  }

  async enterDeleteMode() {
    this.isInDeleteMode = true;
  }

  async acceptDeleteMode() {
    this.isInDeleteMode = false;
    const itemsToDelete = this.itemList.selectedOptions;
    if (!itemsToDelete.selected.length) { return; }

    const items = itemsToDelete.selected.map(i => i.value);
    this.deleteRequested.emit(items);
  }

  async abortDeleteMode() {
    this.isInDeleteMode = false;
  }

  async onAdd() {
    const text = await this.dialogService.textInput('Neuer Eintrag:');
    if (!text) {
      return;
    }

    this.createRequested.next(text);
  }
}
