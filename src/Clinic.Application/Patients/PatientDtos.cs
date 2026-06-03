using Clinic.Application.Common.Models;

namespace Clinic.Application.Patients;

public sealed record PatientDto(
    Guid Id,
    Guid TenantId,
    Guid LocationId,
    string MedicalRecordNumber,
    string FirstName,
    string MiddleName,
    string LastName,
    DateOnly BirthDate,
    string Gender,
    string Email,
    string Phone,
    string Address);

public sealed record PatientDetailDto(
    Guid Id,
    Guid TenantId,
    Guid LocationId,
    string MedicalRecordNumber,
    string FirstName,
    string MiddleName,
    string LastName,
    DateOnly BirthDate,
    string Gender,
    string Email,
    string Phone,
    string Address,
    IReadOnlyList<PatientContactDto> Contacts,
    IReadOnlyList<PatientDocumentDto> Documents);

public sealed record PatientContactDto(
    Guid Id,
    Guid PatientId,
    string Name,
    string Relationship,
    string Email,
    string Phone,
    bool IsPrimary);

public sealed record PatientDocumentDto(
    Guid Id,
    Guid PatientId,
    string FileName,
    string ContentType,
    long SizeBytes,
    string ObjectKey,
    DateTimeOffset UploadedAtUtc);

public sealed record PatientTimelineEventDto(
    DateTimeOffset OccurredAtUtc,
    string Type,
    string Title,
    string Description);

public sealed record PatientSearchResult(PagedResult<PatientDto> Patients);
