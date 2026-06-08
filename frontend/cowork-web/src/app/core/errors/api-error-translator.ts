import { HttpErrorResponse } from '@angular/common/http';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class ApiErrorTranslator {
  translate(error: unknown): string {
    if (!(error instanceof HttpErrorResponse)) {
      return 'Ocurrió un error inesperado.';
    }

    const backendError = this.readBackendError(error);

    if (error.status === 0) {
      return 'No se pudo conectar con el servidor. Verifica que la API esté ejecutándose.';
    }

    if (error.status === 400) {
      return this.translateBadRequest(backendError);
    }

    if (error.status === 401) {
      return 'Tu sesión expiró o no has iniciado sesión.';
    }

    if (error.status === 403) {
      return 'No tienes permisos para realizar esta acción.';
    }

    if (error.status === 404) {
      return 'No se encontró el recurso solicitado.';
    }

    if (error.status === 409) {
      return this.translateConflict(backendError);
    }

    if (error.status >= 500) {
      return 'Ocurrió un error interno en el servidor.';
    }

    return backendError.message || 'No se pudo completar la operación.';
  }

  private translateConflict(error: BackendError): string {
    const text = `${error.error} ${error.message}`.toLowerCase();

    if (
      text.includes('reservation conflict') ||
      text.includes('already reserved') ||
      text.includes('no longer available') ||
      text.includes('overlap')
    ) {
      return 'El espacio ya está reservado en ese horario.';
    }

    if (
      text.includes('duplicated') ||
      text.includes('unique') ||
      text.includes('already exists')
    ) {
      return 'Ya existe un registro con los mismos datos.';
    }

    return 'Existe un conflicto con los datos enviados.';
  }

  private translateBadRequest(error: BackendError): string {
    const text = `${error.error} ${error.message}`.toLowerCase();

    if (text.includes('duration')) {
      return 'La duración de la reserva no cumple las reglas permitidas.';
    }

    if (text.includes('opening') || text.includes('closing') || text.includes('business hours')) {
      return 'La reserva debe estar dentro del horario disponible del espacio.';
    }

    if (text.includes('maintenance')) {
      return 'El espacio no está disponible para reservas en este momento.';
    }

    if (text.includes('invalid reference')) {
      return 'Uno de los datos seleccionados no existe o ya no está disponible.';
    }

    return 'Los datos enviados no son válidos.';
  }

  private readBackendError(error: HttpErrorResponse): BackendError {
    const body = error.error;

    if (!body || typeof body !== 'object') {
      return {
        error: '',
        message: ''
      };
    }

    return {
      error: String(body.error ?? body.title ?? ''),
      message: String(body.message ?? body.detail ?? '')
    };
  }
}

interface BackendError {
  error: string;
  message: string;
}