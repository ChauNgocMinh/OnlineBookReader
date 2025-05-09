using System.ComponentModel.DataAnnotations;

namespace OnlineBookReader.ViewModel
{
    public class ChangePasswordViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "Mật khẩu ít nhất 6 ký tự.")]
        public string NewPassword { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Xác nhận mật khẩu không đúng.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
