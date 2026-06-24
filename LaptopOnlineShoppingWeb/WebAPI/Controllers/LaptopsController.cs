//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.OData.Query;
//using Microsoft.AspNetCore.OData.Routing.Controllers;
//using Microsoft.EntityFrameworkCore;
//using WebAPI.Data;
//using WebAPI.DTOs.Laptops;
//using WebAPI.Entities;

//namespace WebAPI.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class LaptopsController : ODataController
//    {
//        private readonly ApplicationDbContext _context;

//        public LaptopsController(ApplicationDbContext context)
//        {
//            _context = context;
//        }

//        // GET: odata/Laptops (OData Queryable)
//        [HttpGet("/odata/Laptops")]
//        [EnableQuery]
//        public IActionResult Get()
//        {
//            return Ok(_context.Laptops);
//        }

//        // GET: odata/Laptops(1) (OData Queryable)
//        [HttpGet("/odata/Laptops({key})")]
//        [EnableQuery]
//        public async Task<IActionResult> Get([FromRoute] int key)
//        {
//            var laptop = await _context.Laptops.FindAsync(key);
//            if (laptop == null)
//            {
//                return NotFound(new { message = "Không tìm thấy laptop này." });
//            }
//            return Ok(laptop);
//        }

//        // GET: api/Laptops/{id}
//        [HttpGet("{id}")]
//        public async Task<IActionResult> GetById(int id)
//        {
//            var laptop = await _context.Laptops
//                .Include(l => l.Category)
//                .FirstOrDefaultAsync(l => l.LaptopId == id);
            
//            if (laptop == null)
//            {
//                return NotFound(new { message = "Không tìm thấy laptop này." });
//            }
//            return Ok(laptop);
//        }

//        // POST: api/Laptops
//        [HttpPost]
//        [Authorize(Roles = "Admin,Staff")]
//        public async Task<IActionResult> Create([FromBody] LaptopCreateDto dto)
//        {
//            if (!ModelState.IsValid) return BadRequest(ModelState);

//            // Check duplicate code
//            if (await _context.Laptops.AnyAsync(l => l.LaptopCode == dto.LaptopCode))
//            {
//                return BadRequest(new { message = "Mã laptop đã tồn tại." });
//            }

//            var laptop = new Laptop
//            {
//                LaptopCode = dto.LaptopCode,
//                LaptopName = dto.LaptopName,
//                Price = dto.Price,
//                StockQuantity = dto.StockQuantity,
//                CategoryId = dto.CategoryId,
//                Status = dto.Status,
//                CreatedDate = DateTime.Now
//            };

//            await _context.Laptops.AddAsync(laptop);
//            await _context.SaveChangesAsync();

//            return CreatedAtAction(nameof(GetById), new { id = laptop.LaptopId }, laptop);
//        }

//        // PUT: api/Laptops/{id}
//        [HttpPut("{id}")]
//        [Authorize(Roles = "Admin,Staff")]
//        public async Task<IActionResult> Update(int id, [FromBody] LaptopUpdateDto dto)
//        {
//            if (!ModelState.IsValid) return BadRequest(ModelState);

//            var laptop = await _context.Laptops.FindAsync(id);
//            if (laptop == null) return NotFound(new { message = "Không tìm thấy laptop này." });

//            // Check duplicate code (excluding current laptop)
//            if (await _context.Laptops.AnyAsync(l => l.LaptopCode == dto.LaptopCode && l.LaptopId != id))
//            {
//                return BadRequest(new { message = "Mã laptop đã tồn tại ở sản phẩm khác." });
//            }

//            laptop.LaptopCode = dto.LaptopCode;
//            laptop.LaptopName = dto.LaptopName;
//            laptop.Price = dto.Price;
//            laptop.StockQuantity = dto.StockQuantity;
//            laptop.CategoryId = dto.CategoryId;
//            laptop.Status = dto.Status;

//            _context.Entry(laptop).State = EntityState.Modified;
//            await _context.SaveChangesAsync();

//            return Ok(laptop);
//        }

//        // DELETE: api/Laptops/{id}
//        [HttpDelete("{id}")]
//        [Authorize(Roles = "Admin")]
//        public async Task<IActionResult> Delete(int id)
//        {
//            var laptop = await _context.Laptops.FindAsync(id);
//            if (laptop == null) return NotFound(new { message = "Không tìm thấy laptop." });

//            // Logic Xóa: Kiểm tra xem Laptop đã có trong OrderDetails chưa. 
//            // Nếu có thì chỉ update Status = false (ngừng kinh doanh), ngược lại cho xóa cứng.
//            bool hasOrders = await _context.OrderDetails.AnyAsync(od => od.LaptopId == id);
//            if (hasOrders)
//            {
//                laptop.Status = false; // Xóa mềm
//                _context.Entry(laptop).State = EntityState.Modified;
//                await _context.SaveChangesAsync();
//                return Ok(new { message = "Laptop đã tồn tại trong hóa đơn bán lẻ. Tự động chuyển trạng thái ngừng kinh doanh (Status = 0).", isSoftDeleted = true });
//            }
//            else
//            {
//                _context.Laptops.Remove(laptop);
//                await _context.SaveChangesAsync();
//                return Ok(new { message = "Đã xóa sản phẩm thành công khỏi hệ thống.", isSoftDeleted = false });
//            }
//        }

//        // POST: api/Laptops/{laptopCode}/upload-image
//        [HttpPost("{laptopCode}/upload-image")]
//        [Authorize(Roles = "Admin,Staff")]
//        public async Task<IActionResult> UploadImage(string laptopCode, IFormFile file)
//        {
//            if (file == null || file.Length == 0) return BadRequest(new { message = "Vui lòng chọn file hình ảnh." });

//            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
//            var extension = Path.GetExtension(file.FileName).ToLower();
//            if (!allowedExtensions.Contains(extension))
//            {
//                return BadRequest(new { message = "Định dạng file không hợp lệ (Chỉ nhận .jpg, .jpeg, .png, .webp)." });
//            }

//            var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
//            if (!Directory.Exists(uploadDir))
//            {
//                Directory.CreateDirectory(uploadDir);
//            }

//            // Clean up any existing image files with different extensions for this laptopCode
//            foreach (var ext in allowedExtensions)
//            {
//                var oldFile = Path.Combine(uploadDir, $"{laptopCode}{ext}");
//                try
//                {
//                    if (System.IO.File.Exists(oldFile))
//                    {
//                        System.IO.File.Delete(oldFile);
//                    }
//                }
//                catch (Exception)
//                {
//                    // Safely ignore if the file is currently locked/in use
//                }
//            }

//            var fileName = $"{laptopCode}{extension}";
//            var filePath = Path.Combine(uploadDir, fileName);

//            using (var stream = new FileStream(filePath, FileMode.Create))
//            {
//                await file.CopyToAsync(stream);
//            }

//            return Ok(new { fileName = fileName, url = $"/uploads/{fileName}" });
//        }
//    }
//}
