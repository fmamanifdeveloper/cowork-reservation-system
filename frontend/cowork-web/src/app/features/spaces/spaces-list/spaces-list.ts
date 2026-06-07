import { CommonModule } from '@angular/common';
import { Component, inject, signal, OnInit } from '@angular/core';
import { SpacesApi } from '@core/api/spaces-api';
import { Space } from '@core/models/space';

@Component({
  selector: 'app-spaces-list',
  imports: [CommonModule],
  templateUrl: './spaces-list.html',
  styleUrl: './spaces-list.scss',
})
export class SpacesList implements OnInit {
  private readonly spacesApi = inject(SpacesApi);

  readonly spaces = signal<Space[]>([]);
  readonly loading = signal(false);
  readonly error = signal<string | null>(null);

  ngOnInit(): void {
    this.loadSpaces();
  }

  loadSpaces(): void {
    this.loading.set(true);
    this.error.set(null);

    this.spacesApi.list().subscribe({
      next: (spaces) => {
        this.spaces.set(spaces);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('No se pudieron cargar los espacios.');
        this.loading.set(false);
      }
    });
  }

  getStatusLabel(status: string): string {
    return status === 'Active' ? 'Activo' : 'Mantenimiento';
  }
}