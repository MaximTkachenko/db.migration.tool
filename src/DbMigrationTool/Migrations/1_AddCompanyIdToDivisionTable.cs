using System.Data;
using Dapper;

namespace DbMigrationTool.Migrations
{
    public class AddCompanyIdToDivisionTable : BaseMigration
    {
        private const string FkCompanyIdDivision = "FK_CompanyId_Division";
        private const string CompanyId = "CompanyId";

        protected override void RunMigration(IDbConnection conn, IDbTransaction tran)
        {
            conn.Execute($@"
ALTER TABLE Division
ADD {CompanyId} INT NULL
CONSTRAINT {FkCompanyIdDivision} FOREIGN KEY({CompanyId}) REFERENCES Company(Id)", null, tran);
            
                conn.Execute($@"
UPDATE d 
SET d.{CompanyId} = csi.CompanyId
FROM Division d
JOIN CompanyStructureItem csi
ON d.Id = csi.DivisionId
WHERE csi.IsDirect = 1", null, tran);
        }

        protected override void RollbackMigration(IDbConnection conn, IDbTransaction tran)
        {
            conn.Execute($@"
ALTER TABLE Division
DROP CONSTRAINT {FkCompanyIdDivision}", null, tran);

            conn.Execute($@"
ALTER TABLE Division
DROP COLUMN {CompanyId}", null, tran);
        }
    }
}
