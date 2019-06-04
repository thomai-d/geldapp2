import { UserService } from '../services/user.service';
import { Component, OnInit, OnDestroy } from '@angular/core';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html'
})
export class HomeComponent implements OnInit, OnDestroy {
  public username: string;

  constructor(private userService: UserService) {}

  private subscriptions: Subscription[] = [];

  ngOnInit() {
    this.subscriptions.push(
      this.userService.currentUser$.subscribe(async u => {
        this.username = u ? u.userName : '';
      }));
  }

  ngOnDestroy() {
    this.subscriptions.forEach(s => s.unsubscribe());
    this.subscriptions = [];
  }
}
