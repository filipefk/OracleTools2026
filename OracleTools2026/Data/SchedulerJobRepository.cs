using OracleTools2026.Models;

namespace OracleTools2026.Data;

public sealed class SchedulerJobRepository(IOracleSession session)
{
    // As comparações com SYSTIMESTAMP são feitas no banco para respeitar o fuso horário de cada data
    // (o original comparava no cliente com o SYSDATE). As datas exibidas ficam no fuso da sessão.
    public async Task<IReadOnlyList<SchedulerJob>> GetJobsAsync(CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT O.CREATED,
                   O.LAST_DDL_TIME,
                   J.OWNER,
                   J.JOB_NAME,
                   J.JOB_ACTION,
                   J.ENABLED,
                   J.JOB_TYPE,
                   J.PROGRAM_OWNER,
                   J.PROGRAM_NAME,
                   J.SCHEDULE_OWNER,
                   J.SCHEDULE_NAME,
                   J.SCHEDULE_TYPE,
                   J.REPEAT_INTERVAL,
                   CAST(J.LAST_START_DATE AT TIME ZONE SESSIONTIMEZONE AS DATE) AS LAST_START_DATE,
                   J.LAST_RUN_DURATION,
                   CAST(J.NEXT_RUN_DATE AT TIME ZONE SESSIONTIMEZONE AS DATE) AS NEXT_RUN_DATE,
                   J.RUN_COUNT,
                   J.FAILURE_COUNT,
                   J.COMMENTS,
                   J.STATE,
                   CASE WHEN J.NEXT_RUN_DATE < SYSTIMESTAMP - INTERVAL '1' MINUTE THEN 1 ELSE 0 END AS PAST_NEXT_RUN_DATE,
                   CASE WHEN J.LAST_START_DATE < SYSTIMESTAMP - INTERVAL '1' HOUR THEN 1 ELSE 0 END AS STARTED_OVER_AN_HOUR_AGO
            FROM ALL_SCHEDULER_JOBS J
                 INNER JOIN ALL_OBJECTS O
                    ON O.OWNER = J.OWNER
                   AND O.OBJECT_NAME = J.JOB_NAME
                   AND O.OBJECT_TYPE = 'JOB'
            ORDER BY J.ENABLED DESC, J.LAST_START_DATE DESC NULLS LAST
            """;

        await using var connection = await session.OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateTextCommand(sql);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var jobs = new List<SchedulerJob>();
        while (await reader.ReadAsync(cancellationToken))
        {
            jobs.Add(new SchedulerJob(
                reader.GetDateTimeOrNull(0),
                reader.GetDateTimeOrNull(1),
                reader.GetStringOrEmpty(2),
                reader.GetStringOrEmpty(3),
                reader.GetStringOrEmpty(4),
                reader.GetStringOrEmpty(5).Equals("TRUE", StringComparison.OrdinalIgnoreCase),
                reader.GetStringOrEmpty(6),
                reader.GetStringOrEmpty(7),
                reader.GetStringOrEmpty(8),
                reader.GetStringOrEmpty(9),
                reader.GetStringOrEmpty(10),
                reader.GetStringOrEmpty(11),
                reader.GetStringOrEmpty(12),
                reader.GetDateTimeOrNull(13),
                reader.IsDBNull(14) ? null : reader.GetTimeSpan(14),
                reader.GetDateTimeOrNull(15),
                reader.GetInt64OrZero(16),
                reader.GetInt64OrZero(17),
                reader.GetStringOrEmpty(18),
                reader.GetStringOrEmpty(19),
                reader.GetInt64OrZero(20) == 1,
                reader.GetInt64OrZero(21) == 1));
        }

        return jobs;
    }
}
