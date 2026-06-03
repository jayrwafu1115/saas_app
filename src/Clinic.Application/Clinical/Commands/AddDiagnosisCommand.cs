using MediatR;

namespace Clinic.Application.Clinical.Commands;

public sealed record AddDiagnosisCommand(Guid EncounterId, string Code, string Description, string Type) : IRequest<DiagnosisDto>;
