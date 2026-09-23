#!/bin/bash
SRC="D:/idocNet/2010.HTC/Dev/50.Source/ERP.V15.DataWH.Release.2025/TERP.BizHTC"
echo "=== myPmt_Payment_CheckTotalValue (2247-2354) ==="
sed -n '2247,2354p' "$SRC/BizHTC.Payment.cs"
echo "=== myPayment_CheckGuarantee (24-83) ==="
sed -n '24,83p' "$SRC/BizHTC.Payment.cs"
