import { Component, OnInit, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material';

export interface TextInputData {
  message: string;
  result: string;
}

@Component({
  selector: 'app-text-input-dialog',
  templateUrl: './text-input-dialog.component.html',
  styleUrls: ['./text-input-dialog.component.css']
})
export class TextInputDialogComponent implements OnInit {

  constructor(
    private dialogRef: MatDialogRef<TextInputDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: TextInputData) {}

  ngOnInit() {
  }

  onAccept() {
    this.dialogRef.close();
  }

  onCancel() {
    this.data.result = '';
    this.dialogRef.close();
  }
}
