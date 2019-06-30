import { Component, OnInit } from '@angular/core';
import { UserService } from 'src/app/services/user.service';
import { UserSummary } from 'src/app/api/model/user-summary';

@Component({
  selector: 'app-user-settings',
  templateUrl: './user-settings.component.html',
  styleUrls: ['./user-settings.component.css']
})
export class UserSettingsComponent implements OnInit {

  constructor(
    private userService: UserService
  ) { }

  users: UserSummary[];
  errorText: string;

  async ngOnInit() {
    try {
      this.users = await this.userService.getUserSummary();
    } catch (ex) {
      this.errorText = ex.message;
    }
  }
}
