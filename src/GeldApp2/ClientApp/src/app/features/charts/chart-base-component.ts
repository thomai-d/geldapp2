import { OnInit, OnDestroy } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Subscription, fromEvent } from 'rxjs';
import { debounceTime } from 'rxjs/operators';

export abstract class ChartBaseComponent implements OnInit, OnDestroy {

  protected accountName: string;
  protected subscriptions: Subscription[] = [];

  constructor(
    protected activatedRoute: ActivatedRoute,
    protected router: Router
  ) { }

  public chartId: string;

  ngOnInit() {
    this.activatedRoute.paramMap.subscribe(async params => {
      this.accountName = params.get('accountName');
      if (!this.accountName) { this.router.navigate(['']); }
      this.onAccountChanged(this.accountName);
    });

    const showCharts$ = fromEvent(window, 'resize').pipe(debounceTime(250));
    this.subscriptions.push(showCharts$.subscribe(v => this.refreshChart()));
  }

  ngOnDestroy() {
    this.subscriptions.forEach(s => s.unsubscribe());
  }

  abstract onAccountChanged(accountName: string): Promise<void>;

  abstract refreshChart(): Promise<void>;

}
