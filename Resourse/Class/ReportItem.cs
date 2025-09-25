using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaGustosa.Resourse.Class
{
    class ReportItem
    {
        public DateTime Date { get; set; }
        public int OrdersCount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AverageCheck { get; set; }
        public string MostPopularDish { get; set; }
    }
}
