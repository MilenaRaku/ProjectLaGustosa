using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaGustosa.Resourse.Class
{
    class DishViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }

        public string Weight { get; set; }

        public byte[] Image { get; set; }

        public string PriceFormatted => Price.ToString("0.##", CultureInfo.InvariantCulture);

        public decimal Price { get; set; }

        public int Quantity { get; set; } = 0;


    }
}
