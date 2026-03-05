namespace DiaryApp.Shared.Models;

public class Payment
{
    public int Id { get; set; }
    public int PeoplesId { get; set; }
    public int Ano { get; set; }
    public int Mes { get; set; }
    public DateTime Fecha { get; set; }
    public string? ComprobanteUrl { get; set; }  // ✅ Cambiado: era "Monto", ahora es "ComprobanteUrl"
    public Person? Person { get; set; }
}