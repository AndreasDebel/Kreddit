using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class Comment

    {
        public Comment(string text, DateTime date, string author, int votes) 
        {
            this.Text = text;
            this.Date = date;
            this.Author = author;
            this.Votes = votes;
        }
        public long CommentId { get; set; }

        public string? Text { get; set; }

        public DateTime? Date { get; set; }

        public string? Author { get; set; }

        public int Votes { get; set; }


    
    }
}
