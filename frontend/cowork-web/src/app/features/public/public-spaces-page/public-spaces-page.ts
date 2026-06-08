import { Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { NotificationStore } from '@core/notifications/notification-store';
import { PublicApi } from '../public-api';
import { PublicSpace } from '../public-models';

@Component({
  selector: 'app-public-spaces-page',
  imports: [RouterLink],
  templateUrl: './public-spaces-page.html',
  styleUrl: './public-spaces-page.scss',
})
export class PublicSpacesPage {
  private readonly publicApi = inject(PublicApi);
  private readonly notificationStore = inject(NotificationStore);

  readonly spaces = signal<PublicSpace[]>([]);
  readonly isLoading = signal(false);

  ngOnInit(): void {
    this.loadSpaces();
  }

  loadSpaces(): void {
    this.isLoading.set(true);

    this.publicApi.listSpaces().subscribe({
      next: spaces => {
        this.spaces.set(spaces);
        this.isLoading.set(false);
      },
      error: () => {
        this.notificationStore.error('No se pudieron cargar los espacios.');
        this.isLoading.set(false);
      }
    });
  }
}
