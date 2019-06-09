import { CacheStateComponent } from './controls/cache-state/cache-state.component';
import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { MatButtonModule, MatCheckboxModule, MatToolbarModule, MatDialogModule, MatIconModule, MatSlideToggleModule, MatBadgeModule, MatProgressBarModule, MatExpansionModule, MatMenuModule } from '@angular/material';
import { MatInputModule } from '@angular/material/input';
import { MatCardModule } from '@angular/material/card';
import { MatSelectModule } from '@angular/material/select';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatTableModule } from '@angular/material/table';
import { FlexLayoutModule } from '@angular/flex-layout';
import { MatListModule } from '@angular/material/list';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { HttpClientModule } from '@angular/common/http';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

@NgModule({
  declarations: [
    CacheStateComponent
  ],
  imports: [
    BrowserAnimationsModule,
    MatButtonModule,
    MatCheckboxModule,
    MatSelectModule,
    MatInputModule,
    MatCardModule,
    MatSidenavModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatToolbarModule,
    FlexLayoutModule,
    MatListModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    CommonModule,
    HttpClientModule,
    FormsModule,
    ReactiveFormsModule,
    MatTableModule,
    MatDialogModule,
    MatIconModule,
    MatSlideToggleModule,
    MatBadgeModule,
    MatProgressBarModule,
    MatExpansionModule,
    MatMenuModule
  ],
  exports: [
    BrowserAnimationsModule,
    MatButtonModule,
    MatCheckboxModule,
    MatSelectModule,
    MatInputModule,
    MatCardModule,
    MatSidenavModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatToolbarModule,
    FlexLayoutModule,
    MatListModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    HttpClientModule,
    FormsModule,
    ReactiveFormsModule,
    MatTableModule,
    MatDialogModule,
    MatIconModule,
    MatSlideToggleModule,
    CacheStateComponent,
    MatBadgeModule,
    MatProgressBarModule,
    MatExpansionModule,
    MatMenuModule
  ]
})
export class GlobalImportsModule { }