﻿using StockportGovUK.NetStandard.Gateways.Enums;
using StockportGovUK.NetStandard.Gateways.Models.Fostering.Application;

namespace fostering_service.Services.Application
{
    public interface IApplicationService
    {
        Task UpdateStatus(string caseId, ETaskStatus status, EFosteringApplicationForm form);

        Task<ETaskStatus> UpdateGpDetails(FosteringCaseGpDetailsUpdateModel model);

        Task<ETaskStatus> UpdateReferences(FosteringCaseReferenceUpdateModel model);

        Task<ETaskStatus> UpdateCouncillorsDetails(FosteringCaseCouncillorsUpdateModel model);

        Task<ETaskStatus> UpdateAddressHistory(FosteringCaseAddressHistoryUpdateModel model);
    }
}
