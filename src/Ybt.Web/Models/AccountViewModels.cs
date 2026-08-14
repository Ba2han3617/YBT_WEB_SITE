using System.ComponentModel.DataAnnotations;

namespace Ybt.Web.Models;

public class LoginViewModel
{
    [Required(ErrorMessage = "Lütfen e-posta adresinizi veya kullanıcı adınızı giriniz.")]
    [Display(Name = "E-posta veya Kullanıcı Adı")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Lütfen şifrenizi giriniz.")]
    [DataType(DataType.Password)]
    [Display(Name = "Şifre")]
    public string Password { get; set; } = null!;

    [Display(Name = "Beni Hatırla")]
    public bool RememberMe { get; set; }
}

public class RegisterViewModel
{
    [Required(ErrorMessage = "Lütfen adınızı giriniz.")]
    [Display(Name = "Ad")]
    public string FirstName { get; set; } = null!;

    [Required(ErrorMessage = "Lütfen soyadınızı giriniz.")]
    [Display(Name = "Soyad")]
    public string LastName { get; set; } = null!;

    [Required(ErrorMessage = "Lütfen fakülte veya yüksekokul seçimi yapınız.")]
    [Display(Name = "Fakülte / Yüksekokul / MYO")]
    public string Faculty { get; set; } = null!;

    [Required(ErrorMessage = "TC Kimlik Numarası zorunludur.")]
    [RegularExpression(@"^[1-9][0-9]{10}$", ErrorMessage = "TC Kimlik Numarası 11 haneli geçerli bir sayı olmalıdır.")]
    [Display(Name = "TC Kimlik Numarası")]
    public string TcNo { get; set; } = null!;

    [Required(ErrorMessage = "Öğrenci Numarası zorunludur.")]
    [RegularExpression(@"^[0-9]+$", ErrorMessage = "Öğrenci Numarası yalnızca sayılardan oluşmalıdır.")]
    [Display(Name = "Öğrenci Numarası")]
    public string StudentNumber { get; set; } = null!;

    [Required(ErrorMessage = "Cep telefonu numarası zorunludur.")]
    [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz.")]
    [RegularExpression(@"^(05\d{9}|5\d{9})$", ErrorMessage = "Geçerli bir cep telefonu giriniz (Örn: 05XXXXXXXXX).")]
    [Display(Name = "Cep Numarası")]
    public string PhoneNumber { get; set; } = null!;

    [Required(ErrorMessage = "E-posta adresi zorunludur.")]
    [EmailAddress(ErrorMessage = "Lütfen geçerli bir e-posta adresi giriniz.")]
    [Display(Name = "E-posta")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Adres alanı zorunludur.")]
    [Display(Name = "Adres")]
    public string Address { get; set; } = null!;

    [Required(ErrorMessage = "Lütfen şifrenizi belirleyiniz.")]
    [StringLength(100, ErrorMessage = "{0} en az {2} karakter uzunluğunda olmalıdır.", MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "Şifre")]
    public string Password { get; set; } = null!;

    [Required(ErrorMessage = "Lütfen şifrenizi tekrar giriniz.")]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Girilen şifreler birbiriyle uyuşmuyor.")]
    [Display(Name = "Şifre Tekrar")]
    public string ConfirmPassword { get; set; } = null!;

    [Range(typeof(bool), "true", "true", ErrorMessage = "Topluluk tüzüğü ve KVKK aydınlatma metnini onaylamanız gerekmektedir.")]
    [Display(Name = "KVKK Onayı")]
    public bool KvkkConsent { get; set; }
}
