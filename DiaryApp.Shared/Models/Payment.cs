namespace DiaryApp.Shared.Models;

public class Payment
{
    public int Id { get; set; }
    public int PeoplesId { get; set; }
    public int Ano { get; set; }
    public int Mes { get; set; }
    public decimal Monto { get; set; }
    public DateTime Fecha { get; set; }
    public Person? Person { get; set; }
}