using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using Clinic.Application.Clinical;
using Clinic.Application.Common.Interfaces;
using Clinic.Domain.AI;

namespace Clinic.Application.AI;

public sealed class AIService(
    IAIGenerationRepository generations,
    IEncounterRepository encounters,
    IAIProviderFactory providerFactory,
    IAIGenerationQueue queue,
    IAIResponseCache cache,
    IDateTimeProvider dateTimeProvider,
    ICurrentUser currentUser)
    : IAIService
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromHours(12);

    public async Task<AIGenerationDto> QueueGenerationAsync(Guid encounterId, AIGenerationType type, string provider, string? model, CancellationToken cancellationToken)
    {
        var encounter = await encounters.GetByIdAsync(encounterId, cancellationToken);
        if (encounter is null)
        {
            throw new KeyNotFoundException("Encounter was not found.");
        }

        var prompt = BuildPrompt(type, encounter.ToDetailDto());
        var promptHash = ComputeHash($"{provider}:{model}:{type}:{prompt}");
        var generation = new AIGeneration(encounter.TenantId, encounter.Id, type, provider, model ?? string.Empty, promptHash, prompt, currentUser.UserId);
        await generations.AddAsync(generation, cancellationToken);
        await queue.EnqueueAsync(generation.Id, cancellationToken);
        return generation.ToDto();
    }

    public async Task ProcessGenerationAsync(Guid generationId, CancellationToken cancellationToken)
    {
        var generation = await generations.GetByIdAsync(generationId, cancellationToken);
        if (generation is null || generation.Status is AIGenerationStatus.Completed or AIGenerationStatus.Processing)
        {
            return;
        }

        generation.MarkProcessing(dateTimeProvider.UtcNow);
        await generations.SaveChangesAsync(cancellationToken);

        try
        {
            var cacheKey = $"ai:{generation.Provider}:{generation.Type}:{generation.PromptHash}";
            var cached = await cache.GetAsync(cacheKey, cancellationToken);
            if (!string.IsNullOrWhiteSpace(cached))
            {
                generation.Complete(cached, 0, 0, 0, 0, dateTimeProvider.UtcNow, true);
                await generations.SaveChangesAsync(cancellationToken);
                return;
            }

            var provider = providerFactory.GetProvider(generation.Provider);
            var stopwatch = Stopwatch.StartNew();
            var response = await provider.GenerateAsync(generation.Type, generation.Prompt, generation.Model, cancellationToken);
            stopwatch.Stop();

            var latency = stopwatch.ElapsedMilliseconds;
            generation.Complete(response.Output, response.PromptTokens, response.CompletionTokens, response.CostUsd, latency, dateTimeProvider.UtcNow, false);
            await cache.SetAsync(cacheKey, response.Output, CacheTtl, cancellationToken);
            await generations.SaveChangesAsync(cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            generation.Fail(exception.Message, dateTimeProvider.UtcNow);
            await generations.SaveChangesAsync(cancellationToken);
        }
    }

    private static string BuildPrompt(AIGenerationType type, EncounterDetailDto encounter)
    {
        var diagnoses = string.Join("; ", encounter.Diagnoses.Select(diagnosis => $"{diagnosis.Code} {diagnosis.Description}"));
        var prescriptions = string.Join("; ", encounter.Prescriptions.Select(prescription => $"{prescription.MedicationName} {prescription.Dosage} {prescription.Frequency} for {prescription.Duration}"));
        var vitals = string.Join("; ", encounter.Vitals.Select(vital => $"Temp {vital.TemperatureCelsius}C BP {vital.SystolicBloodPressure}/{vital.DiastolicBloodPressure} HR {vital.HeartRate} SpO2 {vital.OxygenSaturation}"));
        var task = type switch
        {
            AIGenerationType.SoapNote => "Generate a polished SOAP note.",
            AIGenerationType.ClinicalSummary => "Generate a concise clinical summary.",
            AIGenerationType.PrescriptionSummary => "Generate a patient-friendly prescription summary.",
            AIGenerationType.VisitSummary => "Generate a patient-friendly visit summary.",
            _ => "Generate a clinical summary."
        };

        return $"""
        {task}

        Chief complaint: {encounter.ChiefComplaint}
        Subjective: {encounter.Subjective}
        Objective: {encounter.Objective}
        Assessment: {encounter.Assessment}
        Plan: {encounter.Plan}
        Notes: {encounter.Notes}
        Vitals: {vitals}
        Diagnoses: {diagnoses}
        Prescriptions: {prescriptions}

        Return only the generated clinical content.
        """;
    }

    private static string ComputeHash(string value)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
