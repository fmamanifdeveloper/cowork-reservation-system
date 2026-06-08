import { computed, Injectable, signal } from '@angular/core';
import { AuthenticatedUser, LoginResponse, UserRole } from './auth-models';

const TOKEN_KEY = 'cowork_access_token';
const USER_KEY = 'cowork_auth_user';

@Injectable({
    providedIn: 'root'
})
export class AuthStore {
    private readonly tokenState = signal<string | null>(this.readToken());
    private readonly userState = signal<AuthenticatedUser | null>(this.readUser());

    readonly token = this.tokenState.asReadonly();
    readonly user = this.userState.asReadonly();

    readonly isAuthenticated = computed(() => !!this.tokenState() && !!this.userState());
    readonly role = computed(() => this.userState()?.role ?? null);
    readonly displayName = computed(() => this.userState()?.displayName ?? null);
    readonly customerId = computed(() => this.userState()?.customerId ?? null);

    setSession(response: LoginResponse): void {
        localStorage.setItem(TOKEN_KEY, response.accessToken);
        localStorage.setItem(USER_KEY, JSON.stringify(response.user));

        this.tokenState.set(response.accessToken);
        this.userState.set(response.user);
    }

    logout(): void {
        localStorage.removeItem(TOKEN_KEY);
        localStorage.removeItem(USER_KEY);

        this.tokenState.set(null);
        this.userState.set(null);
    }

    hasAnyRole(roles: UserRole[]): boolean {
        const currentRole = this.role();
        return !!currentRole && roles.includes(currentRole);
    }

    private readToken(): string | null {
        return localStorage.getItem(TOKEN_KEY);
    }

    private readUser(): AuthenticatedUser | null {
        const rawUser = localStorage.getItem(USER_KEY);

        if (!rawUser) {
            return null;
        }

        try {
            return JSON.parse(rawUser) as AuthenticatedUser;
        } catch {
            localStorage.removeItem(USER_KEY);
            return null;
        }
    }
}