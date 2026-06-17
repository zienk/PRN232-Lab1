using FluentValidation;
using PRN232.LMS.Services.Models.Requests;

namespace PRN232.LMS.Services.Validators;

// FluentValidation validator cho StudentRequestModel
// Lab 2 yêu cầu ít nhất 1 model phải có FluentValidation
// FluentValidation cho phép viết rule validation phức tạp hơn so với Data Annotations
public class StudentRequestValidator : AbstractValidator<StudentRequestModel>
{
    public StudentRequestValidator()
    {
        // Rule validate FullName
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Họ tên không được để trống")
            .MaximumLength(100).WithMessage("Họ tên tối đa 100 ký tự")
            .MinimumLength(2).WithMessage("Họ tên ít nhất 2 ký tự");

        // Rule validate Email
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email không được để trống")
            .EmailAddress().WithMessage("Email không đúng định dạng")
            .MaximumLength(100).WithMessage("Email tối đa 100 ký tự");

        // Rule validate DateOfBirth
        RuleFor(x => x.DateOfBirth)
            .NotEmpty().WithMessage("Ngày sinh không được để trống")
            // Sinh viên phải đủ 16 tuổi trở lên
            .Must(dob => dob.Date <= DateTime.UtcNow.Date.AddYears(-16))
            .WithMessage("Sinh viên phải ít nhất 16 tuổi")
            // Không được nhập ngày sinh quá xa trong quá khứ
            .Must(dob => dob >= new DateTime(1900, 1, 1))
            .WithMessage("Ngày sinh không hợp lệ");

        // Custom rule validate StudentCode (FPTU format)
        // Đây là custom validation rule theo yêu cầu Lab 2
        // Mã SV FPTU: 2 chữ cái hoa + 5 chữ số (VD: SE19886, CE18793)
        RuleFor(x => x.StudentCode)
            .Must(code => code == null || IsValidStudentCode(code))
            .WithMessage("Mã sinh viên phải có dạng 2 chữ cái hoa + 5 số (VD: SE19886, CE18793)");

        // Rule validate PhoneNumber nếu có nhập
        RuleFor(x => x.PhoneNumber)
            .MaximumLength(15).WithMessage("Số điện thoại tối đa 15 ký tự")
            // Chỉ cho phép số, dấu + ở đầu, dấu - và khoảng trắng
            .Matches(@"^[\d\+\-\s]*$").WithMessage("Số điện thoại chỉ được chứa số và ký tự +, -, khoảng trắng")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber)); // Chỉ validate nếu có nhập
    }

    // Hàm private kiểm tra mã sinh viên FPTU
    // Quy tắc: 2 chữ cái viết hoa + 5 chữ số
    // Ví dụ: SE19886, CE18793, AI22001
    private static bool IsValidStudentCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code)) return true; // Optional field

        // Kiểm tra độ dài: đúng 7 ký tự
        if (code.Length != 7) return false;

        // 2 ký tự đầu phải là chữ cái hoa (A-Z)
        if (!char.IsUpper(code[0]) || !char.IsUpper(code[1])) return false;

        // 5 ký tự sau phải là chữ số (0-9)
        for (int i = 2; i < 7; i++)
        {
            if (!char.IsDigit(code[i])) return false;
        }

        return true;
    }
}
