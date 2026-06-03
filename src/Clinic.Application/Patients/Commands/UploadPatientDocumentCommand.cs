using MediatR;

namespace Clinic.Application.Patients.Commands;

public sealed record UploadPatientDocumentCommand(
    Guid PatientId,
    string FileName,
    string ContentType,
    long SizeBytes,
    Stream Content) : IRequest<PatientDocumentDto>;
