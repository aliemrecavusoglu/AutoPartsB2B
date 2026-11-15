using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPartsB2B.Core.DTOs
{
    public class OrderRequestDto
    {
        public int UserId { get; set; }
        public List<CartItemDto> Items { get; set; }
    }
}
