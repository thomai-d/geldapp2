import { Component, OnInit, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material';

export interface EnterNumberDialogData {
  message: string;
  result: number;
  hasAccepted: boolean;
}

@Component({
  selector: 'app-enter-number',
  templateUrl: './enter-number.component.html',
  styleUrls: ['./enter-number.component.css']
})
export class EnterNumberComponent implements OnInit {

  constructor(
    private dialogRef: MatDialogRef<EnterNumberComponent>,
    @Inject(MAT_DIALOG_DATA) public data: EnterNumberDialogData) {}

  ngOnInit() {
  }

  onAccept() {
    this.data.hasAccepted = true;
    this.dialogRef.close();
  }

  onClose() {
    this.data.result = 0;
    this.dialogRef.close();
  }

}
