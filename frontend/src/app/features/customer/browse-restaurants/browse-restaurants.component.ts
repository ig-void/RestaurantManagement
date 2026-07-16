import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { RestaurantService } from '../../../core/services/restaurant.service';
import { ReservationService } from '../../../core/services/reservation.service';
import { LookupService } from '../../../core/services/lookup.service';
import { SearchFilterComponent, DropdownFilter } from '../../../shared/components/search-filter/search-filter.component';
import { StatusBadgeComponent } from '../../../shared/components/status-badge/status-badge.component';

@Component({
  selector: 'app-browse-restaurants',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, SearchFilterComponent, StatusBadgeComponent],
  templateUrl: './browse-restaurants.component.html'
})
export class BrowseRestaurantsComponent implements OnInit {
  private readonly restaurantService = inject(RestaurantService);
  private readonly reservationService = inject(ReservationService);
  private readonly lookupService = inject(LookupService);
  private readonly fb = inject(FormBuilder);

  // States
  protected readonly restaurants = signal<any[]>([]);
  protected readonly loading = signal<boolean>(true);
  protected readonly filters = signal<DropdownFilter[]>([]);

  // Search/Pagination states
  protected searchTerm = '';
  protected cuisineTypeId: number | null = null;
  protected readonly pageNumber = signal<number>(1);
  protected readonly totalPages = signal<number>(1);
  protected totalCount = 0;
  protected readonly pageSize = 6;

  // Reservation Modal states
  protected readonly showModal = signal<boolean>(false);
  protected readonly selectedRestaurant = signal<any | null>(null);

  protected readonly reservationForm = this.fb.group({
    guestCount: [2, [Validators.required, Validators.min(1), Validators.max(10)]],
    reservationDate: ['', [Validators.required]],
    reservationTime: ['', [Validators.required]],
    specialRequests: ['']
  });
  
  protected minDate = '';
  protected maxDate = '';
  protected readonly modalLoading = signal<boolean>(false);
  protected readonly modalError = signal<string | null>(null);
  protected readonly modalSuccess = signal<string | null>(null);

  ngOnInit(): void {
    this.loadLookups();
    this.loadRestaurants();
    this.initDateLimits();
  }

  initDateLimits(): void {
    const today = new Date();
    const max = new Date();
    max.setDate(today.getDate() + 30);

    this.minDate = this.formatDate(today);
    this.maxDate = this.formatDate(max);
  }

  formatDate(d: Date): string {
    const month = '' + (d.getMonth() + 1);
    const day = '' + d.getDate();
    const year = d.getFullYear();

    return [year, month.padStart(2, '0'), day.padStart(2, '0')].join('-');
  }

  loadLookups(): void {
    this.lookupService.getCuisines().subscribe(cuisines => {
      const options = cuisines.map(c => ({ value: c.id, label: c.name }));
      this.filters.set([
        {
          key: 'cuisineTypeId',
          label: 'Cuisines',
          options
        }
      ]);
    });
  }

  loadRestaurants(): void {
    this.loading.set(true);
    this.restaurantService.getRestaurants({
      cuisineTypeId: this.cuisineTypeId || undefined,
      searchTerm: this.searchTerm,
      pageNumber: this.pageNumber(),
      pageSize: this.pageSize,
      status: 'Active' // Customer can only browse Active restaurants
    }).subscribe({
      next: (res) => {
        this.loading.set(false);
        this.restaurants.set(res.items);
        this.totalPages.set(res.totalPages);
        this.totalCount = res.totalCount;
      },
      error: () => this.loading.set(false)
    });
  }

  onSearch(term: string): void {
    this.searchTerm = term;
    this.pageNumber.set(1);
    this.loadRestaurants();
  }

  onFilterChange(event: { key: string; value: any }): void {
    if (event.key === 'cuisineTypeId') {
      this.cuisineTypeId = event.value ? Number(event.value) : null;
    }
    this.pageNumber.set(1);
    this.loadRestaurants();
  }

  onPageChange(page: number): void {
    this.pageNumber.set(page);
    this.loadRestaurants();
  }

  // Modal Actions
  openReservationModal(restaurant: any): void {
    this.selectedRestaurant.set(restaurant);
    this.modalError.set(null);
    this.modalSuccess.set(null);
    
    // Reset Form
    this.reservationForm.reset({
      guestCount: 2,
      reservationDate: '',
      reservationTime: '',
      specialRequests: ''
    });

    this.showModal.set(true);
  }

  closeModal(): void {
    this.showModal.set(false);
    this.selectedRestaurant.set(null);
  }

  submitReservation(): void {
    if (this.reservationForm.invalid) {
      this.reservationForm.markAllAsTouched();
      return;
    }

    const restaurant = this.selectedRestaurant();
    if (!restaurant) return;

    this.modalLoading.set(true);
    this.modalError.set(null);
    this.modalSuccess.set(null);

    const formValues = this.reservationForm.getRawValue();

    const payload = {
      restaurantId: restaurant.id,
      guestCount: formValues.guestCount,
      reservationDate: formValues.reservationDate,
      reservationTime: formValues.reservationTime + ':00', // Include seconds format expected by API
      specialRequests: formValues.specialRequests
    };

    this.reservationService.requestReservation(payload).subscribe({
      next: (res) => {
        this.modalLoading.set(false);
        if (res.success) {
          this.modalSuccess.set('Your reservation request has been submitted! It is pending manager confirmation.');
          // Refresh available count
          this.loadRestaurants();
          setTimeout(() => this.closeModal(), 2000);
        } else {
          this.modalError.set(res.message || 'Failed to submit reservation request.');
        }
      },
      error: (err) => {
        this.modalLoading.set(false);
        this.modalError.set(err.error?.message || 'Server error occurred while submitting reservation.');
      }
    });
  }
}
