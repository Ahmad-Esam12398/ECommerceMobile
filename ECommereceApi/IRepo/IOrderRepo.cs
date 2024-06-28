﻿using ECommereceApi.DTOs.Order;
using ECommereceApi.DTOs.Product;
using ECommereceApi.Enums;

namespace ECommereceApi.IRepo
{
    public interface IOrderRepo
    {
        Task<bool> IsAllCartItemsAvailableAsync(CartProductsDTO cartProducts);
        Task<OrderPreviewDTO> GetOrderPreviewAsync(CartProductsDTO cartProductsDTO);
        //Task<OrderPreviewWithoutOffersDTO> AddOrderWithoutOfferAsync(CartProductsDTO cartProductsDTO, AddOrderOfferDTO addOrderOfferDTO);
        Task<OrderPreviewWithoutOffersDTO> AddOrderAsync(CartProductsDTO cartProductsDTO, AddOrderOfferDTO addOrderOfferDTO, double finalOfferPrice);
        Task<Order> GetOrderByIdAsync(Guid orderId);
        Task ChangeOrderStatusAsync(Guid orderId, OrderStatus newStatus);
        Task<PagedResult<OrderDisplayDTO>> GetUserOrdersPaginatedAsync(int userId, int page, int pageSize);
        Task<PagedResult<OrderDisplayDTO>> GetAllOrdersPaginatedAsync(int page, int pageSize);
        Task<PagedResult<OrderDisplayDTO>> GetUserOrdersByStatusPaginatedAsync(int userId, OrderStatus orderStatus, int page, int pageSize);
        Task<PagedResult<OrderDisplayDTO>> GetOrdersByStatusPaginatedAsync(OrderStatus orderStatus, int page, int pageSize);
        Task<Tuple<int, int>> GetFinalOfferPriceAsync(int offerId, int userId);
        Task<Guid> ConfirmOrder(AddOrderOfferDTO addOrderOfferDTO);
    }
}
