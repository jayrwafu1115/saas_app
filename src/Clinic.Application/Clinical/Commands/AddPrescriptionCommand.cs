using MediatR;

namespace Clinic.Application.Clinical.Commands;

public sealed record AddPrescriptionCommand(
    Guid EncounterId,
    string MedicationName,
    string Dosage,
    string Frequency,
    string Duration,
    string? Instructions) : IRequest<PrescriptionDto>;
