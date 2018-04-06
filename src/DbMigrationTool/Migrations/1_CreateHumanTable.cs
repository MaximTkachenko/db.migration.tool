using System.Data;
using Dapper;

namespace DbMigrationTool.Migrations
{
    public class CreateHumanTable : BaseMigration
    {
        protected override void RunMigration(IDbConnection conn, IDbTransaction tran)
        {
            conn.Execute(@"
CREATE TABLE Human(
    Id INT IDENTITY(1,1) NOT NULL,
    Name NVARCHAR(100) NOT NULL,
    BirthDay DATETIME NOT NULL,
    CONSTRAINT PK_Id_Human PRIMARY KEY (Id))", null, tran);
        }

        protected override void RollbackMigration(IDbConnection conn, IDbTransaction tran)
        {
            conn.Execute("DROP TABLE Human", null, tran);
        }
    }
}
