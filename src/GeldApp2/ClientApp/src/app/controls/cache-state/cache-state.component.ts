import { Component, Input, OnChanges } from '@angular/core';
import { CacheableItem, ItemState } from 'src/app/services/cache.service';
import { trigger, style, state, transition, animate } from '@angular/animations';

@Component({
  selector: 'app-cache-state',
  templateUrl: './cache-state.component.html',
  styleUrls: ['./cache-state.component.css'],
  animations: [
    trigger('showHide', [
      state('*', style({
        height: '0px',
        opacity: '0'
      })),
      state('show', style({
        height: '*',
        opacity: '1'
      })),
      state('hide', style({
        height: '0px',
        opacity: '0'
      })),
      transition('show <=> hide', animate('0.5s ease-in'))
    ])
  ]
})
export class CacheStateComponent implements OnChanges {

  public show = false;

  public text: string;

  public isError: boolean;

  @Input() item: CacheableItem<any>;

  constructor() { }

  ngOnChanges() {
    if (!this.item) {
      this.show = false;
      return;
    }

    switch (this.item.state) {
      case ItemState.Online:
        this.show = false;
        this.isError = false;
        this.text = '';
        break;
      case ItemState.Cached:
        this.show = true;
        this.text = `Letzte Aktualisierung: ${this.item.getTimestamp().toLocaleString()}`;
        this.isError = false;
        break;
      case ItemState.Offline:
        this.show = true;
        this.text = 'Es kann momentan keine Verbindung zum Server hergestellt werden.';
        this.isError = false;
        break;
      case ItemState.Error:
        this.show = true;
        this.text = `Es ist ein Fehler aufgetreten: ${this.item.error}`;
        this.isError = true;
        break;
    }
  }
}
