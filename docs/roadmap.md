# CookieCommerce Roadmap

This roadmap describes the intended milestones for the CookieCommerce application. It is a planning document, not a commitment to implement every proposed feature unchanged: confirm each milestone's API contracts, safety boundaries, and scope before starting it.

## Customer account administration — Complete

- Backoffice administrators can create a customer using first name, last name, and email.
- Every backoffice-created customer is provisioned with a linked passwordless customer account.
- Welcome email delivery is optional during creation and can be requested later from customer detail.
- Welcome links open the first-party password setup page; successful setup signs the customer in.
- Administrators can upgrade customers to administrators and downgrade other administrators.
- Self-downgrade and removal of the final administrator are rejected by the API.
- Role safeguards have automated tests, and the solution build passes.
