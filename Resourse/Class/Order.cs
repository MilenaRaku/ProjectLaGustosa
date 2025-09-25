using LaGustosa.DataModel.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaGustosa.Resourse.Class
{
    class Order
    {
        public int Number { get; set; }
        public System.DateTime OrderOpenings { get; set; }
        public List<OrderComposition> OrderCompositions { get; set; }

        public Order()
        {
            OrderCompositions = new List<OrderComposition>();
        }

        public void AddDish(Dish dish)
        {
            var orderComposition = OrderCompositions.FirstOrDefault(oc => oc.DishId == dish.Id);
            if (orderComposition != null)
            {
                orderComposition.NumberPositions++; // Увеличиваем количество, если блюдо уже есть в заказе
            }
            else
            {
                OrderCompositions.Add(new OrderComposition
                {
                    DishId = dish.Id,
                    NumberPositions = 1,
                    Dish = dish
                });
            }
        }

        public void RemoveDish(int dishId)
        {
            var orderComposition = OrderCompositions.FirstOrDefault(oc => oc.DishId == dishId);
            if (orderComposition != null)
            {
                orderComposition.NumberPositions--; // Уменьшаем количество
                if (orderComposition.NumberPositions <= 0)
                {
                    OrderCompositions.Remove(orderComposition); // Удаляем блюдо, если количество = 0
                }
            }
        }
    }
}
