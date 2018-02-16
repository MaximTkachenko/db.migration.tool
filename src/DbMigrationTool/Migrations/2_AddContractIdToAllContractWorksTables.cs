using System.Data;
using Dapper;

namespace DbMigrationTool.Migrations
{
    public class AddContractIdToAllContractWorksTables : BaseMigration
    {
        private const string ContractId = "ContractId";
        private const string FkContractIdWork = "FK_ContractId_Work";
        private const string FkContractIdDistribution = "FK_ContractId_Distribution";
        private const string FkContractIdAmount = "FK_ContractId_Amount";

        protected override void RunMigration(IDbConnection conn, IDbTransaction tran)
        {
            conn.Execute($@"
ALTER TABLE Work
ADD {ContractId} INT NULL
CONSTRAINT {FkContractIdWork} FOREIGN KEY({ContractId}) REFERENCES Contract(Id)", null, tran);
            conn.Execute($@"
ALTER TABLE Distribution
ADD {ContractId} INT NULL
CONSTRAINT {FkContractIdDistribution} FOREIGN KEY({ContractId}) REFERENCES Contract(Id)", null, tran);
            conn.Execute($@"
ALTER TABLE Amount
ADD {ContractId} INT NULL
CONSTRAINT {FkContractIdAmount} FOREIGN KEY({ContractId}) REFERENCES Contract(Id)", null, tran);

            conn.Execute($@"
UPDATE w 
SET w.{ContractId} = s.{ContractId}
FROM Work w
JOIN Stage s
ON w.StageId = s.Id", null, tran);
            conn.Execute($@"
UPDATE d 
SET d.{ContractId} = w.{ContractId}
FROM Distribution d
JOIN Work w
ON d.WorkId = w.Id", null, tran);
            conn.Execute($@"
UPDATE a 
SET a.{ContractId} = d.{ContractId}
FROM Amount a
JOIN Distribution d
ON a.DistributionId = d.Id", null, tran);

            conn.Execute($"ALTER TABLE Work ALTER COLUMN {ContractId} INT NOT NULL", null, tran);
            conn.Execute($"ALTER TABLE Distribution ALTER COLUMN {ContractId} INT NOT NULL", null, tran);
            conn.Execute($"ALTER TABLE Amount ALTER COLUMN {ContractId} INT NOT NULL", null, tran);
        }

        protected override void RollbackMigration(IDbConnection conn, IDbTransaction tran)
        {
            conn.Execute($@"
ALTER TABLE Work
DROP CONSTRAINT {FkContractIdWork}", null, tran);
            conn.Execute($@"
ALTER TABLE Distribution
DROP CONSTRAINT {FkContractIdDistribution}", null, tran);
            conn.Execute($@"
ALTER TABLE Amount
DROP CONSTRAINT {FkContractIdAmount}", null, tran);

            conn.Execute($@"
ALTER TABLE Work
DROP COLUMN {ContractId}", null, tran);
            conn.Execute($@"
ALTER TABLE Distribution
DROP COLUMN {ContractId}", null, tran);
            conn.Execute($@"
ALTER TABLE Amount
DROP COLUMN {ContractId}", null, tran);
        }
    }
}
