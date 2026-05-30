using System;

namespace BugTracker.Models
{
    public class BugItem
    {
        public int      IdNapake          { get; set; }
        public string   Naslov            { get; set; }
        public string   Opis              { get; set; }
        public string   Status            { get; set; }
        public string   Prioriteta        { get; set; }
        public int?     IdKategorije      { get; set; }
        public string   Kategorija        { get; set; }
        public int      IdUstvaritelja    { get; set; }
        public string   Ustvaritelj       { get; set; }
        public int?     IdDodeljenega     { get; set; }
        public string   Dodeljen          { get; set; }
        public DateTime DatumUstvarjeno   { get; set; }
        public DateTime DatumSpremenjeno  { get; set; }
    }

    public class UserItem
    {
        public int    IdUporabnika    { get; set; }
        public string UporabniskoIme  { get; set; }
        public string Ime             { get; set; }
        public string Priimek         { get; set; }
        public string ImeInPriimek    => $"{Ime} {Priimek}";
        public string Email           { get; set; }
        public string Vloga           { get; set; }
        public bool   Aktiven         { get; set; }
        public string AktivenText     => Aktiven ? "Aktiven" : "Neaktiven";
    }

    public class CategoryItem
    {
        public int    IdKategorije { get; set; }
        public string Naziv        { get; set; }
        public string Opis         { get; set; }
        public int    StNapak      { get; set; }
    }

    public class HistoryItem
    {
        public int    IdZgodovine   { get; set; }
        public string UporabniskoIme{ get; set; }
        public string Polje         { get; set; }
        public string StaraVrednost { get; set; }
        public string NovaVrednost  { get; set; }
        public string DatumCas      { get; set; }
        public string Opis => $"{DatumCas}  |  {UporabniskoIme}  |  {Polje}: {StaraVrednost} → {NovaVrednost}";
    }

    public class StatsModel
    {
        public int SkupajNapak   { get; set; }
        public int Odprtih       { get; set; }
        public int VDelu         { get; set; }
        public int Resenih       { get; set; }
        public int Zaprtih       { get; set; }
        public int SkupajUsers   { get; set; }
        public int AktivnihUsers { get; set; }
    }
}
