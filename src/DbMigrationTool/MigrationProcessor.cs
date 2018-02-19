using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Dapper;
using DbMigrationTool.Migrations;
using Microsoft.Extensions.Configuration;

namespace DbMigrationTool
{
    public enum Mode
    {
        Run = 1,
        Rollback = 2
    }

    public class MigrationProcessor
    {
        private readonly string _connectionString = Program.Configuration.GetConnectionString("ConnectionStrings:DefaultConnection");
        private const string MigrationsSequence = "MigrationsSequence.xml";

        public MigrationProcessor()
        {
            using (IDbConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                conn.Execute(@"
IF (NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES 
    WHERE TABLE_SCHEMA = 'dbo' 
    AND  TABLE_NAME = '__Migration'))
BEGIN
    CREATE TABLE __Migration(
        Name NVARCHAR(100) NOT NULL,
        Hash NVARCHAR(100) NOT NULL,
        AppliedDateTime DATETIME NOT NULL,
        DurationInSeconds INT NOT NULL,
        IsOk BIT NOT NULL,
        Mode TINYINT NOT NULL,
        Error VARCHAR(MAX) NULL,
        CONSTRAINT PK_Name__Migration PRIMARY KEY (Name),
        CONSTRAINT UX_Name_Hash__Migration UNIQUE NONCLUSTERED (Name, Hash)
    )
END");
            }
        }

        public void Process(Mode mode)
        {
            IDictionary<string, Type> types = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && 
                    !string.IsNullOrEmpty(t.Namespace) && t.Namespace.EndsWith(".Migrations"))
                .ToDictionary(x => x.Name, x => x);

            foreach (var migration in GetMigrationSequence())
            {
                if (!types.ContainsKey(migration.Key))
                {
                    continue;
                }
                ((BaseMigration)Activator.CreateInstance(types[migration.Key])).Execute(migration.Key, migration.Value, mode);
            }
        }

        private KeyValuePair<string, string>[] GetMigrationSequence()
        {
            string workFolder = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            var doc = XDocument.Load(Path.Combine(workFolder, MigrationsSequence));
            return doc.Descendants("migrations").Elements("migration")
                .Select(m => new KeyValuePair<string, string>(m.Attribute("name").Value, m.Attribute("hash").Value))
                .ToArray();
        }
    }
}
