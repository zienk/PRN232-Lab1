using System;
using System.ComponentModel.DataAnnotations;

namespace PRN232.LMS.Services.Models.Requests
{
    // Model nhận dữ liệu tạo/cập nhật Student từ client
    // Đầy đủ các validation attribute theo yêu cầu Lab 2
    public class StudentRequestModel
    {
        // [Required]: bắt buộc nhập
        // [StringLength]: giới hạn độ dài chuỗi
        [Required(ErrorMessage = "Họ tên là bắt buộc")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Họ tên phải từ 2 đến 100 ký tự")]
        public string FullName { get; set; } = string.Empty;

        // [EmailAddress]: kiểm tra đúng định dạng email
        [Required(ErrorMessage = "Email là bắt buộc")]
        [StringLength(100, ErrorMessage = "Email tối đa 100 ký tự")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        public string Email { get; set; } = string.Empty;

        // [Required]: ngày sinh bắt buộc
        [Required(ErrorMessage = "Ngày sinh là bắt buộc")]
        public DateTime DateOfBirth { get; set; }

        // [Phone]: kiểm tra định dạng số điện thoại
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [StringLength(15, ErrorMessage = "Số điện thoại tối đa 15 ký tự")]
        public string? PhoneNumber { get; set; }

        // [RegularExpression]: custom validation cho mã sinh viên kiểu FPTU
        // Ví dụ hợp lệ: SE19886, CE18793, AI22001
        // Pattern: 2 chữ cái + 5 chữ số
        [RegularExpression(@"^[A-Z]{2}\d{5}$",
            ErrorMessage = "Mã sinh viên phải có dạng 2 chữ cái hoa + 5 số (VD: SE19886, CE18793)")]
        public string? StudentCode { get; set; }
    }
}
