import { Component, Input, Output, EventEmitter, OnInit, OnChanges, SimpleChanges, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';

@Component({
  selector: 'app-table-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './table-form.component.html'
})
export class TableFormComponent implements OnInit, OnChanges {
  private readonly fb = inject(FormBuilder);

  @Input() initialData: any = null;
  @Input() restaurants: any[] = [];
  @Input() tableTypes: any[] = [];
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
          status: 'Available'
        });
      }
    }
  }

  private initForm(): void {
    this.form = this.fb.group({
      restaurantId: ['', Validators.required],
      tableNumber: ['', Validators.required],
      tableTypeId: ['', Validators.required],
      seatingCapacity: ['', [Validators.required, Validators.min(1)]],
      status: ['Available', Validators.required]
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
