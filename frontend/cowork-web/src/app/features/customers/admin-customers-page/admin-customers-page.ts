import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CustomersApi } from '@core/api/customers-api';
import { ApiErrorTranslator } from '@core/errors/api-error-translator';
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
  private readonly apiErrorTranslator = inject(ApiErrorTranslator);

  readonly customers = signal<Customer[]>([]);
  readonly isLoading = signal(false);
  readonly isSaving = signal(false);
  readonly isDeleting = signal(false);
  readonly isFormModalOpen = signal(false);
  readonly customerToDelete = signal<Customer | null>(null);
  readonly wasSubmitted = signal(false);

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

  openCreateModal(): void {
    this.editingId = null;
    this.form = this.createEmptyForm();
    this.wasSubmitted.set(false);
    this.isFormModalOpen.set(true);
  }

  openEditModal(customer: Customer): void {
    this.editingId = customer.id;
    this.wasSubmitted.set(false);

    this.form = {
      fullName: customer.fullName,
      email: customer.email ?? '',
      phone: customer.phone ?? '',
      documentNumber: customer.documentNumber ?? ''
    };

    this.isFormModalOpen.set(true);
  }

  closeFormModal(): void {
    if (this.isSaving()) {
      return;
    }

    this.resetFormModal();
  }

  save(): void {
    this.wasSubmitted.set(true);

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

        this.resetFormModal();
        this.loadCustomers();
      },
      error: error => {
        this.notificationStore.error(this.apiErrorTranslator.translate(error));
      }
    });
  }

  openDeleteModal(customer: Customer): void {
    this.customerToDelete.set(customer);
  }

  closeDeleteModal(): void {
    if (this.isDeleting()) {
      return;
    }

    this.customerToDelete.set(null);
  }

  confirmDelete(): void {
    const customer = this.customerToDelete();

    if (!customer) {
      return;
    }

    this.isDeleting.set(true);

    this.customersApi
      .delete(customer.id)
      .pipe(finalize(() => this.isDeleting.set(false)))
      .subscribe({
        next: () => {
          this.notificationStore.success('Cliente eliminado correctamente.');
          this.customerToDelete.set(null);
          this.loadCustomers();
        },
        error: () => this.notificationStore.error('No se pudo eliminar el cliente.')
      });
  }

  isFullNameInvalid(): boolean {
    return this.wasSubmitted() && !this.form.fullName.trim();
  }

  isEmailInvalid(): boolean {
    if (!this.wasSubmitted()) {
      return false;
    }

    const email = this.form.email?.trim();

    if (!email) {
      return false;
    }

    return !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email);
  }

  isPhoneInvalid(): boolean {
    if (!this.wasSubmitted()) {
      return false;
    }

    const phone = this.form.phone?.trim();

    if (!phone) {
      return false;
    }

    return phone.length < 6;
  }

  isDocumentInvalid(): boolean {
    if (!this.wasSubmitted()) {
      return false;
    }

    const documentNumber = this.form.documentNumber?.trim();

    if (!documentNumber) {
      return false;
    }

    return documentNumber.length < 6;
  }

  private validateForm(): boolean {
    if (!this.form.fullName.trim()) {
      this.notificationStore.warning('Ingresa el nombre completo del cliente.');
      return false;
    }

    if (this.isEmailInvalid()) {
      this.notificationStore.warning('Ingresa un correo válido.');
      return false;
    }

    if (this.isPhoneInvalid()) {
      this.notificationStore.warning('El teléfono ingresado no es válido.');
      return false;
    }

    if (this.isDocumentInvalid()) {
      this.notificationStore.warning('El documento ingresado no es válido.');
      return false;
    }

    return true;
  }

  private resetFormModal(): void {
    this.isFormModalOpen.set(false);
    this.editingId = null;
    this.wasSubmitted.set(false);
    this.form = this.createEmptyForm();
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
