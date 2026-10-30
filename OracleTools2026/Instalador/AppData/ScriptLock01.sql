-- Consulta 1: Detalhes dos locks ativos
-- Origem: app/rest/server/v1/monitora/index.php (Monitoramento::monitoraLockTable)
SELECT
    vs.audsid audsid,
    to_char(locks.sid) sid,
    to_char(vs.serial#) serial,
    vs.username oracle_user,
    vs.osuser os_user,
    vs.program program,
    vs.module module,
    vs.action action,
    vs.process process,
    decode(locks.lmode,
           1, NULL,
           2, 'Row Share',
           3, 'Row Exclusive',
           4, 'Share',
           5, 'Share Row Exclusive',
           6, 'Exclusive', 'None') lock_mode_held,
    decode(locks.request,
           1, NULL,
           2, 'Row Share',
           3, 'Row Exclusive',
           4, 'Share',
           5, 'Share Row Exclusive',
           6, 'Exclusive', 'None') lock_mode_requested,
    decode(locks.type,
           'MR', 'Media Recovery',
           'RT', 'Redo Thread',
           'UN', 'User Name',
           'TX', 'Transaction',
           'TM', 'DML',
           'UL', 'PL/SQL User Lock',
           'DX', 'Distributed Xaction',
           'CF', 'Control File',
           'IS', 'Instance State',
           'FS', 'File Set',
           'IR', 'Instance Recovery',
           'ST', 'Disk Space Transaction',
           'TS', 'Temp Segment',
           'IV', 'Library Cache Invalidation',
           'LS', 'Log Start or Log Switch',
           'RW', 'Row Wait',
           'SQ', 'Sequence Number',
           'TE', 'Extend Table',
           'TT', 'Temp Table',
           locks.type) lock_type,
    objs.owner object_owner,
    objs.object_name object_name,
    objs.object_type object_type,
    round( locks.ctime/60, 2 ) lock_time_in_minutes
FROM v$session vs,
     v$lock locks,
     all_objects objs,
     all_tables tbls
WHERE locks.id1 = objs.object_id
 AND vs.sid = locks.sid
 AND objs.owner = tbls.owner
 AND objs.object_name =  tbls.table_name
 AND objs.owner != 'SYS'
 AND locks.type = 'TM'
 AND username <> 'MAXICON'
 AND round( locks.ctime/60, 2 ) > 1
ORDER BY lock_time_in_minutes DESC;
