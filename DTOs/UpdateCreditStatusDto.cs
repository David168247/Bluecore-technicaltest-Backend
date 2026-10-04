namespace BluecoreApi.DTOs
{
    using System.ComponentModel.DataAnnotations;
    public class UpdateCreditStatusDto
    {
        [Required(ErrorMessage = "El estado es obligatorio.")]
        public string Status { get; set; } = string.Empty;

        [Required(ErrorMessage = "El comentario es obligatorio.")]
        public string Comment { get; set; } = string.Empty;
    }
}
