import { AccountService } from 'src/app/services/account.service';
import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AdminGuard } from 'src/app/guards/admin.guard';

@Component({
  selector: 'app-settings-overview',
  templateUrl: './settings-overview.component.html',
  styleUrls: ['./settings-overview.component.css']
})
export class SettingsOverviewComponent implements OnInit {

  constructor(
    private router: Router,
    private accountService: AccountService,
    public adminGuard: AdminGuard
  ) { }

  ngOnInit() {
  }

  gotoCategories() {
    this.router.navigate(['/settings', 'categories', this.accountService.lastSelectedAccountName]);
  }

}
