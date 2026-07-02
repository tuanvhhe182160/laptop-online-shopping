using WebAPI.DTOs;
using WebAPI.Entities;
using WebAPI.Repositories;

namespace WebAPI.Services
{
    public interface ICustomerService
    {
        Task<IEnumerable<CustomerResponseDTO>> GetAllCustomersAsync();
        Task<CustomerResponseDTO?> GetCustomerByIdAsync(int id);
        Task<CustomerResponseDTO> CreateCustomerAsync(CustomerCreateDTO dto);
        Task<bool> UpdateCustomerAsync(int id, CustomerUpdateDTO dto);
        Task<bool> DeleteCustomerAsync(int id);
    }

    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _repository;
        private readonly ICartRepository _cartRepository; // Tự động tạo Cart

        public CustomerService(ICustomerRepository repository, ICartRepository cartRepository)
        {
            _repository = repository;
            _cartRepository = cartRepository;
        }

        public async Task<IEnumerable<CustomerResponseDTO>> GetAllCustomersAsync()
        {
            var customers = await _repository.GetAllAsync();
            return customers.Select(c => new CustomerResponseDTO
            {
                CustomerId = c.CustomerId,
                Username = c.Username,
                FullName = c.FullName,
                Email = c.Email,
                Phone = c.Phone,
                Address = c.Address,
                IsActive = c.IsActive
            });
        }

        public async Task<CustomerResponseDTO?> GetCustomerByIdAsync(int id)
        {
            var c = await _repository.GetByIdAsync(id);
            if (c == null) return null;

            return new CustomerResponseDTO
            {
                CustomerId = c.CustomerId,
                Username = c.Username,
                FullName = c.FullName,
                Email = c.Email,
                Phone = c.Phone,
                Address = c.Address,
                IsActive = c.IsActive
            };
        }

        public async Task<CustomerResponseDTO> CreateCustomerAsync(CustomerCreateDTO dto)
        {
            var customer = new Customer
            {
                Username = dto.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                FullName = dto.FullName,
                Email = dto.Email,
                Phone = dto.Phone,
                Address = dto.Address,
                IsActive = true
            };

            await _repository.AddAsync(customer);
            await _repository.SaveAsync();

            // FR4.1: Khởi tạo Giỏ hàng khi khách tạo tài khoản
            var cart = new Cart
            {
                CustomerId = customer.CustomerId,
                CreatedDate = System.DateTime.Now
            };
            await _cartRepository.AddAsync(cart);
            await _cartRepository.SaveAsync();

            return new CustomerResponseDTO
            {
                CustomerId = customer.CustomerId,
                Username = customer.Username,
                FullName = customer.FullName,
                Email = customer.Email,
                Phone = customer.Phone,
                Address = customer.Address,
                IsActive = customer.IsActive
            };
        }

        public async Task<bool> UpdateCustomerAsync(int id, CustomerUpdateDTO dto)
        {
            var customer = await _repository.GetByIdAsync(id);
            if (customer == null) return false;

            customer.FullName = dto.FullName;
            customer.Phone = dto.Phone;
            customer.Address = dto.Address;
            customer.IsActive = dto.IsActive;

            _repository.Update(customer);
            await _repository.SaveAsync();
            return true;
        }

        public async Task<bool> DeleteCustomerAsync(int id)
        {
            var customer = await _repository.GetByIdAsync(id);
            if (customer == null) return false;

            // Xóa mềm
            customer.IsActive = false;
            _repository.Update(customer);
            await _repository.SaveAsync();
            return true;
        }
    }
}
