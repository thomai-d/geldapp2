import { GeldAppApi } from '../api/geldapp-api';
import { Injectable } from '@angular/core';
import { Logger } from './logger';
import { AccountSummary } from '../api/model/account-summary';
import { CacheableItem, CacheService, ItemState } from './cache.service';
import { isOfflineException } from '../helpers/exception-helper';

@Injectable({ providedIn: 'root' })
export class ChartSummaryService {

  private readonly monthSummaryCacheKey = 'services.chart-summary.month';

  constructor(
    private cache: CacheService,
    private api: GeldAppApi,
    private log: Logger) {
  }

  /* Properties */

  public async getSummaryMonth(): Promise<CacheableItem<AccountSummary[]>> {
    try {
      const summary = await this.api.getAccountSummaryMonth();
      this.cache.set(this.monthSummaryCacheKey, summary);
      return CacheableItem.live<AccountSummary[]>(summary);
    } catch (ex) {
      const cachedItem = this.cache.get<AccountSummary[]>(this.monthSummaryCacheKey);
      if (cachedItem) {
        return cachedItem;
      }
      if (isOfflineException(ex)) {
        return CacheableItem.offline();
      }
      this.log.error('services.chart-summary', `Error while fetching month summary: ${JSON.stringify(ex)}`);
      return CacheableItem.error(ex.status);
    }
  }
}
