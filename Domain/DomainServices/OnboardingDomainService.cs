using RudFitAI.Domain.Enums;

namespace RudFitAI.Domain.DomainServices;

public sealed class OnboardingDomainService
{
    public void EnsureValidAnswers(
        GoalType goal,
        GenderType gender,
        int age,
        decimal height,
        decimal weight,
        decimal startingWeight,
        decimal targetWeight,
        ActivityLevelType activityLevel,
        int dailyRoutineLevel,
        int goalIntensity)
    {
        if (!Enum.IsDefined(goal))
        {
            throw new ArgumentException("Valor de objetivo inválido.", nameof(goal));
        }

        if (!Enum.IsDefined(gender))
        {
            throw new ArgumentException("Valor de gênero inválido.", nameof(gender));
        }

        if (!Enum.IsDefined(activityLevel))
        {
            throw new ArgumentException("Valor de nível de atividade inválido.", nameof(activityLevel));
        }

        if (age < 12 || age > 100)
        {
            throw new ArgumentException("Idade deve estar entre 12 e 100 anos.", nameof(age));
        }

        if (height < 100m || height > 250m)
        {
            throw new ArgumentException("Altura deve estar entre 100 e 250 cm.", nameof(height));
        }

        if (weight <= 0m)
        {
            throw new ArgumentException("Peso deve ser maior que zero.", nameof(weight));
        }

        if (startingWeight <= 0m)
        {
            throw new ArgumentException("Peso inicial deve ser maior que zero.", nameof(startingWeight));
        }

        if (targetWeight <= 0m)
        {
            throw new ArgumentException("Peso alvo deve ser maior que zero.", nameof(targetWeight));
        }

        if (dailyRoutineLevel < 1 || dailyRoutineLevel > 4)
        {
            throw new ArgumentException("Nível de rotina diária deve estar entre 1 e 4.", nameof(dailyRoutineLevel));
        }

        if (goalIntensity < 1 || goalIntensity > 3)
        {
            throw new ArgumentException("Intensidade do objetivo deve estar entre 1 e 3.", nameof(goalIntensity));
        }
    }
}
