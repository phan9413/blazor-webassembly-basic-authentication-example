namespace BlazorApp.Models
{
    //public class User
    //{
    //    public int Id { get; set; }
    //    public string FirstName { get; set; }
    //    public string LastName { get; set; }
    //    public string Username { get; set; }
    //    public string AuthData { get; set; }
    //}
    public class LoginModel
    {
        [System.ComponentModel.DataAnnotations.Required]
        public string UserName { get; set; }

        //[Required]
        public string Password { get; set; }
    }

    public class User
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public Company Company { get; set; }
        public Employee Employee { get; set; }
        public string AuthData { get; set; }
    }
    public class Company
    {
        public int Oid { get; set; }
        public string BoCode { get; set; }
        public string BoName { get; set; }

    }
    public class Employee
    {
        public int Oid { get; set; }
        public string FullName { get; set; }
        public vwWarehouses WhsCode { get; set; }
    }

    public class vwBusinessPartners
    {
        public string BoKey { get; set; }
        public string CardCode { get; set; }
        public string CardName { get; set; }
        public string CardType { get; set; }
    }
    public class vwItemMasters
    {
        public string BoKey { get; set; }
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
    }
    public class vwWarehouseBins
    {
        public string BoKey { get; set; }
        public string BinCode { get; set; }
        public string WhsCode { get; set; }
        public string WhsName { get; set; }
        public int BinAbsEntry { get; set; }
    }
    public class vwWarehouses
    {
        public string BoKey { get; set; }
        public string WhsCode { get; set; }
        public string WhsName { get; set; }
    }
}