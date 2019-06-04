import { Logger } from 'src/app/services/logger';
import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-not-found',
  templateUrl: './not-found.component.html',
  styleUrls: ['./not-found.component.css']
})
export class NotFoundComponent implements OnInit {

  public path: string;

  constructor(
    private router: Router,
    private log: Logger
  ) { }

  ngOnInit() {
    this.path = this.router.url;
    this.log.warn('pages.notfound', `404 for ${this.path}`);
  }
}
