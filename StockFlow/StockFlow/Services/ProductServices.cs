using Microsoft.AspNetCore.Http.HttpResults;
using StockFlow.DTOs.Product;
using StockFlow.Interfaces;
using StockFlow.IRepository;
using StockFlow.Models;

using StockFlow.Exceptions;

namespace StockFlow.Services
{
    public class ProductServices : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepo _categoryRepository;


        public ProductServices(IProductRepository productRepository, ICategoryRepo categoryRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
        }

        public async Task<ProductResponse> CreateAsync(CreateProductRequest request)
        {
            var categoryExists = await _categoryRepository.ExistsAsync(request.CategoryId);
            if (!categoryExists)
                throw new NotFoundException("Category not found.");
            var skuExists = await _productRepository.ExistsBySKUAsync(request.SKU);
            if (skuExists)
                throw new ConflictException("SKU already exists.");
            if (request.Price < 0)
                throw new ValidationException("Price cannot be negative.");
            Product p = new Product
            {
                Name = request.Name,
                Price = request.Price,
                SKU = request.SKU,
                Description = request.Description,
                CategoryId = request.CategoryId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            await _productRepository.AddAsync(p);
            await _productRepository.SaveChangesAsync();
            return new ProductResponse
            {
                Id = p.Id,
                Name = p.Name,
                SKU = p.SKU,
                Description = p.Description,
                Price = p.Price,
                IsActive = p.IsActive,
                CategoryId = p.CategoryId,
                CategoryName = (await _categoryRepository.GetByIdAsync(p.CategoryId))?.Name ?? string.Empty,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            };
        }

        public async Task<string> DeactivateAsync(int id)
        {
            //ExistsAsync return bool
            var exists = await _productRepository.ExistsAsync(id);
            if (!exists)
                throw new NotFoundException("Product not found.");
            //return object of product
            var product = await _productRepository.GetByIdAsync(id);
            if(product.IsActive==false)
            {
                throw new ConflictException("Product already deactive.");
            }
            product.IsActive = false;
            product.UpdatedAt = DateTime.UtcNow;
            _productRepository.Update(product);
            await _productRepository.SaveChangesAsync();

            return "Product deactivated successfully.";

        }

        public async Task<IEnumerable<ProductResponse>> GetAllAsync()
        {
            //GetAllWithCategoryAsync returns a list of products with their categories
            var products = await _productRepository.GetAllWithCategoryAsync();
            return products.Select(p => new ProductResponse
            {
                Id = p.Id,
                Name = p.Name,
                SKU = p.SKU,
                Description = p.Description,
                Price = p.Price,
                IsActive = p.IsActive,
                CategoryId = p.CategoryId,
                CategoryName = p.Category?.Name ?? string.Empty,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            });
            
        }

        public async Task<ProductResponse?> GetByIdAsync(int id)
        {
            var product = await _productRepository.GetByIdWithCategoryAsync(id);
            if (product == null)
                throw new NotFoundException("Product not found.");
            return new ProductResponse
            {
                Id = product.Id,
                Name = product.Name,
                SKU = product.SKU,
                Description = product.Description,
                Price = product.Price,
                IsActive = product.IsActive,
                CategoryId = product.CategoryId,
                CategoryName = product.Category?.Name ?? string.Empty,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt
            };

        }

        public async Task<ProductResponse> UpdateAsync(int id, UpdateProductRequest request)
        {
            var product = await _productRepository.GetByIdAsync(id);

            if (product == null)
                throw new NotFoundException("Product not found.");

            var categoryExists =
                await _categoryRepository.ExistsAsync(request.CategoryId);

            if (!categoryExists)
                throw new NotFoundException("Category not found.");

            if (product.SKU != request.SKU)
            {
                var skuExists =
                    await _productRepository.ExistsBySKUAsync(request.SKU);

                if (skuExists)
                    throw new ConflictException("SKU already exists.");
            }

            if (request.Price < 0)
                throw new ValidationException("Price cannot be negative.");

            product.Name = request.Name;
            product.SKU = request.SKU;
            product.Description = request.Description;
            product.Price = request.Price;
            product.CategoryId = request.CategoryId;
            product.IsActive = request.IsActive;
            product.UpdatedAt = DateTime.UtcNow;

            _productRepository.Update(product);

            await _productRepository.SaveChangesAsync();

            var updatedProduct =
                await _productRepository.GetByIdAsync(id);

            return new ProductResponse
            {
                Name=updatedProduct.Name,
                SKU=updatedProduct.SKU,
                Description=updatedProduct.Description,
                Price=updatedProduct.Price,
                IsActive=updatedProduct.IsActive,
                UpdatedAt=updatedProduct.UpdatedAt,
            };
        }

      
    }
}