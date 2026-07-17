import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RestaurantService } from '../../../core/services/restaurant.service';
import { LookupService } from '../../../core/services/lookup.service';
import { RestaurantFormComponent } from './restaurant-form/restaurant-form.component';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog/confirm-dialog.component';
import { PaginationComponent } from '../../../shared/components/pagination/pagination.component';
import { StatusBadgeComponent } from '../../../shared/components/status-badge/status-badge.component';

@Component({
  selector: 'app-restaurants-admin',
  standalone: true,
  imports: [
    CommonModule, 
    FormsModule, 
    RestaurantFormComponent, 
    ConfirmDialogComponent, 
    PaginationComponent, 
    StatusBadgeComponent
  ],
  templateUrl: './restaurants-admin.component.html'
})
export class RestaurantsAdminComponent implements OnInit {
  private readonly restaurantService = inject(RestaurantService);
  private readonly lookupService = inject(LookupService);

  // States
  protected readonly restaurants = signal<any[]>([]);
  protected readonly loading = signal<boolean>(true);
  protected readonly errorMessage = signal<string | null>(null);

  // Search, Filters & Pagination
  protected searchTerm = '';
  protected filterCuisine: number | null = null;
  protected filterStatus: string | null = null;
  protected readonly pageNumber = signal<number>(1);
  protected totalCount = 0;
  protected readonly pageSize = 10;
  protected sortBy = 'name';
  protected isAscending = true;

  // Lookups
  protected cuisines: any[] = [];

  // Modal forms states
  protected readonly showFormModal = signal<boolean>(false);
  protected isEditMode = false;
  protected readonly editingData = signal<any>(null);
  protected readonly formLoading = signal<boolean>(false);

  // Delete dialog states
  protected readonly showDeleteDialog = signal<boolean>(false);
  protected readonly deletingRestaurant = signal<any | null>(null);
  private searchTimeout: any;

  ngOnInit(): void {
    this.loadLookups();
    this.loadRestaurants();
  }

  loadLookups(): void {
    this.lookupService.getCuisines().subscribe(c => {
      this.cuisines = c;
    });
  }

  loadRestaurants(): void {
    this.loading.set(true);
    this.errorMessage.set(null);

    this.restaurantService.getRestaurants({
      searchTerm: this.searchTerm,
      cuisineTypeId: this.filterCuisine || undefined,
      status: this.filterStatus || undefined,
      sortBy: this.sortBy,
      isAscending: this.isAscending,
      pageNumber: this.pageNumber(),
      pageSize: this.pageSize
    }).subscribe({
      next: (res) => {
        this.loading.set(false);
        this.restaurants.set(res.items);
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
      this.loadRestaurants();
    }, 400);
  }

  protected toggleSort(column: string): void {
    if (this.sortBy === column) {
      this.isAscending = !this.isAscending;
    } else {
      this.sortBy = column;
      this.isAscending = true;
    }
    this.loadRestaurants();
  }

  onPageChange(page: number): void {
    this.pageNumber.set(page);
    this.loadRestaurants();
  }

  // Add/Edit Form Actions
  openAddForm(): void {
    this.isEditMode = false;
    this.editingData.set(null);
    this.showFormModal.set(true);
  }

  openEditForm(row: any): void {
    this.isEditMode = true;
    
    // Ensure times map cleanly to HTML timepicker (HH:mm)
    const formattedData = {
      ...row,
      openingTime: row.openingTime.substring(0, 5),
      closingTime: row.closingTime.substring(0, 5)
    };
    
    this.editingData.set(formattedData);
    this.showFormModal.set(true);
  }

  closeFormModal(): void {
    this.showFormModal.set(false);
    this.editingData.set(null);
  }

  onSubmitForm(formData: any): void {
    this.formLoading.set(true);
    this.errorMessage.set(null);

    // Format times for backend parser (hh:mm:ss)
    const payload = {
      ...formData,
      openingTime: formData.openingTime + ':00',
      closingTime: formData.closingTime + ':00'
    };

    const request = this.isEditMode
      ? this.restaurantService.updateRestaurant(this.editingData().id, { id: this.editingData().id, ...payload })
      : this.restaurantService.createRestaurant(payload);

    request.subscribe({
      next: (res) => {
        this.formLoading.set(false);
        if (res.success) {
          this.closeFormModal();
          this.loadRestaurants();
        } else {
          this.errorMessage.set(res.message);
        }
      },
      error: (err) => {
        this.formLoading.set(false);
        this.errorMessage.set(err.error?.message || 'Failed to submit form.');
      }
    });
  }

  // Delete Actions
  openDeleteConfirmation(row: any): void {
    this.deletingRestaurant.set(row);
    this.showDeleteDialog.set(true);
  }

  handleConfirmDelete(confirmed: boolean): void {
    const r = this.deletingRestaurant();
    this.showDeleteDialog.set(false);
    this.deletingRestaurant.set(null);

    if (confirmed && r) {
      this.loading.set(true);
      this.restaurantService.deleteRestaurant(r.id).subscribe({
        next: (res) => {
          if (res.success) {
            this.loadRestaurants();
          } else {
            this.loading.set(false);
            this.errorMessage.set(res.message);
          }
        },
        error: (err) => {
          this.loading.set(false);
          this.errorMessage.set(err.error?.message || 'Failed to delete restaurant.');
        }
      });
    }
  }
}
