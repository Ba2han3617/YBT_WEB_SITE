using System.ComponentModel.DataAnnotations;

namespace Ybt.Web.Models;

public class LoginViewModel
{
    [Required(ErrorMessage = "Kullanıcı adı veya email gereklidir.")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Şifre gereklidir.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = null!;

    public bool RememberMe { get; set; }
}

public class RegisterViewModel
{
    [Required(ErrorMessage = "Ad alanı gereklidir.")]
    [Display(Name = "Ad")]
    public string FirstName { get; set; } = null!;

    [Required(ErrorMessage = "Soyad alanı gereklidir.")]
    [Display(Name = "Soyad")]
    public string LastName { get; set; } = null!;

    [Required(ErrorMessage = "Fakülte / Yüksekokul / MYO seçimi zorunludur.")]
    [Display(Name = "Fakülte / Yüksekokul / MYO")]
    public string Faculty { get; set; } = null!;

    [Required(ErrorMessage = "TC Kimlik Numarası gereklidir.")]
    [RegularExpression(@"^[1-9][0-9]{10}$", ErrorMessage = "TC Kimlik Numarası 11 haneli sayı olmalıdır.")]
    [Display(Name = "TC Numarası")]
    public string TcNo { get; set; } = null!;

    [Required(ErrorMessage = "Öğrenci Numarası gereklidir.")]
    [RegularExpression(@"^[0-9]+$", ErrorMessage = "Öğrenci Numarası yalnızca sayılardan oluşmalıdır.")]
    [Display(Name = "Öğrenci Numarası")]
    public string StudentNumber { get; set; } = null!;

    [Required(ErrorMessage = "Cep Numarası gereklidir.")]
    [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz.")]
    [RegularExpression(@"^(05\d{9}|5\d{9})$", ErrorMessage = "Geçerli bir cep telefonu giriniz (Örn: 05XXXXXXXXX).")]
    [Display(Name = "Cep Numarası")]
    public string PhoneNumber { get; set; } = null!;

    [Required(ErrorMessage = "E-posta adresi gereklidir.")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
    [Display(Name = "E-posta")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Adres alanı gereklidir.")]
    [Display(Name = "Adres")]
    public string Address { get; set; } = null!;

    [Required(ErrorMessage = "Şifre gereklidir.")]
    [StringLength(100, ErrorMessage = "{0} en az {2} karakter uzunluğunda olmalıdır.", MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "Şifre")]
    public string Password { get; set; } = null!;

    [Required(ErrorMessage = "Şifre Tekrar alanı gereklidir.")]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Şifreler uyuşmuyor.")]
    [Display(Name = "Şifre Tekrar")]
    public string ConfirmPassword { get; set; } = null!;
}
