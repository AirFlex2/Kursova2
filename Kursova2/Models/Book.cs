namespace Kursova2.Models
{
    public class Book
    {
        public int BookID { get; set; }
        public string BookName { get; set; }
        public string YearOfPub { get; set; }
        public string Genre { get; set; }
        public string BookStatus { get; set; }
        public string Publisher { get; set; }
        public string PageCount { get; set; }
        public string Price { get; set; }
        public string Theme { get; set; }
        public string CopyCount { get; set; }
        public int AvailableCount { get; set; }
    }
}