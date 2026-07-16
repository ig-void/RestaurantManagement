import { Component, inject, signal } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators, FormGroup, AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

interface DietaryOption {
  key: string;
  label: string;
  selected: boolean;
}

const passwordMatchValidator: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
  const password = control.get('password');
  const confirmPassword = control.get('confirmPassword');
  if (password && confirmPassword && password.value !== confirmPassword.value) {
    return { passwordMismatch: true };
  }
  return null;
};

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [ReactiveFormsModule, RouterModule],
  templateUrl: './register.component.html'
})
export class RegisterComponent {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly fb = inject(FormBuilder);

  protected readonly registerForm = this.fb.group({
    fullName: ['', [Validators.required, Validators.maxLength(100), Validators.pattern(/^[a-zA-Z\s]+$/)]],
    email: ['', [Validators.required, Validators.maxLength(100), Validators.email]],
    phoneNumber: ['', [Validators.required, Validators.pattern(/^\d{10}$/)]],
    city: ['', [Validators.required, Validators.maxLength(50)]],
    password: ['', [Validators.required, Validators.pattern(/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,24}$/)]],
    confirmPassword: ['', [Validators.required]]
  }, { validators: [passwordMatchValidator] });

  protected readonly dietaryOptions = signal<DietaryOption[]>([
    { key: 'veg', label: 'Vegetarian', selected: false },
    { key: 'vegan', label: 'Vegan', selected: false },
    { key: 'gf', label: 'Gluten-Free', selected: false },
    { key: 'peanuts', label: 'No Peanuts', selected: false }
  ]);

  protected readonly loading = signal<boolean>(false);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly successMessage = signal<string | null>(null);

  toggleDietaryOption(option: DietaryOption): void {
    this.dietaryOptions.update(opts => 
      opts.map(o => o.key === option.key ? { ...o, selected: !o.selected } : o)
    );
  }

  onSubmit(): void {
    if (this.registerForm.invalid) {
      this.registerForm.markAllAsTouched();
      return;
    }

    this.loading.set(true);
    this.errorMessage.set(null);
    this.successMessage.set(null);

    const formVal = this.registerForm.getRawValue();
    const selectedPrefs = this.dietaryOptions()
      .filter(o => o.selected)
      .map(o => o.label);

    const payload = {
      fullName: formVal.fullName,
      email: formVal.email,
      password: formVal.password,
      confirmPassword: formVal.confirmPassword,
      phoneNumber: formVal.phoneNumber,
      city: formVal.city,
      dietaryPreferences: selectedPrefs
    };

    this.authService.register(payload).subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.success) {
          this.successMessage.set('Registration successful! Redirecting to login...');
          setTimeout(() => {
            this.router.navigate(['/login']);
          }, 2000);
        } else {
          this.errorMessage.set(res.message || 'Registration failed.');
        }
      },
      error: (err) => {
        this.loading.set(false);
        this.errorMessage.set(err.error?.message || 'Server error occurred.');
      }
    });
  }
}
