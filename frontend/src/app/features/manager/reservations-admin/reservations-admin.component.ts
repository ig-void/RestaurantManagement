import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ReservationService } from '../../../core/services/reservation.service';
import { LookupService } from '../../../core/services/lookup.service';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog/confirm-dialog.component';
import { PaginationComponent } from '../../../shared/components/pagination/pagination.component';
import { StatusBadgeComponent } from '../../../shared/components/status-badge/status-badge.component';

@Component({
  selector: 'app-reservations-admin',
  standalone: true,
  imports: [
    CommonModule, 
    FormsModule, 
    ConfirmDialogComponent, 
    PaginationComponent, 
    StatusBadgeComponent
  ],
  templateUrl: './reservations-admin.component.html'
})
export class ReservationsAdminComponent implements OnInit {
  private readonly reservationService = inject(ReservationService);
  private readonly lookupService = inject(LookupService);

  // States
  protected readonly reservations = signal<any[]>([]);
  protected readonly loading = signal<boolean>(true);
  protected readonly errorMessage = signal<string | null>(null);

  // Search, Filters & Pagination
  protected searchTerm = '';
  protected filterRestaurant: string | null = null;
  protected filterDate: string | null = null;
  protected filterStatus: string | null = null;
  protected readonly pageNumber = signal<number>(1);
  protected totalCount = 0;
  protected readonly pageSize = 10;
  protected sortBy = 'reservationdate';
  protected isAscending = false; // Show newest by default

  // Lookups
  protected restaurants: any[] = [];

  // Action Dialog States
  protected readonly showConfirmDialog = signal<boolean>(false);
  protected readonly confirmTitle = signal<string>('');
  protected readonly confirmMessage = signal<string>('');
  
  private pendingAction: { name: string; row: any } | null = null;
  private searchTimeout: any;

  ngOnInit(): void {
    this.loadLookups();
    this.loadReservations();
  }

  loadLookups(): void {
    this.lookupService.getRestaurants().subscribe(res => {
      this.restaurants = res;
    });
  }

  loadReservations(): void {
    this.loading.set(true);
    this.errorMessage.set(null);

    this.reservationService.getReservations({
      searchTerm: this.searchTerm,
      restaurantId: this.filterRestaurant || undefined,
      reservationDate: this.filterDate || undefined,
      status: this.filterStatus || undefined,
      sortBy: this.sortBy,
      isAscending: this.isAscending,
      pageNumber: this.pageNumber(),
      pageSize: this.pageSize
    }).subscribe({
      next: (res) => {
        this.loading.set(false);
        this.reservations.set(res.items);
        this.totalCount = res.totalCount;
      },
      error: () => this.loading.set(false)
    });
  }

  onSearchChange(term: string): void {
    if (this.searchTimeout) {
      clearTimeout(this.searchTimeout);
    }
    this.searchTimeout = setTimeout(() => {
      this.searchTerm = term;
      this.pageNumber.set(1);
      this.loadReservations();
    }, 400);
  }

  protected toggleSort(column: string): void {
    if (this.sortBy === column) {
      this.isAscending = !this.isAscending;
    } else {
      this.sortBy = column;
      this.isAscending = true;
    }
    this.loadReservations();
  }

  onPageChange(page: number): void {
    this.pageNumber.set(page);
    this.loadReservations();
  }

  protected onAction(actionName: string, row: any): void {
    this.pendingAction = { name: actionName, row };
    
    // Set up confirm dialog messages
    if (actionName === 'confirm') {
      this.confirmTitle.set('Confirm Reservation');
      this.confirmMessage.set(`Are you sure you want to CONFIRM reservation for ${row.customerName}? An available table closest in size will be automatically allocated.`);
    } else if (actionName === 'checkin') {
      this.confirmTitle.set('Check-In Customer');
      this.confirmMessage.set(`Are you sure you want to CHECK-IN ${row.customerName}? Assigned Table: ${row.tableNumber}. This will change Table status to Occupied.`);
    } else if (actionName === 'complete') {
      this.confirmTitle.set('Complete Reservation');
      this.confirmMessage.set(`Are you sure you want to COMPLETE reservation for ${row.customerName}? This will release Table ${row.tableNumber} and mark it as Available.`);
    } else if (actionName === 'cancel') {
      this.confirmTitle.set('Cancel Reservation');
      this.confirmMessage.set(`Are you sure you want to CANCEL reservation for ${row.customerName}? Any allocated table will be released, and a cancellation email will be sent.`);
    }

    this.showConfirmDialog.set(true);
  }

  handleActionConfirm(confirmed: boolean): void {
    const action = this.pendingAction;
    this.showConfirmDialog.set(false);
    this.pendingAction = null;

    if (confirmed && action) {
      this.loading.set(true);
      this.errorMessage.set(null);

      let apiCall;
      if (action.name === 'confirm') {
        apiCall = this.reservationService.confirmReservation(action.row.id);
      } else if (action.name === 'checkin') {
        apiCall = this.reservationService.checkInReservation(action.row.id);
      } else if (action.name === 'complete') {
        apiCall = this.reservationService.completeReservation(action.row.id);
      } else {
        apiCall = this.reservationService.cancelReservation(action.row.id);
      }

      apiCall.subscribe({
        next: (res) => {
          if (res.success) {
            this.loadReservations();
          } else {
            this.loading.set(false);
            this.errorMessage.set(res.message);
          }
        },
        error: (err) => {
          this.loading.set(false);
          this.errorMessage.set(err.error?.message || 'Action execution failed.');
        }
      });
    }
  }
}
