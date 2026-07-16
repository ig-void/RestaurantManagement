import { Component, input, output } from '@angular/core';

@Component({
  selector: 'app-confirm-dialog',
  standalone: true,
  templateUrl: './confirm-dialog.component.html'
})
export class ConfirmDialogComponent {
  readonly title = input<string>('Confirm Action');
  readonly message = input<string>('Are you sure you want to proceed?');
  readonly confirm = output<boolean>();

  cancel(): void {
    this.confirm.emit(false);
  }

  confirmAction(): void {
    this.confirm.emit(true);
  }
}
