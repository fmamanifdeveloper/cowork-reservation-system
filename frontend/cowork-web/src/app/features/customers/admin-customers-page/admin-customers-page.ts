import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CustomersApi } from '@core/api/customers-api';
import { CreateCustomerRequest, Customer } from '@core/models/customer';
import { NotificationStore } from '@core/notifications/notification-store';
import { finalize } from 'rxjs';

@Component({
  selector: 'app-admin-customers-page',
  imports: [FormsModule],
  templateUrl: './admin-customers-page.html',
  styleUrl: './admin-customers-page.scss',
})
export class AdminCustomersPage {
  private readonly customersApi = inject(CustomersApi);
  private readonly notificationStore = inject(NotificationStore);

  readonly customers = signal<Customer[]>([]);
  readonly isLoading = signal(false);
  readonly isSaving = signal(false);

  editingId: string | null = null;

  form: CreateCustomerRequest = this.createEmptyForm();

  ngOnInit(): void {
    this.loadCustomers();
  }

  loadCustomers(): void {
    this.isLoading.set(true);

    this.customersApi
      .list()
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: customers => this.customers.set(customers),
        error: () => this.notificationStore.error('No se pudieron cargar los clientes.')
      });
  }

  save(): void {
    if (!this.validateForm()) {
      return;
    }

    this.isSaving.set(true);

    const request: CreateCustomerRequest = {
      fullName: this.form.fullName.trim(),
      email: this.form.email?.trim() || null,
      phone: this.form.phone?.trim() || null,
      documentNumber: this.form.documentNumber?.trim() || null
    };

    const action = this.editingId
      ? this.customersApi.update(this.editingId, request)
      : this.customersApi.create(request);

    action.pipe(finalize(() => this.isSaving.set(false))).subscribe({
      next: () => {
        this.notificationStore.success(
          this.editingId ? 'Cliente actualizado correctamente.' : 'Cliente creado correctamente.'
        );

        this.cancelEdit();
        this.loadCustomers();
      },
      error: error => {
        if (error.status === 409) {
          this.notificationStore.error('Ya existe un cliente con ese correo.');
          return;
        }

        this.notificationStore.error('No se pudo guardar el cliente.');
      }
    });
  }

  edit(customer: Customer): void {
    this.editingId = customer.id;

    this.form = {
      fullName: customer.fullName,
      email: customer.email,
      phone: customer.phone,
      documentNumber: customer.documentNumber
    };

    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  delete(customer: Customer): void {
    const confirmed = window.confirm(`¿Eliminar el cliente "${customer.fullName}"?`);

    if (!confirmed) {
      return;
    }

    this.customersApi.delete(customer.id).subscribe({
      next: () => {
        this.notificationStore.success('Cliente eliminado correctamente.');
        this.loadCustomers();
      },
      error: () => this.notificationStore.error('No se pudo eliminar el cliente.')
    });
  }

  cancelEdit(): void {
    this.editingId = null;
    this.form = this.createEmptyForm();
  }

  private validateForm(): boolean {
    if (!this.form.fullName.trim()) {
      this.notificationStore.warning('Ingresa el nombre completo.');
      return false;
    }

    return true;
  }

  private createEmptyForm(): CreateCustomerRequest {
    return {
      fullName: '',
      email: '',
      phone: '',
      documentNumber: ''
    };
  }
}
