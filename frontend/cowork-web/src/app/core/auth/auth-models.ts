export type UserRole = 'Admin' | 'Staff' | 'Customer';

export interface LoginRequest {
    username: string;
    password: string;
}

export interface AuthenticatedUser {
    id: string;
    customerId: string | null;
    username: string;
    displayName: string;
    role: UserRole;
}

export interface LoginResponse {
    accessToken: string;
    user: AuthenticatedUser;
}