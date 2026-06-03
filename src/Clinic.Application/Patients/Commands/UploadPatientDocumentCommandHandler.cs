using Clinic.Application.Common.Interfaces;
using Clinic.Domain.Patients;
using MediatR;

namespace Clinic.Application.Patients.Commands;

public sealed class UploadPatientDocumentCommandHandler(
    IPatientRepository patients,
    IObjectStorageService storage,
    IDateTimeProvider dateTimeProvider)
    : IRequestHandler<UploadPatientDocumentCommand, PatientDocumentDto>
{
    public async Task<PatientDocumentDto> Handle(UploadPatientDocumentCommand request, CancellationToken cancellationToken)
    {
        var patient = await patients.GetByIdAsync(request.PatientId, cancellationToken);
        if (patient is null)
        {
            throw new KeyNotFoundException("Patient was not found.");
        }

        var safeFileName = Path.GetFileName(request.FileName);
        var objectKey = $"{patient.TenantId}/patients/{patient.Id}/documents/{Guid.NewGuid()}-{safeFileName}";
        var storedObject = await storage.UploadAsync(
            objectKey,
            request.Content,
            request.SizeBytes,
            request.ContentType,
            cancellationToken);

        var document = new PatientDocument(
            patient.Id,
            safeFileName,
            request.ContentType,
            request.SizeBytes,
            storedObject.ObjectKey,
            dateTimeProvider.UtcNow);

        await patients.AddDocumentAsync(document, cancellationToken);
        return document.ToDto();
    }
}
