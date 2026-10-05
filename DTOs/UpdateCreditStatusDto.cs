namespace BluecoreApi.DTOs
{
    using System.ComponentModel.DataAnnotations;
    public class UpdateCreditStatusDto
    {
        [Required(ErrorMessage = "El estado es obligatorio.")]
        [StringLength(20)]
        public string Status { get; set; } = string.Empty;

        [Required(ErrorMessage = "El comentario es obligatorio.")]
        [StringLength(2000)]
        public string Comment { get; set; } = string.Empty;
    }
}
