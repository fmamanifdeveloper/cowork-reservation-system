import { HttpClient } from "@angular/common/http";
import { Injectable, inject } from "@angular/core";
import { AuditLog } from "@core/models/audit-log";
import { API_BASE_URL } from "./api-config";

@Injectable({
  providedIn: 'root'
})
export class AuditLogsApi {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${API_BASE_URL}/audit-logs`;

  list() {
    return this.http.get<AuditLog[]>(this.baseUrl);
  }
}