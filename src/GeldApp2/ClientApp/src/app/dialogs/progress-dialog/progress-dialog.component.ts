import { catchError } from 'rxjs/operators';
import { Component, OnInit, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material';
import { Observable } from 'rxjs';

export interface Progress {
  percentComplete: number;
  statusText: string;
}

@Component({
  selector: 'app-progress-dialog',
  templateUrl: './progress-dialog.component.html',
  styleUrls: ['./progress-dialog.component.css']
})
export class ProgressDialogComponent implements OnInit {

  progress: number;
  statusText: string;

  constructor(
    private dialogRef: MatDialogRef<ProgressDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: Observable<Progress>) {
      this.dialogRef.disableClose = true;
    }

  ngOnInit() {
    this.data
      .subscribe(d => { this.progress = d.percentComplete; this.statusText = d.statusText; },
                 err => { this.dialogRef.close(); },
                 () => { this.dialogRef.close(); });
  }

}
