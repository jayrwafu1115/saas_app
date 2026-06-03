# Patient Management API

All patient endpoints require a bearer token with the `patients.manage` permission.

## Patients

- `GET /api/patients?tenantId=&locationId=&search=&pageNumber=1&pageSize=20`
  - Returns a paged patient list.
  - Search matches medical record number, first name, last name, email, or phone.
- `GET /api/patients/{id}`
  - Returns patient profile details with contacts and documents.
- `POST /api/patients`
  - Creates a patient.
- `PUT /api/patients/{id}`
  - Updates patient demographics and assigned location.
- `DELETE /api/patients/{id}`
  - Soft deletes a patient.

## Contacts

- `POST /api/patients/{id}/contacts`
  - Creates a patient contact.
- `PUT /api/patients/{id}/contacts/{contactId}`
  - Updates a patient contact.
- `DELETE /api/patients/{id}/contacts/{contactId}`
  - Soft deletes a patient contact.

## Documents

- `POST /api/patients/{id}/documents`
  - Accepts `multipart/form-data` with a `file` field.
  - Uploads the file to MinIO and stores document metadata.

## Timeline

- `GET /api/patients/{id}/timeline`
  - Returns patient-created, contact-added, and document-uploaded events ordered newest first.
