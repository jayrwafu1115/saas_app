using MediatR;

namespace Clinic.Application.Clinical.Commands;

public sealed record SignEncounterCommand(Guid EncounterId) : IRequest<EncounterDto>;
