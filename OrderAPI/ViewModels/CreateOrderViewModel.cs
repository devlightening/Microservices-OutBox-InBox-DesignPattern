namespace OrderAPI.ViewModels
{
    public class CreateOrderViewModel
    {
        public int BuyerId { get; set; }
        public List<CreateOrderItemViewModel> OrderItems { get; set; }
    }
}
