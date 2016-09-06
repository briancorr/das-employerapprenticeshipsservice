﻿using System.Data;
using System.Threading.Tasks;
using Dapper;
using SFA.DAS.EmployerApprenticeshipsService.Domain.Configuration;
using SFA.DAS.EmployerApprenticeshipsService.Domain.Data;

namespace SFA.DAS.EmployerApprenticeshipsService.TestCommon.DbCleanup
{
    public class CleanDatabase : BaseRepository, ICleanDatabase
    {
        public CleanDatabase(EmployerApprenticeshipsServiceConfiguration configuration) : base(configuration)
        {
        }

        public async Task Execute()
        {
            var parameters = new DynamicParameters();
            parameters.Add("@INCLUDEUSERTABLE", 1, DbType.Int16);
            await WithConnection<int>(async c => await c.ExecuteAsync(
                "[account].[Cleardown]",
                parameters,
                commandType: CommandType.StoredProcedure));

            await WithConnection<int>(async c => await c.ExecuteAsync(
                "[account].[SeedDataForRoles]",
                null,
                commandType: CommandType.StoredProcedure));

        }
    }
}