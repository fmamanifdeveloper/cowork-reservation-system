-- ============================================================
-- CoWork Spaces
-- ============================================================

-- ============================================================
-- COMENTARIOS DE TABLAS
-- ============================================================

COMMENT ON TABLE app_user_roles IS 'Catálogo de roles de usuario del sistema. Define los niveles de acceso: Administrador, Personal interno y Cliente.';
COMMENT ON TABLE app_user_statuses IS 'Catálogo de estados de usuario del sistema. Controla si un usuario puede iniciar sesión y operar.';
COMMENT ON TABLE space_statuses IS 'Catálogo de estados de espacios. Determina si un espacio está disponible para reservas, en mantenimiento o inactivo.';
COMMENT ON TABLE reservation_statuses IS 'Catálogo de estados de reservas. Gestiona el ciclo de vida: Pendiente, Confirmada, Cancelada y Completada.';

COMMENT ON TABLE customers IS 'Almacena los perfiles comerciales de los clientes. Un cliente puede tener múltiples reservas y opcionalmente una cuenta de usuario asociada.';
COMMENT ON TABLE app_users IS 'Almacena cuentas de usuario autenticadas del sistema. Incluye administradores, personal interno y clientes con credenciales de acceso.';
COMMENT ON TABLE spaces IS 'Almacena los espacios de cowork disponibles para reserva. Incluye información de capacidad, tarifas, horarios y zona horaria.';
COMMENT ON TABLE reservations IS 'Almacena los registros de reservas con información de cliente, espacio, precios y todo el ciclo de vida de la reserva.';
COMMENT ON TABLE audit_logs IS 'Almacena eventos de auditoría a nivel de negocio. Registra quién, cuándo y qué cambió en las entidades del sistema.';

-- ============================================================
-- COMENTARIOS DE COLUMNAS - APP_USERS
-- ============================================================

COMMENT ON COLUMN app_users.id IS 'Identificador único universal (UUID) del usuario. Clave primaria.';
COMMENT ON COLUMN app_users.customer_id IS 'Referencia opcional al perfil de cliente. Requerido SOLO cuando role_id es Customer (3). Para administradores y staff debe ser NULL.';
COMMENT ON COLUMN app_users.username IS 'Nombre de usuario único para autenticación. Case-sensitive, mínimo 1 carácter.';
COMMENT ON COLUMN app_users.password_hash IS 'Hash de la contraseña. Almacenado con bcrypt o algoritmo similar. Longitud suficiente para hash seguro.';
COMMENT ON COLUMN app_users.display_name IS 'Nombre visible del usuario en la interfaz. No necesita ser único.';
COMMENT ON COLUMN app_users.role_id IS 'Rol del usuario: 1=Admin (administrador), 2=Staff (personal interno), 3=Customer (cliente con cuenta).';
COMMENT ON COLUMN app_users.status_id IS 'Estado del usuario: 1=Active (activo), 2=Inactive (inactivo), 3=Locked (bloqueado por intentos fallidos).';
COMMENT ON COLUMN app_users.is_active IS 'Bandera de activación. Si es FALSE, el usuario no puede iniciar sesión independientemente del status_id.';
COMMENT ON COLUMN app_users.is_deleted IS 'Soft delete. TRUE indica que el usuario fue eliminado lógicamente.';
COMMENT ON COLUMN app_users.last_login_at IS 'Timestamp del último inicio de sesión exitoso. Útil para auditoría y detección de cuentas inactivas.';
COMMENT ON COLUMN app_users.created_at IS 'Fecha y hora de creación del registro.';
COMMENT ON COLUMN app_users.updated_at IS 'Fecha y hora de la última actualización del registro. Actualizado automáticamente por trigger.';
COMMENT ON COLUMN app_users.deleted_at IS 'Fecha y hora de la eliminación lógica. Populado solo si is_deleted = TRUE.';
COMMENT ON COLUMN app_users.created_by_user_id IS 'Referencia al usuario que creó este registro. Autoreferencia a app_users.';
COMMENT ON COLUMN app_users.updated_by_user_id IS 'Referencia al usuario que realizó la última actualización. Autoreferencia a app_users.';
COMMENT ON COLUMN app_users.deleted_by_user_id IS 'Referencia al usuario que realizó la eliminación lógica. Autoreferencia a app_users.';
COMMENT ON COLUMN app_users.version IS 'Número de versión para control de concurrencia optimista. Se incrementa automáticamente en cada UPDATE.';

-- ============================================================
-- COMENTARIOS DE COLUMNAS - CUSTOMERS
-- ============================================================

COMMENT ON COLUMN customers.id IS 'Identificador único universal (UUID) del cliente. Clave primaria.';
COMMENT ON COLUMN customers.full_name IS 'Nombre completo del cliente. Campo obligatorio, mínimo 1 carácter.';
COMMENT ON COLUMN customers.email IS 'Correo electrónico del cliente. Opcional pero único si está presente y no eliminado.';
COMMENT ON COLUMN customers.phone IS 'Número de teléfono del cliente. Opcional, formato libre.';
COMMENT ON COLUMN customers.document_number IS 'Número de documento de identidad (DNI, RUC, pasaporte). Opcional, para facturación.';
COMMENT ON COLUMN customers.is_deleted IS 'Soft delete. TRUE indica que el cliente fue eliminado lógicamente. Las reservas históricas se mantienen.';
COMMENT ON COLUMN customers.created_at IS 'Fecha y hora de creación del registro.';
COMMENT ON COLUMN customers.updated_at IS 'Fecha y hora de la última actualización del registro. Actualizado automáticamente por trigger.';
COMMENT ON COLUMN customers.deleted_at IS 'Fecha y hora de la eliminación lógica. Populado solo si is_deleted = TRUE.';
COMMENT ON COLUMN customers.created_by_user_id IS 'Referencia al usuario del sistema que creó este cliente.';
COMMENT ON COLUMN customers.updated_by_user_id IS 'Referencia al usuario del sistema que actualizó este cliente.';
COMMENT ON COLUMN customers.deleted_by_user_id IS 'Referencia al usuario del sistema que eliminó lógicamente este cliente.';
COMMENT ON COLUMN customers.version IS 'Número de versión para control de concurrencia optimista. Se incrementa automáticamente en cada UPDATE.';

-- ============================================================
-- COMENTARIOS DE COLUMNAS - SPACES
-- ============================================================

COMMENT ON COLUMN spaces.id IS 'Identificador único universal (UUID) del espacio. Clave primaria.';
COMMENT ON COLUMN spaces.name IS 'Nombre descriptivo del espacio de cowork (ej: "Sala Ejecutiva", "Auditorio Principal").';
COMMENT ON COLUMN spaces.capacity IS 'Capacidad máxima de personas que pueden usar el espacio simultáneamente. Debe ser mayor a 0.';
COMMENT ON COLUMN spaces.base_hourly_rate IS 'Tarifa base por hora en moneda local. Base para el cálculo de tarifas dinámicas.';
COMMENT ON COLUMN spaces.opening_time IS 'Hora de apertura del espacio. Formato TIME sin zona horaria, se interpreta según time_zone_id.';
COMMENT ON COLUMN spaces.closing_time IS 'Hora de cierre del espacio. Debe ser mayor que opening_time.';
COMMENT ON COLUMN spaces.time_zone_id IS 'Zona horaria IANA (ej: "America/Lima", "America/Mexico_City"). Usada para interpretar opening_time y closing_time.';
COMMENT ON COLUMN spaces.status_id IS 'Estado del espacio: 1=Active (activo/disponible), 2=Maintenance (mantenimiento), 3=Inactive (inactivo/eliminado lógicamente).';
COMMENT ON COLUMN spaces.is_deleted IS 'Soft delete. TRUE indica que el espacio fue eliminado lógicamente. Las reservas históricas se mantienen.';
COMMENT ON COLUMN spaces.created_at IS 'Fecha y hora de creación del registro.';
COMMENT ON COLUMN spaces.updated_at IS 'Fecha y hora de la última actualización del registro. Actualizado automáticamente por trigger.';
COMMENT ON COLUMN spaces.deleted_at IS 'Fecha y hora de la eliminación lógica. Populado solo si is_deleted = TRUE.';
COMMENT ON COLUMN spaces.created_by_user_id IS 'Referencia al usuario que creó este espacio.';
COMMENT ON COLUMN spaces.updated_by_user_id IS 'Referencia al usuario que realizó la última actualización.';
COMMENT ON COLUMN spaces.deleted_by_user_id IS 'Referencia al usuario que realizó la eliminación lógica.';
COMMENT ON COLUMN spaces.version IS 'Número de versión para control de concurrencia optimista. Se incrementa automáticamente en cada UPDATE.';

-- ============================================================
-- COMENTARIOS DE COLUMNAS - RESERVATIONS
-- ============================================================

COMMENT ON COLUMN reservations.id IS 'Identificador único universal (UUID) de la reserva. Clave primaria.';
COMMENT ON COLUMN reservations.reservation_code IS 'Código legible por humanos para identificar la reserva (ej: "COW-20240607-1234"). Único y generado automáticamente.';
COMMENT ON COLUMN reservations.space_id IS 'Referencia al espacio reservado. Un espacio puede tener múltiples reservas no solapadas.';
COMMENT ON COLUMN reservations.customer_id IS 'Cliente que comercialmente posee la reserva. Responsable del pago y asistencia.';
COMMENT ON COLUMN reservations.created_by_user_id IS 'Usuario autenticado del sistema que CREÓ la reserva (puede ser admin, staff o el mismo cliente).';
COMMENT ON COLUMN reservations.updated_by_user_id IS 'Usuario autenticado que realizó la última MODIFICACIÓN de la reserva.';
COMMENT ON COLUMN reservations.cancelled_by_user_id IS 'Usuario que CANCELÓ la reserva. Populado solo cuando status_id cambia a Cancelled.';
COMMENT ON COLUMN reservations.completed_by_user_id IS 'Usuario que marcó la reserva como COMPLETADA. Populado solo cuando status_id cambia a Completed.';
COMMENT ON COLUMN reservations.start_time IS 'Fecha y hora de inicio de la reserva. En UTC (TIMESTAMPTZ). Debe ser menor que end_time.';
COMMENT ON COLUMN reservations.end_time IS 'Fecha y hora de fin de la reserva. En UTC (TIMESTAMPTZ). Debe ser mayor que start_time.';
COMMENT ON COLUMN reservations.status_id IS 'Estado actual de la reserva: 1=Pending (pendiente), 2=Confirmed (confirmada), 3=Cancelled (cancelada), 4=Completed (completada).';
COMMENT ON COLUMN reservations.base_amount IS 'Monto calculado como tarifa_base_hora × horas. Sin aplicar reglas de tarifas dinámicas.';
COMMENT ON COLUMN reservations.final_amount IS 'Monto final después de aplicar todas las reglas de tarifas dinámicas (hora pico, fin de semana, descuentos).';
COMMENT ON COLUMN reservations.refund_amount IS 'Monto reembolsado al cliente en caso de cancelación. Depende de la antelación de la cancelación.';
COMMENT ON COLUMN reservations.pricing_breakdown IS 'Objeto JSON que desglosa el cálculo del precio final. Incluye cada regla aplicada y su impacto en el monto.';
COMMENT ON COLUMN reservations.created_at IS 'Fecha y hora de creación de la reserva. Timestamp de cuando se insertó el registro.';
COMMENT ON COLUMN reservations.updated_at IS 'Fecha y hora de la última actualización del registro. Actualizado automáticamente por trigger.';
COMMENT ON COLUMN reservations.cancelled_at IS 'Fecha y hora de cancelación. Populado cuando status_id cambia a Cancelled.';
COMMENT ON COLUMN reservations.completed_at IS 'Fecha y hora de completado. Populado cuando status_id cambia a Completed.';
COMMENT ON COLUMN reservations.version IS 'Número de versión para control de concurrencia optimista. Se incrementa automáticamente en cada UPDATE.';

-- ============================================================
-- COMENTARIOS DE COLUMNAS - AUDIT_LOGS
-- ============================================================

COMMENT ON COLUMN audit_logs.id IS 'Identificador único universal (UUID) del registro de auditoría. Clave primaria.';
COMMENT ON COLUMN audit_logs.event_type IS 'Tipo de evento de auditoría (ej: "CREATE", "UPDATE", "DELETE", "CANCEL", "CONFIRM", "COMPLETE").';
COMMENT ON COLUMN audit_logs.entity_type IS 'Nombre de la entidad afectada (ej: "Reservation", "Space", "Customer", "AppUser").';
COMMENT ON COLUMN audit_logs.entity_id IS 'Identificador UUID de la entidad afectada. Permite rastrear el historial completo de una entidad específica.';
COMMENT ON COLUMN audit_logs.actor_user_id IS 'Usuario autenticado del sistema que realizó la acción. Populado cuando la acción la hace un usuario con cuenta.';
COMMENT ON COLUMN audit_logs.actor_customer_id IS 'Cliente asociado a la acción (cuando la acción la realiza un cliente no autenticado o como cliente).';
COMMENT ON COLUMN audit_logs.action IS 'Acción realizada (ej: "ReservationCreated", "ReservationCancelled", "PriceUpdated").';
COMMENT ON COLUMN audit_logs.description IS 'Descripción legible de la acción. Máximo 300 caracteres.';
COMMENT ON COLUMN audit_logs.old_values IS 'Estado anterior de la entidad o de los campos relevantes en formato JSON.';
COMMENT ON COLUMN audit_logs.new_values IS 'Nuevo estado de la entidad o de los campos relevantes en formato JSON.';
COMMENT ON COLUMN audit_logs.metadata IS 'Metadatos adicionales del evento como: IP de origen, ID de solicitud, contexto de ejecución, etc.';
COMMENT ON COLUMN audit_logs.ip_address IS 'Dirección IP desde la cual se realizó la acción. Útil para auditoría de seguridad.';
COMMENT ON COLUMN audit_logs.user_agent IS 'User Agent del navegador o cliente que realizó la solicitud.';
COMMENT ON COLUMN audit_logs.created_at IS 'Fecha y hora en que se registró el evento de auditoría.';

-- ============================================================
-- COMENTARIOS DE RESTRICCIONES (CONSTRAINTS)
-- ============================================================

COMMENT ON CONSTRAINT uq_app_users_username ON app_users IS 'Garantiza que cada username sea único en el sistema.';
COMMENT ON CONSTRAINT uq_app_users_customer_id ON app_users IS 'Un cliente puede tener como máximo una cuenta de usuario. Permite NULL para admin/staff.';
COMMENT ON CONSTRAINT chk_app_users_customer_role ON app_users IS 'Regla de negocio: si rol es Customer (3), debe tener customer_id. Si es Admin o Staff, customer_id debe ser NULL.';
COMMENT ON CONSTRAINT chk_reservations_min_duration ON reservations IS 'Garantiza que la reserva cumpla la duración mínima de 30 minutos según requerimiento funcional.';
COMMENT ON CONSTRAINT chk_reservations_max_duration ON reservations IS 'Garantiza que la reserva cumpla la duración máxima de 8 horas según requerimiento funcional.';
COMMENT ON CONSTRAINT excl_reservations_no_overlap ON reservations IS 'Restricción de exclusión a nivel base de datos que previene solapamiento de reservas activas (no canceladas) para el mismo espacio. Es la última línea de defensa contra overbooking.';

-- ============================================================
-- COMENTARIOS DE ÍNDICES
-- ============================================================

COMMENT ON INDEX idx_reservations_space_time IS 'Índice crítico para consultas de disponibilidad. Optimiza la búsqueda de reservas por espacio y rango de tiempo.';
COMMENT ON INDEX idx_reservations_customer IS 'Optimiza consultas de historial de reservas por cliente. Útil para reportes y vista "Mis Reservas".';
COMMENT ON INDEX idx_reservations_status IS 'Optimiza filtrado por estado de reserva (pendientes, confirmadas, canceladas).';
COMMENT ON INDEX uq_customers_email_not_deleted IS 'Índice único parcial que permite emails únicos solo para clientes no eliminados. Clientes eliminados pueden reutilizar el email.';
COMMENT ON INDEX uq_spaces_name_not_deleted IS 'Índice único parcial que evita nombres duplicados en espacios activos. Espacios eliminados pueden reutilizar el nombre.';
COMMENT ON INDEX idx_audit_logs_entity IS 'Optimiza la búsqueda del historial completo de una entidad específica (ej: todas las auditorías de una reserva).';

-- ============================================================
-- COMENTARIOS DE FUNCIONES Y TRIGGERS
-- ============================================================

COMMENT ON FUNCTION set_updated_at_and_version() IS 'Función de trigger que actualiza automáticamente updated_at e incrementa el contador de versión (optimistic concurrency) en cada UPDATE.';
COMMENT ON TRIGGER trg_app_users_updated_at_version ON app_users IS 'Trigger que mantiene updated_at y version para la tabla app_users.';
COMMENT ON TRIGGER trg_customers_updated_at_version ON customers IS 'Trigger que mantiene updated_at y version para la tabla customers.';
COMMENT ON TRIGGER trg_spaces_updated_at_version ON spaces IS 'Trigger que mantiene updated_at y version para la tabla spaces.';
COMMENT ON TRIGGER trg_reservations_updated_at_version ON reservations IS 'Trigger que mantiene updated_at y version para la tabla reservations.';