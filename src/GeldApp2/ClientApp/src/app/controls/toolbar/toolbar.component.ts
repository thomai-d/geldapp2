import { Component, OnInit, Output, EventEmitter } from '@angular/core';
import { AuthGuard } from 'src/app/guards/auth.guard';
import { ToolbarService, ToolbarItem } from 'src/app/services/toolbar.service';
import { delay } from 'rxjs/operators';
import { Location } from '@angular/common';

@Component({
  selector: 'app-toolbar',
  templateUrl: './toolbar.component.html',
  styleUrls: ['./toolbar.component.css']
})
export class ToolbarComponent implements OnInit {

  title: string;
  buttons: ToolbarItem[];

  constructor(
    public auth: AuthGuard,
    public location: Location,
    toolbarService: ToolbarService
  ) {
    toolbarService.title$.pipe(delay(0))
                  .subscribe(t => this.title = t);
    toolbarService.buttons$.pipe(delay(0))
                  .subscribe(b => {
                    this.buttons = b;
                  });
  }

  @Output() toggleMenu = new EventEmitter<any>();

  ngOnInit() {
  }

  async click (item: ToolbarItem) {
    item.isExecuting = true;
    try {
    await item.action();
    } finally {
      item.isExecuting = false;
    }
  }

}
