using FluentValidation;

namespace Clinic.Application.AI.Commands;

public sealed class QueueAIGenerationCommandValidator : AbstractValidator<QueueAIGenerationCommand>
{
    public QueueAIGenerationCommandValidator()
    {
        RuleFor(command => command.EncounterId).NotEmpty();
        RuleFor(command => command.Type).IsInEnum();
        RuleFor(command => command.Provider).NotEmpty().Must(provider => provider.Equals("openai", StringComparison.OrdinalIgnoreCase) || provider.Equals("ollama", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Provider must be OpenAI or Ollama.");
        RuleFor(command => command.Model).MaximumLength(120);
    }
}
