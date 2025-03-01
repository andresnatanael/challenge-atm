using System.ComponentModel.DataAnnotations;

namespace AtmChallenge.Domain.Entities
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        public string Username { get; set; }
        public string DocumentNumber { get; set; }
        public List<Card.Card> Cards { get; set; }
    }

   
}