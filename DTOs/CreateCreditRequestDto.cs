namespace BluecoreApi.DTOs
{
    using System.ComponentModel.DataAnnotations;

    public class CreateCreditRequestDto
    {
        [Required(ErrorMessage = "La cédula del solicitante es obligatoria.")]
        [StringLength(50)]
        public string ApplicantId { get; set; } = string.Empty;

        [Range(typeof(decimal), "500", "50000", ParseLimitsInInvariantCulture = true,
            ConvertValueInInvariantCulture = true, ErrorMessage = "El monto debe estar entre $500 y $50,000.")]
        public decimal Amount { get; set; }

        [Range(6, 60, ErrorMessage = "El plazo debe estar entre 6 y 60 meses.")]
        public int TermMonths { get; set; }
    }
}
