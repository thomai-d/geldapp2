import { UserService } from 'src/app/services/user.service';
import { Logger } from './logger';
import { Injectable } from '@angular/core';
import { DialogService } from './dialog.service';
import { environment } from 'src/environments/environment';
import { GeldAppApi } from '../api/geldapp-api';
import { isOfflineException } from '../helpers/exception-helper';

@Injectable({
  providedIn: 'root'
})
export class UpdateService {

  constructor(
    private dialogService: DialogService,
    private api: GeldAppApi,
    private log: Logger
  ) {
    setTimeout(() => this.checkForUpdate(), 60000);
    setInterval(() => this.checkForUpdate(), 600000);
  }

  async checkForUpdate() {
    try {
      const currentVersion = environment.version;
      const version = await this.api.getAppVersion();
      if (version !== currentVersion) {
        await this.dialogService.showError(`Es ist eine neuere Programmversion verfügbar!\n` +
          `Lokal: ${currentVersion}\n` +
          `Neue Version: ${version}\n\n` +
          `Es wird jetzt eine Aktualisierung durchgeführt.`);
        window.location.reload(true);
      }
    } catch (ex) {
      if (isOfflineException(ex)) { return; }
      this.log.error('update-service', `Exception getting software version: ${JSON.stringify(ex)}`);
    }
  }

  check() {
    const currentVersion = environment.version;
    const releaseDate = environment.versionDate;
    const localVersion = localStorage.getItem('version');
    if (localVersion && localVersion !== currentVersion) {
      this.dialogService.showSnackbar(`Version aktualisiert: ${currentVersion} vom ${releaseDate}.`, 10000);
    }

    localStorage.setItem('version', currentVersion);
  }
}
