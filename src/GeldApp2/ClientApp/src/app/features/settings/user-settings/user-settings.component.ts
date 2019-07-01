import { Component, OnInit } from '@angular/core';
import { UserService } from 'src/app/services/user.service';
import { UserSummary } from 'src/app/api/model/user-summary';
import { DialogService } from 'src/app/services/dialog.service';

@Component({
  selector: 'app-user-settings',
  templateUrl: './user-settings.component.html',
  styleUrls: ['./user-settings.component.css']
})
export class UserSettingsComponent implements OnInit {

  constructor(
    private userService: UserService,
    private dialogService: DialogService
  ) { }

  users: UserSummary[];
  errorText: string;

  new_username: string;
  new_password: string;
  new_account = true;

  ngOnInit() {
    this.refreshAsync();
  }

  async onAddUser() {
    try {
      await this.userService.addUser(this.new_username, this.new_password, this.new_account);
      this.new_username = '';
      this.new_password = '';
    } catch (ex) {
      if (ex.status && ex.error) {
        this.dialogService.showError(`Es ist ein Fehler aufgetreten (Code ${ex.status}):<br/>${ex.error}`);
      } else {
        this.dialogService.showError(`Es ist ein Fehler aufgetreten:<br/>${JSON.stringify(ex)}`);
      }
      return;
    }

    await this.refreshAsync();
  }

  private async refreshAsync() {
    try {
      this.users = await this.userService.getUserSummary();
      this.errorText = null;
    } catch (ex) {
      this.errorText = ex.message;
    }
  }
}
