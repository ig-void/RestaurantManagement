import { Component, Input, Output, EventEmitter, OnInit, OnChanges, SimpleChanges, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';

@Component({
  selector: 'app-restaurant-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './restaurant-form.component.html'
})
export class RestaurantFormComponent implements OnInit, OnChanges {
  private readonly fb = inject(FormBuilder);

  @Input() initialData: any = null;
  @Input() cuisines: any[] = [];
  @Input() loading = false;
  @Input() submitLabel = 'Submit';

  @Output() readonly submitForm = new EventEmitter<any>();
  @Output() readonly cancel = new EventEmitter<void>();

  protected form!: FormGroup;

  ngOnInit(): void {
    this.initForm();
    if (this.initialData) {
      this.form.patchValue(this.initialData);
    }
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['initialData'] && this.form) {
      if (this.initialData) {
        this.form.patchValue(this.initialData);
      } else {
        this.form.reset({
          status: 'Active'
        });
      }
    }
  }

  private initForm(): void {
    this.form = this.fb.group({
      name: ['', Validators.required],
      cuisineTypeId: ['', Validators.required],
      address: ['', Validators.required],
      city: ['', Validators.required],
      phoneNumber: ['', Validators.required],
      openingTime: ['', Validators.required],
      closingTime: ['', Validators.required],
      averageCostPerPerson: ['', [Validators.required, Validators.min(1)]],
      capacity: ['', [Validators.required, Validators.min(1)]],
      status: ['Active', Validators.required]
    });
  }

  protected onSubmit(): void {
    if (this.form.valid) {
      this.submitForm.emit(this.form.value);
    } else {
      this.form.markAllAsTouched();
    }
  }

  protected onCancel(): void {
    this.cancel.emit();
  }

  protected hasError(controlName: string): boolean {
    const control = this.form.get(controlName);
    return !!(control && control.invalid && (control.dirty || control.touched));
  }
}
