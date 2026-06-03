using FluentValidation;

namespace Clinic.Application.Clinical.Commands;

public sealed class AddVitalCommandValidator : AbstractValidator<AddVitalCommand>
{
    public AddVitalCommandValidator()
    {
        RuleFor(command => command.EncounterId).NotEmpty();
        RuleFor(command => command.RecordedAtUtc).NotEmpty();
        RuleFor(command => command.TemperatureCelsius).InclusiveBetween(25, 45).When(command => command.TemperatureCelsius.HasValue);
        RuleFor(command => command.SystolicBloodPressure).InclusiveBetween(50, 260).When(command => command.SystolicBloodPressure.HasValue);
        RuleFor(command => command.DiastolicBloodPressure).InclusiveBetween(30, 180).When(command => command.DiastolicBloodPressure.HasValue);
        RuleFor(command => command.HeartRate).InclusiveBetween(20, 240).When(command => command.HeartRate.HasValue);
        RuleFor(command => command.RespiratoryRate).InclusiveBetween(5, 80).When(command => command.RespiratoryRate.HasValue);
        RuleFor(command => command.OxygenSaturation).InclusiveBetween(50, 100).When(command => command.OxygenSaturation.HasValue);
        RuleFor(command => command.HeightCm).InclusiveBetween(20, 260).When(command => command.HeightCm.HasValue);
        RuleFor(command => command.WeightKg).InclusiveBetween(1, 500).When(command => command.WeightKg.HasValue);
        RuleFor(command => command.Notes).MaximumLength(1000);
    }
}
