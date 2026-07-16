import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReservationService } from '../../../core/services/reservation.service';
import { SearchFilterComponent } from '../../../shared/components/search-filter/search-filter.component';
import { CardComponent } from '../../../shared/components/card/card.component';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog/confirm-dialog.component';

@Component({
  selector: 'app-my-reservations',
  standalone: true,
  imports: [CommonModule, SearchFilterComponent, CardComponent, ConfirmDialogComponent],
  templateUrl: './my-reservations.component.html'
})
export class MyReservationsComponent implements OnInit {
  private readonly reservationService = inject(ReservationService);

  // States
  protected readonly reservations = signal<any[]>([]);
  protected readonly loading = signal<boolean>(true);
  
  // Search & Pagination
  protected searchTerm = '';
  protected readonly pageNumber = signal<number>(1);
  protected readonly totalPages = signal<number>(1);
  protected totalCount = 0;
  protected readonly pageSize = 6;

  // Cancel Confirmation states
  protected readonly showConfirmDialog = signal<boolean>(false);
  protected readonly activeReservation = signal<any | null>(null);

  ngOnInit(): void {
    this.loadReservations();
  }

  loadReservations(): void {
    this.loading.set(true);
    this.reservationService.getMyReservations({
      searchTerm: this.searchTerm,
      pageNumber: this.pageNumber(),
      pageSize: this.pageSize
    }).subscribe({
      next: (res) => {
        this.loading.set(false);
        this.reservations.set(res.items);
        this.totalPages.set(res.totalPages);
        this.totalCount = res.totalCount;
      },
      error: () => this.loading.set(false)
    });
  }

  onSearch(term: string): void {
    this.searchTerm = term;
    this.pageNumber.set(1);
    this.loadReservations();
  }

  onPageChange(page: number): void {
    this.pageNumber.set(page);
    this.loadReservations();
  }

  // Cancel Handler
  openConfirmCancelDialog(res: any): void {
    this.activeReservation.set(res);
    this.showConfirmDialog.set(true);
  }

  handleConfirmCancel(confirmed: boolean): void {
    const res = this.activeReservation();
    this.showConfirmDialog.set(false);
    this.activeReservation.set(null);

    if (confirmed && res) {
      this.loading.set(true);
      this.reservationService.cancelReservation(res.id).subscribe({
        next: () => {
          this.loadReservations();
        },
        error: (err) => {
          this.loading.set(false);
          alert(err.error?.message || 'Failed to cancel reservation.');
        }
      });
    }
  }
}
