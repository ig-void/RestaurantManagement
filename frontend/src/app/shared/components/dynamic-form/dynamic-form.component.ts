import { Component, input, output, inject, OnInit, OnChanges, SimpleChanges } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';

export interface FormField {
  key: string;
  label: string;
  type: 'text' | 'number' | 'select' | 'time' | 'checkbox' | 'email' | 'password';
  required?: boolean;
  options?: { value: any; label: string }[];
  min?: number;
  max?: number;
  regex?: string;
  regexMessage?: string;
}

@Component({
  selector: 'app-dynamic-form',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './dynamic-form.component.html'
})
export class DynamicFormComponent implements OnInit, OnChanges {
  readonly fields = input.required<FormField[]>();
  readonly initialData = input<any>(null);
  readonly submitLabel = input<string>('Submit');
  readonly loading = input<boolean>(false);

  readonly submitForm = output<any>();
  readonly cancel = output<void>();

  private readonly fb = inject(FormBuilder);
  protected form!: FormGroup;

  ngOnInit(): void {
    this.buildForm();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['initialData'] && this.form) {
      this.form.patchValue(this.initialData() || {});
    }
  }

  private buildForm(): void {
    const group: any = {};
    const data = this.initialData() || {};

    this.fields().forEach(field => {
      const validators = [];
      if (field.required) {
        validators.push(Validators.required);
      }
      if (field.min !== undefined) {
        validators.push(Validators.min(field.min));
      }
      if (field.max !== undefined) {
        validators.push(Validators.max(field.max));
      }
      if (field.regex) {
        validators.push(Validators.pattern(new RegExp(field.regex)));
      }

      const value = data[field.key] !== undefined ? data[field.key] : (field.type === 'checkbox' ? false : '');
      group[field.key] = [value, validators];
    });

    this.form = this.fb.group(group);
  }

  hasError(key: string): boolean {
    const control = this.form.get(key);
    return !!(control && control.invalid && (control.dirty || control.touched));
  }

  onSubmit(): void {
    if (this.form.valid) {
      this.submitForm.emit(this.form.value);
    } else {
      this.form.markAllAsTouched();
    }
  }

  onCancel(): void {
    this.cancel.emit();
  }
}
