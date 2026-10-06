using System.ComponentModel.DataAnnotations;
using api_custo_prato.src.Models;

namespace api_custo_prato.src.Requests
{
    public class RegisterRequest : Request
    {
        [Required(ErrorMessage = "O Nome é obrigatório.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "O E-mail é obrigatório.")]
        [EmailAddress(ErrorMessage = "E-mail inválido.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "A Senha é obrigatória.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "O nome do estabelecimento é obrigatório.")]
        public string StoreName { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;
    }

    public class LoginRequest : Request
    {
        [Required(ErrorMessage = "O E-mail é obrigatório.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "A Senha é obrigatória.")]
        public string Password { get; set; } = string.Empty;

        public UserDevice Device { get; set; } = new();
    }

    public class ForgotPasswordRequest : Request
    {
        [Required(ErrorMessage = "O E-mail é obrigatório.")]
        public string Email { get; set; } = string.Empty;
    }

    public class ResetPasswordRequest : Request
    {
        [Required(ErrorMessage = "O Código é obrigatório.")]
        public string Code { get; set; } = string.Empty;

        [Required(ErrorMessage = "A Senha é obrigatória.")]
        public string Password { get; set; } = string.Empty;
    }

    public class NewCodeRequest : Request
    {
        [Required(ErrorMessage = "O E-mail é obrigatório.")]
        public string Email { get; set; } = string.Empty;
    }

    public class UpdateConfirmAccountDTO : Request
    {
        [Required(ErrorMessage = "O Código é obrigatório.")]
        public string Code { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    public class UpdateUserDTO : Request
    {
        public string Id { get; set; } = string.Empty;

        [Required(ErrorMessage = "O Nome é obrigatório.")]
        public string Name { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;
    }

    public class ChangePasswordDTO : Request
    {
        public string Id { get; set; } = string.Empty;

        [Required(ErrorMessage = "A senha atual é obrigatória.")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "A nova senha é obrigatória.")]
        public string NewPassword { get; set; } = string.Empty;
    }
}
