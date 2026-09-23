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

public enum BankBillMinutesStatus { Draft = 0, PendingHandover = 1, Delivered = 2, BankReceived = 3, Settled = 4, Cancelled = 5 }

public enum BankBillDetailStatus { Pending = 0, Delivered = 1, BankVerified = 2, Cancelled = 3 }

public enum PaymentPDIStatus { Draft = 0, TCMSApproved = 1, HTVApproved = 2, Paid = 3, Rejected = 4, Cancelled = 5 }

public enum PDISignStatus { Pending = 0, Signed = 1 }

public enum PaymentPDIDetailStatus { Pending = 0, Approved = 1, Adjusted = 2, Cancelled = 3 }

public enum LatePaymentPenaltyStatus { Draft = 0, Calculated = 1, Reviewed = 2, Approved = 3, Settled = 4, Waived = 5, Cancelled = 6 }

public enum LatePaymentPenaltyDetailStatus { Pending = 0, Calculated = 1, Approved = 2, Settled = 3, Cancelled = 4 }

public enum TransportInsStatus { Draft = 0, TCMSApproved = 1, HTVApproved = 2, Signed = 3, Settled = 4, Rejected = 5, Cancelled = 6 }

public enum TransportSignCAStatus { Pending = 0, Signed = 1 }

public enum TransportInsDetailStatus { Pending = 0, Approved = 1, Adjusted = 2, Settled = 3, Cancelled = 4 }

public enum TransportCommandType { CarTransport = 0, StorageRearrange = 1, StorageRearrCB = 2, CarRetrieve = 3 }

public enum PaymentStorageStatus { Draft = 0, HTVApproved = 1, TCMSApproved = 2, Signed = 3, Settled = 4, Rejected = 5, Cancelled = 6 }

public enum StorageSignCAStatus { Pending = 0, Signed = 1 }

public enum PaymentStorageDetailStatus { Pending = 0, Approved = 1, Adjusted = 2, Cancelled = 3 }

public enum GuaranteeExtensionStatus { Draft = 0, PendingSign = 1, Signed = 2, BankAccepted = 3, BankRejected = 4, Cancelled = 5 }

public enum ExtensionSignCAStatus { Pending = 0, Signed = 1 }

public enum GuaranteeExtensionDetailStatus { Pending = 0, Active = 1, BankAccepted = 2, BankRejected = 3, Cancelled = 4 }

public enum GuaranteeClaimStatus { Draft = 0, Submitted = 1, SignedCA = 2, SentToBank = 3, Settled = 4, BankRejected = 5, Cancelled = 6 }

public enum ClaimSignCAStatus { Pending = 0, Signed = 1 }

public enum GuaranteeClaimDetailStatus { Pending = 0, Claimed = 1, Settled = 2, BankRejected = 3, Cancelled = 4 }

public enum PaymentAVNStatus { Draft = 0, TCMSApproved = 1, HTVApproved = 2, Signed = 3, Settled = 4, Rejected = 5, Cancelled = 6 }

public enum AVNSignCAStatus { Pending = 0, Signed = 1 }

public enum PaymentAVNDetailStatus { Pending = 0, Approved = 1, Adjusted = 2, Cancelled = 3 }

public enum PaymentGPSStatus { Draft = 0, HTVApproved = 1, TCMSApproved = 2, Signed = 3, Settled = 4, Rejected = 5, Cancelled = 6 }

public enum GPSSignCAStatus { Pending = 0, Signed = 1 }

public enum PaymentGPSDetailStatus { Pending = 0, Approved = 1, Adjusted = 2, Cancelled = 3 }

public enum FinancialExpenseStatus { Draft = 0, DlrApproved1 = 1, DlrSigned = 2, HTCApproved1 = 3, HTCSigned = 4, Settled = 5, Cancelled = 6 }

public enum FnExpSignCAStatus { Pending = 0, Signed = 1 }

public enum FinancialExpenseDetailStatus { Active = 0, Excluded = 1 }

public enum VehicleAssemblyType { CKD = 0, CBU = 1 }

public enum BankingTransType { GNTT = 0, PhatHanhBLLC = 1, PhatHanhLC = 2, HTDB = 3, GNTTLC = 4 }

public enum BankingTransStatus { Draft = 0, SentToBank = 1, Processing = 2, Completed = 3, Cancelled = 4 }

public enum BankingTransBankStatus { Pending = 0, SentWaiting = 1, Reviewing = 2, InvalidFile = 3, RequireMoreFiles = 4, RequireMoreDocs = 5, RequireSignCA = 6, Disbursed = 7, Rejected = 8, Cancelled = 9 }

public enum BankFileDocumentType { DeNghiVay = 0, PhuLucHopDong = 1, DangKyKinhDoanh = 2, CamKetTraNo = 3, ChungTuKhac = 4 }

public enum BankFileSignStatus { Pending = 0, Signed = 1 }

public enum CancelBankMDStatus { Pending = 0, Approved = 1, Finished = 2, Rejected = 3, Cancelled = 4 }

public enum CancelBankMDReasonType { ChangeBank = 0, SwitchToOwnCapital = 1, ContractRestructuring = 2, Other = 3 }

public enum CancelBankMDDetailStatus { Pending = 0, Approved = 1, Finished = 2, Cancelled = 3 }

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

/// <summary>Biên bản bàn giao xe & chứng từ gốc theo Hối phiếu ngân hàng — tương ứng Car_BankBillMinutes trong BizHTC / FrmQuanLyBBBGTheoHoiPhieu & FrmTaoBBBGTheoHoiPhieu.</summary>
public sealed class BankBillMinutes
{
    public long Id { get; set; }
    public Guid OrgId { get; set; }
    public string BankBillMnNo { get; set; } = "";             // Số biên bản bàn giao (BBBG-20250514-001)
    public string BankCode { get; set; } = "";                // Mã ngân hàng tài trợ/nhận hối phiếu (VPB, CTG, MBB, TCB, VCB...)
    public string BankName { get; set; } = "";                // Tên ngân hàng
    public string PartnerCode { get; set; } = "";             // Mã đại lý ký phát hối phiếu (DealerCode)
    public string PartnerName { get; set; } = "";             // Tên đại lý
    public DateTime BankBillDate { get; set; } = DateTime.Today; // Ngày lập biên bản bàn giao
    public DateTime? BankBillPrintDate { get; set; }          // Ngày in / xuất trình hối phiếu
    public DateTime? BankBillReciveDate { get; set; }         // Ngày ngân hàng tiếp nhận chứng từ gốc (BankBillReciveDate trong BizHTC)
    public int TotalVehicles { get; set; }                    // Tổng số xe bàn giao trong biên bản
    public long TotalClaimAmount { get; set; }                // Tổng giá trị thanh toán theo hối phiếu (VND)
    public BankBillMinutesStatus Status { get; set; } = BankBillMinutesStatus.PendingHandover; // Trạng thái biên bản
    public string? Remark { get; set; }                       // Diễn giải / ghi chú biên bản
    public string? CreatedBy { get; set; }                    // Người lập biên bản
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public string? HandedOverBy { get; set; }                 // Cán bộ bàn giao chứng từ
    public DateTime? HandedOverAt { get; set; }               // Thời điểm bàn giao sang ngân hàng
    public string? BankReceivedBy { get; set; }               // Đại diện ngân hàng tiếp nhận hồ sơ gốc
    public DateTime? BankReceivedAt { get; set; }             // Thời điểm ngân hàng ký nhận hồ sơ gốc
    public string? SettledBy { get; set; }                    // Người xác nhận quyết toán hối phiếu
    public DateTime? SettledAt { get; set; }                  // Thời điểm quyết toán hối phiếu
    public DateTime? CancelledAt { get; set; }                // Thời điểm hủy

    public List<BankBillMinutesDetail> Details { get; set; } = [];
}

/// <summary>Chi tiết hồ sơ gốc xe bàn giao theo Hối phiếu ngân hàng — tương ứng Car_BankBillMinutesDtl trong BizHTC.</summary>
public sealed class BankBillMinutesDetail
{
    public long Id { get; set; }
    public long MinutesId { get; set; }
    public Guid OrgId { get; set; }
    public string VIN { get; set; } = "";                     // Số khung xe (17 ký tự VIN)
    public string ModelCode { get; set; } = "";               // Mã model (SANTAFE, TUCSON, ACCENT, CRETA...)
    public string? SpecCode { get; set; }                     // Mã đặc tả kỹ thuật xe
    public string? SpecDescription { get; set; }              // Tên thương mại / mô tả chi tiết xe
    public string? EngineNo { get; set; }                     // Số máy xe
    public string? CONo { get; set; }                         // Số Phiếu kiểm tra chất lượng xuất xưởng CO
    public string? CabinCONo { get; set; }                    // Số chứng nhận xuất xưởng Cabin / Chassis
    public string? DeclarationNo { get; set; }                // Số Tờ khai hải quan nhập khẩu
    public string? BankGuaranteeNo { get; set; }              // Số Thư bảo lãnh / L/C ngân hàng
    public string? HTCInvoiceNo { get; set; }                 // Số Hóa đơn GTGT phân phối HTC
    public string? TCGInvoiceNo { get; set; }                 // Số Hóa đơn GTGT sản xuất TCG
    public string? TransportMinutesNo { get; set; }           // Số Biên bản vận chuyển giao nhận xe
    public long ClaimAmount { get; set; }                     // Giá trị theo hối phiếu của xe (VND)
    public DateTime? GuaranteeDateStart { get; set; }         // Ngày bắt đầu bảo lãnh
    public DateTime? GuaranteeDateOpen { get; set; }          // Ngày phát hành bảo lãnh
    public int NumberOfDaysDeferred { get; set; } = 30;       // Thời hạn trả chậm theo hối phiếu (ngày)
    public BankBillDetailStatus Status { get; set; } = BankBillDetailStatus.Pending; // Trạng thái dòng
    public string? Note { get; set; }                         // Tình trạng chứng từ (bản gốc CO, CQ, tờ khai...)
}

/// <summary>Bảng kê thanh toán chi phí kiểm tra kỹ thuật xe PDI — tương ứng Pmt_PaymentPDI trong BizHTC.Payment / FrmQuanLyThanhToanPDI & FrmSuaThanhToanPDI.</summary>
public sealed class PaymentPDI
{
    public long Id { get; set; }
    public Guid OrgId { get; set; }
    public string PmtPDINo { get; set; } = "";             // Số bảng kê (PDI-202505-001)
    public string PmtMonth { get; set; } = "";             // Kỳ / tháng thanh toán PDI (ví dụ 2025-05)
    public string ServiceUnitCode { get; set; } = "TCMS";  // Mã đơn vị kiểm định PDI (TCMS, HTMV-PDI...)
    public string ServiceUnitName { get; set; } = "Trung tâm Quản lý Kỹ thuật & Dịch vụ Xe TCMS"; // Tên đơn vị kiểm định
    public int TotalVehicles { get; set; }                 // Tổng số lượng xe kiểm tra trong kỳ
    public long TotalCostIn { get; set; }                  // Tổng chi phí PDI xe nhập kho (PDIN)
    public long TotalCostOut { get; set; }                 // Tổng chi phí PDI xe xuất kho (PDIX)
    public long TotalAmount { get; set; }                  // Tổng tiền trước VAT (= TotalCostIn + TotalCostOut)
    public decimal VATRate { get; set; } = 10.0m;          // Thuế suất VAT (%)
    public long AmountVAT { get; set; }                    // Tiền thuế VAT
    public long TotalAmountAfterVAT { get; set; }          // Tổng tiền thanh toán sau thuế VAT
    public PaymentPDIStatus Status { get; set; } = PaymentPDIStatus.Draft; // Trạng thái bảng kê
    public PDISignStatus TCMSSignStatus { get; set; } = PDISignStatus.Pending; // Trạng thái ký TCMS
    public string? TCMSSignUser { get; set; }              // Người ký số TCMS
    public DateTime? TCMSSignDTime { get; set; }           // Thời điểm ký TCMS
    public PDISignStatus HTVSignStatus { get; set; } = PDISignStatus.Pending;  // Trạng thái ký HTV
    public string? HTVSignUser { get; set; }               // Người ký số HTV
    public DateTime? HTVSignDTime { get; set; }            // Thời điểm ký HTV
    public string? Appr1By { get; set; }                   // Người duyệt cấp 1 (TCMS)
    public DateTime? Appr1DTime { get; set; }              // Thời điểm duyệt cấp 1
    public string? Appr2By { get; set; }                   // Người duyệt cấp 2 (HTV)
    public DateTime? Appr2DTime { get; set; }              // Thời điểm duyệt cấp 2
    public string? PaidBy { get; set; }                    // Kế toán thanh toán / tất toán
    public DateTime? PaidAt { get; set; }                  // Ngày giờ thanh toán
    public string? BankTxnRef { get; set; }                // Mã giao dịch chi tiền ngân hàng / UNC
    public string? RejectReason { get; set; }              // Lý do từ chối
    public DateTime? CancelledAt { get; set; }             // Ngày giờ hủy
    public string? FilePath { get; set; }                  // Đường dẫn biên bản / chứng từ scan có ký số
    public string? Remark { get; set; }                    // Diễn giải / ghi chú bảng kê
    public string? CreatedBy { get; set; }                 // Người lập bảng kê
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public List<PaymentPDIDetail> Details { get; set; } = [];
}

/// <summary>Chi tiết xe kiểm tra kỹ thuật trong bảng kê PDI — tương ứng Pmt_PaymentPDIDetail trong BizHTC.Payment.</summary>
public sealed class PaymentPDIDetail
{
    public long Id { get; set; }
    public long PaymentPDIId { get; set; }
    public Guid OrgId { get; set; }
    public string VIN { get; set; } = "";                  // Số khung xe (17 ký tự VIN)
    public string? CarId { get; set; }                     // Mã định danh xe nội bộ
    public string ModelCode { get; set; } = "";            // Dòng xe (SANTAFE, TUCSON, ACCENT, CRETA...)
    public string? ModelName { get; set; }                 // Tên thương mại dòng xe
    public string? SpecCode { get; set; }                  // Mã phiên bản
    public string? SpecDescription { get; set; }           // Mô tả chi tiết phiên bản
    public string? ColorExtNameVN { get; set; }            // Màu sơn ngoại thất
    public string StorageCodeInit { get; set; } = "";      // Mã kho bãi kiểm tra xe (KHO-NINHBINH, KHO-DONGANH...)
    public DateTime? StoreDate { get; set; }               // Ngày xe nhập kho bãi
    public DateTime? DeliveryOutDate { get; set; }         // Ngày xe xuất kho giao đại lý
    public string? DlvMnNo { get; set; }                   // Số biên bản xuất kho giao xe (Sto_DlvMinutes)
    public string? DealerCode { get; set; }                // Mã đại lý nhận xe (DealerCode)
    public long CostInCheck { get; set; }                  // Chi phí kiểm tra xe nhập kho (PDIN)
    public long CostOutCheck { get; set; }                 // Chi phí kiểm tra xe xuất kho (PDIX)
    public long TotalCostCheck { get; set; }               // Tổng chi phí PDI xe (= CostInCheck + CostOutCheck)
    public PaymentPDIDetailStatus Status { get; set; } = PaymentPDIDetailStatus.Pending; // Trạng thái dòng
    public string? Remark { get; set; }                    // Ghi chú chi tiết xe kiểm tra
}

/// <summary>Hồ sơ tính và phạt chậm thanh toán đơn hàng/hợp đồng xe — tương ứng TblRptPenaltyPmtDelay trong BizHTC.Report / BizHTC.Payment / FrmRptPenaltyPmtDelay &amp; FrmUpdatePenaltyPmtDelayReal.</summary>
public sealed class LatePaymentPenalty
{
    public long Id { get; set; }
    public Guid OrgId { get; set; }
    public string PenaltyRecordNo { get; set; } = "";     // Số hồ sơ tính phạt (PEN-202505-001)
    public string SOCode { get; set; } = "";              // Số đơn hàng xe (Ord_SalesOrder.SOCode)
    public string DealerCode { get; set; } = "";          // Mã đại lý (Mst_Dealer.DealerCode)
    public string? DealerName { get; set; }               // Tên đại lý
    public string? ContractNo { get; set; }               // Số hợp đồng bán buôn xe
    public DateTime? SOApprovedDate { get; set; }         // Ngày duyệt đơn hàng SO (ApprovedDate2)
    public int TotalApprovedQuantity { get; set; }        // Tổng số lượng xe được duyệt trong đơn
    public long TotalUnitPriceActual { get; set; }        // Tổng giá trị đơn hàng thực tế (VND)
    public int MaxDelayDaysDeposit { get; set; }          // Số ngày chậm nộp cọc (Max_QtyDateDelayPmtCoc)
    public int MaxDelayDaysGrtOpen { get; set; }          // Số ngày chậm mở bảo lãnh (Max_QtyDateDelayOpenGrm)
    public int MaxDelayDaysGrtPay { get; set; }           // Số ngày chậm thanh toán bảo lãnh (Max_QtyDateDelayPmtGrm)
    public int MaxDelayDays60Pmt { get; set; }            // Số ngày chậm thanh toán 60% (Max_QtyDateDelay60Pmt)
    public int MaxDelayDaysRemain { get; set; }           // Số ngày chậm thanh toán 40% còn lại (Max_QtyDelay40PmtRemain)
    public int TotalDatePenalty { get; set; }             // Tổng số ngày tính phạt = Max(...)
    public decimal PenaltyRateAnnual { get; set; } = 12.0m; // Lãi suất phạt (%/năm theo Mst_Discount)
    public long AmountPenaltySystem { get; set; }         // Số tiền phạt hệ thống tính toán (AmountPenaltyTTC)
    public long PenalizeActual { get; set; }              // Số tiền phạt chốt thực tế (Ord_SalesOrder.PenalizeActual)
    public long WaivedAmount { get; set; }                // Số tiền phạt được miễn giảm
    public LatePaymentPenaltyStatus Status { get; set; } = LatePaymentPenaltyStatus.Draft;
    public string? Remark { get; set; }                   // Diễn giải / ghi chú
    public string? AdjustmentReason { get; set; }         // Lý do điều chỉnh / giải trình miễn giảm phạt
    public string? PaymentProofRef { get; set; }          // Mã tham chiếu giao dịch thu phạt / UNC / cấn trừ
    public string? CreatedBy { get; set; }                // Người lập hồ sơ
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? CalculatedAt { get; set; }           // Ngày giờ tính toán phạt
    public string? ReviewedBy { get; set; }               // Kế toán thẩm định mức phạt
    public DateTime? ReviewedAt { get; set; }             // Ngày giờ thẩm định
    public string? ApprovedBy { get; set; }               // Ban Giám đốc phê duyệt chốt phạt
    public DateTime? ApprovedAt { get; set; }             // Ngày giờ duyệt chốt phạt
    public string? SettledBy { get; set; }                // Người quyết toán thu tiền phạt / cấn trừ
    public DateTime? SettledAt { get; set; }              // Ngày giờ quyết toán
    public DateTime? CancelledAt { get; set; }            // Ngày giờ hủy

    public List<LatePaymentPenaltyDetail> Details { get; set; } = [];
}

/// <summary>Chi tiết xe và mốc thanh toán trong hồ sơ phạt chậm thanh toán — tương ứng #tbl_Summary / Pmt_PaymentDetailAccum trong BizHTC.Report.</summary>
public sealed class LatePaymentPenaltyDetail
{
    public long Id { get; set; }
    public long PenaltyId { get; set; }
    public Guid OrgId { get; set; }
    public string? CarId { get; set; }                    // Mã xe nội bộ
    public string VIN { get; set; } = "";                 // Số khung xe (17 ký tự VIN)
    public string ModelCode { get; set; } = "";           // Dòng xe (SANTAFE, TUCSON, CRETA, ACCENT...)
    public string? ModelName { get; set; }                // Tên thương mại dòng xe
    public string? ColorName { get; set; }                // Tên màu xe
    public long UnitPriceActual { get; set; }             // Đơn giá thực tế xe (VND)
    public DateTime? DepositDueDate { get; set; }         // Hạn cam kết nộp cọc (DepositDutyEndDate)
    public DateTime? ActualDepositDate { get; set; }      // Ngày nộp cọc thực tế
    public DateTime? GrtDueDate { get; set; }             // Hạn phát hành bảo lãnh (GrtEndDate)
    public DateTime? ActualGrtDate { get; set; }          // Ngày mở bảo lãnh thực tế (DateOpen)
    public DateTime? GrtPayDueDate { get; set; }          // Hạn thanh toán bảo lãnh (GrtDateEnd)
    public DateTime? ActualGrtPayDate { get; set; }       // Ngày thực tế thanh toán bảo lãnh
    public DateTime? Payment60DueDate { get; set; }       // Hạn cam kết thanh toán 60%
    public DateTime? Actual60PayDate { get; set; }        // Ngày thực tế thanh toán 60%
    public DateTime? PaymentRemainDueDate { get; set; }   // Hạn cam kết thanh toán 100% (40% còn lại)
    public DateTime? ActualRemainPayDate { get; set; }    // Ngày thực tế thanh toán 100%
    public int DelayDaysDeposit { get; set; }             // Số ngày chậm cọc
    public int DelayDaysGrtOpen { get; set; }             // Số ngày chậm mở bảo lãnh
    public int DelayDaysGrtPay { get; set; }              // Số ngày chậm thanh toán bảo lãnh
    public int DelayDays60Pmt { get; set; }               // Số ngày chậm 60%
    public int DelayDaysRemain { get; set; }              // Số ngày chậm 40% còn lại
    public int MaxDelayDays { get; set; }                 // Số ngày trễ lớn nhất của xe = Max(...)
    public long ItemPenaltyAmount { get; set; }           // Tiền phạt xe theo hệ thống tính
    public long ActualItemPenalty { get; set; }           // Tiền phạt xe chốt thực tế
    public LatePaymentPenaltyDetailStatus Status { get; set; } = LatePaymentPenaltyDetailStatus.Pending;
    public string? Note { get; set; }                     // Ghi chú chi tiết dòng xe
}

/// <summary>Bảng kê thanh toán chi phí vận chuyển &amp; bảo hiểm xe — tương ứng Pmt_TransportIns trong BizHTC.Payment / 0.34.Contract / FrmQuanLyThanhToanVanTaiBaoHiem.</summary>
public sealed class TransportInsPayment
{
    public long Id { get; set; }
    public Guid OrgId { get; set; }
    public string TransportInsNo { get; set; } = "";             // Số bảng kê thanh toán (ví dụ: VTBH-202505-001)
    public string PmtMonth { get; set; } = "";                   // Kỳ / tháng thanh toán (ví dụ: 2025-05)
    public string TransporterCode { get; set; } = "";            // Mã đơn vị vận tải (TRANS-NEWWAY, TRANS-DATDUC...)
    public string TransporterName { get; set; } = "";            // Tên đơn vị vận tải
    public string InsuranceCompanyCode { get; set; } = "PVI";    // Mã công ty bảo hiểm (INS-PVI, INS-BAOVIET...)
    public string InsuranceCompanyName { get; set; } = "Tổng Công ty Bảo hiểm Dầu khí (PVI)"; // Tên công ty bảo hiểm
    public string? InsuranceContractNo { get; set; }             // Số hợp đồng bảo hiểm vận tải nguyên tắc
    public int TotalVehicles { get; set; }                       // Tổng số lượng xe vận chuyển
    public long TotalTransportCost { get; set; }                 // Tổng cước phí vận chuyển thực tế (TotalTransportCost = Σ TFValReal)
    public long TotalDelayPenalty { get; set; }                  // Tổng tiền phạt giao chậm xe thực tế (TotalDelayPenalty = Σ TPValReal)
    public long TotalInsuranceCost { get; set; }                 // Tổng phí bảo hiểm vận tải hàng hóa (TotalInsuranceCost = Σ InsuranceCost)
    public long TotalAmount { get; set; }                        // Tổng thanh toán sau thuế VAT (= Σ Val_Transport)
    public decimal VATRate { get; set; } = 10.0m;                // Thuế suất VAT (%)
    public long TotalBeforeVAT { get; set; }                     // Tổng tiền trước VAT (= TotalAmount / (1 + VATRate/100))
    public long AmountVAT { get; set; }                          // Tiền thuế VAT (= TotalAmount - TotalBeforeVAT)
    public TransportInsStatus Status { get; set; } = TransportInsStatus.Draft;
    public TransportSignCAStatus TCMSSignStatus { get; set; } = TransportSignCAStatus.Pending; // Trạng thái ký TCMS
    public string? TCMSSignUser { get; set; }                    // Người ký số TCMS
    public DateTime? TCMSSignDTime { get; set; }                 // Thời điểm ký số TCMS
    public TransportSignCAStatus HTVSignStatus { get; set; } = TransportSignCAStatus.Pending;  // Trạng thái ký HTV
    public string? HTVSignUser { get; set; }                     // Người ký số HTV
    public DateTime? HTVSignDTime { get; set; }                  // Thời điểm ký số HTV
    public string? Appr1By { get; set; }                         // Người duyệt cấp 1 (TCMS Thẩm định)
    public DateTime? Appr1DTime { get; set; }                    // Thời điểm duyệt cấp 1
    public string? Appr2By { get; set; }                         // Người duyệt cấp 2 (HTV Phê duyệt)
    public DateTime? Appr2DTime { get; set; }                    // Thời điểm duyệt cấp 2
    public string? SettledBy { get; set; }                       // Kế toán thanh toán qua UNC ngân hàng
    public DateTime? SettledAt { get; set; }                     // Thời điểm thanh toán
    public string? BankTxnRef { get; set; }                      // Mã giao dịch thanh toán UNC ngân hàng
    public string? RejectReason { get; set; }                    // Lý do từ chối bảng kê
    public DateTime? CancelledAt { get; set; }                   // Thời điểm hủy bảng kê
    public string? FilePath { get; set; }                        // Đường dẫn / mã chứng từ file ký số CA
    public string? Remark { get; set; }                          // Ghi chú / diễn giải bảng kê
    public string? CreatedBy { get; set; }                       // Người lập bảng kê
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public List<TransportInsPaymentDetail> Details { get; set; } = [];
}

/// <summary>Chi tiết xe vận chuyển &amp; bảo hiểm trong bảng kê — tương ứng Pmt_TransportInsDetail trong BizHTC.Payment / 0.34.Contract.</summary>
public sealed class TransportInsPaymentDetail
{
    public long Id { get; set; }
    public long TransportInsPaymentId { get; set; }
    public Guid OrgId { get; set; }
    public string VIN { get; set; } = "";                        // Số khung xe (17 ký tự VIN)
    public string? CarId { get; set; }                           // Mã định danh xe nội bộ
    public string ModelCode { get; set; } = "";                  // Dòng xe (SANTAFE, TUCSON, CRETA, ACCENT...)
    public string? ModelName { get; set; }                       // Tên thương mại dòng xe
    public string? SpecCode { get; set; }                        // Mã phiên bản
    public string? SpecDescription { get; set; }                 // Mô tả phiên bản
    public string? ColorName { get; set; }                       // Màu ngoại thất xe
    public string FStorageCode { get; set; } = "";               // Kho xuất xe / điểm đi (KHO-NINHBINH, KHO-DONGANH...)
    public string? FProvinceName { get; set; }                   // Tỉnh xuất phát (Ninh Bình, Hà Nội...)
    public string TStorageCode { get; set; } = "";               // Kho đại lý / điểm đến (KHO-THANHXUAN, KHO-SAIGON...)
    public string? TProvinceName { get; set; }                   // Tỉnh nơi đến (Hà Nội, TP.HCM, Đà Nẵng...)
    public string TranspReqType { get; set; } = "CARTRANSPORT";  // Loại lệnh vận chuyển (CARTRANSPORT, STORAGEREARRANGE, STORAGEREARRCB, CARRETRIEVE)
    public string? DlvMnNo { get; set; }                         // Số biên bản giao nhận / Lệnh vận chuyển
    public DateTime? DlvStartDate { get; set; }                  // Ngày xuất kho bắt đầu vận chuyển
    public int ExpectedDays { get; set; } = 3;                   // Số ngày vận chuyển định mức
    public DateTime? ExpectedDlvEndDate { get; set; }            // Ngày dự kiến đến nơi theo định mức (= DlvStartDate + ExpectedDays)
    public DateTime? DlvEndDate { get; set; }                    // Ngày giao nhận thực tế tại đại lý
    public int DelayDays { get; set; }                           // Số ngày chậm vận chuyển (= Max(0, (DlvEndDate - ExpectedDlvEndDate).Days))
    public long TFValReal { get; set; }                          // Cước phí vận chuyển thực tế xe (Transport Fee Real)
    public long TPValReal { get; set; }                          // Tiền phạt chậm giao xe thực tế (Transport Penalty Real)
    public long PriceCar { get; set; }                           // Giá trị xe khai báo bảo hiểm hàng hóa
    public decimal InsurancePercent { get; set; } = 0.05m;       // Tỷ lệ phí bảo hiểm vận tải (%)
    public long InsuranceCost { get; set; }                      // Phí bảo hiểm vận tải hàng hóa xe
    public long Val_Transport { get; set; }                      // Tổng tiền thanh toán dòng (= TFValReal + InsuranceCost - TPValReal)
    public string? StandardRemark { get; set; }                  // Lý do điều chỉnh hạn mức định mức ngày đến
    public string? FProvinceRemark { get; set; }                 // Lý do điều chỉnh cung đường / nơi đến
    public TransportInsDetailStatus Status { get; set; } = TransportInsDetailStatus.Pending;
    public string? Remark { get; set; }                          // Ghi chú chi tiết dòng xe
}

/// <summary>Bảng kê thanh toán chi phí lưu kho xe ô tô — tương ứng Pmt_PaymentStorage trong BizHTC.Payment / 0.34.Contract / FrmQuanLyThanhToanLuuKho.</summary>
public sealed class PaymentStorage
{
    public long Id { get; set; }
    public Guid OrgId { get; set; }
    public string PaymentStorageNo { get; set; } = "";             // Số bảng kê thanh toán lưu kho (LK-202505-001)
    public string PmtMonth { get; set; } = "";                     // Kỳ / tháng thanh toán lưu kho (ví dụ: 2025-05)
    public string StorageOperatorCode { get; set; } = "KHO-NBD";   // Mã đơn vị vận hành kho xe (KHO-NBD, KHO-HP, KHO-DN, KHO-BD...)
    public string StorageOperatorName { get; set; } = "Tổng kho Phân phối Ô tô Hyundai Ninh Bình"; // Tên đơn vị quản lý kho
    public int TotalVehicles { get; set; }                         // Tổng số lượng xe lưu kho trong kỳ
    public long TotalCoatCost { get; set; }                        // Tổng chi phí bạt che phủ bảo quản xe (= Σ CostCoat)
    public long TotalStorageCost { get; set; }                     // Tổng chi phí lưu giữ kho bãi (= Σ CostStorage)
    public long TotalAmount { get; set; }                          // Tổng chi phí trước thuế VAT (= TotalCoatCost + TotalStorageCost)
    public decimal VATRate { get; set; } = 10.0m;                  // Thuế suất VAT (%)
    public long UnitPriceVAT { get; set; }                         // Tiền thuế VAT (= TotalAmount * VATRate / 100)
    public long AmountTotal { get; set; }                          // Tổng thanh toán sau thuế VAT (= TotalAmount + UnitPriceVAT)
    public PaymentStorageStatus Status { get; set; } = PaymentStorageStatus.Draft;
    public StorageSignCAStatus TCMSSignStatus { get; set; } = StorageSignCAStatus.Pending; // Trạng thái ký TCMS
    public string? TCMSSignUser { get; set; }                      // Người ký số TCMS
    public DateTime? TCMSSignDTime { get; set; }                   // Thời điểm ký số TCMS
    public StorageSignCAStatus HTVSignStatus { get; set; } = StorageSignCAStatus.Pending;  // Trạng thái ký HTV
    public string? HTVSignUser { get; set; }                       // Người ký số HTV
    public DateTime? HTVSignDTime { get; set; }                    // Thời điểm ký số HTV
    public string? Appr1By { get; set; }                           // Người duyệt cấp 1 (HTV Duyệt sơ bộ)
    public DateTime? Appr1DTime { get; set; }                      // Thời điểm duyệt cấp 1
    public string? Appr2By { get; set; }                           // Người duyệt cấp 2 (TCMS Thẩm định duyệt)
    public DateTime? Appr2DTime { get; set; }                      // Thời điểm duyệt cấp 2
    public string? SettledBy { get; set; }                         // Kế toán thanh toán qua UNC ngân hàng
    public DateTime? SettledAt { get; set; }                       // Thời điểm quyết toán chi trả
    public string? BankTxnRef { get; set; }                        // Mã UNC chuyển tiền ngân hàng
    public string? RejectReason { get; set; }                      // Lý do từ chối bảng kê
    public DateTime? CancelledAt { get; set; }                     // Thời điểm hủy bảng kê
    public string? FilePath { get; set; }                          // Đường dẫn / mã chứng từ file ký số CA (CR_PAYMENT_STORAGE.pdf)
    public string? Remark { get; set; }                            // Ghi chú / diễn giải bảng kê
    public string? CreatedBy { get; set; }                         // Người lập bảng kê
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public List<PaymentStorageDetail> Details { get; set; } = [];
}

/// <summary>Chi tiết xe tính phí lưu kho trong bảng kê — tương ứng Pmt_PaymentStorageDetail trong BizHTC.Payment / 0.34.Contract.</summary>
public sealed class PaymentStorageDetail
{
    public long Id { get; set; }
    public long PaymentStorageId { get; set; }
    public Guid OrgId { get; set; }
    public string PaymentStorageNo { get; set; } = "";             // Số bảng kê lưu kho
    public string VIN { get; set; } = "";                          // Số khung xe (17 ký tự VIN)
    public string? CarId { get; set; }                             // Mã xe nội bộ
    public string ModelCode { get; set; } = "";                    // Dòng xe (SANTAFE, TUCSON, CRETA, ACCENT...)
    public string? ModelName { get; set; }                         // Tên thương mại dòng xe
    public string? SpecCode { get; set; }                          // Mã phiên bản xe
    public string? SpecDescription { get; set; }                   // Mô tả chi tiết phiên bản
    public string? ColorExtNameVN { get; set; }                    // Tên màu ngoại thất xe
    public string StorageCodeInit { get; set; } = "KHO-NBD";       // Kho nhập lưu xe
    public DateTime StorageDate { get; set; }                      // Ngày xe nhập kho
    public DateTime? ApprovedDate2 { get; set; }                   // Ngày duyệt lệnh xuất xe LXX A2
    public DateTime? DeliveryOutDate { get; set; }                 // Ngày thực tế xuất kho giao xe
    public string? DealerCode { get; set; }                        // Mã đại lý nhận xe
    public string? DealerName { get; set; }                        // Tên đại lý nhận xe
    public DateTime InCostStorageDate { get; set; }                // Ngày bắt đầu tính phí lưu kho trong kỳ
    public DateTime OutCostStorageDate { get; set; }               // Ngày kết thúc tính phí lưu kho trong kỳ
    public int CostStorageMonth { get; set; }                      // Số ngày lưu kho tính phí trong tháng (= OutCost - InCost + 1)
    public int LevelStorage { get; set; } = 15;                    // Định mức hạn ngày lưu kho (ngày)
    public long DailyStorageRate { get; set; } = 25_000;           // Đơn giá lưu kho/xe/ngày (VND)
    public long CostCoat { get; set; } = 50_000;                   // Chi phí bạt che phủ chống bụi nắng mưa (VND)
    public long CostStorage { get; set; }                          // Chi phí lưu kho xe (= CostStorageMonth * DailyStorageRate)
    public long TotalAmount { get; set; }                          // Tổng chi phí dòng xe (= CostCoat + CostStorage)
    public PaymentStorageDetailStatus Status { get; set; } = PaymentStorageDetailStatus.Pending;
    public string? Remark { get; set; }                            // Ghi chú chi tiết dòng xe
}

/// <summary>Công văn đề nghị gia hạn thời hạn hiệu lực Thư bảo lãnh thanh toán ngân hàng — tương ứng Pmt_GrtClaimExt trong BizHTC.Payment / Biz.HTC.PaymentGrtExt &amp; FrmQLCVanGiaHan_PhatHanhBL.</summary>
public sealed class GuaranteeExtensionDispatch
{
    public long Id { get; set; }
    public Guid OrgId { get; set; }
    public string DispatchNo { get; set; } = "";                     // Số công văn đề nghị (ví dụ: CVGH-202505-001)
    public string DealerCode { get; set; } = "";                     // Mã đại lý đề nghị gia hạn (DealerCode)
    public string DealerName { get; set; } = "";                     // Tên đại lý phân phối
    public string BankCode { get; set; } = "";                       // Mã ngân hàng phát hành thư bảo lãnh (VPB, CTG, TCB, MBB, VCB...)
    public string BankName { get; set; } = "";                       // Tên ngân hàng phát hành thư bảo lãnh
    public string? BankCodeMonitor { get; set; }                     // Ngân hàng giám sát / quản lý tài khoản phong tỏa HTC
    public string FlagIsHTC { get; set; } = "1";                     // Pháp nhân bán xe: "1" = Hyundai Thành Công HTC, "2" = Hyundai Liên Doanh HTV
    public int NumberOfDaysExt { get; set; } = 30;                   // Số ngày đề nghị gia hạn bảo lãnh (NumberOfGuaranteeExt, ví dụ 15, 30 ngày)
    public int TotalCarCount { get; set; }                           // Tổng số lượng xe đề nghị gia hạn
    public long TotalAmount { get; set; }                            // Tổng giá trị bảo lãnh các dòng xe (VND)
    public int TotalCarsNotDelivered { get; set; }                   // Số lượng xe chưa giao nhận thực tế (TotalCarId_NoStart)
    public int TotalCarsDelivered { get; set; }                      // Số lượng xe đã bàn giao đại lý (TotalCarId_Start)
    public GuaranteeExtensionStatus Status { get; set; } = GuaranteeExtensionStatus.Draft;
    public ExtensionSignCAStatus SignCAStatus { get; set; } = ExtensionSignCAStatus.Pending;
    public string? SignedBy { get; set; }                            // Người ký số công văn CA
    public DateTime? SignedAt { get; set; }                          // Ngày giờ ký số CA
    public string? CertThumbprint { get; set; }                      // Dấu vân tay chứng thư số điện tử CA
    public string? BankResponseRef { get; set; }                     // Số văn bản/thông báo chấp thuận gia hạn của ngân hàng
    public DateTime? BankAcceptedAt { get; set; }                    // Ngày giờ ngân hàng chấp thuận
    public string? BankRejectReason { get; set; }                    // Lý do ngân hàng từ chối gia hạn
    public DateTime? CancelledAt { get; set; }                       // Ngày giờ hủy công văn
    public string? FilePath { get; set; }                            // Đường dẫn file công văn ký số điện tử (CR_ClaimPM.pdf)
    public string? Remark { get; set; }                              // Lý do / giải trình đề nghị gia hạn bảo lãnh
    public string? CreatedBy { get; set; }                           // Người lập công văn
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public List<GuaranteeExtensionDetail> Details { get; set; } = [];
}

/// <summary>Chi tiết xe trong công văn đề nghị gia hạn bảo lãnh ngân hàng — tương ứng Pmt_GrtClaimExtDtl trong BizHTC.Payment.</summary>
public sealed class GuaranteeExtensionDetail
{
    public long Id { get; set; }
    public long DispatchId { get; set; }
    public Guid OrgId { get; set; }
    public string? CarId { get; set; }                               // Mã xe nội bộ
    public string VIN { get; set; } = "";                            // Số khung xe (17 ký tự VIN)
    public string ModelCode { get; set; } = "";                      // Mã model xe (SANTAFE, TUCSON, CRETA, ACCENT...)
    public string? ModelName { get; set; }                           // Tên thương mại mẫu xe
    public string? SpecCode { get; set; }                            // Đặc tả phiên bản xe
    public string? SpecDescription { get; set; }                     // Mô tả phiên bản
    public string? ColorName { get; set; }                           // Tên màu xe ngoại thất
    public string? SOCode { get; set; }                              // Mã đơn đặt hàng xe bán buôn
    public string? DlrCtrNo { get; set; }                            // Số Phụ lục hợp đồng mua bán xe
    public string? GuaranteeNo { get; set; }                         // Mã bảo lãnh hệ thống
    public string BankGuaranteeNo { get; set; } = "";                // Số thư bảo lãnh gốc ngân hàng (BankGuaranteeNo)
    public DateTime GrtDateStart { get; set; }                       // Ngày bắt đầu hiệu lực bảo lãnh ban đầu
    public DateTime GrtDateExpired { get; set; }                     // Ngày hết hạn hiệu lực bảo lãnh cũ
    public DateTime ExtendedDate { get; set; }                       // Ngày hết hạn mới sau khi gia hạn (= GrtDateExpired + NumberOfDaysExt)
    public long GrtValue { get; set; }                               // Giá trị bảo lãnh xe áp dụng (VND)
    public long UnitPrice { get; set; }                              // Giá trị xe theo phụ lục hợp đồng (VND)
    public bool IsDelivered { get; set; } = false;                   // Đã giao xe thực tế hay chưa
    public DateTime? DeliveryDate { get; set; }                      // Ngày bàn giao xe thực tế
    public GuaranteeExtensionDetailStatus Status { get; set; } = GuaranteeExtensionDetailStatus.Pending;
    public string? Remark { get; set; }                              // Ghi chú dòng xe
}

/// <summary>Hồ sơ công văn yêu cầu đòi tiền bảo lãnh thanh toán ngân hàng do đại lý quá hạn nợ — tương ứng Pmt_GrtClaim trong BizHTC.Payment / FrmMngGrtClaim &amp; FrmNewGrtClaim.</summary>
public sealed class BankGuaranteeClaim
{
    public long Id { get; set; }
    public Guid OrgId { get; set; }
    public string ClaimNo { get; set; } = "";                        // Số công văn đòi bảo lãnh (ví dụ: CVDBL-202505-001)
    public string DealerCode { get; set; } = "";                     // Mã đại lý vi phạm cam kết thanh toán (DealerCode)
    public string DealerName { get; set; } = "";                     // Tên đại lý phân phối
    public string BankCode { get; set; } = "";                       // Ngân hàng phát hành thư bảo lãnh bị đòi tiền (VCB, TCB, MBB, CTG, VPB...)
    public string BankName { get; set; } = "";                       // Tên ngân hàng phát hành bảo lãnh
    public string? BankCodeMonitor { get; set; }                     // Ngân hàng giám sát / quản lý tài khoản phong tỏa
    public string FlagIsHTC { get; set; } = "1";                     // Pháp nhân: "1" = Hyundai Thành Công Việt Nam (HTC), "2" = Hyundai Thương Mại (HTV)
    public int TotalCarCount { get; set; }                           // Tổng số lượng xe vi phạm nợ cần đòi bảo lãnh
    public long TotalClaimAmount { get; set; }                       // Tổng số tiền đòi ngân hàng thanh toán bồi hoàn (VND)
    public long SettledAmount { get; set; }                          // Số tiền ngân hàng đã giải ngân bồi hoàn thực tế (VND)
    public GuaranteeClaimStatus Status { get; set; } = GuaranteeClaimStatus.Draft;
    public ClaimSignCAStatus SignCAStatus { get; set; } = ClaimSignCAStatus.Pending; // Trạng thái ký số CA
    public string? SignedBy { get; set; }                            // Lãnh đạo ký số điện tử CA
    public DateTime? SignedAt { get; set; }                          // Thời điểm ký số CA
    public string? CertThumbprint { get; set; }                      // Dấu vân tay chứng thư số điện tử CA
    public DateTime? SentToBankAt { get; set; }                      // Thời điểm gửi công văn tới Hội sở Ngân hàng
    public string? BankRefNo { get; set; }                           // Số tiếp nhận / mã tham chiếu hồ sơ từ ngân hàng
    public DateTime? SettledAt { get; set; }                         // Thời điểm ngân hàng giải ngân bồi hoàn
    public string? SettledBy { get; set; }                           // Kế toán xác nhận thu hồi nợ bảo lãnh
    public string? BankTxnRef { get; set; }                          // Mã bút toán / UNC ngân hàng chuyển tiền bồi hoàn
    public string? BankRejectReason { get; set; }                    // Lý do ngân hàng từ chối chi trả bồi hoàn bảo lãnh
    public DateTime? CancelledAt { get; set; }                       // Thời điểm hủy công văn
    public string? CancelReason { get; set; }                        // Lý do hủy công văn (đại lý đã nộp tiền trực tiếp...)
    public string? FilePath { get; set; }                            // Đường dẫn file công văn ký số điện tử (CR_ClaimPM.pdf)
    public string? Remark { get; set; }                              // Căn cứ vi phạm hợp đồng / diễn giải nội dung đòi bảo lãnh
    public string? CreatedBy { get; set; }                           // Chuyên viên tín dụng / pháp chế lập hồ sơ
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public List<BankGuaranteeClaimDetail> Details { get; set; } = [];
}

/// <summary>Chi tiết xe ô tô trong công văn đòi bảo lãnh ngân hàng — tương ứng Pmt_GrtClaimDetail trong BizHTC.Payment.</summary>
public sealed class BankGuaranteeClaimDetail
{
    public long Id { get; set; }
    public long ClaimId { get; set; }
    public Guid OrgId { get; set; }
    public string? CarId { get; set; }                               // Mã xe nội bộ HTC
    public string VIN { get; set; } = "";                            // Số khung xe (17 ký tự VIN)
    public string ModelCode { get; set; } = "";                      // Mã model xe (SANTAFE, TUCSON, CRETA, ACCENT, STAREX...)
    public string? ModelName { get; set; }                           // Tên thương mại mẫu xe
    public string? SpecCode { get; set; }                            // Mã phiên bản
    public string? SpecDescription { get; set; }                     // Diễn giải phiên bản
    public string? ColorName { get; set; }                           // Màu sơn ngoại thất
    public string? SOCode { get; set; }                              // Số đơn đặt hàng bán lẻ
    public string? ContractNo { get; set; }                          // Số phụ lục hợp đồng mua bán buôn xe
    public string? GuaranteeNo { get; set; }                         // Mã bảo lãnh hệ thống HTC
    public string BankGuaranteeNo { get; set; } = "";                // Số thư bảo lãnh chính thức của ngân hàng (BankGuaranteeNo)
    public DateTime DateOpen { get; set; }                           // Ngày phát hành thư bảo lãnh
    public DateTime DateExpired { get; set; }                        // Ngày hết hạn hiệu lực thư bảo lãnh thanh toán
    public int OverdueDays { get; set; }                             // Số ngày quá hạn thanh toán nợ xe
    public long UnitPriceActual { get; set; }                        // Giá trị thực tế của xe theo hợp đồng (VND)
    public long GrtValue { get; set; }                               // Số tiền bảo lãnh của xe yêu cầu ngân hàng trích thanh toán (VND)
    public double GrtPercent { get; set; } = 100.0;                  // Tỷ lệ % bảo lãnh xe
    public GuaranteeClaimDetailStatus Status { get; set; } = GuaranteeClaimDetailStatus.Pending; // Trạng thái xe đòi nợ
    public string? Remark { get; set; }                              // Ghi chú chi tiết dòng xe
}

/// <summary>Bảng kê thanh toán chi phí thiết bị âm thanh giải trí & định vị dẫn đường AVN xe ô tô — tương ứng Pmt_PaymentAVN trong BizHTC.Payment / FrmQuanLyThanhToanAVN &amp; FrmTaoThanhToanAVN.</summary>
public sealed class PaymentAVN
{
    public long Id { get; set; }
    public Guid OrgId { get; set; }
    public string PaymentAVNNo { get; set; } = "";             // Số bảng kê thanh toán AVN (ví dụ: AVN-202505-001)
    public string PmtMonth { get; set; } = "";                 // Kỳ / tháng thanh toán AVN (ví dụ: 2025-05)
    public string SupplierCode { get; set; } = "MOBIS-VN";     // Mã đơn vị cung cấp linh kiện / lắp ráp AVN (MOBIS-VN, MOTREX-VN, VIETTEL-AVN...)
    public string SupplierName { get; set; } = "Công ty TNHH Mobis Auto Parts Việt Nam"; // Tên nhà cung cấp / đối tác AVN
    public int TotalVehicles { get; set; }                     // Tổng số lượng xe gắn thiết bị AVN trong kỳ
    public long TotalAmount { get; set; }                      // Tổng chi phí AVN trước thuế VAT (= Σ UnitPriceAVN)
    public decimal VATRate { get; set; } = 10.0m;              // Thuế suất VAT (%)
    public long AmountVAT { get; set; }                        // Tiền thuế VAT (= TotalAmount * VATRate / 100)
    public long TotalAmountAfterVAT { get; set; }              // Tổng giá trị thanh toán sau thuế VAT (= TotalAmount + AmountVAT)
    public PaymentAVNStatus Status { get; set; } = PaymentAVNStatus.Draft; // Trạng thái bảng kê
    public AVNSignCAStatus TCMSSignStatus { get; set; } = AVNSignCAStatus.Pending; // Trạng thái ký TCMS (P: Chưa ký, A: Đã ký)
    public string? TCMSSignUser { get; set; }                  // Người ký số TCMS
    public DateTime? TCMSSignDTime { get; set; }               // Thời điểm ký số TCMS
    public AVNSignCAStatus HTVSignStatus { get; set; } = AVNSignCAStatus.Pending;  // Trạng thái ký HTV (P: Chưa ký, A: Đã ký)
    public string? HTVSignUser { get; set; }                   // Người ký số HTV
    public DateTime? HTVSignDTime { get; set; }                // Thời điểm ký số HTV
    public string? Appr1By { get; set; }                       // Người duyệt cấp 1 (TCMS Thẩm định duyệt sơ bộ A1)
    public DateTime? Appr1DTime { get; set; }                  // Thời điểm duyệt cấp 1
    public string? Appr2By { get; set; }                       // Người duyệt cấp 2 (HTV Ban Kế toán/Phụ tùng duyệt A2)
    public DateTime? Appr2DTime { get; set; }                  // Thời điểm duyệt cấp 2
    public string? SettledBy { get; set; }                     // Kế toán thanh toán qua UNC ngân hàng
    public DateTime? SettledAt { get; set; }                   // Thời điểm quyết toán chi trả
    public string? BankTxnRef { get; set; }                    // Mã bút toán / UNC ngân hàng chuyển tiền
    public string? RejectReason { get; set; }                  // Lý do từ chối bảng kê
    public DateTime? CancelledAt { get; set; }                 // Thời điểm hủy bảng kê
    public string? FilePath { get; set; }                      // Đường dẫn / mã chứng từ file ký số CA (CR_PAYMENT_AVN.pdf)
    public string? Remark { get; set; }                        // Ghi chú / diễn giải bảng kê
    public string? CreatedBy { get; set; }                     // Người lập bảng kê
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public List<PaymentAVNDetail> Details { get; set; } = [];
}

/// <summary>Chi tiết dòng xe tính phí thiết bị AVN — tương ứng Pmt_PaymentAVNDetail trong BizHTC.Payment.</summary>
public sealed class PaymentAVNDetail
{
    public long Id { get; set; }
    public long PaymentAVNId { get; set; }
    public Guid OrgId { get; set; }
    public string PaymentAVNNo { get; set; } = "";             // Số bảng kê thanh toán AVN
    public string VIN { get; set; } = "";                      // Số khung xe (17 ký tự VIN)
    public string? EngineNo { get; set; }                      // Số máy xe
    public string ModelCode { get; set; } = "";                // Mã model xe (SANTAFE, TUCSON, CRETA, ACCENT, ELANTRA...)
    public string? ModelName { get; set; }                     // Tên thương mại dòng xe
    public string? SpecCode { get; set; }                      // Mã phiên bản đặc tả kỹ thuật
    public string? SpecDescription { get; set; }               // Diễn giải phiên bản xe
    public string? ColorName { get; set; }                     // Màu sơn ngoại thất
    public string AVNCode { get; set; } = "";                  // Mã chủng loại thiết bị AVN (AVN-SANTAFE-GEN5, AVN-TUCSON-1025...)
    public string SerialNo { get; set; } = "";                 // Số serial thiết bị AVN dập trên vỏ/màn hình
    public long UnitPriceAVN { get; set; }                     // Đơn giá thiết bị AVN (VND) tra theo danh mục Mst_UnitPriceAVN
    public DateTime? AVNDate { get; set; }                     // Ngày lắp ráp / kích hoạt hệ thống AVN
    public DateTime? InStorageDate { get; set; }               // Ngày xe hoàn thiện nhập kho lưu bãi nhà máy
    public PaymentAVNDetailStatus Status { get; set; } = PaymentAVNDetailStatus.Pending; // Trạng thái dòng
    public string? Remark { get; set; }                        // Ghi chú chi tiết dòng xe
}

public sealed class PaymentAVNItemInputDto
{
    public string VIN { get; set; } = "";
    public string? EngineNo { get; set; }
    public string ModelCode { get; set; } = "";
    public string? ModelName { get; set; }
    public string? SpecCode { get; set; }
    public string? SpecDescription { get; set; }
    public string? ColorName { get; set; }
    public string AVNCode { get; set; } = "";
    public string SerialNo { get; set; } = "";
    public long? UnitPriceAVN { get; set; }
    public DateTime? AVNDate { get; set; }
    public DateTime? InStorageDate { get; set; }
    public string? Remark { get; set; }
}

public sealed class PaymentAVNAdviceDto
{
    public string PaymentAVNNo { get; set; } = "";
    public string PmtMonth { get; set; } = "";
    public string PmtMonthFormatted { get; set; } = "";
    public string PrintDate { get; set; } = "";
    public string SupplierName { get; set; } = "";
    public int TotalVehicles { get; set; }
    public long TotalAmount { get; set; }
    public decimal VATRate { get; set; }
    public long AmountVAT { get; set; }
    public long TotalAmountAfterVAT { get; set; }
    public string AmountInWords { get; set; } = "";
    public string StatusText { get; set; } = "";
    public string TCMSSignInfo { get; set; } = "";
    public string HTVSignInfo { get; set; } = "";
    public string? BankTxnRef { get; set; }
    public List<PaymentAVNDetailAdviceDto> Items { get; set; } = [];
}

public sealed class PaymentAVNDetailAdviceDto
{
    public int No { get; set; }
    public string VIN { get; set; } = "";
    public string ModelCode { get; set; } = "";
    public string ModelName { get; set; } = "";
    public string SpecCode { get; set; } = "";
    public string SpecDescription { get; set; } = "";
    public string EngineNo { get; set; } = "";
    public string AVNCode { get; set; } = "";
    public string SerialNo { get; set; } = "";
    public long UnitPriceAVN { get; set; }
    public string? AVNDate { get; set; }
    public string? InStorageDate { get; set; }
    public string Status { get; set; } = "";
}

public sealed class PaymentAVNSummaryDto
{
    public int TotalStatements { get; set; }
    public int DraftCount { get; set; }
    public int ApprovedCount { get; set; }
    public int SignedCount { get; set; }
    public int SettledCount { get; set; }
    public int CancelledCount { get; set; }
    public int TotalVehiclesInstalled { get; set; }
    public long TotalAmountBeforeVAT { get; set; }
    public long TotalVATAmount { get; set; }
    public long TotalSettledAmount { get; set; }
}

/// <summary>Bảng kê thanh toán chi phí quản lý & giám sát thiết bị định vị vệ tinh GPS trên xe ô tô — tương ứng Pmt_PaymentGPS trong BizHTC.Payment / FrmQuanLyThanhToanGPS &amp; FrmTaoThanhToanGPS.</summary>
public sealed class PaymentGPS
{
    public long Id { get; set; }
    public Guid OrgId { get; set; }
    public string PaymentGPSNo { get; set; } = "";             // Số bảng kê thanh toán GPS (ví dụ: GPS-202505-001)
    public string PmtMonth { get; set; } = "";                 // Kỳ / tháng thanh toán GPS (ví dụ: 2025-05)
    public string ContractNo { get; set; } = "HD-GPS-VIETTEL"; // Số hợp đồng cung cấp dịch vụ định vị GPS
    public string ProviderCode { get; set; } = "VIETTEL";      // Mã đối tác viễn thông / thiết bị GPS (VIETTEL, VNPT, BAGPS, BKAV...)
    public string ProviderName { get; set; } = "Tổng Công ty Viễn thông Viettel (Viettel Telecom)"; // Tên nhà cung cấp dịch vụ
    public int TotalVehicles { get; set; }                     // Tổng số lượng xe lắp đặt / kích hoạt GPS trong kỳ
    public int TotalPlanDays { get; set; }                     // Tổng số ngày tính phí dự kiến (Σ PlanCostGPSDate)
    public int TotalDeductDays { get; set; }                   // Tổng số ngày khấu trừ (Σ DeductDate)
    public int TotalActualDays { get; set; }                   // Tổng số ngày tính phí thực tế (Σ ActualCostGPSDate)
    public long AmountTotal { get; set; }                      // Tổng chi phí GPS trước thuế VAT (= Σ AmountGPS)
    public decimal VATRate { get; set; } = 10.0m;              // Thuế suất VAT (%)
    public long UnitPriceVAT { get; set; }                     // Tiền thuế VAT (= AmountTotal * VATRate / 100)
    public long TotalAmountVAT { get; set; }                   // Tổng giá trị thanh toán sau thuế VAT (= AmountTotal + UnitPriceVAT)
    public PaymentGPSStatus Status { get; set; } = PaymentGPSStatus.Draft; // Trạng thái bảng kê (P, A1, A2, F, Settled, Rejected, C)
    public GPSSignCAStatus HTVSignStatus { get; set; } = GPSSignCAStatus.Pending;  // Trạng thái ký HTV (P: Chưa ký, A: Đã ký)
    public string? HTVSignUser { get; set; }                   // Người đại diện HTV ký số CA
    public DateTime? HTVSignDTime { get; set; }                // Thời điểm ký số HTV
    public GPSSignCAStatus TCMSSignStatus { get; set; } = GPSSignCAStatus.Pending; // Trạng thái ký TCMS (P: Chưa ký, A: Đã ký)
    public string? TCMSSignUser { get; set; }                  // Người đại diện TCMS ký số CA
    public DateTime? TCMSSignDTime { get; set; }               // Thời điểm ký số TCMS
    public string? Appr1By { get; set; }                       // Người duyệt cấp 1 (HTV Thẩm định duyệt sơ bộ A1)
    public DateTime? Appr1DTime { get; set; }                  // Thời điểm duyệt cấp 1
    public string? Appr2By { get; set; }                       // Người duyệt cấp 2 (TCMS Ban Tài chính / Kế toán duyệt A2)
    public DateTime? Appr2DTime { get; set; }                  // Thời điểm duyệt cấp 2
    public string? SettledBy { get; set; }                     // Kế toán quyết toán thanh toán chuyển khoản UNC ngân hàng
    public DateTime? SettledAt { get; set; }                   // Thời điểm quyết toán chi trả
    public string? BankTxnRef { get; set; }                    // Mã bút toán / số UNC ủy nhiệm chi ngân hàng
    public string? RejectReason { get; set; }                  // Lý do từ chối bảng kê
    public DateTime? CancelledAt { get; set; }                 // Thời điểm hủy bảng kê
    public string? FilePath { get; set; }                      // Đường dẫn / mã chứng từ file ký số CA (CR_Pmt_PaymentGPS.pdf)
    public string? Remark { get; set; }                        // Ghi chú / diễn giải bảng kê
    public string? CreatedBy { get; set; }                     // Người lập bảng kê
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public List<PaymentGPSDetail> Details { get; set; } = [];
}

/// <summary>Chi tiết dòng xe tính phí dịch vụ định vị GPS — tương ứng Pmt_PaymentGPSDetail trong BizHTC.Payment.</summary>
public sealed class PaymentGPSDetail
{
    public long Id { get; set; }
    public long PaymentGPSId { get; set; }
    public Guid OrgId { get; set; }
    public string PaymentGPSNo { get; set; } = "";             // Số bảng kê thanh toán GPS
    public string VIN { get; set; } = "";                      // Số khung xe (17 ký tự VIN)
    public string? EngineNo { get; set; }                      // Số máy xe
    public string? CarID { get; set; }                         // Mã định danh xe kho nội bộ
    public string ModelCode { get; set; } = "";                // Mã model xe (SANTAFE, TUCSON, CRETA, ACCENT, ELANTRA...)
    public string? ModelName { get; set; }                     // Tên thương mại dòng xe
    public string? SpecCode { get; set; }                      // Mã phiên bản đặc tả kỹ thuật
    public string? SpecDescription { get; set; }               // Diễn giải phiên bản xe
    public string GPSID { get; set; } = "";                    // Mã / serial / IMEI thiết bị định vị vệ tinh GPS
    public string ContractGPS { get; set; } = "HD-GPS-VIETTEL";// Hợp đồng dịch vụ GPS áp dụng
    public DateTime? GPSStartDate { get; set; }                // Thời điểm map thiết bị GPS vào xe
    public DateTime? RetailDate { get; set; }                  // Ngày đại lý khai báo bán lẻ / bàn giao xe
    public DateTime CostGPSStartDate { get; set; }             // Ngày bắt đầu tính phí GPS trong kỳ
    public DateTime CostGPSEndDate { get; set; }               // Ngày kết thúc tính phí GPS trong kỳ
    public int PlanCostGPSDate { get; set; }                   // Số ngày tính phí GPS dự kiến (= (End - Start).Days + 1)
    public int DeductDate { get; set; }                        // Số ngày khấu trừ (ngưng phát sóng / bảo trì xe)
    public int ActualCostGPSDate { get; set; }                 // Số ngày tính phí GPS thực tế (= PlanCostGPSDate - DeductDate)
    public long PriceGPS { get; set; }                         // Đơn giá thuê bao / quản lý GPS theo ngày (VND/ngày)
    public long AmountGPS { get; set; }                        // Phí quản lý GPS của xe (= ActualCostGPSDate * PriceGPS)
    public PaymentGPSDetailStatus Status { get; set; } = PaymentGPSDetailStatus.Pending; // Trạng thái dòng
    public string? Remark { get; set; }                        // Ghi chú chi tiết dòng xe
}

/// <summary>Bảng đơn giá định mức phí dịch vụ GPS theo hợp đồng — tương ứng Mst_UnitPriceGPS / TblMst_UnitPriceGPS.</summary>
public sealed class UnitPriceGPS
{
    public long Id { get; set; }
    public Guid OrgId { get; set; }
    public string ContractNo { get; set; } = "";               // Số hợp đồng nhà cung cấp GPS (CONTRACTNO)
    public string ProviderCode { get; set; } = "";             // Mã nhà cung cấp (VIETTEL, VNPT, BAGPS, BKAV)
    public string ProviderName { get; set; } = "";             // Tên đối tác cung cấp dịch vụ GPS
    public long DailyPrice { get; set; }                       // Đơn giá cước theo ngày (VND/ngày)
    public long MonthlyRate { get; set; }                      // Mức cước tháng tham chiếu (VND/tháng)
    public DateTime EffectiveStartDate { get; set; }           // Ngày bắt đầu hiệu lực hợp đồng (EFFSTARTDATE)
    public bool IsActive { get; set; } = true;                 // Cờ kích hoạt (FLAGACTIVE)
    public string? Remark { get; set; }                        // Ghi chú điều khoản hợp đồng
}

public sealed class PaymentGPSItemInputDto
{
    public string VIN { get; set; } = "";
    public string? EngineNo { get; set; }
    public string? CarID { get; set; }
    public string ModelCode { get; set; } = "";
    public string? ModelName { get; set; }
    public string? SpecCode { get; set; }
    public string? SpecDescription { get; set; }
    public string GPSID { get; set; } = "";
    public string? ContractGPS { get; set; }
    public DateTime? GPSStartDate { get; set; }
    public DateTime? RetailDate { get; set; }
    public DateTime? CostGPSStartDate { get; set; }
    public DateTime? CostGPSEndDate { get; set; }
    public int? DeductDate { get; set; }
    public long? PriceGPS { get; set; }
    public string? Remark { get; set; }
}

public sealed class UpdatePaymentGPSDetailItemDto
{
    public long DetailId { get; set; }
    public DateTime? CostGPSStartDate { get; set; }
    public DateTime? CostGPSEndDate { get; set; }
    public int? DeductDate { get; set; }
    public long? PriceGPS { get; set; }
    public string? Remark { get; set; }
}

public sealed class PaymentGPSAdviceDto
{
    public string PaymentGPSNo { get; set; } = "";
    public string PmtMonth { get; set; } = "";
    public string PmtMonthFormatted { get; set; } = "";
    public string PrintDate { get; set; } = "";
    public string ContractNo { get; set; } = "";
    public string ProviderName { get; set; } = "";
    public int TotalVehicles { get; set; }
    public int TotalPlanDays { get; set; }
    public int TotalDeductDays { get; set; }
    public int TotalActualDays { get; set; }
    public long AmountTotal { get; set; }
    public decimal VATRate { get; set; }
    public long UnitPriceVAT { get; set; }
    public long TotalAmountVAT { get; set; }
    public string AmountInWords { get; set; } = "";
    public string StatusText { get; set; } = "";
    public string TCMSSignInfo { get; set; } = "";
    public string HTVSignInfo { get; set; } = "";
    public string? BankTxnRef { get; set; }
    public List<PaymentGPSDetailAdviceDto> Items { get; set; } = [];
}

public sealed class PaymentGPSDetailAdviceDto
{
    public int No { get; set; }
    public string VIN { get; set; } = "";
    public string ModelCode { get; set; } = "";
    public string ModelName { get; set; } = "";
    public string SpecCode { get; set; } = "";
    public string SpecDescription { get; set; } = "";
    public string EngineNo { get; set; } = "";
    public string GPSID { get; set; } = "";
    public string ContractGPS { get; set; } = "";
    public string? GPSStartDate { get; set; }
    public string? RetailDate { get; set; }
    public string CostGPSStartDate { get; set; } = "";
    public string CostGPSEndDate { get; set; } = "";
    public int PlanCostGPSDate { get; set; }
    public int DeductDate { get; set; }
    public int ActualCostGPSDate { get; set; }
    public long PriceGPS { get; set; }
    public long AmountGPS { get; set; }
    public string Status { get; set; } = "";
}

public sealed class PaymentGPSSummaryDto
{
    public int TotalStatements { get; set; }
    public int DraftCount { get; set; }
    public int ApprovedCount { get; set; }
    public int SignedCount { get; set; }
    public int SettledCount { get; set; }
    public int CancelledCount { get; set; }
    public int TotalVehiclesTracked { get; set; }
    public int TotalTrackedDays { get; set; }
    public long TotalAmountBeforeVAT { get; set; }
    public long TotalVATAmount { get; set; }
    public long TotalSettledAmount { get; set; }
}

public sealed class CandidateVehicleGPSDto
{
    public string VIN { get; set; } = "";
    public string? EngineNo { get; set; }
    public string ModelCode { get; set; } = "";
    public string ModelName { get; set; } = "";
    public string SpecCode { get; set; } = "";
    public string SpecDescription { get; set; } = "";
    public string GPSDvNo { get; set; } = "";
    public string ContractNo { get; set; } = "";
    public DateTime GPSMapVINDateTime { get; set; }
    public DateTime? DealDate { get; set; }
    public long DefaultDailyPrice { get; set; }
}

/// <summary>Bảng tính Hỗ trợ Chi phí Tài chính (CPTC) và Chiết khấu Thanh toán TCG (CKTT) cho Đại lý — tương ứng DMS40_FnExp_Calc_FnExp_PmDc trong BizHTC.Payment / 0.41.CalcFnExp &amp; FrmDMS40_2019_MngDMS40_FnExp_Calc_FnExp_PmDc.</summary>
public sealed class FinancialExpenseStatement
{
    public long Id { get; set; }
    public Guid OrgId { get; set; }
    public string CaNo { get; set; } = "";                             // Số bảng tính chi phí tài chính (CANO)
    public string DealerCode { get; set; } = "";                     // Mã đại lý thụ hưởng (DEALERCODE)
    public string DealerName { get; set; } = "";                     // Tên đại lý phân phối
    public string CAName { get; set; } = "";                         // Tiêu đề / tên đợt quyết toán (CANAME)
    public DateTime TermFrom { get; set; }                           // Kỳ tính CPTC từ ngày (TERMFROM)
    public DateTime TermTo { get; set; }                             // Kỳ tính CPTC đến ngày (TERMTO)
    public DateTime TermPrevFrom { get; set; }                       // Kỳ tính CPTC trước từ ngày (TERMPREVFROM)
    public DateTime TermPrevTo { get; set; }                         // Kỳ tính CPTC trước đến ngày (TERMPREVTO)
    public decimal FnExpPercent { get; set; } = 8.5m;                // Tỷ lệ lãi suất hỗ trợ chi phí tài chính %/năm (FNEXPPERCENT)
    public decimal PmtDsTCGPercent { get; set; } = 1.2m;             // Tỷ lệ chiết khấu thanh toán sớm TCG % (PMTDSTCGPERCENT)
    public int TotalVehicles { get; set; }                           // Tổng số xe trong bảng tính
    public long TotalFnDepositAmount { get; set; }                   // Tổng tiền CPTC tiền cọc (FNDEPOSITAMOUNT)
    public long TotalFnGrtAmount { get; set; }                       // Tổng tiền CPTC bảo lãnh (FNGRTAMOUNT)
    public long TotalFnAmount { get; set; }                          // Tổng tiền hỗ trợ chi phí tài chính (= TotalFnDeposit + TotalFnGrt)
    public long TotalPDAmount { get; set; }                          // Tổng tiền chiết khấu thanh toán sớm TCG (PDAMOUNT)
    public long TotalSettlementAmount { get; set; }                  // Tổng tiền quyết toán hỗ trợ đại lý (= TotalFnAmount + TotalPDAmount)
    public FinancialExpenseStatus Status { get; set; } = FinancialExpenseStatus.Draft; // Trạng thái bảng kê
    public FnExpSignCAStatus DlrSignStatus { get; set; } = FnExpSignCAStatus.Pending; // Trạng thái ký số ĐL (DLRSIGNSTATUS)
    public string? DlrSignUser { get; set; }                         // Giám đốc đại lý ký số CA
    public DateTime? DlrSignDTime { get; set; }                      // Thời điểm đại lý ký số CA
    public FnExpSignCAStatus HTCSignStatus { get; set; } = FnExpSignCAStatus.Pending; // Trạng thái ký số HTC (HTCSIGNSTATUS)
    public string? HTCSignUser { get; set; }                         // Lãnh đạo HTC/HTV ký số CA
    public DateTime? HTCSignDTime { get; set; }                      // Thời điểm HTC ký số CA
    public string? DlrAppr1By { get; set; }                          // Kế toán trưởng đại lý thẩm định duyệt cấp 1 (DLRAPPR1BY)
    public DateTime? DlrAppr1DTime { get; set; }                     // Thời điểm đại lý duyệt cấp 1
    public string? HTCAppr1By { get; set; }                          // Chuyên viên QLPP&TC HTC thẩm định duyệt cấp 1 (HTCAPPR1BY)
    public DateTime? HTCAppr1DTime { get; set; }                     // Thời điểm HTC duyệt cấp 1
    public string? SettledBy { get; set; }                           // Kế toán thanh toán lập UNC quyết toán
    public DateTime? SettledAt { get; set; }                         // Thời điểm hoàn tất quyết toán
    public string? BankTxnRef { get; set; }                          // Số UNC / mã giao dịch ngân hàng chuyển khoản
    public string? CancelBy { get; set; }                            // Người hủy bảng tính
    public DateTime? CancelDTime { get; set; }                       // Thời điểm hủy bảng tính
    public string? CancelReason { get; set; }                        // Lý do hủy bảng tính
    public string? FilePathFnExp { get; set; }                       // Đường dẫn file chứng từ CPTC ký số CA
    public string? FilePathPmtDc { get; set; }                       // Đường dẫn file chứng từ CKTT ký số CA
    public string? Remark { get; set; }                              // Ghi chú đợt quyết toán
    public string? CreatedBy { get; set; }                           // Người lập bảng tính
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public List<FinancialExpenseDetail> Details { get; set; } = [];
}

/// <summary>Chi tiết dòng xe tính CPTC và CKTT — tương ứng DMS40_FnExp_Calc_FnExp_PmDcDtl trong BizHTC.Payment.</summary>
public sealed class FinancialExpenseDetail
{
    public long Id { get; set; }
    public long StatementId { get; set; }
    public Guid OrgId { get; set; }
    public string CaNo { get; set; } = "";                           // Số bảng tính (CANO)
    public string? CarId { get; set; }                               // Mã xe nội bộ kho HTC (CARID)
    public string VIN { get; set; } = "";                            // Số khung xe (17 ký tự VIN)
    public string ModelCode { get; set; } = "";                      // Mã model xe (SANTAFE, TUCSON, CRETA, ACCENT, CUSTIN, IONIQ5...)
    public string? ModelName { get; set; }                           // Tên thương mại mẫu xe
    public string? SpecCode { get; set; }                            // Mã đặc tả phiên bản xe
    public string? SpecDescription { get; set; }                     // Diễn giải phiên bản xe
    public string? ColorName { get; set; }                           // Màu sắc ngoại thất
    public string? SOCode { get; set; }                              // Số đơn đặt hàng bán buôn (SOCODE)
    public VehicleAssemblyType AssemblyType { get; set; } = VehicleAssemblyType.CKD; // Phân loại: CKD (lắp ráp) hoặc CBU (nhập khẩu)
    public long UnitPriceActual { get; set; }                        // Giá trị thực tế của xe sau chiết khấu bán buôn (UNITPRICEACTUAL)
    public DateTime? SodApprovedDate { get; set; }                   // Ngày xác nhận đơn hàng xe (SODAPPROVEDDATE)
    public DateTime? SodDepositDutyEndDate { get; set; }             // Hạn nộp tiền cọc hợp đồng (SODDEPOSITDUTYENDDATE)
    public DateTime? TotalCompletedDate { get; set; }                // Ngày đại lý hoàn tất thanh toán 100% giá trị xe (TOTALCOMPLETEDDATE)
    public DateTime? DateStart { get; set; }                         // Ngày bắt đầu hiệu lực bảo lãnh/khoản vay (DATESTART)
    public DateTime? DateEnd { get; set; }                           // Ngày kết thúc nghĩa vụ thanh toán / hạn bảo lãnh (DATEEND)
    public int TermActual { get; set; }                              // Kỳ hạn tài trợ thực tế (ngày) (TERMACTUAL)
    public int FnDepositCountDate { get; set; }                      // Số ngày hỗ trợ chi phí tài chính tiền cọc (FNDEPOSITCOUNTDATE)
    public long FnDepositAmount { get; set; }                        // Số tiền CPTC tiền cọc (FNDEPOSITAMOUNT)
    public int FnGrtCountDate { get; set; }                          // Số ngày hỗ trợ chi phí tài chính bảo lãnh (FNGRTCOUNTDATE)
    public long FnGrtAmount { get; set; }                            // Số tiền CPTC bảo lãnh (FNGRTAMOUNT)
    public long FnTotalAmount { get; set; }                          // Tổng số tiền CPTC của xe (= FnDepositAmount + FnGrtAmount)
    public int PDCountDate { get; set; }                             // Số ngày thanh toán sớm hưởng chiết khấu (PDCOUNTDATE)
    public long PDAmount { get; set; }                               // Số tiền chiết khấu thanh toán sớm TCG (PDAMOUNT)
    public long CarTotalSettlement { get; set; }                     // Tổng tiền hỗ trợ quyết toán dòng xe (= FnTotalAmount + PDAmount)
    public FinancialExpenseDetailStatus Status { get; set; } = FinancialExpenseDetailStatus.Active; // Trạng thái dòng
    public string? Remark { get; set; }                              // Ghi chú chi tiết xe
}

public sealed class FinancialExpenseItemInputDto
{
    public string VIN { get; set; } = "";
    public string? CarId { get; set; }
    public string ModelCode { get; set; } = "";
    public string? ModelName { get; set; }
    public string? SpecCode { get; set; }
    public string? SpecDescription { get; set; }
    public string? ColorName { get; set; }
    public string? SOCode { get; set; }
    public string AssemblyType { get; set; } = "CKD"; // "CKD" hoặc "CBU"
    public long UnitPriceActual { get; set; }
    public DateTime? SodApprovedDate { get; set; }
    public DateTime? SodDepositDutyEndDate { get; set; }
    public DateTime? TotalCompletedDate { get; set; }
    public DateTime? DateStart { get; set; }
    public DateTime? DateEnd { get; set; }
    public int? TermActual { get; set; }
    public int? FnDepositCountDate { get; set; }
    public int? FnGrtCountDate { get; set; }
    public int? PDCountDate { get; set; }
    public string? Remark { get; set; }
}

public sealed class UpdateFnExpDetailItemDto
{
    public long DetailId { get; set; }
    public int? FnDepositCountDate { get; set; }
    public int? FnGrtCountDate { get; set; }
    public int? PDCountDate { get; set; }
    public DateTime? TotalCompletedDate { get; set; }
    public string? Remark { get; set; }
}

public sealed class FinancialExpenseAdviceDto
{
    public string CaNo { get; set; } = "";
    public string CAName { get; set; } = "";
    public string DealerCode { get; set; } = "";
    public string DealerName { get; set; } = "";
    public string TermFromFormatted { get; set; } = "";
    public string TermToFormatted { get; set; } = "";
    public string TermPrevFromFormatted { get; set; } = "";
    public string TermPrevToFormatted { get; set; } = "";
    public decimal FnExpPercent { get; set; }
    public decimal PmtDsTCGPercent { get; set; }
    public int TotalVehicles { get; set; }
    public long TotalFnDepositAmount { get; set; }
    public long TotalFnGrtAmount { get; set; }
    public long TotalFnAmount { get; set; }
    public long TotalPDAmount { get; set; }
    public long TotalSettlementAmount { get; set; }
    public string TotalSettlementInWords { get; set; } = "";
    public string StatusText { get; set; } = "";
    public string DlrSignInfo { get; set; } = "";
    public string HTCSignInfo { get; set; } = "";
    public string? BankTxnRef { get; set; }
    public string PrintDate { get; set; } = "";
    public List<FinancialExpenseDetailAdviceDto> Items { get; set; } = [];
}

public sealed class FinancialExpenseDetailAdviceDto
{
    public int No { get; set; }
    public string VIN { get; set; } = "";
    public string ModelCode { get; set; } = "";
    public string ModelName { get; set; } = "";
    public string SOCode { get; set; } = "";
    public string AssemblyType { get; set; } = "";
    public long UnitPriceActual { get; set; }
    public string? TotalCompletedDate { get; set; }
    public string? DateEnd { get; set; }
    public int FnDepositCountDate { get; set; }
    public long FnDepositAmount { get; set; }
    public int FnGrtCountDate { get; set; }
    public long FnGrtAmount { get; set; }
    public long FnTotalAmount { get; set; }
    public int PDCountDate { get; set; }
    public long PDAmount { get; set; }
    public long CarTotalSettlement { get; set; }
    public string Status { get; set; } = "";
}

public sealed class FinancialExpenseSummaryDto
{
    public int TotalStatements { get; set; }
    public int DraftCount { get; set; }
    public int PendingDlrApprovalCount { get; set; }
    public int PendingHTCApprovalCount { get; set; }
    public int SignedCount { get; set; }
    public int SettledCount { get; set; }
    public int CancelledCount { get; set; }
    public int TotalVehiclesSubsidized { get; set; }
    public long TotalFnDepositSubsidized { get; set; }
    public long TotalFnGrtSubsidized { get; set; }
    public long TotalFnAmountSubsidized { get; set; }
    public long TotalEarlyPaymentDiscountSubsidized { get; set; }
    public long TotalSettlementDisbursed { get; set; }
}

public sealed class CandidateVehicleFnExpDto
{
    public string VIN { get; set; } = "";
    public string? CarId { get; set; }
    public string DealerCode { get; set; } = "";
    public string DealerName { get; set; } = "";
    public string ModelCode { get; set; } = "";
    public string ModelName { get; set; } = "";
    public string SpecCode { get; set; } = "";
    public string SpecDescription { get; set; } = "";
    public string ColorName { get; set; } = "";
    public string SOCode { get; set; } = "";
    public string AssemblyType { get; set; } = "CKD";
    public long UnitPriceActual { get; set; }
    public DateTime SodApprovedDate { get; set; }
    public DateTime SodDepositDutyEndDate { get; set; }
    public DateTime TotalCompletedDate { get; set; }
    public DateTime DateStart { get; set; }
    public DateTime DateEnd { get; set; }
    public int TermActual { get; set; }
    public int FnDepositCountDate { get; set; }
    public int FnGrtCountDate { get; set; }
    public int PDCountDate { get; set; }
}




/// <summary>Hồ sơ Đề nghị Giao dịch Ngân hàng & Tài trợ Vốn Vay / Bảo lãnh / L/C cho Đại lý Xe Ô tô — tương ứng RQ_BankingTransactions trong BizHTC.Payment / FrmDeNghiGDNganHang &amp; FrmQL_DeNghiGDNganHang.</summary>
public sealed class BankingDisbursementRequest
{
    public long Id { get; set; }
    public Guid OrgId { get; set; }
    public string TransNo { get; set; } = "";                     // Số đề nghị (RQ_BankingTransNo, ví dụ: DNTT-202505-001)
    public BankingTransType TransType { get; set; } = BankingTransType.GNTT; // Loại ĐN: GNTT, PhatHanhBLLC, PhatHanhLC, HTDB, GNTTLC
    public string DealerCode { get; set; } = "";                 // Mã đại lý (DEALERCODE)
    public string DealerName { get; set; } = "";                 // Tên đại lý phân phối
    public string BizResNumber { get; set; } = "";               // Số ĐKKD / Mã số thuế (BIZRESNUMBER)
    public string BankCode { get; set; } = "";                   // Mã ngân hàng tài trợ (VPBANK, VIETINBANK, VIB, TCB, MBB...)
    public string BankName { get; set; } = "";                   // Tên ngân hàng tài trợ
    public string? PaymentAccount { get; set; }                  // Số tài khoản trích nợ / vay của đại lý
    public string? PaymentBankName { get; set; }                 // Ngân hàng mở tài khoản đại lý
    public string ReceivingUnit { get; set; } = "CÔNG TY CỔ PHẦN LIÊN DOANH Ô TÔ HYUNDAI THÀNH CÔNG VIỆT NAM"; // Đơn vị thụ hưởng
    public string ReceivingAccount { get; set; } = "113000088999"; // Số tài khoản thụ hưởng của hãng xe
    public string ReceivingBank { get; set; } = "VietinBank - CN Đống Đa"; // Ngân hàng thụ hưởng
    public int TotalCars { get; set; }                           // Tổng số xe đề nghị tài trợ
    public long TotalContractAmount { get; set; }                // Tổng giá trị xe theo hợp đồng (VND)
    public long TotalDisbursementAmount { get; set; }            // Tổng số tiền đề nghị ngân hàng tài trợ / giải ngân (VND)
    public long ActualDisbursedAmount { get; set; }              // Số tiền ngân hàng đã giải ngân thực tế (VND)
    public BankingTransStatus Status { get; set; } = BankingTransStatus.Draft; // Trạng thái nội bộ (BKTRANSSTATUS)
    public BankingTransBankStatus BankStatus { get; set; } = BankingTransBankStatus.Pending; // Trạng thái phía ngân hàng (BKTRANSBANKSTATUS)
    public string? RefBankCode { get; set; }                     // Mã số tiếp nhận / hồ sơ do ngân hàng cấp (REFBANKCODE)
    public string? BankRemark { get; set; }                      // Ý kiến / thông báo phản hồi của ngân hàng (BANKREMARK)

    // Khế ước vay vốn (GNTT / GNTTLC)
    public string? LDNo { get; set; }                            // Số khế ước nhận nợ vay (Loan Document - LDNo)
    public DateTime? DisbursementDate { get; set; }              // Ngày ngân hàng giải ngân thực tế
    public string? DisbursementTerm { get; set; } = "03 tháng";  // Kỳ hạn vay vốn
    public decimal DisbursementInterestRate { get; set; } = 8.5m;// Lãi suất vay vốn (%/năm)
    public DateTime? FirstInterestPmtDate { get; set; }          // Ngày trả lãi đầu tiên
    public long LoanLimit { get; set; }                          // Hạn mức tín dụng được duyệt (VND)

    // Bảo lãnh thanh toán / L/C (PhatHanhBLLC / PhatHanhLC)
    public string? MDNo { get; set; }                            // Số thư bảo lãnh ngân hàng phát hành (MDNo)
    public long GrtAmount { get; set; }                          // Số tiền bảo lãnh được cấp (VND)
    public DateTime? GrtDateStart { get; set; }                  // Ngày bắt đầu hiệu lực bảo lãnh
    public DateTime? GrtDateEnd { get; set; }                    // Ngày hết hạn hiệu lực bảo lãnh
    public string? GrtTerm { get; set; } = "45 ngày";            // Thời hạn bảo lãnh
    public long GrtFee { get; set; }                             // Phí phát hành bảo lãnh (VND)
    public string? LCNo { get; set; }                            // Số thư tín dụng L/C (LCNo)
    public long LCAmount { get; set; }                           // Giá trị thư tín dụng L/C (VND)
    public DateTime? LCStartDate { get; set; }                   // Ngày mở L/C
    public DateTime? LCEndDate { get; set; }                     // Ngày hết hạn L/C

    public DateTime? SentToBankAt { get; set; }                  // Thời điểm đẩy sang e-Banking ngân hàng
    public DateTime? CompletedAt { get; set; }                   // Thời điểm hoàn tất giải ngân
    public DateTime? CancelledAt { get; set; }                   // Thời điểm hủy đề nghị
    public string? CancelReason { get; set; }                    // Lý do hủy
    public string? Remark { get; set; }                          // Ghi chú đề nghị
    public string? CreatedBy { get; set; }                       // Người lập hồ sơ
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public List<BankingDisbursementDetail> Details { get; set; } = [];
    public List<BankingDisbursementFile> BankFiles { get; set; } = [];
}

/// <summary>Chi tiết xe / phụ lục hợp đồng trong đề nghị giao dịch ngân hàng — tương ứng RQ_BankingTransCtr trong BizHTC.</summary>
public sealed class BankingDisbursementDetail
{
    public long Id { get; set; }
    public long RequestId { get; set; }
    public Guid OrgId { get; set; }
    public string TransNo { get; set; } = "";                    // Số đề nghị
    public string DlrCtrNo { get; set; } = "";                   // Số phụ lục hợp đồng mua bán xe (DLRCTRNO)
    public string ModelCode { get; set; } = "";                  // Mã model xe (SANTAFE, TUCSON, CRETA, ACCENT, ELANTRA...)
    public string? ModelName { get; set; }                       // Tên thương mại mẫu xe
    public string SpecCode { get; set; } = "";                   // Mã phiên bản đặc tả kỹ thuật (SPECCODE)
    public string? SpecDescription { get; set; }                 // Diễn giải phiên bản xe
    public VehicleAssemblyType AssemblyType { get; set; } = VehicleAssemblyType.CKD; // Loại xe (CKD / CBU)
    public DateTime? ContractDate { get; set; }                  // Ngày ký PLHĐ
    public string? PrincipalContractNo { get; set; }             // Số HĐ nguyên tắc (BKTRANSCTRPCPNO)
    public DateTime? PrincipalContractDate { get; set; }         // Ngày ký HĐ nguyên tắc (BKTRANSCTRPCPDATE)
    public DateTime? DeliveryDate { get; set; }                  // Ngày giao hàng dự kiến (BKTRANSCTRDATE)
    public int Qty { get; set; } = 1;                            // Số lượng xe (QTY)
    public long UnitPrice { get; set; }                          // Đơn giá bán xe (UNITPRICE)
    public long TotalAmount { get; set; }                        // Thành tiền xe (= Qty * UnitPrice)
    public decimal LtvRate { get; set; } = 80.0m;                // Tỷ lệ cho vay / tài trợ vốn (% LTV)
    public long DisbursementAmount { get; set; }                 // Số tiền đề nghị giải ngân/bảo lãnh (= TotalAmount * LtvRate / 100)
    public string? Remark { get; set; }                          // Ghi chú dòng xe
}

/// <summary>Hồ sơ chứng từ tài chính ký số gửi ngân hàng — tương ứng RQ_BankingTransBankFile trong BizHTC.</summary>
public sealed class BankingDisbursementFile
{
    public long Id { get; set; }
    public long RequestId { get; set; }
    public Guid OrgId { get; set; }
    public string TransNo { get; set; } = "";                    // Số đề nghị
    public BankFileDocumentType DocType { get; set; } = BankFileDocumentType.DeNghiVay; // Loại hồ sơ
    public string FileName { get; set; } = "";                   // Tên file (BFileName)
    public string? FilePath { get; set; }                        // Đường dẫn lưu file
    public BankFileSignStatus SignStatus { get; set; } = BankFileSignStatus.Pending; // Trạng thái ký số (SignStatus)
    public string? SignedUser { get; set; }                      // Người đại diện ký số CA
    public DateTime? SignedAt { get; set; }                      // Thời điểm ký số CA
    public string? CertSerialNumber { get; set; }                // Số serial chứng thư số (SerialNumber)
    public DateTime UploadDate { get; set; } = DateTime.Now;     // Ngày nạp tài liệu
}

public sealed class CreateDisbursementRequestDto
{
    public BankingTransType TransType { get; set; } = BankingTransType.GNTT;
    public string DealerCode { get; set; } = "";
    public string DealerName { get; set; } = "";
    public string BizResNumber { get; set; } = "";
    public string BankCode { get; set; } = "";
    public string BankName { get; set; } = "";
    public string? PaymentAccount { get; set; }
    public string? PaymentBankName { get; set; }
    public string? ReceivingUnit { get; set; }
    public string? ReceivingAccount { get; set; }
    public string? ReceivingBank { get; set; }
    public string? DisbursementTerm { get; set; }
    public decimal? DisbursementInterestRate { get; set; }
    public string? Remark { get; set; }
    public List<DisbursementDetailInputDto> Details { get; set; } = [];
    public List<DisbursementFileInputDto>? Files { get; set; }
}

public sealed class DisbursementDetailInputDto
{
    public string DlrCtrNo { get; set; } = "";
    public string ModelCode { get; set; } = "";
    public string? ModelName { get; set; }
    public string SpecCode { get; set; } = "";
    public string? SpecDescription { get; set; }
    public VehicleAssemblyType AssemblyType { get; set; } = VehicleAssemblyType.CKD;
    public DateTime? ContractDate { get; set; }
    public string? PrincipalContractNo { get; set; }
    public DateTime? PrincipalContractDate { get; set; }
    public DateTime? DeliveryDate { get; set; }
    public int Qty { get; set; } = 1;
    public long UnitPrice { get; set; }
    public decimal LtvRate { get; set; } = 80.0m;
    public string? Remark { get; set; }
}

public sealed class DisbursementFileInputDto
{
    public BankFileDocumentType DocType { get; set; } = BankFileDocumentType.DeNghiVay;
    public string FileName { get; set; } = "";
    public string? FilePath { get; set; }
}

public sealed class BankReviewDto
{
    public string? RefBankCode { get; set; }
    public string? BankRemark { get; set; }
}

public sealed class RequestMoreDocsDto
{
    public string Reason { get; set; } = "";
    public bool IsMissingFiles { get; set; } = false;
}

public sealed class SignBankFileDto
{
    public string SignedUser { get; set; } = "Nguyen Van A - Giam Doc";
    public string CertSerialNumber { get; set; } = "54018899AACC4520";
}

public sealed class ApproveDisburseDto
{
    public string? LDNo { get; set; }
    public string? MDNo { get; set; }
    public string? LCNo { get; set; }
    public long? ActualAmount { get; set; }
    public decimal? InterestRate { get; set; }
    public string? Term { get; set; }
    public string? BankRemark { get; set; }
}

public sealed class RejectDisbursementDto
{
    public string Reason { get; set; } = "";
}

public sealed class DisbursementAdviceDto
{
    public string TransNo { get; set; } = "";
    public string TransTypeText { get; set; } = "";
    public string PrintDate { get; set; } = "";
    public string DealerCode { get; set; } = "";
    public string DealerName { get; set; } = "";
    public string BizResNumber { get; set; } = "";
    public string BankName { get; set; } = "";
    public string BankCode { get; set; } = "";
    public string PaymentAccount { get; set; } = "";
    public string ReceivingUnit { get; set; } = "";
    public string ReceivingAccount { get; set; } = "";
    public string ReceivingBank { get; set; } = "";
    public int TotalCars { get; set; }
    public long TotalContractAmount { get; set; }
    public long TotalDisbursementAmount { get; set; }
    public long ActualDisbursedAmount { get; set; }
    public string AmountInWords { get; set; } = "";
    public string StatusText { get; set; } = "";
    public string BankStatusText { get; set; } = "";
    public string? RefBankCode { get; set; }
    public string? LDNo { get; set; }
    public string? MDNo { get; set; }
    public string? LCNo { get; set; }
    public string? DisbursementTerm { get; set; }
    public decimal DisbursementInterestRate { get; set; }
    public string? DisbursementDate { get; set; }
    public List<DisbursementDetailAdviceDto> Cars { get; set; } = [];
    public List<string> SignedDocuments { get; set; } = [];
}

public sealed class DisbursementDetailAdviceDto
{
    public int No { get; set; }
    public string DlrCtrNo { get; set; } = "";
    public string ModelCode { get; set; } = "";
    public string SpecCode { get; set; } = "";
    public string AssemblyTypeText { get; set; } = "";
    public int Qty { get; set; }
    public long UnitPrice { get; set; }
    public long TotalAmount { get; set; }
    public decimal LtvRate { get; set; }
    public long DisbursementAmount { get; set; }
}

public sealed class DisbursementSummaryDto
{
    public int TotalRequests { get; set; }
    public int DraftCount { get; set; }
    public int SentToBankCount { get; set; }
    public int ProcessingCount { get; set; }
    public int CompletedCount { get; set; }
    public int CancelledCount { get; set; }
    public int TotalVehiclesFinanced { get; set; }
    public long TotalContractAmount { get; set; }
    public long TotalRequestedDisbursement { get; set; }
    public long TotalActualDisbursed { get; set; }
}

/// <summary>Hồ sơ Đề nghị Hủy Gán Ngân Hàng Bảo Lãnh Cho Hợp Đồng Xe Ô Tô — tương ứng DMS40_DlrCtr_CancelBankMD trong BizHTC.Payment / DMS40.Contract &amp; FrmDMS40_DlrCtr_CancelBankMD.</summary>
public sealed class ContractBankMDCancel
{
    public long Id { get; set; }
    public Guid OrgId { get; set; }
    public string CancelBankMDNo { get; set; } = "";             // Số đề nghị hủy gán NHBL (ví dụ: CANMD-202505-001)
    public string DlrCtrNo { get; set; } = "";                   // Số Phụ lục hợp đồng mua bán buôn xe đại lý (DLRCTRNO)
    public string DealerCode { get; set; } = "";                 // Mã đại lý (DEALERCODE)
    public string DealerName { get; set; } = "";                 // Tên đại lý
    public string BankCodeMD { get; set; } = "";                 // Mã ngân hàng bảo lãnh cần hủy (BANKCODEMD: VPB, CTG, MBB, TCB, VCB...)
    public string BankNameMD { get; set; } = "";                 // Tên ngân hàng bảo lãnh
    public string? NewBankCodeMD { get; set; }                   // Ngân hàng bảo lãnh mới dự kiến thay thế (nếu có)
    public string? NewBankNameMD { get; set; }                   // Tên ngân hàng bảo lãnh mới
    public GuaranteeType GuaranteeType { get; set; } = GuaranteeType.Payment; // Loại bảo lãnh (BL thanh toán, bảo lãnh thực hiện HĐ, L/C...)
    public long ContractAmount { get; set; }                     // Tổng giá trị hợp đồng mua xe (VND)
    public long GuaranteeAmount { get; set; }                    // Giá trị bảo lãnh cần hủy gán (VND)
    public CancelBankMDReasonType ReasonType { get; set; } = CancelBankMDReasonType.ChangeBank; // Phân loại lý do hủy
    public string? ReasonDescription { get; set; }               // Diễn giải chi tiết lý do đề nghị hủy
    public CancelBankMDStatus Status { get; set; } = CancelBankMDStatus.Pending; // Trạng thái: P (Pending), A (Approved - Ngân hàng duyệt), F (Finished - HTC duyệt), R (Rejected), C (Cancelled)
    public int TotalVehicles { get; set; }                       // Tổng số lượng xe trong đề nghị
    public string? RemarkDlr { get; set; }                       // Ý kiến / đề xuất của Đại lý
    public string? RemarkBank { get; set; }                      // Ý kiến thẩm định / chấp thuận của Ngân hàng
    public string? RemarkHTC { get; set; }                       // Ý kiến phê duyệt của HTC/HTV
    public string? ApproveBy { get; set; }                       // Cán bộ ngân hàng duyệt
    public DateTime? ApproveDateTime { get; set; }               // Thời điểm ngân hàng duyệt
    public string? FinishBy { get; set; }                        // Cán bộ HTC duyệt hoàn tất
    public DateTime? FinishDTime { get; set; }                   // Thời điểm HTC duyệt hoàn tất
    public string? RejectBy { get; set; }                        // Người từ chối
    public DateTime? RejectDateTime { get; set; }                // Thời điểm từ chối
    public string? RejectReason { get; set; }                    // Lý do từ chối
    public string? CancelBy { get; set; }                        // Người hủy
    public DateTime? CancelDateTime { get; set; }                // Thời điểm hủy
    public string? CancelReason { get; set; }                    // Lý do hủy
    public string? CreatedBy { get; set; }                       // Người lập đề nghị
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public List<ContractBankMDCancelDetail> Details { get; set; } = [];
}

/// <summary>Chi tiết xe trong đề nghị hủy gán ngân hàng bảo lãnh — tương ứng các dòng xe phụ lục hợp đồng trong DMS40_DlrCtr_CancelBankMD.</summary>
public sealed class ContractBankMDCancelDetail
{
    public long Id { get; set; }
    public long CancelBankMDId { get; set; }
    public Guid OrgId { get; set; }
    public string CancelBankMDNo { get; set; } = "";             // Số đề nghị hủy
    public string VIN { get; set; } = "";                        // Số khung xe (17 ký tự VIN)
    public string? CarId { get; set; }                           // Mã xe nội bộ kho HTC
    public string ModelCode { get; set; } = "";                  // Mã model xe (SANTAFE, TUCSON, CRETA, ACCENT, ELANTRA...)
    public string? ModelName { get; set; }                       // Tên thương mại mẫu xe
    public string? SpecCode { get; set; }                        // Mã phiên bản đặc tả kỹ thuật
    public string? SpecDescription { get; set; }                 // Diễn giải phiên bản xe
    public string? ColorExtNameVN { get; set; }                  // Màu sơn ngoại thất
    public string? EngineNo { get; set; }                        // Số máy xe
    public long UnitPrice { get; set; }                          // Đơn giá bán xe theo hợp đồng (VND)
    public long GuaranteeAmount { get; set; }                    // Giá trị bảo lãnh được phân bổ cho xe (VND)
    public CancelBankMDDetailStatus Status { get; set; } = CancelBankMDDetailStatus.Pending; // Trạng thái xe
    public string? Remark { get; set; }                          // Ghi chú chi tiết dòng xe
}

public sealed class CreateCancelBankMDDto
{
    public string DlrCtrNo { get; set; } = "";
    public string DealerCode { get; set; } = "";
    public string? DealerName { get; set; }
    public string BankCodeMD { get; set; } = "";
    public string? BankNameMD { get; set; }
    public string? NewBankCodeMD { get; set; }
    public string? NewBankNameMD { get; set; }
    public GuaranteeType? GuaranteeType { get; set; }
    public CancelBankMDReasonType? ReasonType { get; set; }
    public string? ReasonDescription { get; set; }
    public string? RemarkDlr { get; set; }
    public string? CreatedBy { get; set; }
    public List<CancelBankMDItemInputDto> Items { get; set; } = [];
}

public sealed class CancelBankMDItemInputDto
{
    public string VIN { get; set; } = "";
    public string? CarId { get; set; }
    public string ModelCode { get; set; } = "";
    public string? ModelName { get; set; }
    public string? SpecCode { get; set; }
    public string? SpecDescription { get; set; }
    public string? ColorExtNameVN { get; set; }
    public string? EngineNo { get; set; }
    public long UnitPrice { get; set; }
    public long? GuaranteeAmount { get; set; }
    public string? Remark { get; set; }
}

public sealed class ApproveCancelBankMDBankDto
{
    public string? ApproverName { get; set; } = "TruongPhongTinDung_NganHang";
    public string? RemarkBank { get; set; }
}

public sealed class FinishCancelBankMDHTCDto
{
    public string? FinisherName { get; set; } = "TruongPhongQuanLyDaiLy_HTC";
    public string? RemarkHTC { get; set; }
}

public sealed class RejectCancelBankMDDto
{
    public string Reason { get; set; } = "";
    public string? RejecterName { get; set; }
}

public sealed class CancelBankMDUserCancelDto
{
    public string? Reason { get; set; }
    public string? CancellerName { get; set; }
}

public sealed class CancelBankMDAdviceDto
{
    public string CancelBankMDNo { get; set; } = "";
    public string DlrCtrNo { get; set; } = "";
    public string PrintDate { get; set; } = "";
    public string DealerCode { get; set; } = "";
    public string DealerName { get; set; } = "";
    public string BankCodeMD { get; set; } = "";
    public string BankNameMD { get; set; } = "";
    public string? NewBankNameMD { get; set; }
    public string GuaranteeTypeText { get; set; } = "";
    public string ReasonTypeText { get; set; } = "";
    public string ReasonDescription { get; set; } = "";
    public int TotalVehicles { get; set; }
    public long ContractAmount { get; set; }
    public long GuaranteeAmount { get; set; }
    public string GuaranteeAmountInWords { get; set; } = "";
    public string StatusText { get; set; } = "";
    public string? RemarkDlr { get; set; }
    public string? RemarkBank { get; set; }
    public string? RemarkHTC { get; set; }
    public string? ApproveBy { get; set; }
    public string? ApproveDateTime { get; set; }
    public string? FinishBy { get; set; }
    public string? FinishDTime { get; set; }
    public List<CancelBankMDDetailAdviceDto> Items { get; set; } = [];
}

public sealed class CancelBankMDDetailAdviceDto
{
    public int No { get; set; }
    public string VIN { get; set; } = "";
    public string ModelCode { get; set; } = "";
    public string ModelName { get; set; } = "";
    public string SpecCode { get; set; } = "";
    public string EngineNo { get; set; } = "";
    public string ColorExtNameVN { get; set; } = "";
    public long UnitPrice { get; set; }
    public long GuaranteeAmount { get; set; }
    public string Status { get; set; } = "";
}

public sealed class CancelBankMDSummaryDto
{
    public int TotalRequests { get; set; }
    public int PendingCount { get; set; }
    public int BankApprovedCount { get; set; }
    public int FinishedCount { get; set; }
    public int RejectedCount { get; set; }
    public int CancelledCount { get; set; }
    public int TotalVehiclesRevoked { get; set; }
    public long TotalGuaranteeAmountRevoked { get; set; }
    public long TotalContractValueRevoked { get; set; }
}

public sealed class CandidateContractForCancelBankMDDto
{
    public string DlrCtrNo { get; set; } = "";
    public string DealerCode { get; set; } = "";
    public string DealerName { get; set; } = "";
    public string BankCodeMD { get; set; } = "";
    public string BankNameMD { get; set; } = "";
    public string GuaranteeType { get; set; } = "Payment";
    public long ContractAmount { get; set; }
    public long GuaranteeAmount { get; set; }
    public int VehicleCount { get; set; }
    public List<CancelBankMDItemInputDto> CandidateVehicles { get; set; } = [];
}

// ==========================================
// Nghiệp vụ Quản lý Thu Tiền Thanh Toán & Quyết Toán Bồi Thường Bảo Hiểm Xe Ô Tô (Vehicle Insurance Claim Payment & Settlement)
// Tương ứng BizCarSv.Debit.cs (SerInsuranceDebitSearch, SerInsuranceDebitDetailGet, SerPaymentCreate, SerPaymentPaperRpt)
// và FrmInsPaymentCreate trong TERP.HTCServiceClient/Views/Debit.
// ==========================================

/// <summary>Trạng thái khoản công nợ bồi thường bảo hiểm xe — tương ứng Ser_InsuranceDebit trong BizCarSv.</summary>
public enum InsuranceDebitStatus
{
    Pending = 0,        // Chờ thanh toán (chưa thanh toán đồng nào)
    PartiallyPaid = 1,  // Đã thanh toán một phần (RemainAmount > 0)
    Settled = 2,        // Đã tất toán hoàn tất 100% (RemainAmount == 0)
    Cancelled = 3       // Hủy hồ sơ bồi thường
}

/// <summary>Hình thức thanh toán bồi thường bảo hiểm — tương ứng PaymentType trong Ser_Payment.</summary>
public enum InsurancePaymentMethod
{
    BankTransfer = 0,   // Chuyển khoản ngân hàng (UNC)
    VnPay = 1,          // Cổng thanh toán điện tử VNPay QR
    Momo = 2,           // Ví điện tử MoMo
    Cash = 3,           // Tiền mặt tại quầy thu ngân
    Offset = 4          // Bù trừ công nợ đối ứng
}

/// <summary>Trạng thái Phiếu thu thanh toán bảo hiểm — tương ứng Ser_Payment.</summary>
public enum InsurancePaymentStatus
{
    Draft = 0,          // Nháp (chưa xác nhận tiền về)
    Confirmed = 1,      // Kế toán đã xác nhận thu tiền
    Settled = 2,        // Đã quyết toán trừ nợ hoàn tất
    Cancelled = 3       // Hủy phiếu thu (hoàn tác trừ nợ RO)
}

/// <summary>
/// Hồ sơ công nợ bảo hiểm bồi thường sửa chữa xe — tương ứng Ser_InsuranceDebit &amp; Ser_CusDebit trong BizCarSv.
/// Mỗi dòng tương ứng một Lệnh sửa chữa RO được hãng bảo hiểm duyệt bồi thường.
/// </summary>
public sealed class InsuranceClaimDebit
{
    public long Id { get; set; }
    public Guid OrgId { get; set; }
    public string DebitNo { get; set; } = "";                      // Số công nợ bồi thường (ví dụ: DEB-INS-202505-001)
    public string InsNo { get; set; } = "";                        // Mã công ty bảo hiểm (INS-PTI, INS-BV, INS-PJICO, INS-PVI, INS-MIC...)
    public string InsName { get; set; } = "";                      // Tên công ty bảo hiểm
    public string RONo { get; set; } = "";                         // Số Lệnh sửa chữa xưởng dịch vụ (RO-2025-0501)
    public string VIN { get; set; } = "";                          // Số khung xe (17 ký tự VIN)
    public string PlateNo { get; set; } = "";                      // Biển số xe (ví dụ: 30H-889.99)
    public string ModelCode { get; set; } = "";                    // Dòng xe (SANTAFE, TUCSON, CRETA, ACCENT, PALISADE...)
    public string? ModelName { get; set; }                         // Tên thương mại mẫu xe
    public string? CustomerName { get; set; }                      // Tên chủ xe / khách hàng
    public string? CustomerPhone { get; set; }                     // Số điện thoại khách hàng
    public DateTime DebitDate { get; set; } = DateTime.Now;        // Ngày phê duyệt bồi thường phát sinh nợ
    public DateTime DueDate { get; set; }                          // Hạn thanh toán cam kết
    public long DebitAmount { get; set; }                          // Số tiền bảo hiểm bồi thường được duyệt (VND)
    public long PaidAmount { get; set; }                           // Số tiền bảo hiểm đã thanh toán lũy kế (VND)
    public long RemainAmount { get; set; }                         // Số tiền nợ còn lại (= DebitAmount - PaidAmount)
    public InsuranceDebitStatus Status { get; set; } = InsuranceDebitStatus.Pending; // Trạng thái nợ
    public string? Note { get; set; }                              // Ghi chú chi tiết bồi thường (hạng mục sửa chữa)
    public string? CreatedBy { get; set; }                         // Người lập hồ sơ (Cố vấn dịch vụ)
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

/// <summary>
/// Phiếu thu thanh toán bảo hiểm — tương ứng Ser_Payment trong BizCarSv.
/// Ghi nhận 1 đợt chuyển khoản/thanh toán tiền bồi thường của công ty bảo hiểm và tự động phân bổ FIFO trừ nợ các hồ sơ RO.
/// </summary>
public sealed class InsurancePayment
{
    public long Id { get; set; }
    public Guid OrgId { get; set; }
    public string PaymentNo { get; set; } = "";                    // Số phiếu thu (format: PM-INS-yyyyMM-xxx)
    public string InsNo { get; set; } = "";                        // Mã công ty bảo hiểm nộp tiền
    public string InsName { get; set; } = "";                      // Tên công ty bảo hiểm
    public DateTime PayDate { get; set; } = DateTime.Now;          // Ngày nộp / chuyển tiền
    public string PayPersonName { get; set; } = "";                // Đại diện bảo hiểm thanh toán / Giám định viên
    public string? PayPersonIDCardNo { get; set; }                 // Số CMT/CCCD người đại diện nộp
    public string? PayPersonPhone { get; set; }                    // Số điện thoại người nộp
    public long PaymentAmount { get; set; }                        // Tổng số tiền bảo hiểm thanh toán đợt này (VND)
    public InsurancePaymentMethod PaymentMethod { get; set; } = InsurancePaymentMethod.BankTransfer; // Phương thức thanh toán
    public string? BankCode { get; set; }                          // Mã ngân hàng thanh toán (CTG, MBB, VCB, TCB, VPB)
    public string? BankName { get; set; }                          // Tên ngân hàng
    public string? BankAccountNo { get; set; }                     // Số tài khoản nhận tiền
    public string? BankTxnRef { get; set; }                        // Mã giao dịch ngân hàng / Cổng điện tử
    public long TotalAllocated { get; set; }                       // Tổng tiền đã phân bổ trừ nợ các lệnh RO
    public long UnallocatedAmount { get; set; }                    // Tiền thừa chưa phân bổ hết (nếu có)
    public InsurancePaymentStatus Status { get; set; } = InsurancePaymentStatus.Draft; // Trạng thái phiếu thu
    public string? Note { get; set; }                              // Ghi chú đợt thanh toán
    public string? CreatedBy { get; set; }                         // Kế toán lập phiếu thu
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public string? ConfirmedBy { get; set; }                       // Kế toán trưởng xác nhận
    public DateTime? ConfirmedAt { get; set; }
    public string? SettledBy { get; set; }                         // Kế toán hoàn tất quyết toán
    public DateTime? SettledAt { get; set; }
    public string? CancelledBy { get; set; }                       // Người hủy
    public DateTime? CancelledAt { get; set; }
    public string? CancelReason { get; set; }                      // Lý do hủy phiếu thu

    public List<InsurancePaymentDetail> Details { get; set; } = [];
}

/// <summary>
/// Chi tiết phân bổ thanh toán cho từng Lệnh RO — tương ứng Ser_PaymentDetail trong BizCarSv.
/// </summary>
public sealed class InsurancePaymentDetail
{
    public long Id { get; set; }
    public long PaymentId { get; set; }
    public long DebitId { get; set; }
    public Guid OrgId { get; set; }
    public string DebitNo { get; set; } = "";                      // Số công nợ bồi thường
    public string RONo { get; set; } = "";                         // Số Lệnh sửa chữa
    public string VIN { get; set; } = "";                          // Số khung xe
    public string PlateNo { get; set; } = "";                      // Biển số xe
    public long DebitAmount { get; set; }                          // Tiền bồi thường gốc ban đầu
    public long DebitAmountBefore { get; set; }                    // Nợ còn lại trước khi trừ khoản thanh toán này
    public long PaymentDetailAmount { get; set; }                  // Số tiền thực tế phân bổ trừ nợ đợt này
    public long DebitAmountLeft { get; set; }                      // Nợ còn lại sau phân bổ (= DebitAmountBefore - PaymentDetailAmount)
    public string? Remark { get; set; }                            // Ghi chú phân bổ dòng
}

public sealed class CreateInsuranceDebitDto
{
    public string? DebitNo { get; set; }
    public string InsNo { get; set; } = "";
    public string? InsName { get; set; }
    public string RONo { get; set; } = "";
    public string VIN { get; set; } = "";
    public string PlateNo { get; set; } = "";
    public string ModelCode { get; set; } = "";
    public string? ModelName { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
    public DateTime? DebitDate { get; set; }
    public DateTime? DueDate { get; set; }
    public long DebitAmount { get; set; }
    public string? Note { get; set; }
    public string? CreatedBy { get; set; }
}

public sealed class CreateInsurancePaymentDto
{
    public string? PaymentNo { get; set; }
    public string InsNo { get; set; } = "";
    public string? InsName { get; set; }
    public DateTime? PayDate { get; set; }
    public string PayPersonName { get; set; } = "";
    public string? PayPersonIDCardNo { get; set; }
    public string? PayPersonPhone { get; set; }
    public long PaymentAmount { get; set; }
    public InsurancePaymentMethod? PaymentMethod { get; set; }
    public string? BankCode { get; set; }
    public string? BankName { get; set; }
    public string? BankAccountNo { get; set; }
    public string? BankTxnRef { get; set; }
    public string? Note { get; set; }
    public string? CreatedBy { get; set; }
    public bool AutoAllocateFifo { get; set; } = true;             // Tự động phân bổ nợ theo thứ tự FIFO
    public List<ManualAllocationItemDto>? ManualAllocations { get; set; } // Phân bổ thủ công (nếu không dùng FIFO)
}

public sealed class ManualAllocationItemDto
{
    public long DebitId { get; set; }
    public long Amount { get; set; }
}

public sealed class ConfirmInsurancePaymentDto
{
    public string? ConfirmedBy { get; set; } = "KeToanTruong";
}

public sealed class SettleInsurancePaymentDto
{
    public string? SettledBy { get; set; } = "KeToanThanhToan";
}

public sealed class CancelInsurancePaymentDto
{
    public string Reason { get; set; } = "";
    public string? CancelledBy { get; set; }
}

public sealed class InsurancePaymentAdviceDto
{
    public string PaymentNo { get; set; } = "";
    public string PrintDate { get; set; } = "";
    public string InsNo { get; set; } = "";
    public string InsName { get; set; } = "";
    public string PayPersonName { get; set; } = "";
    public string? PayPersonIDCardNo { get; set; }
    public string? PayPersonPhone { get; set; }
    public string PaymentMethodText { get; set; } = "";
    public string? BankCode { get; set; }
    public string? BankName { get; set; }
    public string? BankAccountNo { get; set; }
    public string? BankTxnRef { get; set; }
    public long PaymentAmount { get; set; }
    public string PaymentAmountInWords { get; set; } = "";
    public long TotalAllocated { get; set; }
    public long UnallocatedAmount { get; set; }
    public string StatusText { get; set; } = "";
    public string? Note { get; set; }
    public string? CreatedBy { get; set; }
    public string? ConfirmedBy { get; set; }
    public string? SettledBy { get; set; }
    public List<InsurancePaymentDetailAdviceDto> Details { get; set; } = [];
}

public sealed class InsurancePaymentDetailAdviceDto
{
    public int No { get; set; }
    public string DebitNo { get; set; } = "";
    public string RONo { get; set; } = "";
    public string PlateNo { get; set; } = "";
    public string VIN { get; set; } = "";
    public long DebitAmount { get; set; }
    public long DebitAmountBefore { get; set; }
    public long PaymentDetailAmount { get; set; }
    public long DebitAmountLeft { get; set; }
    public string StatusAfterPayment { get; set; } = "";
}

public sealed class InsuranceDebitSummaryDto
{
    public int TotalClaims { get; set; }
    public int PendingClaims { get; set; }
    public int PartiallyPaidClaims { get; set; }
    public int SettledClaims { get; set; }
    public int CancelledClaims { get; set; }
    public long TotalClaimAmount { get; set; }
    public long TotalPaidAmount { get; set; }
    public long TotalRemainingDebt { get; set; }
    public decimal SettlementRate { get; set; }
    public int TotalPaymentReceipts { get; set; }
    public long TotalReceiptsAmount { get; set; }
    public List<InsuranceCompanyStatDto> CompanyStats { get; set; } = [];
}

public sealed class InsuranceCompanyStatDto
{
    public string InsNo { get; set; } = "";
    public string InsName { get; set; } = "";
    public int ClaimCount { get; set; }
    public long TotalClaimAmount { get; set; }
    public long TotalPaidAmount { get; set; }
    public long RemainingDebt { get; set; }
}

public sealed class EligibleDebitForPaymentDto
{
    public long Id { get; set; }
    public string DebitNo { get; set; } = "";
    public string RONo { get; set; } = "";
    public string VIN { get; set; } = "";
    public string PlateNo { get; set; } = "";
    public string ModelCode { get; set; } = "";
    public string? CustomerName { get; set; }
    public DateTime DebitDate { get; set; }
    public DateTime DueDate { get; set; }
    public long DebitAmount { get; set; }
    public long PaidAmount { get; set; }
    public long RemainAmount { get; set; }
    public string Status { get; set; } = "";
}
