using AutoPartsB2B.Core.DTOs;
using AutoPartsB2B.Core.Entities;
using AutoPartsB2B.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoPartsB2B.WebAPI.Hubs;
using Microsoft.AspNetCore.SignalR;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AutoPartsB2B.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<StockHub> _hubContext;

        public OrdersController(AppDbContext context, IHubContext<StockHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;   
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] OrderRequestDto request)
        {
            // İşlem Güvenliği (Transaction): Hata olursa hiçbir stok değişmez
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                decimal total = 0;
                var order = new Order { UserId = request.UserId, OrderItems = new List<OrderItem>() };

                foreach (var item in request.Items)
                {
                    var product = await _context.Products.FindAsync(item.ProductId);
                    if (product == null) return NotFound($"Ürün bulunamadı (ID: {item.ProductId})");

                    if (product.StockQuantity < item.Quantity)
                        return BadRequest($"{product.Name} için yeterli stok yok!");

                    // Stoktan düşme işlemi
                    product.StockQuantity -= item.Quantity;

                    order.OrderItems.Add(new OrderItem
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = (decimal)product.Price
                    });

                    total += (item.Quantity * (decimal)product.Price);
                }

                order.TotalAmount = total;
                _context.Orders.Add(order);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync(); // İşlemi kalıcı olarak onayla

                foreach (var item in request.Items)
                {
                    var product = await _context.Products.FindAsync(item.ProductId);
                    if (product != null)
                    {
                        await _hubContext.Clients.All.SendAsync("ReceiveStockUpdate", product.Id, product.StockQuantity);
                    }
                }

                return Ok(new { Message = "Sipariş başarıyla oluşturuldu, stoklar güncellendi.", OrderId = order.Id });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(); // Hata durumunda işlemi geri al
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUserOrders(int userId)
        {
            // EF Core ile ilişkili tabloları (Include/ThenInclude) çekiyoruz
            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.Id)
                .Select(o => new {
                    o.Id,
                    o.TotalAmount,
                    Items = o.OrderItems.Select(oi => new {
                        oi.Product.Name,
                        oi.Quantity,
                        oi.UnitPrice,
                        SubTotal = oi.Quantity * oi.UnitPrice
                    })
                })
                .ToListAsync();

            return Ok(orders);
        }

        [HttpGet("invoice/{orderId}")]
        public async Task<IActionResult> DownloadInvoice(int orderId)
        {
            // İlgili siparişi ve ürün detaylarını veritabanından çek
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null) return NotFound("Sipariş bulunamadı.");

            // QuestPDF topluluk lisansını (ücretsiz kullanım) onayla
            QuestPDF.Settings.License = LicenseType.Community;

            // PDF Belgesini Tasarla
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    // BAŞLIK KISMI
                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("KAYSERİ OTO YEDEK PARÇA B2B").SemiBold().FontSize(20).FontColor(Colors.Orange.Darken2);
                            col.Item().Text("Resmi Olmayan Sipariş Özeti").FontSize(14).FontColor(Colors.Grey.Medium);
                        });
                        row.ConstantItem(100).AlignRight().Text($"Sipariş No: #{order.Id}").SemiBold().FontSize(14);
                    });

                    // İÇERİK (TABLO) KISMI
                    page.Content().PaddingVertical(1, Unit.Centimetre).Column(col =>
                    {
                        col.Item().PaddingBottom(10).Text($"Tarih: {DateTime.Now:dd.MM.yyyy HH:mm}").FontSize(10);

                        col.Item().Table(table =>
                        {
                            // Sütun genişlikleri
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(); // Ürün Adı (Esnek)
                                columns.ConstantColumn(60); // Miktar
                                columns.ConstantColumn(80); // Birim Fiyat
                                columns.ConstantColumn(80); // Toplam
                            });

                            // Tablo Başlıkları
                            table.Header(header =>
                            {
                                header.Cell().BorderBottom(1).Padding(5).Text("Ürün Adı").SemiBold();
                                header.Cell().BorderBottom(1).Padding(5).AlignRight().Text("Miktar").SemiBold();
                                header.Cell().BorderBottom(1).Padding(5).AlignRight().Text("Birim Fiyat").SemiBold();
                                header.Cell().BorderBottom(1).Padding(5).AlignRight().Text("Toplam").SemiBold();
                            });

                            // Sipariş Edilen Ürünleri Tabloya Ekle
                            foreach (var item in order.OrderItems)
                            {
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(item.Product.Name);
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignRight().Text($"{item.Quantity} Adet");
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignRight().Text($"{item.UnitPrice:N2} ₺");
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignRight().Text($"{item.Quantity * item.UnitPrice:N2} ₺");
                            }
                        });

                        // GENEL TOPLAM
                        col.Item().PaddingTop(15).AlignRight().Text($"GENEL TOPLAM: {order.TotalAmount:N2} ₺")
                            .SemiBold().FontSize(14).FontColor(Colors.Red.Medium);
                    });

                    // Footer
                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Sayfa ");
                        x.CurrentPageNumber();
                        x.Span(" / ");
                        x.TotalPages();
                    });
                });
            });

            // PDF'i byte dizisi olarak oluştur ve dosyaya çevir
            var pdfBytes = document.GeneratePdf();
            return File(pdfBytes, "application/pdf", $"Siparis_Faturasi_{order.Id}.pdf");
        }
    }
}