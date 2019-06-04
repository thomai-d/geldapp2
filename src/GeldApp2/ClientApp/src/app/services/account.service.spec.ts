import { TestBed } from '@angular/core/testing';

import { AccountService } from './account.service';
import { GeldAppApi } from '../api/geldapp-api';
import { skip } from 'rxjs/operators';
import { CacheService, CacheableItem } from './cache.service';

describe('AccountService', () => {
  // let getAccountNamesSucceeds = true;

  // beforeEach(() => {
  //   const apiMock = {
  //     async getAccountNames(): Promise<string[]> {
  //       if (getAccountNamesSucceeds) { return ['online']; }
  //       throw { status: 500 };
  //     }
  //   };

  //   const cacheMock = {
  //     get<T>(key: string) {
  //       if (key === AccountService.accountNamesCacheKey) {
  //         return CacheableItem.cached([ 'offline' ], 1);
  //       }
  //     }
  //   };

  //   TestBed.configureTestingModule({
  //     providers: [
  //       { provide: GeldAppApi, useValue: apiMock },
  //       { provide: CacheService, useValue: cacheMock }
  //     ]
  //   });
  // });

  // it('Accounts are read on start', (done: DoneFn) => {
  //   getAccountNamesSucceeds = true;
  //   const service: AccountService = TestBed.get(AccountService);
  //   service.accountNames$.pipe(skip(1)).subscribe(n => {
  //     expect(n).toEqual(['online']);
  //     expect(service.accountNames).toEqual(['online']);
  //     done();
  //   });
  // });
});
