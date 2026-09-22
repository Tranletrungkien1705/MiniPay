namespace MiniPay.Models;

/// <summary>Tổ chức (merchant tenant). Mỗi org 1 ApiKey để tạo giao dịch.</summary>
public sealed class Org
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "";
    public string ApiKey { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

public enum PayStatus { Pending = 0, Paid = 1, Failed = 2 }

public enum ReconcileStatus { Pending = 0, Completed = 1, Discrepant = 2 }

public enum MatchStatus { Pending = 0, Matched = 1, AmountMismatch = 2, NotFound = 3 }

public enum PayoutStatus { Draft = 0, PendingApproval = 1, Approved = 2, Processing = 3, Completed = 4, Rejected = 5, Cancelled = 6 }

public enum PayoutTransType { Inhouse = 0, Napas247 = 1, Citad = 2 }

public enum DisbursementType { Supplier = 0, Refund = 1, Commission = 2, Salary = 3, Other = 4 }

public enum PayoutItemStatus { Pending = 0, Success = 1, Failed = 2 }

public enum DiscountRequestStatus { Draft = 0, PartnerSigned = 1, Approved = 2, Settled = 3, Rejected = 4, Cancelled = 5 }

public enum DiscountSignStatus { Pending = 0, Signed = 1 }

public enum DiscountItemStatus { Active = 0, Excluded = 1 }

public enum GuaranteeStatus { Draft = 0, PendingApproval = 1, Active = 2, Claimed = 3, Settled = 4, Expired = 5, Rejected = 6, Cancelled = 7 }

public enum GuaranteeType { Payment = 0, ContractPerformance = 1, DeferredPayment = 2, AdvancePayment = 3 }

public enum GuaranteeDetailStatus { Active = 0, Paid = 1, Claimed = 2, Released = 3, Cancelled = 4 }

public enum MortgageStatus { Draft = 0, PendingApproval = 1, Approved = 2, Finished = 3, Rejected = 4, Cancelled = 5 }

public enum MortgageDetailStatus { Pending = 0, Approved = 1, Redeemed = 2, Cancelled = 3 }

public enum RedeemStatus { Draft = 0, PendingApproval = 1, Approved = 2, Completed = 3, Rejected = 4, Cancelled = 5 }

public enum RedeemDetailStatus { Pending = 0, Approved = 1, Cancelled = 2 }

public enum PaymentOrderType { Deposit = 0, OrderPayment = 1, UNC = 2, GuaranteePayment = 3, Cash = 4, Offset = 5 }

public enum PaymentFundType { OwnCapital = 0, BankLoan = 1, CreditLine = 2 }

public enum PaymentOrderStatus { Draft = 0, PendingApproval = 1, Approved = 2, Finished = 3, Rejected = 4, Cancelled = 5 }

public enum PaymentOrderDetailStatus { Pending = 0, Approved = 1, Finished = 2, Cancelled = 3 }

/// <summary>Ý định thanh toán (payment intent) — 1 dòng / 1 lần khởi tạo cổng.</summary>
public sealed class PaymentIntent
{
    public long Id { get; set; }
    public Guid OrgId { get; set; }
    public string TxnRef { get; set; } = "";      // vnp_TxnRef (duy nhất)
    public string OrderId { get; set; } = "";      // mã đơn phía merchant
    public long Amount { get; set; }               // VND (đồng), chưa ×100
    public string OrderInfo { get; set; } = "";
    public string Provider { get; set; } = "vnpay";
    public PayStatus Status { get; set; } = PayStatus.Pending;
    public string? ResponseCode { get; set; }      // vnp_ResponseCode
    public string? BankCode { get; set; }
    public string? VnpTransactionNo { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? PaidAt { get; set; }
}

/// <summary>Đợt đối soát sao kê ngân hàng (Reconcile Batch) — tương ứng Bank_BankStatement trong BizHTC.</summary>
public sealed class ReconcileBatch
{
    public long Id { get; set; }
    public Guid OrgId { get; set; }
    public string BatchCode { get; set; } = "";           // Mã đợt đối soát (ví dụ REC-20250514-001)
    public string BankCode { get; set; } = "";            // Mã ngân hàng: VCB, TCB, MB, VIB, VNPAY, MOMO
    public string? AccountNo { get; set; }                // Số tài khoản nhận sao kê
    public DateTime StatementDate { get; set; } = DateTime.Today; // Ngày/kỳ sao kê
    public int TotalRecords { get; set; }                 // Tổng số dòng giao dịch sao kê
    public int MatchedCount { get; set; }                 // Số dòng khớp thành công
    public int MismatchedCount { get; set; }              // Số dòng lệch số tiền
    public int UnmatchedCount { get; set; }               // Số dòng không tìm thấy trên hệ thống
    public long TotalAmount { get; set; }                 // Tổng tiền sao kê (VND)
    public long MatchedAmount { get; set; }               // Tổng tiền đã khớp (VND)
    public ReconcileStatus Status { get; set; } = ReconcileStatus.Pending;
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? CompletedAt { get; set; }

    public List<ReconcileDetail> Details { get; set; } = [];
}

/// <summary>Chi tiết dòng sao kê đối soát — tương ứng Bank_BankStatementDtl trong BizHTC.</summary>
public sealed class ReconcileDetail
{
    public long Id { get; set; }
    public long BatchId { get; set; }
    public Guid OrgId { get; set; }
    public string BankTxnNo { get; set; } = "";           // Mã giao dịch ngân hàng / AutoId sao kê
    public string? TxnRef { get; set; }                   // Mã đơn/giao dịch trích xuất từ nội dung
    public DateTime TxnTime { get; set; } = DateTime.Now; // Thời điểm giao dịch ngân hàng
    public long Amount { get; set; }                      // Số tiền ghi có (ValGhiCo)
    public string? SenderAccount { get; set; }            // Tài khoản gửi
    public string? ReceiverAccount { get; set; }          // Tài khoản nhận
    public string Remark { get; set; } = "";              // Nội dung chuyển khoản (RemarkNDCT)
    public MatchStatus MatchStatus { get; set; } = MatchStatus.Pending;
    public long? PaymentIntentId { get; set; }            // Id giao dịch hệ thống nếu khớp
    public long? SystemAmount { get; set; }               // Số tiền ghi nhận trên hệ thống
    public string? DiscrepancyReason { get; set; }         // Diễn giải kết quả / lý do sai lệch
    public DateTime? MatchedAt { get; set; }
}

/// <summary>Lô lệnh chi chuyển tiền ngân hàng — tương ứng RQ_BankingTransactions trong BizHTC.VietinBank / MBBank.</summary>
public sealed class BankingPayoutBatch
{
    public long Id { get; set; }
    public Guid OrgId { get; set; }
    public string BatchNo { get; set; } = "";          // Mã lệnh chi (RQ_BankingTransNo, ví dụ BTX-20250514-001)
    public string BankCode { get; set; } = "";         // Ngân hàng trích nợ: CTG (VietinBank), MBB (MBBank), VCB, TCB, VPB
    public string SourceAccount { get; set; } = "";    // Số tài khoản trích nợ
    public string? SourceAccountName { get; set; }     // Tên chủ tài khoản nguồn
    public string? BizResNumber { get; set; }          // Số căn cứ / chứng từ gốc (hợp đồng, quyết định chi, hóa đơn)
    public string Remark { get; set; } = "";           // Diễn giải lô lệnh chi
    public int TotalTrans { get; set; }                // Tổng số món chuyển
    public int SuccessTrans { get; set; }              // Số món thành công
    public int FailedTrans { get; set; }               // Số món thất bại
    public long TotalAmount { get; set; }              // Tổng tiền chi (VND)
    public long SuccessAmount { get; set; }            // Tổng tiền chi thành công (VND)
    public PayoutStatus Status { get; set; } = PayoutStatus.PendingApproval; // BkTransStatus
    public string? BankStatusCode { get; set; }        // BkTransBankStatus (00: Success, ERR_..., v.v.)
    public string? RefBankCode { get; set; }           // Mã tham chiếu phía cổng ngân hàng cấp
    public string? BankRemark { get; set; }            // Thông báo phản hồi từ cổng ngân hàng
    public string? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? RejectReason { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? CancelledAt { get; set; }

    public List<BankingPayoutDetail> Details { get; set; } = [];
}

/// <summary>Chi tiết dòng chuyển tiền trong lệnh chi — tương ứng RQ_BankingTransPmt trong BizHTC.</summary>
public sealed class BankingPayoutDetail
{
    public long Id { get; set; }
    public long BatchId { get; set; }
    public Guid OrgId { get; set; }
    public string TransNo { get; set; } = "";          // Mã giao dịch con
    public PayoutTransType TransType { get; set; } = PayoutTransType.Napas247; // BkTransType
    public DisbursementType DisbursementType { get; set; } = DisbursementType.Supplier; // DisbursementType
    public string? RefNo { get; set; }                 // Mã tham chiếu đơn hàng/phiếu chi (PaymentNo / OrderId)
    public string ReceivingUnit { get; set; } = "";    // Tên người / đơn vị thụ hưởng
    public string BankAccountReceive { get; set; } = ""; // Số tài khoản thụ hưởng
    public string BankNameReceive { get; set; } = "";  // Tên ngân hàng người nhận (VCB, TCB, MB, CTG, ACB, VPB, ...)
    public string? ProvinceName { get; set; }          // Chi nhánh / Tỉnh thành
    public long TransferAmount { get; set; }           // Số tiền chuyển (VND)
    public string TransferRemark { get; set; } = "";   // Nội dung chuyển tiền
    public PayoutItemStatus Status { get; set; } = PayoutItemStatus.Pending; // BkTransPmtStatus
    public string? BankTxnRef { get; set; }            // TransactionID trả về từ ngân hàng
    public string? ErrorMessage { get; set; }          // Lỗi nếu thất bại
    public DateTime? ExecutedAt { get; set; }
}

/// <summary>Hồ sơ đề nghị chiết khấu thanh toán sớm — tương ứng Req_PaymentDiscount trong BizHTC.PaymentDiscount.</summary>
public sealed class PaymentDiscountRequest
{
    public long Id { get; set; }
    public Guid OrgId { get; set; }
    public string DiscountNo { get; set; } = "";             // Mã hồ sơ chiết khấu (PaymentDiscountNo: DIS-20250514-001)
    public string PartnerCode { get; set; } = "";            // Mã đối tác / đại lý (DealerCode)
    public string PartnerName { get; set; } = "";            // Tên đối tác / đại lý (DealerName)
    public string? ContractNo { get; set; }                  // Mã hợp đồng / thỏa thuận tài trợ / thư bảo lãnh
    public long TotalPaymentAmount { get; set; }             // Tổng số tiền thanh toán gốc (VND)
    public long TotalDiscountAmount { get; set; }            // Tổng tiền chiết khấu được hưởng (VND - SUMTotalDiscountPrice)
    public long NetPaymentAmount { get; set; }               // Tổng tiền thực nộp sau chiết khấu (VND)
    public decimal DefaultAnnualRate { get; set; } = 7.5m;   // Tỷ lệ chiết khấu năm mặc định (%/năm)
    public DiscountRequestStatus Status { get; set; } = DiscountRequestStatus.Draft; // PmtDctStatus
    public DiscountSignStatus PartnerSignStatus { get; set; } = DiscountSignStatus.Pending; // DlrSignStatus
    public string? PartnerSignedBy { get; set; }             // DlrSignBy
    public DateTime? PartnerSignedAt { get; set; }           // DlrSignDTime
    public DiscountSignStatus ApproverSignStatus { get; set; } = DiscountSignStatus.Pending; // HTCSignStatus
    public string? ApprovedBy { get; set; }                  // HTCApprBy
    public DateTime? ApprovedAt { get; set; }                // HTCApprDTime
    public string? SettledBy { get; set; }                   // HTCSignBy
    public DateTime? SettledAt { get; set; }                 // HTCSignDTime
    public string? RejectReason { get; set; }                // Lý do từ chối (RejectBy / RejectDTime)
    public DateTime? CancelledAt { get; set; }               // CancelDTime
    public string Remark { get; set; } = "";                 // Diễn giải / ghi chú
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public List<PaymentDiscountDetail> Details { get; set; } = [];
}

/// <summary>Chi tiết đợt thanh toán đề nghị chiết khấu — tương ứng Req_PaymentDiscountDtl trong BizHTC.PaymentDiscount.</summary>
public sealed class PaymentDiscountDetail
{
    public long Id { get; set; }
    public long RequestId { get; set; }
    public Guid OrgId { get; set; }
    public string ItemRefNo { get; set; } = "";              // Mã tham chiếu (CarId / Số hóa đơn / Phase: INV-01, PHASE-1)
    public string Description { get; set; } = "";            // Diễn giải hàng hóa/xe/hợp đồng
    public DateTime DueDate { get; set; }                    // Hạn thanh toán theo thỏa thuận / bảo lãnh (DateEnd / PaymentEndDate)
    public DateTime ActualPaymentDate { get; set; }          // Ngày thanh toán thực tế (PaymentEndDatePhase)
    public int EarlyDays { get; set; }                       // Số ngày thanh toán trước hạn (DiscountDateNumberPhase)
    public long OriginalAmount { get; set; }                 // Số tiền thanh toán đợt này (AmountPhase)
    public decimal AnnualDiscountRate { get; set; }          // Tỷ lệ chiết khấu năm áp dụng (%/năm - DiscountPercentPhase)
    public long DiscountAmount { get; set; }                 // Số tiền chiết khấu (DiscountPricePhase = Amount * Rate/100 * Days / 360)
    public long NetPayAmount { get; set; }                   // Số tiền thực trả đợt này (OriginalAmount - DiscountAmount)
    public DiscountItemStatus Status { get; set; } = DiscountItemStatus.Active; // PmtDctDtlStatus
    public string? Note { get; set; }
}

/// <summary>Thư bảo lãnh thanh toán ngân hàng — tương ứng Pmt_Guarantee trong BizHTC.Payment / FrmMngGrt.</summary>
public sealed class PaymentGuarantee
{
    public long Id { get; set; }
    public Guid OrgId { get; set; }
    public string GuaranteeNo { get; set; } = "";             // Số bảo lãnh hệ thống (GRT-20250514-001)
    public string BankGuaranteeNo { get; set; } = "";         // Số thư bảo lãnh ngân hàng cấp (BL-VCB-2025/088)
    public string BankCode { get; set; } = "";                // Mã ngân hàng phát hành (VCB, TCB, MBB, CTG, BIDV, VPB...)
    public string BankName { get; set; } = "";                // Tên ngân hàng
    public string PartnerCode { get; set; } = "";             // Mã đại lý / đối tác (DealerCode)
    public string PartnerName { get; set; } = "";             // Tên đại lý / đối tác (DealerName)
    public string? ContractNo { get; set; }                  // Số hợp đồng mua bán / hạn mức
    public GuaranteeType GuaranteeType { get; set; } = GuaranteeType.Payment; // Loại bảo lãnh
    public long TotalAmount { get; set; }                     // Tổng giá trị bảo lãnh (VND)
    public long UtilizedAmount { get; set; }                  // Giá trị bảo lãnh đã phân bổ/sử dụng (VND)
    public long RemainingAmount { get; set; }                 // Giá trị bảo lãnh còn lại khả dụng (VND)
    public DateTime DateOpen { get; set; } = DateTime.Today;  // Ngày mở / phát hành thư bảo lãnh
    public DateTime DateEnd { get; set; }                     // Hạn thanh toán theo bảo lãnh
    public DateTime DateExpired { get; set; }                 // Ngày hết hạn hiệu lực thư bảo lãnh
    public int TermDays { get; set; }                         // Thời hạn bảo lãnh (ngày)
    public int TermWarningDays { get; set; } = 15;            // Số ngày cảnh báo trước khi hết hạn
    public decimal FeePercent { get; set; } = 1.2m;           // Phí phát hành bảo lãnh (%/năm)
    public DateTime? DateRecieveGrtRoot { get; set; }         // Ngày nhận bản gốc thư bảo lãnh (DateRecieveGrtRoot trong BizHTC)
    public GuaranteeStatus Status { get; set; } = GuaranteeStatus.PendingApproval; // Trạng thái bảo lãnh
    public string? Remark { get; set; }                       // Diễn giải / ghi chú
    public string? RemarkReject { get; set; }                 // Lý do từ chối nếu bị reject
    public long? ClaimedAmount { get; set; }                  // Số tiền đã yêu cầu ngân hàng đòi bảo lãnh (VND)
    public string? ClaimReason { get; set; }                  // Lý do kích hoạt đòi bảo lãnh (GrtClaim)
    public DateTime? ClaimedAt { get; set; }                  // Thời điểm kích hoạt đòi bảo lãnh
    public string? ClaimedBy { get; set; }                    // Người kích hoạt đòi bảo lãnh
    public string? CreatedBy { get; set; }                    // Người lập
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public string? ApprovedBy { get; set; }                   // Người phê duyệt
    public DateTime? ApprovedAt { get; set; }                 // Thời điểm phê duyệt
    public string? SettledBy { get; set; }                    // Người ký tất toán
    public DateTime? SettledAt { get; set; }                  // Thời điểm hoàn tất tất toán / giải tỏa
    public DateTime? CancelledAt { get; set; }                // Thời điểm hủy

    public List<PaymentGuaranteeDetail> Details { get; set; } = [];
}

/// <summary>Chi tiết dòng đơn hàng/hạng mục trong thư bảo lãnh — tương ứng Pmt_GuaranteeDetail trong BizHTC.</summary>
public sealed class PaymentGuaranteeDetail
{
    public long Id { get; set; }
    public long GuaranteeId { get; set; }
    public Guid OrgId { get; set; }
    public string ItemRefNo { get; set; } = "";               // Mã đơn hàng / hợp đồng / số khung VIN xe (CarId/SoCode/Vin)
    public string Description { get; set; } = "";             // Diễn giải hàng hóa/xe/hạng mục
    public long OrderAmount { get; set; }                     // Giá trị đơn hàng gốc (UnitPriceActual trong BizHTC)
    public long GuaranteeValue { get; set; }                  // Giá trị bảo lãnh phân bổ cho mục này (GrtValue)
    public decimal GuaranteePercent { get; set; }             // Tỷ lệ % bảo lãnh so với giá trị đơn hàng (GrtPercent)
    public DateTime DateStart { get; set; } = DateTime.Today; // Ngày bắt đầu hiệu lực bảo lãnh của món
    public DateTime DateEnd { get; set; }                     // Hạn thanh toán của món
    public GuaranteeDetailStatus Status { get; set; } = GuaranteeDetailStatus.Active; // Trạng thái món bảo lãnh
    public string? Note { get; set; }
}

/// <summary>Hồ sơ đề nghị thế chấp tài sản / kho xe vay ngân hàng — tương ứng RM_ReqMortgage trong BizHTC.GiaiChap / FrmMngRM_ReqMortgage.</summary>
public sealed class MortgageRequest
{
    public long Id { get; set; }
    public Guid OrgId { get; set; }
    public string ReqRMNo { get; set; } = "";             // Số đề nghị thế chấp (RM-20250514-001)
    public string BankCode { get; set; } = "";            // Mã ngân hàng tài trợ vốn (CTG, MBB, TCB, VCB, BIDV, VPB...)
    public string BankName { get; set; } = "";            // Tên ngân hàng nhận thế chấp
    public string PartnerCode { get; set; } = "";         // Mã đơn vị / đại lý thế chấp (DealerCode)
    public string PartnerName { get; set; } = "";         // Tên đơn vị / đại lý thế chấp (DealerName)
    public string? CreditContractNo { get; set; }         // Số hợp đồng tín dụng / hạn mức vay thế chấp
    public DateTime MortgageDate { get; set; } = DateTime.Today; // Ngày lập đề nghị thế chấp
    public int TotalItems { get; set; }                   // Tổng số lượng tài sản / xe thế chấp
    public int ActiveItems { get; set; }                  // Số lượng tài sản đang thế chấp (trạng thái Approved)
    public int RedeemedItems { get; set; }                // Số lượng tài sản đã giải chấp thành công (trạng thái Redeemed)
    public long TotalCollateralValue { get; set; }        // Tổng giá trị định giá tài sản bảo đảm (VND)
    public long TotalLoanAmount { get; set; }             // Tổng số tiền vay giải ngân thế chấp (VND)
    public long RemainingLoanAmount { get; set; }         // Dư nợ vay thế chấp còn lại chưa giải chấp (VND)
    public decimal InterestRate { get; set; } = 8.5m;     // Lãi suất vay thế chấp (%/năm)
    public int LoanPeriodDays { get; set; } = 90;         // Thời hạn vay vốn thế chấp (ngày)
    public MortgageStatus Status { get; set; } = MortgageStatus.PendingApproval; // Trạng thái hồ sơ thế chấp
    public string? CreatedBy { get; set; }                // Người lập đề nghị
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public string? ApprovedBy { get; set; }               // Người / Ngân hàng phê duyệt
    public DateTime? ApprovedAt { get; set; }             // Thời điểm phê duyệt thế chấp
    public string? FinishedBy { get; set; }               // Người tất toán toàn bộ hồ sơ
    public DateTime? FinishedAt { get; set; }             // Thời điểm giải chấp tất toán toàn bộ
    public string? RejectReason { get; set; }             // Lý do từ chối
    public DateTime? CancelledAt { get; set; }            // Thời điểm hủy
    public string? Remark { get; set; }                   // Ghi chú / diễn giải hồ sơ thế chấp

    public List<MortgageDetail> Details { get; set; } = [];
}

/// <summary>Chi tiết danh mục tài sản / xe thế chấp ngân hàng — tương ứng RM_ReqMortgageDtl trong BizHTC.GiaiChap.</summary>
public sealed class MortgageDetail
{
    public long Id { get; set; }
    public long MortgageRequestId { get; set; }
    public Guid OrgId { get; set; }
    public string ItemRefNo { get; set; } = "";           // Số khung / Mã tài sản (VIN - cv_VIN)
    public string ModelCode { get; set; } = "";           // Model xe / Chủng loại tài sản (cv_ModelCode)
    public string? EngineNo { get; set; }                 // Số máy (cv_EngineNo)
    public string? CQNo { get; set; }                     // Số chứng nhận kiểm định an toàn kỹ thuật / Đăng kiểm (cv_CQNo)
    public string? CONo { get; set; }                     // Số chứng nhận chất lượng xuất xưởng / Nguồn gốc (cv_CONo)
    public string? DeclarationNo { get; set; }            // Số tờ khai hải quan nhập khẩu (cv_DeclarationNo)
    public DateTime? CODate { get; set; }                 // Ngày chứng từ nguồn gốc (cv_CODate)
    public long CollateralValue { get; set; }             // Giá trị định giá tài sản bảo đảm (VND)
    public long LoanAmount { get; set; }                  // Số tiền vay thế chấp phân bổ (VND)
    public MortgageDetailStatus Status { get; set; } = MortgageDetailStatus.Pending; // RMDtlStatus
    public string? ApprovedBy { get; set; }               // Người duyệt dòng
    public DateTime? ApprovedAt { get; set; }             // Thời điểm duyệt dòng
    public string? ReqDMNo { get; set; }                  // Số đề nghị giải chấp liên kết khi đã giải chấp (rdrrd_ReqDMNo)
    public DateTime? RedeemedAt { get; set; }             // Ngày hoàn tất giải chấp (rdrrd_ApprovedDate)
    public string? Note { get; set; }
}

/// <summary>Hồ sơ đề nghị giải chấp tài sản ngân hàng — tương ứng RD_ReqRedeem trong BizHTC.GiaiChap / FrmMngRedeem.</summary>
public sealed class RedeemRequest
{
    public long Id { get; set; }
    public Guid OrgId { get; set; }
    public string ReqDMNo { get; set; } = "";             // Số đề nghị giải chấp (DM-20250514-001)
    public string BankCode { get; set; } = "";            // Ngân hàng nhận đề nghị giải chấp
    public string BankName { get; set; } = "";            // Tên ngân hàng
    public string PartnerCode { get; set; } = "";         // Mã đơn vị / đại lý đề nghị giải chấp
    public string PartnerName { get; set; } = "";         // Tên đơn vị / đại lý
    public DateTime RedeemDate { get; set; } = DateTime.Today; // Ngày lập đề nghị giải chấp
    public int TotalItems { get; set; }                   // Tổng số tài sản đề nghị giải chấp
    public int ApprovedItems { get; set; }                // Số tài sản đã duyệt giải chấp
    public long TotalSettlementAmount { get; set; }       // Tổng tiền nộp tất toán nợ vay giải chấp (VND)
    public string? PaymentProofNo { get; set; }           // Mã chứng từ thanh toán / Ủy nhiệm chi (UNC / TransNo)
    public RedeemStatus Status { get; set; } = RedeemStatus.PendingApproval; // DMReqStatus
    public string? CreatedBy { get; set; }                // Người lập
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public string? ApprovedBy { get; set; }               // Người / Ngân hàng phê duyệt giải chấp
    public DateTime? ApprovedAt { get; set; }             // Thời điểm phê duyệt giải chấp
    public string? RejectReason { get; set; }             // Lý do từ chối giải chấp
    public DateTime? CancelledAt { get; set; }            // Thời điểm hủy
    public string? Remark { get; set; }                   // Diễn giải / ghi chú hồ sơ giải chấp

    public List<RedeemDetail> Details { get; set; } = [];
}

/// <summary>Chi tiết tài sản đề nghị giải chấp ngân hàng — tương ứng RD_ReqRedeemDtl trong BizHTC.GiaiChap.</summary>
public sealed class RedeemDetail
{
    public long Id { get; set; }
    public long RedeemRequestId { get; set; }
    public Guid OrgId { get; set; }
    public string ItemRefNo { get; set; } = "";           // Số khung / Mã tài sản giải chấp (VIN)
    public string ReqRMNo { get; set; } = "";             // Số hồ sơ thế chấp gốc tương ứng (ReqRMNo)
    public long? MortgageDetailId { get; set; }           // ID dòng thế chấp gốc
    public string? ModelCode { get; set; }                // Model xe / Tên tài sản
    public string? DealerCode { get; set; }               // Đại lý tiếp nhận tài sản sau giải chấp
    public long SettlementAmount { get; set; }            // Số tiền nộp tất toán giải chấp món này (VND)
    public RedeemDetailStatus Status { get; set; } = RedeemDetailStatus.Pending; // DMReqDtlStatus
    public string? ApprovedBy { get; set; }               // Người duyệt giải chấp
    public DateTime? ApprovedAt { get; set; }             // Thời điểm duyệt giải chấp
    public string? Note { get; set; }
}

/// <summary>Phiếu thanh toán & Ủy nhiệm chi ngân hàng — tương ứng Pmt_Payment trong BizHTC.Payment / FrmMngPM & FrmNewPM.</summary>
public sealed class PaymentOrder
{
    public long Id { get; set; }
    public Guid OrgId { get; set; }
    public string PaymentNo { get; set; } = "";          // Số phiếu thanh toán (PM-20250514-001)
    public PaymentOrderType PaymentType { get; set; } = PaymentOrderType.UNC; // Loại thanh toán (Deposit, OrderPayment, UNC...)
    public string? BankPaymentNo { get; set; }           // Số chứng từ ngân hàng / Số UNC (UNC-VCB-8812)
    public DateTime PaymentEndDate { get; set; } = DateTime.Today.AddDays(30); // Hạn thanh toán
    public string PartnerCode { get; set; } = "";        // Mã đại lý / đối tác nộp/chuyển tiền (DealerCode)
    public string PartnerName { get; set; } = "";        // Tên đại lý / đối tác
    public string BankCodeSend { get; set; } = "";       // Ngân hàng chuyển / trích nợ (VCB, CTG, MBB, TCB...)
    public string? BankNameSend { get; set; }            // Tên ngân hàng chuyển
    public string BankAccountSend { get; set; } = "";    // Số tài khoản trích nợ
    public string BankCodeReceive { get; set; } = "";    // Ngân hàng thụ hưởng
    public string? BankNameReceive { get; set; }         // Tên ngân hàng thụ hưởng
    public string BankAccountReceive { get; set; } = ""; // Số tài khoản thụ hưởng
    public PaymentFundType Funds { get; set; } = PaymentFundType.OwnCapital; // Nguồn tiền: Vốn tự có, Vay ngân hàng, Hạn mức bảo lãnh
    public string? BankLending { get; set; }             // Ngân hàng cho vay (khi Funds = BankLoan)
    public decimal InterestRate { get; set; }            // Lãi suất vay (%/năm)
    public int LoanPeriodMonths { get; set; }            // Kỳ hạn vay (tháng)
    public string? AccountingRecordNo { get; set; }      // Số chứng từ kế toán ghi sổ
    public long TotalAmount { get; set; }                // Tổng tiền thanh toán đợt này (VND)
    public long TotalAccumAmount { get; set; }           // Tổng tiền tích lũy sau đợt này (VND)
    public PaymentOrderStatus Status { get; set; } = PaymentOrderStatus.PendingApproval; // Trạng thái phiếu
    public string Remark { get; set; } = "";             // Diễn giải / Gợi ý nội dung UNC
    public string? RejectReason { get; set; }            // Lý do từ chối
    public string? CreatedBy { get; set; }               // Người lập phiếu
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public string? ApprovedBy { get; set; }              // Kế toán trưởng phê duyệt
    public DateTime? ApprovedAt { get; set; }            // Thời điểm duyệt
    public string? FinishedBy { get; set; }              // Người xác nhận hoàn tất / ghi sổ UNC
    public DateTime? FinishedAt { get; set; }            // Thời điểm hoàn tất
    public DateTime? CancelledAt { get; set; }           // Thời điểm hủy

    public List<PaymentOrderDetail> Details { get; set; } = [];
}

/// <summary>Chi tiết dòng thanh toán theo đơn / xe / hợp đồng — tương ứng Pmt_PaymentDetail trong BizHTC.Payment.</summary>
public sealed class PaymentOrderDetail
{
    public long Id { get; set; }
    public long PaymentOrderId { get; set; }
    public Guid OrgId { get; set; }
    public string ItemRefNo { get; set; } = "";          // Số khung VIN xe / Mã đơn / Hợp đồng (CarVIN / ItemRefNo)
    public string Description { get; set; } = "";        // Tên hàng hóa / mô tả xe / đơn hàng
    public string? ModelCode { get; set; }               // Dòng xe / Model
    public long UnitPriceActual { get; set; }            // Đơn giá thực tế xe / đơn hàng (VND)
    public long AmountAccum { get; set; }                // Tích lũy đã thanh toán trước đó (VND)
    public decimal PercentAccum { get; set; }            // % đã thanh toán trước đó
    public long Amount { get; set; }                     // Số tiền thanh toán đợt này (VND)
    public decimal PercentCurrent { get; set; }          // % thanh toán đợt này
    public long AmountTotal { get; set; }                // Tổng lũy kế sau đợt này (= Amount + AmountAccum)
    public decimal PercentTotal { get; set; }            // Tổng % lũy kế sau đợt này (= PercentCurrent + PercentAccum)
    public string? GuaranteeNo { get; set; }             // Số bảo lãnh hệ thống áp dụng
    public string? BankGrtNo { get; set; }               // Số thư bảo lãnh ngân hàng cấp
    public PaymentOrderDetailStatus Status { get; set; } = PaymentOrderDetailStatus.Pending; // Trạng thái dòng
    public string? Note { get; set; }
}

