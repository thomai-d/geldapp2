import { Injectable } from '@angular/core';

export enum ItemState {
  Online,
  Cached,
  Offline,
  Error
}

export class CacheableItem<T> {
  data: T;

  timestamp: number;

  state: ItemState;

  error: string;

  isBackgroundLoading: boolean;

  private constructor() { }

  static cached<T>(data: T, ts: number): CacheableItem<T> {
    const i = new CacheableItem<T>();
    i.data = data;
    i.state = ItemState.Cached;
    i.timestamp = ts;
    return i;
  }

  static live<T>(data: T, isBackgroundLoading: boolean = false): CacheableItem<T> {
    const i = new CacheableItem<T>();
    i.data = data;
    i.state = ItemState.Online;
    i.timestamp = Date.now();
    i.isBackgroundLoading = isBackgroundLoading;
    return i;
  }

  static offline<T>(): CacheableItem<T> {
    const i = new CacheableItem<T>();
    i.state = ItemState.Offline;
    i.timestamp = Date.now();
    return i;
  }

  static error<T>(error: string): CacheableItem<T> {
    const i = new CacheableItem<T>();
    i.state = ItemState.Offline;
    i.timestamp = Date.now();
    i.error = error;
    return i;
  }

  getTimestamp() {
    return new Date(this.timestamp);
  }

  isNewerThan(milliseconds: number): boolean {
    const delta = Date.now() - this.timestamp;
    return (delta < milliseconds);
  }
}

@Injectable({ providedIn: 'root' })
export class CacheService {

  constructor() { }

  set<T>(key: string, data: T) {
    const item = <CacheableItem<T>>{
      data: data,
      timestamp: Date.now()
    };

    localStorage.setItem(`cache.${key}`, JSON.stringify(item));
  }

  get<T>(key: string): CacheableItem<T> {
    const item = <CacheableItem<T>>JSON.parse(localStorage.getItem(`cache.${key}`));
    if (!item) {
      return null;
    }

    return CacheableItem.cached<T>(item.data, item.timestamp);
  }

  clear(key: string) {
    localStorage.removeItem(`cache.${key}`);
  }
}
