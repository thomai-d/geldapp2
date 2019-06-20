import { UserService } from 'src/app/services/user.service';
import { ChangePassword } from '../../../api/model/request-types';
import { DialogService } from './../../../services/dialog.service';
import { FormBuilder, Validators, AbstractControl } from '@angular/forms';
import { Component, OnInit } from '@angular/core';
import { GeldAppApi } from 'src/app/api/geldapp-api';
import { ToolbarService, ToolbarItem } from 'src/app/services/toolbar.service';
import { isOfflineException } from 'src/app/helpers/exception-helper';

@Component({
  selector: 'app-change-password',
  templateUrl: './change-password.component.html',
  styleUrls: ['./change-password.component.css']
})
export class ChangePasswordComponent implements OnInit {

  constructor(
    private formBuilder: FormBuilder,
    private toolbar: ToolbarService,
    private dialogService: DialogService,
    private userService: UserService,
    private api: GeldAppApi,
  ) { }

  public form = this.formBuilder.group({
    oldPassword: ['', Validators.required],
    newPassword: ['', [ Validators.required, Validators.minLength(5) ] ],
    newPasswordConfirmed: ['', [ Validators.required, Validators.minLength(5) ]],
  });

  ngOnInit() {
    this.form.reset();
    this.toolbar.setButtons([new ToolbarItem('done', () => this.onSubmit(), () => true, () => this.form.valid)]);
  }

  async onSubmit() {
    if (this.form.get('newPassword').value !== this.form.get('newPasswordConfirmed').value) {
      await this.dialogService.showError('Die Passwörter stimmen nicht überein.');
      return;
    }

    try {
    await this.api.changePassword(this.form.get('oldPassword').value, this.form.get('newPassword').value);
    } catch (ex) {
      if (isOfflineException(ex)) {
        await this.dialogService.showError('Es ist keine Verbindung zum Server möglich.');
        return;
      }

      if (ex.status === 403) {
        await this.dialogService.showError('Das alte Passwort ist nicht korrekt.');
        return;
      }

      await this.dialogService.showError('Es ist ein Fehler aufgetreten. Das Passwort kann aktuell nicht geändert werden.');
      return;
    }

    await this.dialogService.showSnackbar('Passwort wurde geändert. Bitte neu anmelden!', 3000);
    setTimeout(() => {
      this.userService.logout();
    }, 3000);
  }
}
