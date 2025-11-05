namespace backend.Models.Payroll.Results
{
    public class PayrollValidationResult
    {
        public bool CanProcess { get; set; }
        public PayrollProcessResult? ErrorResult { get; set; }

        public static PayrollValidationResult AsValid() => new() { CanProcess = true };

        public static PayrollValidationResult AsError(Payroll existingPayroll, List<PayrollPayment> details)
            => new()
            {
                CanProcess = false,
                ErrorResult = new PayrollProcessResult
                {
                    Success = false,
                    Message = "El período ya ha sido calculado.",
                    ExistingPayroll = existingPayroll,
                    PayrollDetails = details
                }
            };

        public static PayrollValidationResult AsError(string message)
            => new()
            {
                CanProcess = false,
                ErrorResult = new PayrollProcessResult
                {
                    Success = false,
                    Message = message
                }
            };
    }
}