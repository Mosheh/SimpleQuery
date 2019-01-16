using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleQuery.Tests
{
    internal class TestData
    {
        static internal Contract GetContract()
        {
            return new Contract
            {
                ID = 1,
                Status = 5,
                ContractDate = DateTime.Now.Date,
                Proposal = 5,
                BusinessPartner = "C001",
                BusinessPartnerName = "MOISÉS J. MIRANDA",
                TypeContract = 1,
                Undertaking = 2,
                UndertakingName = "Gran Ville",
                UndertakingBlock = "15",
                UndertakingUnit = 21,
                UndertakingUnitName = "GRAN House",
                Property = 5m,
                PropertyRate = 1.2m,
                Classification = 5,
                ClassificationDate = null,
                RenegotiationRate = 2,
                AnticipationRate = 1.5m,
                AnticipationRateTP = 1.6m,
                TypeFineResidue = null,
                FineRateResidue = 2,
                DeliveryDate = new DateTime(2019, 01, 16),
                SignatureDate = DateTime.Now.Date,
                CostCenter = "1.1",
                ContractOrigin = null,
                ProRataLow = true,
                ProRataDiscount = false,
                ProRataLowDelay = null,
                ProRataAnticipation = true,
                DirectComission = 5000,
                TotalComission = 1000,
                GenerateCreeditLetterOnOverPayment = false,
                StatusDate = DateTime.Now.Date,
                Comments = "comments",
                TypeCalcMora = 1,
                DocTotal = 25382000.99m,
                SaleValue = 30000000.00m,
                AssignmentRights = false,
                AdjustmentOnlyCalculateDueDate = true,
                NotGenerateNegativeMonetaryCorrection = true,
                LagOfInterestDay = 0,
                LagOfFine = 0,
                AccPeriodId = 1,
                MonthsOfGrowthForCorrectionYearly = 1,
                AccountNumber = 123,
                Accounted = true,
                InstructionBoE = "payment credit card",
                BlockPaymentWithFutureDate = true,
                CostUnit = 830,
                EnableDiscountPunctuality = true,
                NumberInstallmentsPunctuality = 12,
                RealEstateTransferDays = 10,
                AdministrationRate = 2,
                RentalTransferGuaranteed = false
            };
        }
    }

    

    /// <summary>
    /// Contract entity
    /// </summary>
    public class Contract
    {
        public int ID { get; set; }

        public int? Status { get; set; }

        public DateTime? ContractDate { get; set; }

        public int? Proposal { get; set; }

        public string BusinessPartner { get; set; }

        public string BusinessPartnerName { get; set; }

        public int? TypeContract { get; set; }

        public int? Undertaking { get; set; }

        public string UndertakingName { get; set; }

        public string UndertakingBlock { get; set; }

        public int? UndertakingUnit { get; set; }

        public string UndertakingUnitName { get; set; }

        public decimal? Property { get; set; }

        public decimal? PropertyRate { get; set; }

        public int? Classification { get; set; }

        public DateTime? ClassificationDate { get; set; }

        public decimal? RenegotiationRate { get; set; }

        public decimal? AnticipationRate { get; set; }

        public decimal? AnticipationRateTP { get; set; }

        public int? TypeFineResidue { get; set; }

        public decimal? FineRateResidue { get; set; }

        public DateTime? DeliveryDate { get; set; }

        public DateTime? SignatureDate { get; set; }

        public string CostCenter { get; set; }

        public int? ContractOrigin { get; set; }

        public bool? ProRataLow { get; set; }

        public bool? ProRataDiscount { get; set; }

        public bool? ProRataLowDelay { get; set; }

        public bool? ProRataResidue { get; set; }

        public bool? ProRataAnticipation { get; set; }

        public decimal? DirectComission { get; set; }

        public decimal? TotalComission { get; set; }

        public bool? GenerateCreeditLetterOnOverPayment { get; set; }

        public DateTime? StatusDate { get; set; }

        public string Comments { get; set; }

        public int? TypeCalcMora { get; set; }

        public decimal? DocTotal { get; set; }

        public decimal? SaleValue { get; set; }

        public bool? AssignmentRights { get; set; }

        public bool? AdjustmentOnlyCalculateDueDate { get; set; }

        public bool? NotGenerateNegativeMonetaryCorrection { get; set; }

        public int? LagOfInterestDay { get; set; }

        public int? LagOfFine { get; set; }

        public int? AccPeriodId { get; set; }

        public int? MonthsOfGrowthForCorrectionYearly { get; set; }

        public int? AccountNumber { get; set; }

        public bool? Accounted { get; set; }

        public string InstructionBoE { get; set; }

        public bool? BlockPaymentWithFutureDate { get; set; }

        public decimal? CostUnit { get; set; }

        public bool? EnableDiscountPunctuality { get; set; }

        public int? NumberInstallmentsPunctuality { get; set; }

        public int? RealEstateTransferDays { get; set; }

        public decimal? AdministrationRate { get; set; }

        public bool? RentalTransferGuaranteed { get; set; }
    }
}
