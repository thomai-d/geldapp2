import { ErrorDialogComponent } from './../dialogs/error-dialog/error-dialog.component';
import { Injectable } from '@angular/core';
import { MatDialog, MatSnackBar } from '@angular/material';
import { Progress, ProgressDialogComponent } from '../dialogs/progress-dialog/progress-dialog.component';
import { Observable } from 'rxjs';
import { DecisionDialogData, DecisionDialogComponent } from '../dialogs/decision-dialog/decision-dialog.component';
import { TextInputDialogComponent, TextInputData } from '../dialogs/text-input-dialog/text-input-dialog.component';
import { EnterNumberComponent, EnterNumberDialogData } from '../dialogs/enter-number/enter-number.component';

@Injectable({
  providedIn: 'root'
})
export class DialogService {

  constructor(
    private dialog: MatDialog,
    private snackBar: MatSnackBar
  ) { }

  public showError(message: string): Promise<any> {
    const dialogRef = this.dialog.open(ErrorDialogComponent, { data: { message: message }});
    return new Promise(resolve => {
        dialogRef.afterClosed()
          .subscribe(_ => resolve());
      });
  }

  public enterNumber(message: string): Promise<number> {
    const dialogData = <EnterNumberDialogData>{
      message: message,
    };

    const dialogRef = this.dialog.open(EnterNumberComponent, { data: dialogData });
    return new Promise(resolve => {
        dialogRef.afterClosed()
          .subscribe(_ => resolve(dialogData.result));
      });
  }

  public decideImportant(message: string, text1: string, value1: any, text2: string, value2: any): Promise<any> {
    const data: DecisionDialogData = {
      message: message,
      button1Text: text1,
      button2Text: text2,
      button1value: value1,
      button2value: value2,
      button1color: 'warn',
      button2color: 'basic',
      result: null
    };
    const dialogRef = this.dialog.open(DecisionDialogComponent, { data: data});
    return new Promise(resolve => {
        dialogRef.afterClosed()
          .subscribe(_ => resolve(data.result));
      });
  }

  public showProgress(progress: Observable<Progress>): Promise<any> {
    const dialogRef = this.dialog.open(ProgressDialogComponent, { data: progress });
    return new Promise(resolve => {
        dialogRef.afterClosed()
          .subscribe(_ => resolve());
      });
  }

  public showSnackbar(message: string, durationMs: number = 3000) {
    this.snackBar.open(message, 'OK', { duration: durationMs });
  }

  public textInput(message: string): Promise<string> {
    const data = <TextInputData>{ message: message, result: '' };
    const dialogRef = this.dialog.open(TextInputDialogComponent, { data: data });
    return new Promise(resolve => {
        dialogRef.afterClosed()
          .subscribe(_ => {
            resolve(data.result);
          });
      });
  }
}
