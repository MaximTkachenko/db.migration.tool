using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using Dapper;

namespace DbMigrationTool.Migrations
{
    public abstract class BaseMigration
    {
        public void Execute(string migrationName, string migrationHash, Mode mode)
        {
            Console.WriteLine($"{migrationName} started");

            DbMigration migration = GetMigrationByName(migrationName);
            if (migration != null && migration.Hash == migrationHash && migration.IsOk && (Mode)migration.Mode == mode)
            {
                Console.WriteLine("skipped");
                return;
            }

            if (migration == null)
            {
                migration = new DbMigration { Name = migrationName };
            }
            migration.Hash = migrationHash;
            migration.Mode = (int) mode;

            var sw = Stopwatch.StartNew();
            using (IDbConnection conn = new SqlConnection(Program.ConnectionString))
            {
                conn.Open();
                using (IDbTransaction tran = conn.BeginTransaction())
                {
                    try
                    {
                        if (mode == Mode.Run)
                        {
                            RunMigration(conn, tran);
                        }
                        else
                        {
                            RollbackMigration(conn, tran);
                        }

                        tran.Commit();
                        sw.Stop();
                        migration.SetResult(sw.Elapsed.Seconds);
                    }
                    catch (Exception ex)
                    {
                        tran.Rollback();
                        sw.Stop();
                        migration.SetResult(sw.Elapsed.Seconds, ex);
                    }
                    finally
                    {
                        AddOrUpdateMigration(migration);
                        Console.WriteLine(migration.IsOk ? "succeeded" : "failed");
                    }
                }
            }
        }

        protected abstract void RunMigration(IDbConnection conn, IDbTransaction tran);

        protected abstract void RollbackMigration(IDbConnection conn, IDbTransaction tran);

        private DbMigration GetMigrationByName(string name)
        {
            using (IDbConnection conn = new SqlConnection(Program.ConnectionString))
            {
                conn.Open();
                return conn.Query<DbMigration>("SELECT * FROM __Migration WHERE Name = @name", new { name }).FirstOrDefault();
            }
        }

        private void AddOrUpdateMigration(DbMigration migration)
        {
            using (IDbConnection conn = new SqlConnection(Program.ConnectionString))
            {
                conn.Open();
                if (migration.IsNew)
                {
                    conn.Execute(@"
INSERT INTO __Migration 
(Name, Hash, AppliedDateTime, DurationInSeconds, IsOk, Mode, Error)
VALUES (@Name, @Hash, @AppliedDateTime, @DurationInSeconds, @IsOk, @Mode, @Error)", migration);
                }
                else
                {
                    conn.Execute(@"
UPDATE __Migration 
SET Hash = @Hash,
AppliedDateTime = @AppliedDateTime,
DurationInSeconds = @DurationInSeconds,
IsOk = @IsOk,
Mode = @Mode,
Error = @Error
WHERE Name = @Name", migration);
                }
            }
        }
    }
}
