export interface AuditLog {
    id: string;
    eventType: string;
    entityType: string;
    entityId: string | null;
    actorUserId: string | null;
    actorCustomerId: string | null;
    action: string;
    description: string;
    oldValues: string | null;
    newValues: string | null;
    metadata: string | null;
    ipAddress: string | null;
    userAgent: string | null;
    createdAt: string;
}