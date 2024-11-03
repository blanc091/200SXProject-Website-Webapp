# 200SXProject Website - Roles
My website converted to ASP.NET MVC; DB inserts, services, form handlings and other.

Website has a user base functionality, wherein registered users are able to access an app where they can submit their own builds with pictures and description, and where other registered users can comment on; an email is sent to notify the build owner if there are new comments added. The comment owner can also delete his/her comment with a simple click.

Registered users are also able to use MaintenApp, which is a feature that allows them to log records of service intervals and part changes by due date; an email notification service is running in the background, checking the due dates and notifying the user of approaching deadlines.

I am also implementing a merchandise page, where registered users can select and buy branded merchandise. The merchandise is stored in a database and is displayed on the page with a picture, description, and price. The user can select the quantity and size of the item and add it to the cart. The cart is stored in a session and is displayed on the page with the total price of the items in the cart. The user can also remove items from the cart. On order placement, both the user and the admin get an email notification, so that the order can be processed with the data entered.

Website is also published on Azure Web Apps platform, which is used sort of like a pre-release testing interface before being published to the live domain. 
