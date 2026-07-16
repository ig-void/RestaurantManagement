import { Component, input, output } from '@angular/core';
import { StatusBadgeComponent } from '../status-badge/status-badge.component';

@Component({
  selector: 'app-card',
  standalone: true,
  imports: [StatusBadgeComponent],
  templateUrl: './card.component.html'
})
export class CardComponent {
  readonly restaurantName = input.required<string>();
  readonly status = input.required<string>();
  readonly cuisineType = input.required<string>();
  readonly date = input.required<string>();
  readonly time = input.required<string>();
  readonly guestCount = input.required<number>();
  readonly tableNumber = input<string | null>(null);
  readonly tableType = input<string | null>(null);
  readonly specialRequests = input<string | null>(null);

  readonly cancel = output<void>();

  showCancelButton(): boolean {
    return this.status().toLowerCase() === 'pending';
  }

  onCancelClick(): void {
    this.cancel.emit();
  }
}
