using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class Entry

    {
        public Entry(string titel, string text, DateTime date, string author, int votes) 
        {
            this.Titel = titel;
            this.Text = text;
            this.Date = date;
            this.Author = author;
            this.Votes = votes;
            this.CommentList = new List<Comment>();
        }
        public long EntryId { get; set; }

        public string? Titel { get; set; }

        public string? Text { get; set; }

        public DateTime? Date { get; set; }

        public string? Author { get; set; }

        public int Votes { get; set; }

        public List<Comment> CommentList  { get; set; }


    
    }
}
