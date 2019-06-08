import { GeldAppApi } from '../api/geldapp-api';
import { Injectable } from '@angular/core';
import { Logger } from './logger';
import { AccountSummary } from '../api/model/account-summary';
import { CacheableItem, CacheService, ItemState } from './cache.service';
import { isOfflineException } from '../helpers/exception-helper';
import { Observable, Observer } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class ChartSummaryService {

  // Timespan in [ms] within which the cached data is reported as 'online'.
  static readonly AssumeOnlineTimeMs = 60000;

  private readonly monthSummaryCacheKey = 'services.chart-summary.month';

  constructor(
    private cache: CacheService,
    private api: GeldAppApi,
    private log: Logger) {
  }

  /* Properties */

  public getSummaryMonth(): Observable<CacheableItem<AccountSummary[]>> {
    return new Observable<CacheableItem<AccountSummary[]>>(subj => {

      const cachedItem = this.cache.get<AccountSummary[]>(this.monthSummaryCacheKey);
      if (cachedItem) {
        if (cachedItem.isNewerThan(ChartSummaryService.AssumeOnlineTimeMs)) {
          cachedItem.state = ItemState.Online;
        } else {
          cachedItem.isBackgroundLoading = true;
        }
        subj.next(cachedItem);
      } else {
        subj.next(CacheableItem.live([], true));
      }

      this.fetchSummaryMonthFromServer(subj);

    });
  }

  private async fetchSummaryMonthFromServer(subj: Observer<CacheableItem<AccountSummary[]>>) {
    try {
      const summary = await this.api.getAccountSummaryMonth();
      this.cache.set(this.monthSummaryCacheKey, summary);
      subj.next(CacheableItem.live(summary));
    } catch (ex) {
      if (isOfflineException(ex)) {
        const cachedItem = this.cache.get<AccountSummary[]>(this.monthSummaryCacheKey);
        if (cachedItem) {
          subj.next(cachedItem);
          subj.complete();
        } else {
          subj.next(CacheableItem.offline());
          subj.complete();
        }
        return;
      }

      this.log.error('services.chart-summary', `Error while fetching month summary: ${JSON.stringify(ex)}`);
      subj.next(CacheableItem.error(ex.status));
      subj.complete();
    }
  }
}
