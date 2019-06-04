import { Component, OnInit, Input, OnChanges } from '@angular/core';
import { CacheableItem, ItemState } from 'src/app/services/cache.service';

@Component({
  selector: 'app-cache-state',
  templateUrl: './cache-state.component.html',
  styleUrls: ['./cache-state.component.css']
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
      case ItemState.Cached:
        this.show = true;
        this.text = `Letzte Aktualisierung: ${this.item.getTimestamp().toLocaleString()}`;
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
