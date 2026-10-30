-- Consulta 2: Bloqueadores e bloqueados
-- Origem: app/rest/server/v1/monitora/index.php (Monitoramento::monitoraLockTable)
SELECT
    blocker.username AS blocking_user,
    blocker.machine AS blocking_machine,
    blocker.program AS blocking_program,
    blocker.sid AS blocking_sid,
    blocker.serial# AS blocking_serial,
    blocked.username AS blocked_user,
    blocked.machine AS blocked_machine,
    blocked.program AS blocked_program,
    blocked.sid AS blocked_sid,
    blocked.serial# AS blocked_serial,
    do.object_name,
    do.object_type
FROM
    v$lock blocker_lock,
    v$lock blocked_lock,
    v$session blocker,
    v$session blocked,
    v$locked_object lo,
    dba_objects do
WHERE
    blocker_lock.block = 1
    AND blocked_lock.request > 0
    AND blocker_lock.id1 = blocked_lock.id1
    AND blocker_lock.id2 = blocked_lock.id2
    AND blocker.sid = blocker_lock.sid
    AND blocked.sid = blocked_lock.sid
    AND lo.session_id = blocker.sid
    AND do.object_id = lo.object_id;
