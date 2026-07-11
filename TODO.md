In this document I will include all the things I should not forget about on the website.

## Functional

[x] Manage Customer Profile
[x] Manage Customer Addresses
[x] Add user roles - Admin should reveal admin area
[ ] Manage SKUs (Admin)
[ ] Manage Products (Admin) - Products page in Backoffice
    - Include list of products with reference, stock (cookies available), status (live for cookies available on the shop or draft for hidden cookies)
    - When selecting each cookie, include variants with cost price, price, stock, status (live or draft) and allow to delete the variant.
    - For each variant allow managing photos
    - For each variant allow managing variant composition (a.k.a SKUs) (add, edit, delete)
    - Create a new page to manage existing SKUs.
[ ] Manage Inventory (Admin) - Inventory page in Backoffice
    - Include metrics per day: 
      - Backed Today, 
      - Remaining to Bake with target from todays orders, 
      - daily quota percentage, fulfillment percentage
      - Cookies to bake (list of cookie product with how many are available and how many remaining to be baked for today)
    - Include batch management per day
      - Table with a list of batches (per cookie type), creation date, status, quantity (planned, actual, defected)
      - Allow single batch management - update status, update planned, actual and defect counts, update notes, add batch runs and allow updating their status, mins, temp and oven settings
[ ] Manage Customers - Customer page in Backoffice
    - Searchable customer table with pagination
    - Allow filtering by all, active or inactive.
    - Customer Page
      - Should include profile informaiton, contact and address and statistics along with a searchable customer order history paginated table
[ ] Manage Cart
    - Add product to cart
    - Manage product quantity in cart
    - Remove product from cart
[ ] Create Order from Cart
[ ] Manage Orders (Admin)
[ ] Implement Backoffice
    - Dashboard should contain metrics for specific date ranges (This Week, This Month, This Year) 
      - Total Orders, Total Revenue, New Customers, Avg. Rating
      - Revenue overview chart for the 7 days of current week for this week, all days of current month for this month, all months of current year for this year.
      - 4 Top selling cookies in descending order including the revenue per cookie type.
    -  Orders page should include a paginated table of all orders
      - Have an order table with pagination
      - For orders in processing status include missing cookies (cookies that are not in inventory and need to be baked).
      - Include list of cookies to bake (including number of cookies per cookie type) for the orders that are confirmed.
      - Default filter is orders that are confirmed, processing and ready to ship.
      - Include user created views with different filters.
[ ] Implement Communication Domain
[ ] Allow guest checkout
[ ] Implement Store Settings
    - Commerce - Allow editing store commerce settings.
    - Promotions - Allow managing promo codes, edit status (active or inactive) and allow deleting promo codes.
[ ] Perhaps include a new page in the backoffice for viewing all available carts and allow filtering (include bandoned carts with retention logs)
[ ] Implement logistics 
    - We want to have the capability to add handling partners. 
    - We need to be able to retrieve live tracking information.
    - Notify customer about tracking status

## Future Implementation

[ ] Advanced Promotions
    - We should include promo codes with date range validity periods.
    - We should allow personalized promo codes
    - We should add optional usage limits to promo codes.

## Decorational

1. In the account dashboard and backoffice, on the left navigation area, My Orders has a static 2. This should reflect the orders that are not in Shipped status for the account dashboard and new orders for backoffice.

2. In the account dashboard, on the header, the notification bell is showing like it has notifications. We should hide that behind a feature flag and only implement it when notifications are implemented in the store.

These are things that I should not forget to add.

 ## Small Tasks

1. Set as primary address doesn't work
1. Add notification popups.
1. Display errors on add address form instead of hanging
1. I should not be able to delete a default/primary address.
1. Rename to either primary or default for addresses.
1. Clicking on Logo should take us to Storefront also from Dashboard and Backoffice