export interface Customer {
    id: string;
    fullName: string;
    email: string | null;
    phone: string | null;
    documentNumber: string | null;
}

export interface CreateCustomerRequest {
    fullName: string;
    email: string | null;
    phone: string | null;
    documentNumber: string | null;
}

export type UpdateCustomerRequest = CreateCustomerRequest;