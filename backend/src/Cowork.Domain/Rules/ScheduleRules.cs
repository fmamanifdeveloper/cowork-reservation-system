namespace Cowork.Domain.Rules;

public static class ScheduleRules
{
    public static bool IsThirtyMinuteStep(TimeOnly value)
    {
        return value.Minute is 0 or 30 &&
               value.Second == 0 &&
               value.Millisecond == 0;
    }

    public static bool IsThirtyMinuteStep(DateTimeOffset value)
    {
        return value.Minute is 0 or 30 &&
               value.Second == 0 &&
               value.Millisecond == 0;
    }
}