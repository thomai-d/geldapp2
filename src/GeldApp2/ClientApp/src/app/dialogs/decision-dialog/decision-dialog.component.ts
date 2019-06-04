import { Component, OnInit, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material';

export interface DecisionDialogData {
  message: string;
  button1Text: string;
  button2Text: string;
  button1color: string;
  button2color: string;
  button1value: any;
  button2value: any;

  result: any;
}

@Component({
  selector: 'app-decision-dialog',
  templateUrl: './decision-dialog.component.html',
  styleUrls: ['./decision-dialog.component.css']
})
export class DecisionDialogComponent implements OnInit {

  constructor(
    private dialogRef: MatDialogRef<DecisionDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: DecisionDialogData) {}

  ngOnInit() {
  }

  onButton1() {
    this.data.result = this.data.button1value;
    this.dialogRef.close();
  }

  onButton2() {
    this.data.result = this.data.button2value;
    this.dialogRef.close();
  }
}
